using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using System.Text;
using Microsoft.PowerShell.Commands;
using PrtgAPI.PowerShell.Base;
using PrtgAPI.Reflection;

namespace PrtgAPI.PowerShell.Progress
{
    internal partial class ProgressManager : IDisposable
    {
        #region Progress Pipeline State

        internal static ProgressPipelineStack progressPipelines = new ProgressPipelineStack();

        internal int ProgressPipelinesCount => progressPipelines.Count;

        public bool IsOnlyRecordInCurrentPipeline => progressPipelines.RecordsInCurrentPipeline == 1;

        public bool HasMultipleRecordsInCurrentPipeline => progressPipelines.RecordsInCurrentPipeline > 1;

        public bool HasMultiplePipelines => progressPipelines.Count > 1;

        #endregion
        #region Progress Record State

        private Lazy<ProgressState> currentState = new Lazy<ProgressState>(() => progressPipelines.CurrentRecordInPipeline);

        public ProgressState CurrentState => currentState.Value;

        private Lazy<ProgressState> previousState = new Lazy<ProgressState>(() => progressPipelines.PreviousRecordInPipeline);

        public ProgressState PreviousState => previousState.Value;

        public ProgressRecordEx CurrentRecord => CurrentState.ProgressRecord;

        public ProgressRecordEx PreviousRecord => PreviousState?.ProgressRecord;

        /// <summary>
        /// Indicates that the current cmdlet expects that it will contain progress, either because it already does, or it will when a record is written to the pipeline.
        /// </summary>
        public bool ExpectsContainsProgress => CurrentRecord.HasActivity && (CurrentRecord.HasDescription || InitialDescription != string.Empty);

        /// <summary>
        /// Indicates that a record has actually been written to the pipeline for this cmdlet, thus initializing the status description.
        /// </summary>
        public bool ProgressWritten => CurrentRecord.HasDescription;

        public bool PreviousContainsProgress => PreviousRecord?.ContainsProgress == true;

        #endregion
        #region Pipeline Chain Analysis

        /// <summary>
        /// Indicates whether the current cmdlet is first in a chain of pure PrtgAPI cmdlets (with no third party filters in between, etc)
        /// </summary>
        public bool FirstInChain => pipeToProgressCompatibleCmdlet && IsOnlyRecordInCurrentPipeline;

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
                    if (pipeFromProgressCompatibleCmdlet && (pipeToProgressCompatibleCmdlet || HasMultipleRecordsInCurrentPipeline))
                        partOfChain = true;
                    else
                        partOfChain = false;
                }

                return partOfChain.Value;
            }
        }

        public bool LastInChain => !pipeToProgressCompatibleCmdlet;

        public bool LastPrtgCmdletInPipeline => CacheManager.GetNextPrtgCmdlet() == null;

        //Display progress when piping multiple values from a variable, or a single value to multiple cmdlets
        public bool PipeFromVariableWithProgress => EntirePipeline?.List.Count > 1 || (EntirePipeline?.List.Count == 1 && PartOfChain) || (EntirePipeline?.List.Count == 1 && progressPipelines.Count > 1);

        public bool PipeFromSingleVariable => EntirePipeline?.List.Count == 1 && HasMultiplePipelines;

        public bool ProgressEnabled => PrtgSessionState.EnableProgress && !UnsupportedSelectObjectProgress && !UnsupportedNestedProgress;

        public bool GetRecordsWithVariableProgress => (PipeFromVariableWithProgress || CanUseSelectObjectProgress || PipelineUpstreamContainsBlockingCmdlet || PipeFromPrtgCmdletPostProcessMode) && ProgressEnabled;

        public bool GetResultsWithProgress => PartOfChain && ProgressEnabled;

        public bool PreviousOperationDestroyed => PipelineContainsOperation && PreviousRecord == null;

        public bool PipeFromPrtgCmdletPostProcessMode
        {
            get
            {
                var previousCmdlet = CacheManager.GetPreviousPrtgCmdletOfType<PrtgPostProcessCmdlet>();

                if (previousCmdlet?.ProgressManager?.PostProcessMode() == true)
                    return true;

                return false;
            }
        }

        public bool NormalSeemsLikePipeFromVariable => (PipeFromVariableWithProgress || PipeFromBlockingSelectObjectCmdlet || PipelineUpstreamContainsBlockingCmdlet) && !PipelineContainsOperation || PreviousOperationDestroyed || PipeFromPrtgCmdletPostProcessMode;

        public bool OperationSeemsLikePipeFromVariable
        {
            get
            {
                if (PipeFromPrtgCmdletPostProcessMode && !(CacheManager.GetPreviousPrtgCmdlet() is PrtgOperationCmdlet))
                    return true;

                if (!PipelineBeforeMeContainsOperation || (PreviousOperationDestroyed && Pipeline != null))
                {
                    if (PipeFromVariableWithProgress)
                        return true;

                    if (PipeFromBlockingSelectObjectCmdlet && SourceBeforeSelectObjectUnusable)
                        return true;

                    if (PipelineUpstreamContainsBlockingCmdlet)
                        return true;
                }

                return false;
            }
        }

        public bool UsePipelineForNormalSeemsLikePipeFromVariable => !PartOfChain || FirstInChain || SourceBeforeSelectObjectUnusable;

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

        public bool NextCmdletIsInternalOperation
        {
            get
            {
                var cmdlet = CacheManager.GetNextPrtgCmdlet();

                if (cmdlet is PrtgOperationCmdlet && cmdlet.ProgressManager?.InternalProgress == true)
                    return true;

                return false;
            }
        }

        public bool NextCmdletIsPostProcessMode => PostProcessMode(true);

        public bool PostProcessMode(bool downstream = false)
        {
            object c = cmdlet;

            if (downstream)
            {
                c = null;
                var commands = CacheManager.GetPipelineCommands();

                var myIndex = commands.IndexOf(cmdlet);

                for (int i = myIndex + 1; i < commands.Count; i++)
                {
                    if (commands[i] is PrtgPostProcessCmdlet)
                    {
                        c = commands[i];
                        break;
                    }

                    if (commands[i] is PrtgOperationCmdlet)
                        continue;

                    if (commands[i] is PrtgCmdlet)
                    {
                        c = commands[i];
                        break;
                    }
                }
            }

            var typedCmdlet = c as PrtgPostProcessCmdlet;

            if (typedCmdlet == null)
                return false;

            if (typedCmdlet.PostProcess)
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

        /// <summary>
        /// If true, indicates that the previous cmdlet before this one is a SelectObjectCommand.
        /// </summary>
        public bool PreviousCmdletIsSelectObject => upstreamSelectObjectManager != null;

        internal readonly SelectObjectManager upstreamSelectObjectManager;
        internal readonly SelectObjectManager downstreamSelectObjectManager;

        internal SelectObjectManager operationUpstreamSelectObjectManager
        {
            get
            {
                if (!PreviousCmdletIsSelectObject && cmdlet is PrtgOperationCmdlet)
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
                if (PreviousCmdletIsSelectObject)
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
                if (PreviousCmdletIsSelectObject)
                {
                    if (Scenario == ProgressScenario.SelectLast)
                    {
                        if (!PipeFromVariableWithProgress)
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

        public bool IsNestedCmdlet => cmdlet.CommandRuntime is DummyRuntime;

        public bool UnsupportedNestedProgress => PipeFromVariableWithProgress && IsNestedCmdlet;

        private bool CalculateIsUnsupportedSelectObjectProgress()
        {
            //Something -> Select -> Select -> Select
            if (PipelineContainsExcessiveSelectObjectCmdlets())
                return true;

            //More than one parameter used at once
            if (PipelineContainsExcessiveSelectObjectParameters)
                return true;


            //Something -> Select -> Action -> Action
            if (PipeToOrFromIllegalMultipleOperationFromSelectObject)
            {
                //If only -First is specified, its OK
                return true;
            }

            if (PipelineContainsMultipleCmdletsBeforeSelectObject)
                return true;

            return false;                
        }

        private bool PipelineContainsMultipleCmdletsBeforeSelectObject
        {
            get
            {
                var commands = CacheManager.GetPipelineCommands().Skip(2);

                if (commands.Where(SelectObjectDescriptor.IsSelectObjectCommand).Any(c => new SelectObjectDescriptor((PSCmdlet) c).HasFilters))
                    return true;

                return false;
            }
        }

        private bool PipeToOrFromIllegalMultipleOperationFromSelectObject
        {
            get
            {
                var commands = CacheManager.GetPipelineCommands();

                for (int i = 0; i < commands.Count - 2; i++)
                {
                    if (SelectObjectDescriptor.IsSelectObjectCommand(commands[i]) && commands[i + 1] is PrtgOperationCmdlet && commands[i + 2] is PrtgOperationCmdlet)
                    {
                        var descriptor = new SelectObjectDescriptor((PSCmdlet) commands[i]);

                        if (descriptor.HasIndex || descriptor.HasLast || descriptor.HasSkip || descriptor.HasSkipLast)
                            return true;

                        return false;
                    }
                }

                return false;
            }
        }

        private bool PipelineContainsExcessiveSelectObjectCmdlets()
        {
            var commands = CacheManager.GetPipelineCommands();

            var selectInARow = 0;

            foreach (object t in commands)
            {
                if (SelectObjectDescriptor.IsSelectObjectCommand(t))
                    selectInARow++;
                else
                    selectInARow = 0;

                if (selectInARow == 3)
                    return true;
            }

            return false;
        }

        private bool PipelineContainsExcessiveSelectObjectParameters
        {
            get
            {
                List<List<PSCmdlet>> selectObjectSets = new List<List<PSCmdlet>>();

                var currentSet = new List<PSCmdlet>();

                foreach (var obj in CacheManager.GetPipelineCommands())
                {
                    if (SelectObjectDescriptor.IsSelectObjectCommand(obj))
                    {
                        currentSet.Add((PSCmdlet) obj);
                    }
                    else
                    {
                        if (currentSet.Count > 0)
                        {
                            selectObjectSets.Add(currentSet);
                            currentSet = new List<PSCmdlet>();
                        }
                    }
                }

                var keys = new[] { "First", "Last", "Skip", "SkipLast", "Index" };

                return selectObjectSets.Select(set => set.SelectMany(c => c.MyInvocation.BoundParameters)
                                       .Count(p => keys.Contains(p.Key)))
                                       .Any(totalParameters => totalParameters > 1);
            }
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

        internal ReadyParser readyParser;
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
                    if (IsOnlyRecordInCurrentPipeline)
                    {
                        //If we're actually preceeded by a Select-Object cmdlet, we actually need a SelectPipeline

                        if (PreviousCmdletIsSelectObject && upstreamSelectObjectManager.HasFirst)
                        {
                            if (SelectPipeline == null)
                            {
                                if (CmdletPipeline == null)
                                    return null;

                                SelectPipeline = new Pipeline(CmdletPipeline.Current, EntirePipeline.List);
                            }

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

        /// <summary>
        /// Specifies that that this progress manager represents an internal operation.
        /// </summary>
        public bool InternalProgress { get; set; }

        #endregion
        #region Regular Fields

        public string InitialDescription { get; set; }

        public int? TotalRecords
        {
            get { return CurrentState?.TotalRecords; }
            set { CurrentState.TotalRecords = value; }
        }

        internal PrtgCmdlet cmdlet;

        internal int RecordsProcessed { get; set; } = -1;

        private bool variableProgressDisplayed;

        private IProgressWriter progressWriter;

        internal static Func<PrtgCmdlet, IProgressWriter> CustomWriter { get; set; }

        internal ProgressScenario Scenario { get; set; }

        private bool sourceIdUpdated;

        internal PSReflectionCacheManager CacheManager { get; set; }

        public bool WatchStream => cmdlet is IWatchableCmdlet && ((IWatchableCmdlet) cmdlet).WatchStream;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressManager"/> class. You MUST dispose the created object, else <see cref="ProgressManager"/> will leak <see cref="progressPipelines"/>.
        /// </summary>
        /// <param name="cmdlet">The cmdlet to manage.</param>
        public ProgressManager(PrtgCmdlet cmdlet)
        {
            this.cmdlet = cmdlet;
            CacheManager = new PSReflectionCacheManager(cmdlet);

            var sourceId = GetLastSourceId();
            progressPipelines.Push(this, sourceId);

            if (PreviousRecord != null)
            {
                CurrentRecord.ParentActivityId = PreviousRecord.ActivityId;

                if (CurrentRecord.ParentActivityId > CurrentRecord.ActivityId)
                    CurrentRecord.ParentActivityId = -1;
            }

            if (cmdlet.ProgressManagerEx.CurrentRecord != null)
                CurrentRecord.ProgressWritten = cmdlet.ProgressManagerEx.CurrentRecord.ProgressWritten;

            cmdlet.ProgressManagerEx.CurrentRecord = CurrentRecord;
            cmdlet.ProgressManagerEx.PreviousRecord = PreviousRecord;
            cmdlet.ProgressManagerEx.RestoreRecordState();

            EntirePipeline = CacheManager.GetPipelineInput();
            CmdletPipeline = CacheManager.GetCmdletPipelineInput();

            progressWriter = GetWriter();

            if (SelectObjectDescriptor.IsSelectObjectCommand(CacheManager.GetUpstreamCmdletNotOfType<WhereObjectCommand>()))
            {
                upstreamSelectObjectManager = new SelectObjectManager(CacheManager, cmdlet, Direction.Upstream);

                if (upstreamSelectObjectManager.Commands.Count >= 3)
                    upstreamSelectObjectManager = null;
            }

            if (SelectObjectDescriptor.IsSelectObjectCommand(CacheManager.GetDownstreamCmdletNotOfType<WhereObjectCommand>()))
            {
                downstreamSelectObjectManager = new SelectObjectManager(CacheManager, cmdlet, Direction.Downstream);

                if (downstreamSelectObjectManager.Commands.Count >= 3)
                    downstreamSelectObjectManager = null;
            }

            readyParser = new ReadyParser(this);
            multiOperationAnalyzer = new MultiOperationRecordAnalyzer(this);

            CalculateProgressScenario();
        }

        private void CalculateProgressScenario()
        {
            if (PipeFromBlockingSelectObjectCmdlet && operationUpstreamSelectObjectManager != null)
            {
                if (operationUpstreamSelectObjectManager.HasLast)
                    Scenario = ProgressScenario.SelectLast;
                else if (operationUpstreamSelectObjectManager.HasSkipLast)
                    Scenario = ProgressScenario.SelectSkipLast;
                else
                    throw new NotImplementedException("Don't know what parameter Select-Object is blocking with.");
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

        [ExcludeFromCodeCoverage]
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

        [ExcludeFromCodeCoverage]
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            cmdlet.ProgressManagerEx.CmdletOwnsRecord = CurrentRecord.CmdletOwnsRecord;

            if (disposing)
            {
                progressPipelines.Pop();
            }

            disposed = true;
        }

        #endregion

        public void RemovePreviousOperation()
        {
            if (PreviousRecord?.HasOperation == true)
            {
                PreviousRecord.CurrentOperation = null;

                WriteProgress(PreviousRecord);
            }
        }

        internal void WriteProgress(bool cache = false)
        {
            if (!ProgressEnabled)
                return;

            WriteProgress(CurrentRecord, cache);
        }

        public void WriteProgress(string activity, string statusDescription)
        {
            CurrentRecord.Activity = activity;
            CurrentRecord.StatusDescription = statusDescription;

            WriteProgress();
        }
        
        private void WriteProgress(ProgressRecordEx progressRecord, bool cache = false)
        {
            WriteProgress(progressRecord, PreviousRecord, cache);
        }

        private void WriteProgress(ProgressRecordEx progressRecord, ProgressRecordEx previousRecord, bool cache = false)
        {
            if (progressRecord.ContainsProgress == false)
            {
                if (cmdlet.Stopping)
                    throw new PipelineStoppedException();

                throw new InvalidOperationException("Attempted to write progress on an uninitialized ProgressRecord. If this is a Release build, please report this bug along with the cmdlet chain you tried to execute. To disable PrtgAPI Cmdlet Progress in the meantime use Disable-PrtgProgress.");
            }

            progressRecord.State.Completed = progressRecord.RecordType == ProgressRecordType.Completed;

            progressRecord.ProgressWritten = true;

            if (previousRecord == null)
            {
                if (cache)
                    progressWriter.WriteProgress(progressRecord.SourceId, progressRecord);
                else
                {
                    progressWriter.WriteProgress(progressRecord);

                    if (!sourceIdUpdated)
                    {
                        progressRecord.SourceId = GetLastSourceId();
                        sourceIdUpdated = true;
                    }
                }
            }
            else
            {
                var sourceId = previousRecord.SourceId;
                progressRecord.SourceId = sourceId;

                progressWriter.WriteProgress(sourceId, progressRecord);
            }
        }

        internal long GetLastSourceId()
        {
            if (!PrtgSessionState.EnableProgress)
                return -1;

            long val;

            if (cmdlet.CommandRuntime is DummyRuntime)
                val = DummyRuntime._lastUsedSourceId;
            else
                val = Convert.ToInt64(cmdlet.CommandRuntime.PSGetInternalStaticField("s_lastUsedSourceId", "_lastUsedSourceId"));

            if (PipeFromVariableWithProgress && FirstInChain)
            {
                if (cmdlet.ProgressManagerEx.lastSourceId == null)
                    cmdlet.ProgressManagerEx.lastSourceId = val;

                return cmdlet.ProgressManagerEx.lastSourceId.Value;
            }

            return val;
        }

        public void CompleteProgress(bool force = false, bool cache = false)
        {
            CompleteProgress(CurrentRecord, force, cache);
        }

        public void TryCompleteProgress()
        {
            try
            {
                //If an exception was thrown, we should try and clean up our outstanding progress record (if applicable)
                if (ExpectsContainsProgress)
                    CompleteProgress(true, cmdlet.ProgressManagerEx.CachedRecord != null);
            }
            catch
            {
            }
        }

        private void CompleteProgress(ProgressRecordEx record, bool force = false, bool cache = false)
        {
            if (!force && !ReadyToComplete())
                return;

            InitialDescription = null;
            RecordsProcessed = -1;

            /*
             * If we've been displaying the number of records we need to process, we have a process record and need to complete it
             * If we're piping from a variable, we need to complete our progress - however if the pipeline before me is an operation,
             *     responsibility has shifted and we're not responsible for the current progress record
             */
            if (TotalRecords > 0 || ((PipeFromVariableWithProgress || PipeFromBlockingSelectObjectCmdlet) && !PipelineBeforeMeContainsOperation) || (TotalRecords == 0 && ProgressWritten) || PostProcessMode() || force)
            {
                /* However if it turns out we didn't actually write any records (such as because the server glitched out
                 * and returned nothing) we never really wrote our progress record. If we were streaming however,
                 * we're still displaying our "detecting total number of items" message, so we need to finally complete it.
                 */
                if (!ProgressWritten && Scenario == ProgressScenario.StreamProgress)
                    record.StatusDescription = "Temp";

                record.RecordType = ProgressRecordType.Completed;

                WriteProgress(record, cache);

                record.ProgressWritten = false;
            }

            if (!PipeToBlockingSelectObjectCmdlet)
                TotalRecords = null;

            record.Activity = ProgressRecordEx.DefaultActivity;
            record.StatusDescription = ProgressRecordEx.DefaultDescription;
            record.CurrentOperation = null;
            record.RecordType = ProgressRecordType.Processing;
        }

        internal void CompleteUncompleted(bool suppressExceptions = false)
        {
            try
            {
                var progressManagerEx = cmdlet.ProgressManagerEx;

                if (progressManagerEx.CurrentRecord == null)
                    return;

                if (!progressManagerEx.CurrentRecord.ProgressWritten)
                    return;

                if (!progressManagerEx.CurrentRecord.CmdletOwnsRecord)
                    return;

                if (progressManagerEx.CurrentRecord.Completed)
                    return;

                progressManagerEx.CurrentRecord.RecordType = ProgressRecordType.Completed;

                WriteProgress(progressManagerEx.CurrentRecord, progressManagerEx.PreviousRecord, progressManagerEx.CachedRecord != null);
            }
            catch
            {
                if (!suppressExceptions)
                    throw;
            }
        }

        internal bool ReadyToComplete()
        {
            if (multiOperationAnalyzer.PipeToMultiOperation || multiOperationAnalyzer.IsMultiOperation)
                return ReadyToCompleteMultiOperation();

            if (PipeFromVariableWithProgress || PipeFromBlockingSelectObjectCmdlet || PipelineUpstreamContainsBlockingCmdlet || PipeFromPrtgCmdletPostProcessMode)
            {
                //If we're not part of a chain, the first in the chain, or the pipeline contains an operation
                if (!PartOfChain || FirstInChain || PipeFromBlockingSelectObjectCmdlet || (PipelineContainsOperation && PreviousRecord == null))
                {
                    //We're a multi operation cmdlet piping from a variable in the middle of our EndProcessing block
                    if (Pipeline.CurrentIndex == -1 && PostProcessMode())
                        return true;

                    if (Pipeline.CurrentIndex < Pipeline.List.Count - 1)
                    {
                        if (PreviousCmdletIsSelectObject)
                        {
                            if (!readyParser.Ready())
                                return false;
                        }
                        else
                            return false;
                    }
                    else
                    {
                        if (!readyParser.Ready())
                            return false;
                    }
                }
                else
                {
                    //We're the Object or Action in Variable -> Object -> Action
                    if (!PipelineContainsOperation || cmdlet is PrtgOperationCmdlet || PipeFromPrtgCmdletPostProcessMode)
                    {
                        var previousCmdlet = CacheManager.GetPreviousPrtgCmdlet();
                        var previousManager = previousCmdlet.ProgressManager;

                        if (previousManager.RecordsProcessed < previousManager.TotalRecords) //new issue: get-sensor doesnt update records processed
                            return false;
                    }
                    else
                    {
                        //We're the second object in Variable -> Object -> Action -> Object. We're responsible for our own record keeping
                        if (RecordsProcessed < TotalRecords)
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

        public void UpdateRecordsProcessed(ProgressRecordEx record, IObject obj, bool writeObject = true)
        {
            //When a variable to cmdlet chain contains an operation, responsibility for updating the number of records processed
            //"resets", and we become responsible for updating our own count again.
            //If we are the internal progress of a PrtgOperationCmdlet however (such as calling GetTree() in Show-PrtgTree -Id <value>)
            //we might not have a pipeline at all, in which case we should proceed as if we're simply incrementing from total records
            if (NormalSeemsLikePipeFromVariable && !(InternalProgress && Pipeline == null))
            {
                //If we're the only cmdlet, the first cmdlet, or the pipeline contains an operation cmdlet
                //we're responsible for updating our own progress, which we must do via analyzing the pipeline.
                if (UsePipelineForNormalSeemsLikePipeFromVariable)
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

        private void IncrementProgressFromTotalRecords(ProgressRecordEx record, ProgressManager manager, IObject obj, bool writeObject)
        {
            if (manager.TotalRecords > 0)
            {
                if (manager.RecordsProcessed < 0)
                    manager.RecordsProcessed++;

                manager.RecordsProcessed++;

                var str = GetStatusDescriptionProgressCount(manager.RecordsProcessed, manager.TotalRecords.Value);

                if (obj != null && Scenario != ProgressScenario.StreamProgress)
                    record.StatusDescription = GetObjectStatusDescription(obj, str);
                else
                    record.StatusDescription = $"{InitialDescription} ({str})";

                if (manager.RecordsProcessed > 0)
                    record.PercentComplete = GetPercentComplete(manager.RecordsProcessed, manager.TotalRecords.Value);

                if (abortProgress.AbortProgress(manager))
                    return;

                if (!writeObject)
                    manager.RecordsProcessed--;

                //If the next cmdlet is an operation cmdlet, avoid saying "Processing record x/y", as the operation cmdlet will display this for us
                //But if the previous cmdlet is a post process cmdlet executing with -PassThru, we're effectively operating in "variable progress"
                //mode, so we need to display some progress for the record that was piped into us.
                //If the next cmdlet is an internal operation cmdlet however (e.g. Show-PrtgTree) we do want to show progress.
                if ((NextCmdletIsOperation && (manager.RecordsProcessed < 2 || NextCmdletIsInternalOperation)) || !NextCmdletIsOperation || PipeFromPrtgCmdletPostProcessMode) //todo: what happens when WE'RE an operation cmdlet?
                    WriteProgress();

                CompletePrematurely(manager);
            }
        }

        internal bool CompletePrematurely(ProgressManager manager)
        {
            if (abortProgress.NeedsCompleting(manager))
            {
                CompleteProgress(true);
                return true;
            }

            return false;
        }

        private void IncrementProgressFromPipeline(ProgressRecordEx record, IObject obj, bool writeObject = true)
        {
            var index = variableProgressDisplayed ? Pipeline.CurrentIndex + 2 : Pipeline.CurrentIndex + 1;

            var maxCount = Pipeline.List.Count;

            if (PreviousCmdletIsSelectObject)
                maxCount = GetIncrementProgressFromPipelineTotalRecords(maxCount);

            var originalIndex = index;
            if (index > maxCount)
                index = maxCount;

            var str = GetStatusDescriptionProgressCount(index, maxCount);

            if (obj != null)
                record.StatusDescription = GetObjectStatusDescription(obj, str);
            else
                record.StatusDescription = $"{InitialDescription} {str}";

            record.PercentComplete = GetPercentComplete(index, maxCount);

            if (writeObject)
                variableProgressDisplayed = true;

            if (originalIndex <= Pipeline.List.Count)
                WriteProgress(record);
        }

        private string GetObjectStatusDescription(IObject obj, string countStr)
        {
            var builder = new StringBuilder();
            builder.AppendFormat("{0} '{1}'", InitialDescription, obj.Name);

            if (obj is IPrtgObject)
                builder.AppendFormat(" (ID: {0})", obj.GetId());

            builder.AppendFormat(" ({0})", countStr);

            return builder.ToString();
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

        public void ProcessOperationProgress(string activity, string progressMessage, bool incrementRecord)
        {
            if (!ProgressEnabled)
                return;

            //If we already had an operation cmdlet, the responsibility of updating the previous cmdlet's
            //records processed has now shifted back on to him, so we don't need to do it for him. As such,
            //we may assume we're now a "normal" pipeline
            if (OperationSeemsLikePipeFromVariable)
            {
                //Variable -> Action
                //Variable -> Action -> Object
                //Variable -> Object -> Action
                //Variable -> Object -> Action -> Object
                ProcessOperationProgressForVariable(activity, progressMessage, incrementRecord);
            }
            else
            {
                //Object -> Action
                //Object -> Action -> Object
                //Object -> Action -> Object -> Action
                ProcessOperationProgressForCmdlet(activity, progressMessage);
            }
        }

        public void ProcessPostProcessProgress(string activity, string progressMessage, int percentComplete = 100,
            int? secondsRemaining = null, string currentOperation = null)
        {
            if (!ProgressEnabled)
                return;

            var cache = false;

            if (cmdlet.ProgressManagerEx.CachedRecord != null)
            {
                CloneRecord(cmdlet.ProgressManagerEx.CachedRecord, CurrentRecord);

                cache = true;
            }

            if (WatchStream)
                percentComplete = percentComplete % 100;

            CurrentRecord.Activity = activity;
            CurrentRecord.StatusDescription = progressMessage;
            CurrentRecord.PercentComplete = percentComplete;
            CurrentRecord.RecordType = ProgressRecordType.Processing;

            if (secondsRemaining != null)
                CurrentRecord.SecondsRemaining = secondsRemaining.Value;

            if (currentOperation != null)
                CurrentRecord.CurrentOperation = currentOperation;

            WriteProgress(CurrentRecord, cache);
        }

        private void ProcessOperationProgressForVariable(string activity, string progressMessage, bool incrementRecord)
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
                ProcessOperationProgressFromCmdletFromVariable(activity, progressMessage, incrementRecord);
            }
        }

        private void ProcessOperationProgressStraightFromVariable(string activity, string progressMessage)
        {
            //1b: Variable -> Action
            //Variable -> Action -> Object
            //    We know the total number of records and where we're up to as we can read this information straight from the pipeline

            //Variable -> Action -> Object
            //    HOW DO WE TELL THE NEXT TABLE THE NUMBER OF ITEMS WE'VE PROCESSED????

            var pipeline = Pipeline ?? EntirePipeline;

            TotalRecords = pipeline?.List.Count;

            if (PreviousCmdletIsSelectObject)
                TotalRecords = GetSelectObjectOperationStraightFromVariableTotalRecords();

            //e.g. Get-Sensor | Select -First | New-Sensor -Factory. At the very least, we say there's 1 record so we have something to show
            if (pipeline == null && TotalRecords == null)
                TotalRecords = 1;

            ////e.g. Get-Sensor | Select -First | New-Sensor -Factory. At the very least, we say there's 1 record so we have something to show
            var count = pipeline?.CurrentIndex + 1 ?? 1;

            CurrentRecord.Activity = activity;
            CurrentRecord.PercentComplete = GetPercentComplete(count, TotalRecords.Value);
            CurrentRecord.StatusDescription = $"{progressMessage} ({GetStatusDescriptionProgressCount(count, TotalRecords.Value)})";

            WriteProgress();
        }

        private void ProcessOperationProgressFromCmdletFromVariable(string activity, string progressMessage, bool incrementRecord)
        {
            //5b: Variable -> Object -> Action
            //Variable -> Object -> Action -> Object
            //    We know the total number of records and where we're up to as we can read this information from the previous cmdlet's ProgressManager

            //Variable -> Action -> Object
            //    HOW DO WE TELL THE NEXT TABLE THE NUMBER OF ITEMS WE'VE PROCESSED????

            var previousCmdlet = CacheManager.GetPreviousPrtgCmdlet();
            var previousManager = previousCmdlet.ProgressManager;
            TotalRecords = previousManager.TotalRecords;

            if (PreviousCmdletIsSelectObject)
                TotalRecords = GetSelectObjectOperationFromCmdletFromVariableTotalRecords();

            if (TotalRecords == null)
                throw new InvalidOperationException("Cannot display records procesed as TotalRecords is not initialized.");

            //Normally the object cmdlet would be responsible for updating the number of records we've processed so far,
            //but for REASONS UNKNOWN (TODO: WHY) thats not the case, so we have to do it instead
            if (previousManager.RecordsProcessed < 0)
                previousManager.RecordsProcessed++;

            previousManager.RecordsProcessed++;

            var count = previousManager.RecordsProcessed;

            if (!incrementRecord)
                previousManager.RecordsProcessed--;

            CurrentRecord.Activity = activity;
            CurrentRecord.PercentComplete = GetPercentComplete(count, TotalRecords.Value);
            CurrentRecord.StatusDescription = $"{progressMessage} ({GetStatusDescriptionProgressCount(count, TotalRecords.Value)})";

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
                    if (!PipelineUpstreamContainsBlockingCmdlet)
                    {
                        var tuple = GetPreviousProgressManagerForOperation(previousCmdlet);

                        total = tuple.Item1;
                        processed = tuple.Item2;

                        var multiPassThruCmdlet = CacheManager.GetPreviousPrtgCmdlet() as IPrtgMultiPassThruCmdlet;

                        if (multiPassThruCmdlet != null && multiPassThruCmdlet.PassThru && multiPassThruCmdlet.CurrentMultiPassThru != null)
                        {
                            processed = multiPassThruCmdlet.PassThruObjects.IndexOf(multiPassThruCmdlet.CurrentMultiPassThru) + 1;
                        }
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

                PreviousRecord.StatusDescription = $"{progressMessage} ({GetStatusDescriptionProgressCount(processed, total.Value)})";
                PreviousRecord.PercentComplete = GetPercentComplete(processed, total.Value);

                PreviousRecord.CmdletOwnsRecord = false;

                WriteProgress(PreviousRecord);

                SkipCurrentRecord();

                CurrentRecord.ProgressWritten = true;
            }
            else
            {
                if (PipeFromSingleVariable)
                    cmdlet.ProgressManagerEx.PipeFromSingleVariable = true;
            }
        }

        private Tuple<int, int> GetPreviousProgressManagerForOperation(PrtgCmdlet previousNonOperationCmdlet)
        {
            var previousCmdlet = CacheManager.GetPreviousPrtgCmdlet();

            var previousManager = previousCmdlet.ProgressManager;

            if (previousManager.TotalRecords != null)
            {
                //The previous cmdlet (whether it be our original non-operation cmdlet or an operation cmdlet
                //contains records, so we'll return his progress manager
                var total = previousManager.TotalRecords;
                var processed = previousManager.RecordsProcessed;

                //The previous manager is just holding onto the TotalRecords for bookkeeping; the records processed are
                //actually stored on the cmdlet before that
                if (previousManager.RecordsProcessed == -1)
                    processed = previousNonOperationCmdlet.ProgressManager.RecordsProcessed;

                var tuple = Tuple.Create(total.Value, processed);

                return tuple;
            }

            //The previous cmdlet's TotalRecords were useless; lets stick with analyzing the previous non-pperation cmdlet
            previousManager = previousNonOperationCmdlet.ProgressManager;

            if (previousManager.TotalRecords != null)
            {
                return Tuple.Create(previousManager.TotalRecords.Value, previousManager.RecordsProcessed);
            }

            //Neither of our previous cmdlets have TotalRecords. This is not allowed
            throw new InvalidOperationException("Cannot display records procesed as previous TotalRecords is not initialized.");
        }

        private void SkipCurrentRecord()
        {
            CloneRecord(PreviousRecord, CurrentRecord);
        }

        internal int GetPercentComplete(int count, int total)
        {
            var percentComplete = (int)(count / Convert.ToDouble(total) * 100);

            if (WatchStream)
                percentComplete = percentComplete % 100;

            return percentComplete;
        }

        private string GetStatusDescriptionProgressCount(int index, int maxCount)
        {
            var totalStr = maxCount.ToString();

            if (WatchStream)
                totalStr = "∞";

            return $"{index}/{totalStr}";
        }

        public static ProgressRecordEx CloneRecord(ProgressRecordEx progressRecord)
        {
            var record = new ProgressRecordEx(progressRecord.ActivityId, progressRecord.Activity, progressRecord.StatusDescription, progressRecord.SourceId)
            {
                CurrentOperation = progressRecord.CurrentOperation,
                ParentActivityId = progressRecord.ParentActivityId,
                PercentComplete = progressRecord.PercentComplete,
                RecordType = progressRecord.RecordType,
                SecondsRemaining = progressRecord.SecondsRemaining,
                State = progressRecord.State
            };

            return record;
        }

        public static void CloneRecord(ProgressRecordEx sourceRecord, ProgressRecordEx destinationRecord)
        {
            destinationRecord.GetType().BaseType.GetInternalFieldInfo("id").SetValue(destinationRecord, sourceRecord.ActivityId);
            destinationRecord.Activity = sourceRecord.Activity;
            destinationRecord.StatusDescription = sourceRecord.StatusDescription;
            destinationRecord.CurrentOperation = sourceRecord.CurrentOperation;
            destinationRecord.ParentActivityId = sourceRecord.ParentActivityId;
            destinationRecord.PercentComplete = sourceRecord.PercentComplete;
            destinationRecord.RecordType = sourceRecord.RecordType;
            destinationRecord.SecondsRemaining = sourceRecord.SecondsRemaining;
            destinationRecord.SourceId = sourceRecord.SourceId;
            destinationRecord.State = sourceRecord.State;
        }

        internal static bool IsPureThirdPartyCmdlet(Type cmdlet)
        {
            if (cmdlet == typeof(WhereObjectCommand))
                return true;

            if (SelectObjectDescriptor.TypeIsSelectObjectCommand(cmdlet))
                return true;

            return false;
        }
    }
}
