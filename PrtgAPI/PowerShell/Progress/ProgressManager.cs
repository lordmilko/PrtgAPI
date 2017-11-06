using System;
using System.Management.Automation;
using System.Reflection;
using Microsoft.PowerShell.Commands;
using PrtgAPI.Helpers;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Progress
{
    internal class ProgressManager : IDisposable
    {
        private static ProgressPipelineStack progressPipelines = new ProgressPipelineStack();

        internal int ProgressPipelinesCount => progressPipelines.Count;

        internal const string DefaultActivity = "Activity";
        internal const string DefaultDescription = "Description";

        public ProgressRecordEx CurrentRecord => progressPipelines.CurrentRecordInPipeline;

        /// <summary>
        /// Indicates that the current cmdlet expects that it will contain progress, either because it already does, or it will when a record is written to the pipeline.
        /// </summary>
        public bool ExpectsContainsProgress => CurrentRecord.Activity != DefaultActivity && (CurrentRecord.StatusDescription != DefaultDescription || InitialDescription != string.Empty);

        /// <summary>
        /// Indicates that a record has actually been written to the pipeline for this cmdlet, this initializing the status description.
        /// </summary>
        public bool ProgressWritten => CurrentRecord.StatusDescription != DefaultDescription;

        public bool PreviousContainsProgress => PreviousRecord != null && PreviousRecord.Activity != DefaultActivity && PreviousRecord.StatusDescription != DefaultDescription;

        /// <summary>
        /// Indicates whether the current cmdlet is first in a chain of pure PrtgAPI cmdlets (with no third party filters in between, etc)
        /// </summary>
        public bool FirstInChain => pipeToProgressCompatibleCmdlet && progressPipelines.RecordsInCurrentPipeline == 1;

        /// <summary>
        /// Indicates whether the current cmdlet is part of a chain of PrtgAPI cmdlets (with no unsupported third party filters in between, etc)
        /// </summary>
        public bool PartOfChain => pipeFromProgressCompatibleCmdlet && (pipeToProgressCompatibleCmdlet || progressPipelines.RecordsInCurrentPipeline > 1);

        /// <summary>
        /// Indicates whether the next cmdlet in the pipeline is compatible with PrtgAPI Progress.
        /// </summary>
        private bool pipeToProgressCompatibleCmdlet
        {
            get
            {
                var downstreamCmdlet = cmdlet.CommandRuntime.GetDownstreamCmdlet();

                if (cmdlet.MyInvocation.MyCommand.ModuleName == downstreamCmdlet?.ModuleName)
                    return true;

                var downstreamCmdletType = downstreamCmdlet?.GetType().GetProperty("ImplementingType")?.GetValue(downstreamCmdlet) as Type;

                if (downstreamCmdletType == typeof (WhereObjectCommand))
                {
                    if(cmdlet.PipelineIsProgressPureToPrtgCmdlet())
                        return true;
                    return false;
                }

                return false;
            }
        }

        private bool pipeFromProgressCompatibleCmdlet
        {
            get
            {
                var upstreamCmdlet = (CommandInfo)cmdlet.GetUpstreamCmdlet()?.GetInternalProperty("CommandInfo");

                if (upstreamCmdlet == null)
                    return true;

                if (cmdlet.MyInvocation.MyCommand.ModuleName == upstreamCmdlet?.ModuleName)
                    return true;

                var upstreamCmdletType = upstreamCmdlet?.GetType().GetProperty("ImplementingType")?.GetValue(upstreamCmdlet) as Type;

                if (upstreamCmdletType == typeof (WhereObjectCommand))
                {
                    if (cmdlet.PipelineIsProgressPureFromPrtgCmdlet())
                        return true;
                    return false;
                }

                return false;
            }
        }

        public bool LastInChain => !pipeToProgressCompatibleCmdlet;

        public string InitialDescription { get; set; }

        public int? TotalRecords { get; set; }

        /// <summary>
        /// The object collection that was piped into all subsequent statements at the start of the entire pipeline.
        /// </summary>
        public Pipeline EntirePipeline { get; set; }

        /// <summary>
        /// The object collection being piped into this cmdlet.<para/>
        /// If Variable -> Where -> PrtgCmdlet, the EntirePipeline will be used (allowing us to bypass the where-object and retrieve the original array)<para/>
        /// If PrtgCmdlet -> Where -> PrtgCmdlet, for the first PrtgCmdlet the CmdletPipeline and EntirePipeline will be the same. For the second PrtgCmdlet, the CmdletPipeline will be used
        /// </summary>
        public Pipeline Pipeline => progressPipelines.RecordsInCurrentPipeline == 1 ? EntirePipeline : CmdletPipeline;

        /// <summary>
        /// The object collection that was piped into this cmdlet from the previous statement.
        /// </summary>
        public Pipeline CmdletPipeline { get; set; }

        //Display progress when piping multiple values from a variable, or a single value to multiple cmdlets
        public bool PipeFromVariableWithProgress => EntirePipeline?.List.Count > 1 || (EntirePipeline?.List.Count == 1 && PartOfChain);

        private bool? pipelineContainsOperation;

        public bool PipelineContainsOperation
        {
            get
            {
                //Cache the result for performance
                if (pipelineContainsOperation == null)
                    pipelineContainsOperation = cmdlet.PipelineSoFarHasCmdlet<PrtgOperationCmdlet>();

                return pipelineContainsOperation.Value;
            }
        }

        private bool? pipelineBeforeMeContainsOperation;

        public bool PipelineBeforeMeContainsOperation
        {
            get
            {
                if (pipelineBeforeMeContainsOperation == null)
                    pipelineBeforeMeContainsOperation = cmdlet.PipelineBeforeMeHasCmdlet<PrtgOperationCmdlet>();

                return pipelineBeforeMeContainsOperation.Value;
            }
        }

        public ProgressRecordEx PreviousRecord => progressPipelines.PreviousRecordInPipeline;

        private PSCmdlet cmdlet;

        private int recordsProcessed = -1;

        private bool variableProgressDisplayed;

        private IProgressWriter progressWriter;

        internal static IProgressWriter CustomWriter { get; set; }

        internal ProgressScenario Scenario { get; set; }

        private bool? pipelineIsPure;

        private bool sourceIdUpdated;

        /// <summary>
        /// Indicates whether the pipeline has been contamined by non-PrtgAPI cmdlets or cmdlets that don't support PrtgAPI progress.
        /// </summary>
        public bool PipelineIsProgressPure
        {
            get
            {
                if (pipelineIsPure == null)
                    pipelineIsPure = cmdlet.PipelineIsProgressPure();

                return pipelineIsPure.Value;
            }
        }

        public ProgressManager(PSCmdlet cmdlet)
        {
            var sourceId = GetLastSourceId(cmdlet.CommandRuntime);
            progressPipelines.Push(DefaultActivity, DefaultDescription, cmdlet, sourceId);

            if (PreviousRecord != null)
                CurrentRecord.ParentActivityId = PreviousRecord.ActivityId;

            this.cmdlet = cmdlet;
            EntirePipeline = cmdlet.CommandRuntime.GetPipelineInput();
            CmdletPipeline = cmdlet.CommandRuntime.GetCmdletPipelineInput(cmdlet);

            progressWriter = GetWriter();

            CalculateProgressScenario();
        }

        private void CalculateProgressScenario()
        {
            if (PartOfChain)
            {
                if (PipeFromVariableWithProgress)
                    Scenario = ProgressScenario.VariableToMultipleCmdlets;
                else
                    Scenario = ProgressScenario.MultipleCmdlets;
            }
            else
            {
                if (PipeFromVariableWithProgress)
                    Scenario = ProgressScenario.VariableToSingleCmdlet;
                else
                    Scenario = ProgressScenario.NoProgress;
            }
        }

        private IProgressWriter GetWriter()
        {
            if (CustomWriter != null)
                return CustomWriter;
            else
                return new ProgressWriter(cmdlet);
        }

        ~ProgressManager()
        {
            Dispose(false);
        }

        #region IDisposable

        private bool disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                progressPipelines.Pop();
            }

            disposed = true;
        }

        #endregion

        public void RemovePreviousOperation()
        {
            if (PreviousRecord != null && PreviousRecord.CurrentOperation != null)
            {
                PreviousRecord.CurrentOperation = null;

                WriteProgress(PreviousRecord);
            }
        }

        private void WriteProgress()
        {
            WriteProgress(CurrentRecord);
        }

        public void WriteProgress(string activity, string statusDescription)
        {
            CurrentRecord.Activity = activity;
            CurrentRecord.StatusDescription = statusDescription;

            WriteProgress();
        }

        private void WriteProgress(ProgressRecordEx progressRecord)
        {
            if (progressRecord.Activity == DefaultActivity || progressRecord.StatusDescription == DefaultDescription)
                throw new InvalidOperationException("Attempted to write progress on an uninitialized ProgressRecord. If this is a Release build, please report this bug along with the cmdlet chain you tried to execute. To disable PrtgAPI Cmdlet Progress in the meantime use Disable-PrtgProgress");

            if (PreviousRecord == null)
            {
                progressWriter.WriteProgress(progressRecord);

                if (!sourceIdUpdated)
                {
                    progressRecord.SourceId = GetLastSourceId(cmdlet.CommandRuntime);
                    sourceIdUpdated = true;
                }
            }
            else
            {
                var sourceId = PreviousRecord.SourceId;
                progressRecord.SourceId = sourceId;

                progressWriter.WriteProgress(sourceId, progressRecord);
            }
        }

        internal static long GetLastSourceId(ICommandRuntime commandRuntime)
        {
            return Convert.ToInt64(commandRuntime.GetType().GetField("_lastUsedSourceId", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null));
        }

        public void CompleteProgress()
        {
            if (PipeFromVariableWithProgress)
            {
                //If we're not part of a chain, the first in the chain, or the pipeline contains an operation
                if (!PartOfChain || FirstInChain || (PipelineContainsOperation && PreviousRecord == null))
                {
                    if (Pipeline.CurrentIndex < Pipeline.List.Count - 1)
                        return;
                }
                else
                {
                    //We're the Object or Action in Variable -> Object -> Action
                    if (!PipelineContainsOperation || cmdlet is PrtgOperationCmdlet)
                    {
                        var previousCmdlet = cmdlet.GetPreviousPrtgCmdlet();
                        var previousManager = previousCmdlet.ProgressManager;

                        if (previousManager.recordsProcessed < previousManager.TotalRecords) //new issue: get-sensor doesnt update records processed
                            return;
                    }
                    else
                    {
                        //We're the second object in Variable -> Object -> Action -> Object. We're responsible for our own record keeping
                        if (recordsProcessed < TotalRecords)
                            return;
                    }
                }
            }

            InitialDescription = null;
            recordsProcessed = -1;

            /*
             * If we've been displaying the number of records we need to process, we have a process record and need to complete it
             * If we're piping from a variable, we need to complete our progress - however if the pipeline before me is an operation,
             *     responsibility has shifted and we're not responsible for the current progress record
             */
            if ((TotalRecords > 0 || (PipeFromVariableWithProgress && !PipelineBeforeMeContainsOperation)))
            {
                /* However if it turns out we didn't actually write any records (such as because the server glitched out
                 * and returned nothing) we never really wrote our progress record. If we were streaming however,
                 * we're still displaying our "detecting total number of items" message, so we need to finally complete it.
                 */
                if (!ProgressWritten && Scenario == ProgressScenario.StreamProgress)
                    CurrentRecord.StatusDescription = "Temp";

                CurrentRecord.RecordType = ProgressRecordType.Completed;

                WriteProgress(CurrentRecord);
            }

            TotalRecords = null;

            CurrentRecord.Activity = DefaultActivity;
            CurrentRecord.StatusDescription = DefaultDescription;
            CurrentRecord.RecordType = ProgressRecordType.Processing;
        }

        public void UpdateRecordsProcessed(ProgressRecordEx record, bool writeObject = true)
        {
            //When a variable to cmdlet chain contains an operation, responsibility of updating the number of records processed
            //"resets", and we become responsible for updating our own count again
            if (PipeFromVariableWithProgress && !PipelineContainsOperation)
            {
                //If we're the only cmdlet, the first cmdlet, or the pipeline contains an operation cmdlet
                if (!PartOfChain || FirstInChain) //todo: will pipelinecontainsoperation break the other tests?
                {
                    var index = variableProgressDisplayed ? Pipeline.CurrentIndex + 2 : Pipeline.CurrentIndex + 1;

                    var originalIndex = index;
                    if (index > Pipeline.List.Count)
                        index = Pipeline.List.Count;
                    
                    record.StatusDescription = $"{InitialDescription} {index}/{Pipeline.List.Count}";

                    record.PercentComplete = (int) ((index)/Convert.ToDouble(Pipeline.List.Count)*100);

                    if(writeObject)
                        variableProgressDisplayed = true;

                    if (originalIndex <= Pipeline.List.Count)
                        WriteProgress(record);
                }
                else
                {
                    var previousCmdlet = cmdlet.GetPreviousPrtgCmdlet();
                    var previousManager = previousCmdlet.ProgressManager;

                    IncrementProgress(record, previousManager, writeObject);
                }
            }
            else
            {
                IncrementProgress(record, this, writeObject);
            }
        }

        private void IncrementProgress(ProgressRecordEx record, ProgressManager manager, bool writeObject)
        {
            if (manager.TotalRecords > 0)
            {
                if (manager.recordsProcessed < 0)
                    manager.recordsProcessed++;

                manager.recordsProcessed++;

                record.StatusDescription = $"{InitialDescription} {manager.recordsProcessed}/{manager.TotalRecords}";

                if (manager.recordsProcessed > 0)
                    record.PercentComplete = (int)(manager.recordsProcessed / Convert.ToDouble(manager.TotalRecords) * 100);

                if (!writeObject)
                    manager.recordsProcessed--;

                WriteProgress();
            }
        }

        public void SetPreviousOperation(string operation)
        {
            if (PreviousRecord.CurrentOperation != operation)
            {
                PreviousRecord.CurrentOperation = operation;

                WriteProgress(PreviousRecord);
            }
        }

        public void DisplayInitialProgress()
        {
            if (PipeFromVariableWithProgress && Pipeline.CurrentIndex > 0)
                return;

            CurrentRecord.StatusDescription = InitialDescription;

            WriteProgress();
        }

        public void ProcessOperationProgress(string activity, string progressMessage)
        {
            //If we already had an operation cmdlet, the responsibility of updating the previous cmdlet's
            //records processed has now shifted back on to him, so we don't need to do it for him. As such,
            //we may assume we're now a "normal" pipeline
            if (PipeFromVariableWithProgress && !PipelineBeforeMeContainsOperation)
            {
                //Variable -> Action
                //Variable -> Action -> Object
                //Variable -> Object -> Action
                //Variable -> Object -> Action -> Object
                ProcessOperationProgressForVariable(activity, progressMessage);
            }
            else
            {
                //Object -> Action
                //Object -> Action -> Object
                //Object -> Action -> Object -> Action
                ProcessOperationProgressForCmdlet(activity, progressMessage);
            }
        }

        private void ProcessOperationProgressForVariable(string activity, string progressMessage)
        {
            if (!PipelineIsProgressPure)
                return;

            RemovePreviousOperation();

            if (PreviousRecord == null)
            {
                //Variable -> Action
                //Variable -> Action -> Object
                ProcessOperationProgressStraightFromVariable(activity, progressMessage);
            }
            else
            {
                //Variable -> Object -> Action
                //Variable -> Object -> Action -> Object
                ProcessOperationProgressFromCmdletFromVariable(activity, progressMessage);
            }
        }

        private void ProcessOperationProgressStraightFromVariable(string activity, string progressMessage)
        {
            //1b: Variable -> Action
            //Variable -> Action -> Object
            //    We know the total number of records and where we're up to as we can read this information straight from the pipeline

            //Variable -> Action -> Object
            //    HOW DO WE TELL THE NEXT TABLE THE NUMBER OF ITEMS WE'VE PROCESSED????

            TotalRecords = Pipeline.List.Count;
            var count = Pipeline.CurrentIndex + 1;

            CurrentRecord.Activity = activity;
            CurrentRecord.PercentComplete = (int)((count) / Convert.ToDouble(TotalRecords) * 100);
            CurrentRecord.StatusDescription = $"{progressMessage} ({count}/{TotalRecords})";

            WriteProgress();
        }

        private void ProcessOperationProgressFromCmdletFromVariable(string activity, string progressMessage)
        {
            //5b: Variable -> Object -> Action
            //Variable -> Object -> Action -> Object
            //    We know the total number of records and where we're up to as we can read this information from the previous cmdlet's ProgressManager

            //Variable -> Action -> Object
            //    HOW DO WE TELL THE NEXT TABLE THE NUMBER OF ITEMS WE'VE PROCESSED????

            var previousCmdlet = cmdlet.GetPreviousPrtgCmdlet();
            var previousManager = previousCmdlet.ProgressManager;
            TotalRecords = previousManager.TotalRecords;
            
            //Normally the object cmdlet would be responsible for updating the number of records we've processed so far,
            //but for REASONS UNKNOWN (TODO: WHY) thats not the case, so we have to do it instead
            if (previousManager.recordsProcessed < 0)
                previousManager.recordsProcessed++;

            previousManager.recordsProcessed++;

            var count = previousManager.recordsProcessed;

            CurrentRecord.Activity = activity;
            CurrentRecord.PercentComplete = (int)((count) / Convert.ToDouble(TotalRecords) * 100);
            CurrentRecord.StatusDescription = $"{progressMessage} ({count}/{TotalRecords})";

            WriteProgress();
        }

        private void ProcessOperationProgressForCmdlet(string activity, string progressMessage)
        {
            if (PreviousContainsProgress)
            {
                //Overwrite the previous cmdlet's ProgressRecord with our operation's activity and status description.

                PreviousRecord.Activity = activity;

                var previousCmdlet = cmdlet.TryGetPreviousPrtgCmdletOfNotType<PrtgOperationCmdlet>();

                var previousManager = previousCmdlet.ProgressManager;

                PreviousRecord.StatusDescription = $"{progressMessage} ({previousManager.recordsProcessed}/{previousManager.TotalRecords})";

                WriteProgress(PreviousRecord);

                SkipCurrentRecord();
            }
        }

        private void SkipCurrentRecord()
        {
            CloneRecord(PreviousRecord, CurrentRecord);
        }

        public static ProgressRecordEx CloneRecord(ProgressRecordEx progressRecord)
        {
            var record = new ProgressRecordEx(progressRecord.ActivityId, progressRecord.Activity, progressRecord.StatusDescription, progressRecord.SourceId)
            {
                CurrentOperation = progressRecord.CurrentOperation,
                ParentActivityId = progressRecord.ParentActivityId,
                PercentComplete = progressRecord.PercentComplete,
                RecordType = progressRecord.RecordType,
                SecondsRemaining = progressRecord.SecondsRemaining
            };

            return record;
        }

        public static void CloneRecord(ProgressRecordEx sourceRecord, ProgressRecordEx destinationRecord)
        {
            destinationRecord.GetType().BaseType.GetField("id", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(destinationRecord, sourceRecord.ActivityId);
            destinationRecord.Activity = sourceRecord.Activity;
            destinationRecord.StatusDescription = sourceRecord.StatusDescription;
            destinationRecord.CurrentOperation = sourceRecord.CurrentOperation;
            destinationRecord.ParentActivityId = sourceRecord.ParentActivityId;
            destinationRecord.PercentComplete = sourceRecord.PercentComplete;
            destinationRecord.RecordType = sourceRecord.RecordType;
            destinationRecord.SecondsRemaining = sourceRecord.SecondsRemaining;
            destinationRecord.SourceId = sourceRecord.SourceId;
        }
    }
}
