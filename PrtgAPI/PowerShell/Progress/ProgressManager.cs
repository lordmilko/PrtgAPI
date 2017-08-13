using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using PrtgAPI.Helpers;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Progress
{
    internal class ProgressManager : IDisposable
    {
        private static Stack<ProgressRecord> progressRecords = new Stack<ProgressRecord>();

        internal const string DefaultActivity = "Activity";
        internal const string DefaultDescription = "Description";

        public ProgressRecord CurrentRecord => progressRecords.Peek();

        public bool ContainsProgress => CurrentRecord.Activity != DefaultActivity && (CurrentRecord.StatusDescription != DefaultDescription || InitialDescription != string.Empty);
        
        public bool PreviousContainsProgress => PreviousRecord != null && PreviousRecord.Activity != DefaultActivity && PreviousRecord.StatusDescription != DefaultDescription;

        /// <summary>
        /// Indicates whether the current cmdlet is first in a chain of pure PrtgAPI cmdlets (with no third party filters in between, etc)
        /// </summary>
        public bool FirstInChain => pipeToPrtgCmdlet && progressRecords.Count == 1;

        /// <summary>
        /// Indicates whether the current cmdlet is part of a chain of pure PrtgAPI cmdlets (with no third party filters in between, etc)
        /// </summary>
        public bool PartOfChain => pipeToPrtgCmdlet || progressRecords.Count > 1;

        private bool pipeToPrtgCmdlet => cmdlet.MyInvocation.MyCommand.ModuleName == cmdlet.CommandRuntime.GetDownstreamCmdlet()?.ModuleName;

        public bool LastInChain => !pipeToPrtgCmdlet;

        public string InitialDescription { get; set; }

        public int? TotalRecords { get; set; }

        public Pipeline Pipeline { get; set; }

        public Pipeline CmdletPipeline { get; set; }

        //Display progress when piping multiple values from a variable, or a single value to multiple cmdlets
        public bool PipeFromVariableWithProgress => Pipeline?.List.Count > 1 || (Pipeline?.List.Count == 1 && PartOfChain);

        public Cmdlet PreviousCmdlet { get; set; }

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

        public ProgressRecord PreviousRecord => progressRecords.Skip(1).FirstOrDefault();

        private PSCmdlet cmdlet;

        private int recordsProcessed = -1;

        private bool variableProgressDisplayed;

        private IProgressWriter progressWriter;

        internal static IProgressWriter CustomWriter { get; set; }

        internal ProgressScenario Scenario { get; set; }

        private bool? pipelineIsPure;

        /// <summary>
        /// Indicates whether the pipeline has been contamined by non-PrtgAPI cmdlets.
        /// </summary>
        public bool PipelineIsPure
        {
            get
            {
                if (pipelineIsPure == null)
                    pipelineIsPure = cmdlet.PipelineIsPure();

                return pipelineIsPure.Value;
            }
        }

        public ProgressManager(PSCmdlet cmdlet)
        {
            progressRecords.Push(new ProgressRecord(progressRecords.Count + 1, DefaultActivity, DefaultDescription));

            if (PreviousRecord != null)
                CurrentRecord.ParentActivityId = PreviousRecord.ActivityId;

            this.cmdlet = cmdlet;
            Pipeline = cmdlet.CommandRuntime.GetPipelineInput();
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
                progressRecords.Pop();
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

        private void WriteProgress(ProgressRecord progressRecord)
        {
            if (progressRecord.Activity == DefaultActivity || progressRecord.StatusDescription == DefaultDescription)
                throw new InvalidOperationException("Attempted to write progress on an uninitialized ProgressRecord. If this is a Release build, please report this bug along with the cmdlet chain you tried to execute. To disable PrtgAPI Cmdlet Progress in the meantime use Disable-PrtgProgress");

            if (PreviousRecord == null)
                progressWriter.WriteProgress(progressRecord);
            else
            {
                var sourceId = GetLastSourceId(cmdlet.CommandRuntime);

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
                if (!PartOfChain || FirstInChain || PipelineContainsOperation)
                {
                    if (CmdletPipeline.CurrentIndex < CmdletPipeline.List.Count - 1)
                        return;
                }
                else
                {
                    var previousCmdlet = cmdlet.GetPreviousPrtgCmdlet();
                    var previousManager = previousCmdlet.ProgressManager;

                    if (previousManager.recordsProcessed < previousManager.TotalRecords)
                        return;                    
                }
            }

            InitialDescription = null;
            recordsProcessed = -1;

            if (TotalRecords > 0 || PipeFromVariableWithProgress)
            {
                CurrentRecord.RecordType = ProgressRecordType.Completed;

                WriteProgress(CurrentRecord);
            }

            TotalRecords = null;

            CurrentRecord.Activity = DefaultActivity;
            CurrentRecord.StatusDescription = DefaultDescription;
            CurrentRecord.RecordType = ProgressRecordType.Processing;
        }

        public void UpdateRecordsProcessed(ProgressRecord record)
        {
            if (PipeFromVariableWithProgress)
            {
                //If we're the only cmdlet, the first cmdlet, or the pipeline contains an operation cmdlet
                if (!PartOfChain || FirstInChain || PipelineContainsOperation) //todo: will pipelinecontainsoperation break the other tests?
                {
                    var index = variableProgressDisplayed ? CmdletPipeline.CurrentIndex + 2 : CmdletPipeline.CurrentIndex + 1;

                    var originalIndex = index;

                    if (index > CmdletPipeline.List.Count)
                        index = CmdletPipeline.List.Count;

                    record.StatusDescription = $"{InitialDescription} {index}/{CmdletPipeline.List.Count}";

                    record.PercentComplete = (int) ((index)/Convert.ToDouble(CmdletPipeline.List.Count)*100);

                    variableProgressDisplayed = true;

                    if (originalIndex <= CmdletPipeline.List.Count)
                        WriteProgress(record);
                }
                else
                {
                    var previousCmdlet = cmdlet.GetPreviousPrtgCmdlet();
                    var previousManager = previousCmdlet.ProgressManager;
                    var totalRecords = previousManager.TotalRecords;

                    if (totalRecords > 0)
                    {
                        if (previousManager.recordsProcessed < 0)
                            previousManager.recordsProcessed++;

                        previousManager.recordsProcessed++;

                        record.StatusDescription = $"{InitialDescription} {previousManager.recordsProcessed}/{totalRecords}";

                        if (previousManager.recordsProcessed > 0)
                            record.PercentComplete = (int)(previousManager.recordsProcessed / Convert.ToDouble(totalRecords) * 100);

                        WriteProgress();
                    }
                }
            }
            else
            {
                if (TotalRecords > 0)
                {
                    if (recordsProcessed < 0)
                        recordsProcessed++;

                    recordsProcessed++;

                    record.StatusDescription = $"{InitialDescription} {recordsProcessed}/{TotalRecords}";

                    if (recordsProcessed > 0)
                        record.PercentComplete = (int)(recordsProcessed / Convert.ToDouble(TotalRecords) * 100);

                    WriteProgress();
                }
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
            if (PipeFromVariableWithProgress && CmdletPipeline.CurrentIndex > 0)
                return;

            CurrentRecord.StatusDescription = InitialDescription;

            WriteProgress();
        }

        public void TryOverwritePreviousOperation(string activity, string progressMessage)
        {
            //i think we need to revert the way we're doing this. say processing sensors, then if we detect we're going to pipe to someone display some progress that counts through our items
            //i think we already use this logic for "normal" pipes, right?
            //yep - so we need to be able to detect this scenario is in place doing an action cmdlet effectively resets things to be normal methoda again
            //NOPE!!! when doing pipe from variable we DONT want to be overwriting - this overwrites the info about what number group we're processing
            //so we need to make it have its own progress record
            //more specifically, $devices|clone-device works, but $groups|get-device|clone-device doesnt work
            //and then regardless, theres still the issue of how to handle piping from clone-device to 1 or 2 more cmdlets
            //need to add tests for ALL of this

            if (PipeFromVariableWithProgress)
            {
                if (!PipelineIsPure)
                    return;

                RemovePreviousOperation();

                var index = variableProgressDisplayed ? CmdletPipeline.CurrentIndex + 2 : CmdletPipeline.CurrentIndex + 1;

                if (index > CmdletPipeline.List.Count)
                    index = CmdletPipeline.List.Count;

                CurrentRecord.Activity = activity;

                if (PreviousRecord == null)
                    TotalRecords = CmdletPipeline.List.Count;
                else
                {
                    var previousCmdlet = cmdlet.GetPreviousPrtgCmdlet();
                    var previousManager = previousCmdlet.ProgressManager;
                    TotalRecords = previousManager.TotalRecords;
                }

                CurrentRecord.PercentComplete = (int) ((index)/Convert.ToDouble(TotalRecords)*100);
                CurrentRecord.StatusDescription = $"{progressMessage} ({index}/{TotalRecords})";

                variableProgressDisplayed = true;

                WriteProgress();
            }
            else
            {
                if (PreviousContainsProgress)
                {
                    //if we're piping from a variable, that means the current count is actually a count of the variable; therefore, we need to inspect the pipeline to get the number of triggers incoming?              

                    PreviousRecord.Activity = activity;

                    var count = PreviousRecord.StatusDescription.Substring(PreviousRecord.StatusDescription.LastIndexOf(" ") + 1);

                    PreviousRecord.StatusDescription = $"{progressMessage} ({count.Trim('(').Trim(')')})";

                    WriteProgress(PreviousRecord);

                    SkipCurrentRecord();
                }
            }
        }

        private void SkipCurrentRecord()
        {
            CloneRecord(PreviousRecord, CurrentRecord);
        }

        public static ProgressRecord CloneRecord(ProgressRecord progressRecord)
        {
            var record = new ProgressRecord(progressRecord.ActivityId, progressRecord.Activity, progressRecord.StatusDescription)
            {
                CurrentOperation = progressRecord.CurrentOperation,
                ParentActivityId = progressRecord.ParentActivityId,
                PercentComplete = progressRecord.PercentComplete,
                RecordType = progressRecord.RecordType,
                SecondsRemaining = progressRecord.SecondsRemaining
            };

            return record;
        }

        public static void CloneRecord(ProgressRecord sourceRecord, ProgressRecord destinationRecord)
        {
            destinationRecord.GetType().GetField("id", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(destinationRecord, sourceRecord.ActivityId);
            destinationRecord.Activity = sourceRecord.Activity;
            destinationRecord.StatusDescription = sourceRecord.StatusDescription;
            destinationRecord.CurrentOperation = sourceRecord.CurrentOperation;
            destinationRecord.ParentActivityId = sourceRecord.ParentActivityId;
            destinationRecord.PercentComplete = sourceRecord.PercentComplete;
            destinationRecord.RecordType = sourceRecord.RecordType;
            destinationRecord.SecondsRemaining = sourceRecord.SecondsRemaining;
        }
    }
}
