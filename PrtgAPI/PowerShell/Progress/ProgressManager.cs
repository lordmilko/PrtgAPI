using System;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using Microsoft.PowerShell.Commands;
using PrtgAPI.Objects.Shared;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Progress
{
    internal partial class ProgressManager : IDisposable
    {
        internal const string DefaultActivity = "Activity";
        internal const string DefaultDescription = "Description";

        #region Progress Pipeline State

        internal static ProgressPipelineStack progressPipelines = new ProgressPipelineStack();

        internal int ProgressPipelinesCount => progressPipelines.Count;

        #endregion
        #region Progress Record State

        public ProgressRecordEx CurrentRecord => progressPipelines.CurrentRecordInPipeline;

        public ProgressRecordEx PreviousRecord => progressPipelines.PreviousRecordInPipeline;

        /// <summary>
        /// Indicates that the current cmdlet expects that it will contain progress, either because it already does, or it will when a record is written to the pipeline.
        /// </summary>
        public bool ExpectsContainsProgress => CurrentRecord.Activity != DefaultActivity && (CurrentRecord.StatusDescription != DefaultDescription || InitialDescription != string.Empty);

        /// <summary>
        /// Indicates that a record has actually been written to the pipeline for this cmdlet, thus initializing the status description.
        /// </summary>
        public bool ProgressWritten => CurrentRecord.StatusDescription != DefaultDescription;

        public bool PreviousContainsProgress => PreviousRecord != null && PreviousRecord.Activity != DefaultActivity && PreviousRecord.StatusDescription != DefaultDescription;

        #endregion
        #region Pipeline Chain Analysis

        /// <summary>
        /// Indicates whether the current cmdlet is first in a chain of pure PrtgAPI cmdlets (with no third party filters in between, etc)
        /// </summary>
        public bool FirstInChain => pipeToProgressCompatibleCmdlet && progressPipelines.RecordsInCurrentPipeline == 1;

        private bool? partOfChain;

        /// <summary>
        /// Indicates whether the current cmdlet is part of a chain of PrtgAPI cmdlets (with no unsupported third party filters in between, etc)
        /// </summary>
        public bool PartOfChain
        {
            get
            {
                if (partOfChain == null)
                {
                    if (pipeFromProgressCompatibleCmdlet && (pipeToProgressCompatibleCmdlet || progressPipelines.RecordsInCurrentPipeline > 1))
                        partOfChain = true;
                    else
                        partOfChain = false;
                }

                return partOfChain.Value;
            }
        }

        public bool LastInChain => !pipeToProgressCompatibleCmdlet;

        //Display progress when piping multiple values from a variable, or a single value to multiple cmdlets
        public bool PipeFromVariableWithProgress => EntirePipeline?.List.Count > 1 || (EntirePipeline?.List.Count == 1 && PartOfChain);

        public bool GetRecordsWithVariableProgress => (PipeFromVariableWithProgress || CanUseSelectObjectProgress || PipelineUpstreamContainsBlockingCmdlet) && !UnsupportedSelectObjectProgress && PrtgSessionState.EnableProgress;

        public bool GetResultsWithProgress => PartOfChain && !UnsupportedSelectObjectProgress && PrtgSessionState.EnableProgress;

        #endregion
        #region Extended Pipeline Chain Analysis
        #region Pipe To/From Progress Compatible Cmdlet

        private bool? pipeFromProgressCompatibleCmdletInternal;

        private bool pipeFromProgressCompatibleCmdlet
        {
            get
            {
                if (pipeFromProgressCompatibleCmdletInternal == null)
                {
                    var upstreamCmdletInfo = CacheManager.GetUpstreamCmdletInfo();

                    if (upstreamCmdletInfo == null)
                    {
                        pipeFromProgressCompatibleCmdletInternal = true;
                    }
                    else
                    {
                        if (cmdlet.MyInvocation.MyCommand.ModuleName == upstreamCmdletInfo.ModuleName)
                        {
                            pipeFromProgressCompatibleCmdletInternal = true;
                        }
                        else
                        {
                            var upstreamCmdlet = CacheManager.GetUpstreamCmdlet();

                            pipeFromProgressCompatibleCmdletInternal = IsPureThirdPartyCmdlet(upstreamCmdlet?.GetType()) && CacheManager.PipelineIsProgressPureFromLastPrtgCmdlet();
                        }
                    }
                }

                return pipeFromProgressCompatibleCmdletInternal.Value;
            }
        }


        private bool? pipeToProgressCompatibleCmdletInternal;

        /// <summary>
        /// Indicates whether the next cmdlet in the pipeline is compatible with PrtgAPI Progress.
        /// </summary>
        private bool pipeToProgressCompatibleCmdlet
        {
            get
            {
                if (pipeToProgressCompatibleCmdletInternal == null)
                {
                    var downstreamCmdletInfo = CacheManager.GetDownstreamCmdletInfo();

                    if (cmdlet.MyInvocation.MyCommand.ModuleName == downstreamCmdletInfo?.ModuleName)
                    {
                        pipeToProgressCompatibleCmdletInternal = true;
                    }
                    else
                    {
                        var downstreamCmdlet = CacheManager.GetDownstreamCmdlet();

                        pipeToProgressCompatibleCmdletInternal = IsPureThirdPartyCmdlet(downstreamCmdlet?.GetType()) && CacheManager.PipelineIsProgressPureToNextPrtgCmdlet();
                    }
                }

                return pipeToProgressCompatibleCmdletInternal.Value;
            }
        }

        #endregion
        #region Pipeline Contains Operation

        private bool? pipelineContainsOperation;

        public bool PipelineContainsOperation
        {
            get
            {
                if (pipelineContainsOperation == null)
                    pipelineContainsOperation = CacheManager.PipelineSoFarHasCmdlet<PrtgOperationCmdlet>();

                return pipelineContainsOperation.Value;
            }
        }

        private bool? pipelineBeforeMeContainsOperation;

        public bool PipelineBeforeMeContainsOperation
        {
            get
            {
                if (pipelineBeforeMeContainsOperation == null)
                    pipelineBeforeMeContainsOperation = CacheManager.PipelineBeforeMeHasCmdlet<PrtgOperationCmdlet>();

                return pipelineBeforeMeContainsOperation.Value;
            }
        }

        public bool NextCmdletIsOperation => CacheManager.GetNextPrtgCmdlet() is PrtgOperationCmdlet;

        public bool NextCmdletIsMultiOperationBatchMode => MultiOperationBatchMode(true);

        public bool MultiOperationBatchMode(bool downstream = false)
        {
            object c = cmdlet;

            if (downstream)
            {
                c = null;
                var commands = CacheManager.GetPipelineCommands();

                var myIndex = commands.IndexOf(cmdlet);

                for (int i = myIndex + 1; i < commands.Count; i++)
                {
                    if (commands[i] is PrtgMultiOperationCmdlet)
                    {
                        c = commands[i];
                        break;
                    }

                    if (commands[i] is PrtgOperationCmdlet)
                        continue;

                    c = commands[i];
                    break;
                }
            }

            var typedCmdlet = c as PrtgMultiOperationCmdlet;

            if (typedCmdlet == null)
                return false;

            if (typedCmdlet.Batch)
                return true;

            return false;
        }

        #endregion
        #region Pipeline Is Pure

        private bool? pipelineIsPure;

        /// <summary>
        /// Indicates whether the pipeline has been contamined by non-PrtgAPI cmdlets or cmdlets that don't support PrtgAPI progress.
        /// </summary>
        public bool PipelineIsProgressPure
        {
            get
            {
                if (pipelineIsPure == null)
                    pipelineIsPure = CacheManager.PipelineIsProgressPure();

                return pipelineIsPure.Value;
            }
        }

        #endregion
        #region Pipe To/From Select Object Cmdlet

        internal readonly SelectObjectManager upstreamSelectObjectManager;
        internal readonly SelectObjectManager downstreamSelectObjectManager;

        internal SelectObjectManager operationUpstreamSelectObjectManager
        {
            get
            {
                if (upstreamSelectObjectManager == null && cmdlet is PrtgOperationCmdlet)
                {
                    var firstOperation = CacheManager.TryGetFirstOperationCmdletAfterSelectObject();

                    return firstOperation?.ProgressManager.upstreamSelectObjectManager;
                }

                return upstreamSelectObjectManager;
            }
        }

        public bool PipeFromBlockingSelectObjectCmdlet
        {
            get
            {
                if(upstreamSelectObjectManager != null)
                    return IsBlockingSelectObjectCmdlet(upstreamSelectObjectManager);

                var firstOperation = CacheManager.TryGetFirstOperationCmdletAfterSelectObject();

                if (firstOperation != null)
                    return IsBlockingSelectObjectCmdlet(firstOperation.ProgressManager?.upstreamSelectObjectManager);

                return false;
            }
        }

        public bool CanUseSelectObjectProgress => PipeFromBlockingSelectObjectCmdlet && SourceBeforeSelectObjectUnusable;

        public bool SourceBeforeSelectObjectUnusable
        {
            get
            {
                if (upstreamSelectObjectManager != null)
                {
                    if (Scenario == ProgressScenario.SelectLast)
                    {
                        if (PipeFromVariableWithProgress)
                        {
                            //If the previous Select-Object is the first in the pipeline
                            if (CacheManager.GetFirstCmdletInPipeline() == CacheManager.GetUpstreamCmdlet())
                            {
                                if (upstreamSelectObjectManager.HasFirst && EntirePipeline.CurrentIndex + 1 <= upstreamSelectObjectManager.First)
                                    return false;
                            }
                        }
                        else
                            return progressPipelines.RecordsInCurrentPipeline == 1;
                    }

                    return true;
                }

                return false;
            }
        }

        private bool PipeToBlockingSelectObjectCmdlet => downstreamSelectObjectManager != null && CacheManager.PipelineContainsBlockingCmdletToNextPrtgCmdletOrEnd(); //todo: should we make the up and downstream
        //caches include deets from all intermediate select-object's somehow?

        private bool IsBlockingSelectObjectCmdlet(SelectObjectManager manager)
        {
            if (manager != null)
            {
                if (manager.HasLast || manager.HasSkipLast)
                    return true;
            }

            return false;
        }

        private bool? unsupportedSelectObjectProgress;

        public bool UnsupportedSelectObjectProgress
        {
            get
            {
                if (unsupportedSelectObjectProgress == null)
                {
                    unsupportedSelectObjectProgress = CalculateIsUnsupportedSelectObjectProgress();
                }

                return unsupportedSelectObjectProgress.Value;
            }
        }

        private bool CalculateIsUnsupportedSelectObjectProgress()
        {
            //Something -> Select -> Select -> Select
            if (PipeToOrFromExcessiveSelectObject())
                return true;

            //Something -> Select -Skip -> Select -Skip -> Something
            if (HasExcessiveSelectItems(c => c.HasSkip))
                return true;

            //Something -> Select -Last     -> Select -Something -> Something
            //Something -> Select -SkipLast -> Select -Something -> Something
            //Something -> Select -Index    -> Select -Something -> Something
            if (PipeToOrFromMultipleSelectObjectStartingWithSomething(c => c?.HasLast == true || c?.HasSkipLast == true || c?.HasIndex == true))
                return true;


            //More than two instances any combination of -First, -Last, -Skip, -SkipLast or -Index have been specified
            if (PipeToOrFromExcessiveSelectObjectParameters)
                return true;

            //The result of the previous cmdlet's calculation
            if (PreviousCmdletDetectedUnsupportedProgress())
                return true;

            //Something -> Select -> Action -> Action
            if (PipeToOrFromIllegalMultipleOperationFromSelectObject)
            {
                //If only -First is specified, its OK
                return true;
            }
                
            return false;
        }
        private bool PreviousCmdletDetectedUnsupportedProgress()
        {
            var commands = CacheManager.GetPipelineCommands();

            var myIndex = commands.IndexOf(cmdlet);

            for (int i = myIndex - 1; i >= 0; i--)
            {
                if (commands[i] is PrtgCmdlet)
                {
                    var prtgCmdlet = commands[i] as PrtgCmdlet;

                    if (prtgCmdlet.ProgressManager?.UnsupportedSelectObjectProgress == true)
                        return true;
                }
            }

            return false;
        }

        private bool PipeToOrFromIllegalMultipleOperationFromSelectObject
        {
            get
            {
                var commands = CacheManager.GetPipelineCommands();

                for (int i = 0; i < commands.Count - 2; i++)
                {
                    if (commands[i] is SelectObjectCommand && commands[i + 1] is PrtgOperationCmdlet && commands[i + 2] is PrtgOperationCmdlet)
                    {
                        var descriptor = new SelectObjectDescriptor((SelectObjectCommand) commands[i]);

                        if (descriptor.HasIndex || descriptor.HasLast || descriptor.HasSkip || descriptor.HasSkipLast)
                            return true;

                        return false;
                    }
                }

                return false;
            }
        }

        private bool PipeToOrFromExcessiveSelectObject()
        {
            var commands = CacheManager.GetPipelineCommands();

            var myIndex = commands.IndexOf(cmdlet);

            var selectInARow = 0;

            for (int i = myIndex + 1; i < commands.Count && i <= myIndex + 3; i++)
            {
                if (commands[i] is SelectObjectCommand)
                    selectInARow++;
                else
                    selectInARow = 0;

                if (selectInARow == 3)
                    return true;
            }

            selectInARow = 0;

            for (int i = myIndex - 1; i >= 0 && i >= myIndex - 3; i--)
            {
                if (commands[i] is SelectObjectCommand)
                    selectInARow++;
                else
                    selectInARow = 0;

                if (selectInARow == 3)
                    return true;
            }

            return false;
        }

        private bool IsParameterUsedWithAnythingElse(Func<SelectObjectDescriptor, bool> predicate)
        {
            if (HasExcessiveSelectItems(predicate, 1))
            {
                if (HasExcessiveParameters(upstreamSelectObjectManager, 1) || HasExcessiveParameters(downstreamSelectObjectManager, 1))
                    return true;
            }

            return false;
        }

        private bool PipeToOrFromMultipleSelectObjectStartingWithSomething(Func<SelectObjectDescriptor, bool> property)
        {
            if (upstreamSelectObjectManager?.Commands.Count >= 2 && property(upstreamSelectObjectManager?.Commands.Last()))
                return true;

            if (downstreamSelectObjectManager?.Commands.Count >= 2 && property(downstreamSelectObjectManager?.Commands.First()))
                return true;

            return false;
        }

        private bool PipeToOrFromExcessiveSelectObjectParameters
        {
            get
            {
                if (HasExcessiveParameters(upstreamSelectObjectManager) || HasExcessiveParameters(downstreamSelectObjectManager))
                    return true;

                return false;
            }
        }

        private bool HasExcessiveSelectItems(Func<SelectObjectDescriptor, bool> predicate, int count = 2)
        {
            if (upstreamSelectObjectManager?.Commands.Count(predicate) >= count || downstreamSelectObjectManager?.Commands.Count(predicate) >= count)
                return true;

            return false;
        }

        private bool HasExcessiveParameters(SelectObjectManager manager, int count = 2)
        {
            if (manager == null)
                return false;

            var keys = new[] {"First", "Last", "Skip", "SkipLast", "Index"};

            var parameters = manager.Commands.SelectMany(c => c.Command.MyInvocation.BoundParameters).Count(p => keys.Contains(p.Key));

            return parameters > count;
        }

        private bool? pipelineUpstreamContainsBlockingCmdlet;

        public bool PipelineUpstreamContainsBlockingCmdlet
        {
            get
            {
                if (pipelineUpstreamContainsBlockingCmdlet == null)
                {
                    if (Scenario == ProgressScenario.MultipleCmdlets)
                    {
                        var commands = CacheManager.GetPipelineCommands();

                        var myIndex = commands.IndexOf(cmdlet);

                        for (int i = myIndex; i >= 0; i--)
                        {
                            if (commands[i] is PrtgCmdlet)
                            {
                                var manager = ((PrtgCmdlet)commands[i]).ProgressManager;

                                if (manager?.Scenario == ProgressScenario.SelectLast || manager?.Scenario == ProgressScenario.SelectSkipLast)
                                {
                                    pipelineUpstreamContainsBlockingCmdlet = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (pipelineUpstreamContainsBlockingCmdlet == null)
                        pipelineUpstreamContainsBlockingCmdlet = false;
                }

                return pipelineUpstreamContainsBlockingCmdlet.Value;
            }
        }

        internal NotReadyParser notReady;
        private MultiOperationRecordAnalyzer multiOperationAnalyzer;

        private AbortProgressParser abortProgress = new AbortProgressParser();

        #endregion
        #endregion
        #region Pipeline State

        /// <summary>
        /// The object collection being piped into this cmdlet.<para/>
        /// If Variable -> Where -> PrtgCmdlet, the EntirePipeline will be used (allowing us to bypass the where-object and retrieve the original array)<para/>
        /// If PrtgCmdlet -> Where -> PrtgCmdlet, for the first PrtgCmdlet the CmdletPipeline and EntirePipeline will be the same. For the second PrtgCmdlet, the CmdletPipeline will be used
        /// </summary>
        public Pipeline Pipeline
        {
            get
            {
                //if we're select -first -last piping from a variable, the object queue is empty.
                //if we're after the first select-object in the pipeline, we can look at the length
                //of the entirepipeline?

                //i think, rather, we need an equivalent of "cmdletbeforeselectobjectunusable" for variable piping.
                //is the VARIABLE itself unusable?

                //we could rename cmdletbeforeselectobjectunusuable to "sourcebeforeselectobjectunusable"
                //and say the source is also unusable if the upstreamcmdlet is also the first cmdlet in the pipeline?
                //that wont work cos that statement will always be true. maybe if we said if last AND first are specified,
                //and we're piping from a variable and we havent processed -first records yet?

                if (PipeFromBlockingSelectObjectCmdlet && SourceBeforeSelectObjectUnusable)
                {
                    if (SelectPipeline == null)
                    {
                        InitializeSelectPipeline();
                    }

                    return SelectPipeline;
                }
                else
                {
                    if (progressPipelines.RecordsInCurrentPipeline == 1)
                    {
                        //If we're actually preceeded by a Select-Object cmdlet, we actually need a SelectPipeline

                        if (upstreamSelectObjectManager != null && upstreamSelectObjectManager.HasFirst)
                        {
                            if (SelectPipeline == null)
                                SelectPipeline = new Pipeline(CmdletPipeline.Current, EntirePipeline.List);

                            return SelectPipeline;
                        }

                        return EntirePipeline;
                    }

                    return CmdletPipeline;
                }
            }
        }

        /// <summary>
        /// The object collection that was piped into all subsequent statements at the start of the entire pipeline.
        /// </summary>
        public Pipeline EntirePipeline { get; set; }

        /// <summary>
        /// The object collection that was piped into this cmdlet from the previous statement.
        /// </summary>
        public Pipeline CmdletPipeline { get; set; }

        public Pipeline SelectPipeline { get; set; }

        #endregion
        #region Regular Fields

        public string InitialDescription { get; set; }

        public int? TotalRecords { get; set; }

        internal PrtgCmdlet cmdlet;

        internal int recordsProcessed = -1;

        private bool variableProgressDisplayed;

        private IProgressWriter progressWriter;

        internal static Func<PrtgCmdlet, IProgressWriter> CustomWriter { get; set; }

        internal ProgressScenario Scenario { get; set; }

        private bool sourceIdUpdated;

        internal ReflectionCacheManager CacheManager { get; set; }

        #endregion

        public ProgressManager(PrtgCmdlet cmdlet)
        {
            this.cmdlet = cmdlet;
            CacheManager = new ReflectionCacheManager(cmdlet);

            var sourceId = GetLastSourceId(cmdlet.CommandRuntime);
            progressPipelines.Push(DefaultActivity, DefaultDescription, this, sourceId);

            if (PreviousRecord != null)
                CurrentRecord.ParentActivityId = PreviousRecord.ActivityId;

            EntirePipeline = CacheManager.GetPipelineInput();
            CmdletPipeline = CacheManager.GetCmdletPipelineInput();

            progressWriter = GetWriter();

            if (CacheManager.GetUpstreamCmdlet() is SelectObjectCommand)
            {
                upstreamSelectObjectManager = new SelectObjectManager(CacheManager, cmdlet, Direction.Upstream);

                if (upstreamSelectObjectManager.Commands.Count >= 3)
                    upstreamSelectObjectManager = null;
            }

            if (CacheManager.GetDownstreamCmdlet() is SelectObjectCommand)
            {
                downstreamSelectObjectManager = new SelectObjectManager(CacheManager, cmdlet, Direction.Downstream);

                if (downstreamSelectObjectManager.Commands.Count >= 3)
                    downstreamSelectObjectManager = null;
            }

            notReady = new NotReadyParser(this);
            multiOperationAnalyzer = new MultiOperationRecordAnalyzer(this);

            CalculateProgressScenario();
        }

        private void CalculateProgressScenario()
        {
            if (PipeFromBlockingSelectObjectCmdlet)
            {
                if (operationUpstreamSelectObjectManager.HasLast)
                    Scenario = ProgressScenario.SelectLast;
                else if (operationUpstreamSelectObjectManager.HasSkipLast)
                    Scenario = ProgressScenario.SelectSkipLast;
                else
                    throw new NotImplementedException("Don't know what parameter Select-Object is blocking with");
            }
            else
            {
                Scenario = CalculateNonBlockingProgressScenario();
            }

            if (PipelineUpstreamContainsBlockingCmdlet)
                Scenario = ProgressScenario.MultipleCmdletsFromBlockingSelect;
        }

        public ProgressScenario CalculateNonBlockingProgressScenario()
        {
            if (PartOfChain)
            {
                if (PipeFromVariableWithProgress)
                    return ProgressScenario.VariableToMultipleCmdlets;

                return ProgressScenario.MultipleCmdlets;
            }

            if (PipeFromVariableWithProgress)
                return ProgressScenario.VariableToSingleCmdlet;

            return ProgressScenario.NoProgress;
        }

        private IProgressWriter GetWriter()
        {
            if (CustomWriter != null)
                return CustomWriter(cmdlet);
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

        private void WriteProgress(ProgressRecordEx progressRecord, bool cache = false)
        {
            if (progressRecord.Activity == DefaultActivity || progressRecord.StatusDescription == DefaultDescription)
                throw new InvalidOperationException("Attempted to write progress on an uninitialized ProgressRecord. If this is a Release build, please report this bug along with the cmdlet chain you tried to execute. To disable PrtgAPI Cmdlet Progress in the meantime use Disable-PrtgProgress");

            if (PreviousRecord == null)
            {
                if (cache)
                    progressWriter.WriteProgress(progressRecord.SourceId, progressRecord);
                else
                {
                    progressWriter.WriteProgress(progressRecord);

                    if (!sourceIdUpdated)
                    {
                        progressRecord.SourceId = GetLastSourceId(cmdlet.CommandRuntime);
                        sourceIdUpdated = true;
                    }
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

        public void CompleteProgress(bool force = false, bool forceReadyOrNot = false)
        {
            CompleteProgress(CurrentRecord, force, forceReadyOrNot);
        }

        private void CompleteProgress(ProgressRecordEx record, bool force = false, bool forceReadyOrNot = false)
        {
            if (!forceReadyOrNot && !ReadyToComplete())
                return;

            InitialDescription = null;
            recordsProcessed = -1;

            /*
             * If we've been displaying the number of records we need to process, we have a process record and need to complete it
             * If we're piping from a variable, we need to complete our progress - however if the pipeline before me is an operation,
             *     responsibility has shifted and we're not responsible for the current progress record
             */
            if (TotalRecords > 0 || ((PipeFromVariableWithProgress || PipeFromBlockingSelectObjectCmdlet) && !PipelineBeforeMeContainsOperation) || (TotalRecords == 0 && ProgressWritten) || MultiOperationBatchMode() || force)
            {
                /* However if it turns out we didn't actually write any records (such as because the server glitched out
                 * and returned nothing) we never really wrote our progress record. If we were streaming however,
                 * we're still displaying our "detecting total number of items" message, so we need to finally complete it.
                 */
                if (!ProgressWritten && Scenario == ProgressScenario.StreamProgress)
                    record.StatusDescription = "Temp";

                record.RecordType = ProgressRecordType.Completed;

                WriteProgress(record);
            }

            if (!PipeToBlockingSelectObjectCmdlet)
                TotalRecords = null;

            record.Activity = DefaultActivity;
            record.StatusDescription = DefaultDescription;
            record.RecordType = ProgressRecordType.Processing;
        }

        private bool ReadyToComplete()
        {
            if (multiOperationAnalyzer.PipeToMultiOperation || multiOperationAnalyzer.IsMultiOperation)
                return ReadyToCompleteMultiOperation();

            if (PipeFromVariableWithProgress || PipeFromBlockingSelectObjectCmdlet || PipelineUpstreamContainsBlockingCmdlet)
            {
                //If we're not part of a chain, the first in the chain, or the pipeline contains an operation
                if (!PartOfChain || FirstInChain || PipeFromBlockingSelectObjectCmdlet || (PipelineContainsOperation && PreviousRecord == null))
                {
                    //We're a multi operation cmdlet piping from a variable in the middle of our EndProcessing block
                    if (Pipeline.CurrentIndex == -1 && MultiOperationBatchMode())
                        return true;

                    if (Pipeline.CurrentIndex < Pipeline.List.Count - 1)
                    {
                        if (upstreamSelectObjectManager != null)
                        {
                            if (notReady.NotReady())
                                return false;
                        }
                        else
                            return false;
                    }
                    else
                    {
                        if (notReady.NotReady())
                            return false;
                    }
                }
                else
                {
                    //We're the Object or Action in Variable -> Object -> Action
                    if (!PipelineContainsOperation || cmdlet is PrtgOperationCmdlet)
                    {
                        var previousCmdlet = CacheManager.GetPreviousPrtgCmdlet();
                        var previousManager = previousCmdlet.ProgressManager;

                        /*if (previousManager.recordsProcessed == -1 && previousCmdlet is PrtgOperationCmdlet)
                        {
                            var previousNonOperation = CacheManager.TryGetPreviousPrtgCmdletOfNotType<PrtgOperationCmdlet>();

                            if (!(previousNonOperation is PrtgOperationCmdlet))
                            {
                                if (previousNonOperation.ProgressManager.recordsProcessed < previousNonOperation.ProgressManager.TotalRecords)
                                    return false;
                            }
                        }*/

                        if (previousManager.recordsProcessed < previousManager.TotalRecords) //new issue: get-sensor doesnt update records processed
                            return false;
                    }
                    else
                    {
                        //We're the second object in Variable -> Object -> Action -> Object. We're responsible for our own record keeping
                        if (recordsProcessed < TotalRecords)
                            return false;
                    }
                }
            }

            return true;
        }

        private bool ReadyToCompleteMultiOperation()
        {
            if (multiOperationAnalyzer.PipeToMultiOperation || multiOperationAnalyzer.IsMultiOperation)
            {
                if (multiOperationAnalyzer.PreviousSourceStillWriting)
                    return false;

                if (multiOperationAnalyzer.PipeToMultiOperation)
                {
                    if (PipeFromVariableWithProgress)
                    {
                        if (!multiOperationAnalyzer.PipelineStillWriting && PreviousRecord == null)
                        {
                            //If we're piping from a variable and we're the first cmdlet in the pipeline, this means the multi operation progress after us has overwritten us
                            if (cmdlet is PrtgOperationCmdlet)
                                return false;

                            //This causes variable -> action -> multi to complete the action after processing all variable items
                            //This causes variable -> table -> action -> multi to complete the table after processing all variable and cmdlet items
                            return true;
                        }
                    }

                    if (PipeFromVariableWithProgress && !multiOperationAnalyzer.PipelineStillWriting && PreviousRecord == null)
                        return true;

                    if (downstreamSelectObjectManager?.HasSkipLast == true || downstreamSelectObjectManager?.HasLast == true)
                        return true;

                    return false;
                }

                if (multiOperationAnalyzer.IsMultiOperation)
                {
                    if (!multiOperationAnalyzer.ReceivedLastRecord)
                        return false;

                    //It's all over; complete the MultiOperationProgress.
                    if (multiOperationAnalyzer.PipelineFinished)
                        return true;

                    //we need to inspect all cmdlets up the chain to see whether they still have records to generate

                    //If a cmdlet up the pipeline still has records to output, we complete progress for now; we'll be back
                    if (multiOperationAnalyzer.PipelineStillWriting)
                        return true;

                    //We just received the last record, stand by to write the MultiOperationProgress
                    return false;
                }
            }

            return true;
        }

        public void UpdateRecordsProcessed(ProgressRecordEx record, PrtgObject obj, bool writeObject = true)
        {
            //When a variable to cmdlet chain contains an operation, responsibility for updating the number of records processed
            //"resets", and we become responsible for updating our own count again
            if ((PipeFromVariableWithProgress || PipeFromBlockingSelectObjectCmdlet || PipelineUpstreamContainsBlockingCmdlet) && !PipelineContainsOperation)
            {
                //If we're the only cmdlet, the first cmdlet, or the pipeline contains an operation cmdlet
                //we're responsible for updating our own progress, which we must do via analyzing the pipeline.
                if (!PartOfChain || FirstInChain || SourceBeforeSelectObjectUnusable)
                {
                    IncrementProgressFromPipeline(record, obj, writeObject);
                }
                else
                {
                    //Otherwise (such as when we're the second cmdlet piped from a variable)
                    //the previous cmdlet tracks the number of records processed.
                    var previousCmdlet = CacheManager.GetPreviousPrtgCmdlet();
                    var previousManager = previousCmdlet.ProgressManager;

                    IncrementProgressFromTotalRecords(record, previousManager, obj, writeObject);
                }
            }
            else
            {
                IncrementProgressFromTotalRecords(record, this, obj, writeObject);
            }
        }

        //ISSUE: select -last actually TERMINATES the previous cmdlet, thus removing its progress
        //as well as any details stored in it
        //it seems like the only possibility is to just start progress all over, from scratch!
        //how does this affect complex scenarios like | select -first 3 | select -last 1. we dont know
        //that theres a blocking cmdlet, unless we consider ALL select cmdlets between now and the next
        //prtg cmdlet, or the end of the pipeline

        //if progress will just start over, do i even need to make any modifications?
        //we do in that if we're feeding in 3 objects, we need to consider it pipe from variable!
        //so then how do we get the total incoming count from selectobject? to show an immediate
        //total we're going to be processing

        //we can add a -Resolve parameter to addobject cmdlets that gets the last object with the specified
        //name under the parent, as identified by the higher ID
        //we can maybe look at the object creation time to have an idea as to whether it was really created?
        //or we can get the objects before and after so we know we've got rhe new one

        //TODO: need to modify the progress scenario handling for when you use both parameters at once
        //also need to implement handling of -index and -wait

        private void IncrementProgressFromTotalRecords(ProgressRecordEx record, ProgressManager manager, PrtgObject obj, bool writeObject)
        {
            if (manager.TotalRecords > 0)
            {
                if (manager.recordsProcessed < 0)
                    manager.recordsProcessed++;

                manager.recordsProcessed++;

                if (abortProgress.AbortProgress(manager))
                    return;

                if (obj != null && Scenario != ProgressScenario.StreamProgress)
                    record.StatusDescription = $"{InitialDescription} '{obj.Name}' ({manager.recordsProcessed}/{manager.TotalRecords})";
                else
                    record.StatusDescription = $"{InitialDescription} {manager.recordsProcessed}/{manager.TotalRecords}";

                if (manager.recordsProcessed > 0)
                    record.PercentComplete = (int)(manager.recordsProcessed / Convert.ToDouble(manager.TotalRecords) * 100);

                if (!writeObject)
                    manager.recordsProcessed--;

                //If the next cmdlet is an operation cmdlet, avoid saying "Processing record x/y", as the operation cmdlet will display this for us
                if ((NextCmdletIsOperation && manager.recordsProcessed < 2) || !NextCmdletIsOperation) //todo: what happens when WE'RE an operation cmdlet?
                    WriteProgress();

                if (abortProgress.NeedsCompleting(manager))
                    CompleteProgress();
            }
        }

        private void IncrementProgressFromPipeline(ProgressRecordEx record, PrtgObject obj, bool writeObject = true)
        {
            var index = variableProgressDisplayed ? Pipeline.CurrentIndex + 2 : Pipeline.CurrentIndex + 1;

            var maxCount = Pipeline.List.Count;

            if (upstreamSelectObjectManager != null)
                maxCount = GetIncrementProgressFromPipelineTotalRecords(maxCount);

            var originalIndex = index;
            if (index > maxCount)
                index = maxCount;

            if(obj != null)
                record.StatusDescription = $"{InitialDescription} '{obj.Name}' ({index}/{maxCount})";
            else
                record.StatusDescription = $"{InitialDescription} {index}/{maxCount}";

            record.PercentComplete = (int)((index) / Convert.ToDouble(maxCount) * 100);

            if (writeObject)
                variableProgressDisplayed = true;

            if (originalIndex <= Pipeline.List.Count)
                WriteProgress(record);
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
            if ((PipeFromVariableWithProgress || PipeFromBlockingSelectObjectCmdlet) && Pipeline.CurrentIndex > 0)
                return;

            CurrentRecord.StatusDescription = InitialDescription;

            WriteProgress();
        }

        public void ProcessOperationProgress(string activity, string progressMessage)
        {
            if (!PrtgSessionState.EnableProgress || UnsupportedSelectObjectProgress)
                return;

            //If we already had an operation cmdlet, the responsibility of updating the previous cmdlet's
            //records processed has now shifted back on to him, so we don't need to do it for him. As such,
            //we may assume we're now a "normal" pipeline
            if ((PipeFromVariableWithProgress ||
                (PipeFromBlockingSelectObjectCmdlet && SourceBeforeSelectObjectUnusable) ||
                PipelineUpstreamContainsBlockingCmdlet)
                && !PipelineBeforeMeContainsOperation)
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

        public void ProcessMultiOperationProgress(string activity, string progressMessage)
        {
            if (!PrtgSessionState.EnableProgress)
                return;

            if (cmdlet.ProgressManagerEx.CachedRecord != null)
            {
                CloneRecord(cmdlet.ProgressManagerEx.CachedRecord, CurrentRecord);
                //var record = cmdlet.ProgressManagerEx.PreviousRecord;

                CurrentRecord.Activity = activity;
                CurrentRecord.StatusDescription = progressMessage;
                CurrentRecord.PercentComplete = 100;

                WriteProgress(CurrentRecord, true);
            }
            else
            {
                CurrentRecord.Activity = activity;
                CurrentRecord.StatusDescription = progressMessage;
                CurrentRecord.PercentComplete = 100;

                WriteProgress(CurrentRecord);
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

            if (upstreamSelectObjectManager != null)
                TotalRecords = GetSelectObjectOperationStraightFromVariableTotalRecords();

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

            var previousCmdlet = CacheManager.GetPreviousPrtgCmdlet();
            var previousManager = previousCmdlet.ProgressManager;
            TotalRecords = previousManager.TotalRecords;

            if (upstreamSelectObjectManager != null)
                TotalRecords = GetSelectObjectOperationFromCmdletFromVariableTotalRecords();

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

                var previousCmdlet = CacheManager.TryGetPreviousPrtgCmdletOfNotType<PrtgOperationCmdlet>();

                var processed = -1;
                int? total = -1;

                if (previousCmdlet != null)
                {
                    if (PipelineUpstreamContainsBlockingCmdlet)
                    {
                        var p = Pipeline;
                    }
                    else
                    {
                        var previousManager = previousCmdlet.ProgressManager;

                        processed = previousManager.recordsProcessed;
                        total = previousManager.TotalRecords;
                    }
                }
                else
                {
                    //We might be Action2 in Variable -> Select -> Action -> Action
                    var firstOp = CacheManager.TryGetFirstOperationCmdletAfterSelectObject();

                    if (firstOp != null)
                    {
                        //todo: what if we're Variable -> Table -> Select -> Action -> Action. we dont have any tests for that!
                        processed = EntirePipeline.CurrentIndex + 1;
                        total = EntirePipeline.List.Count;
                    }
                }

                if (previousCmdlet is PrtgOperationCmdlet && processed == -1)
                {
                    processed = EntirePipeline.CurrentIndex + 1;
                    total = EntirePipeline.List.Count;
                }

                PreviousRecord.StatusDescription = $"{progressMessage} ({processed}/{total})";

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

        internal static bool IsPureThirdPartyCmdlet(Type cmdlet)
        {
            if (cmdlet == typeof(WhereObjectCommand))
                return true;

            if (cmdlet == typeof(SelectObjectCommand))
                return true;

            return false;
        }
    }
}
