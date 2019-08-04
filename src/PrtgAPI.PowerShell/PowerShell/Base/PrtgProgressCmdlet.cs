using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using PrtgAPI.PowerShell.Progress;
using PrtgAPI.Utilities;

namespace PrtgAPI.PowerShell.Base
{
    /// <summary>
    /// Base class for all cmdlets that display progress while reading and writing to the pipeline.
    /// </summary>
    public abstract class PrtgProgressCmdlet : PrtgCmdlet
    {
        /// <summary>
        /// Description of the object type output by this cmdlet.
        /// </summary>
        internal string TypeDescription;

        internal string OperationTypeDescription;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrtgProgressCmdlet"/> class.
        /// </summary>
        /// <param name="typeDescription"></param>
        protected PrtgProgressCmdlet(string typeDescription)
        {
            TypeDescription = typeDescription;
        }

        /// <summary>
        /// Write a basic PSObject to the pipeline while displaying an appropriate progress message.
        /// </summary>
        /// <param name="obj">The object to write to the pipeline.</param>
        protected void WriteObjectWithProgress(Func<object> obj)
        {
            DisplayProgress();

            var result = obj();

            if (result != null)
                WriteObject(result, true);

            PostUpdateProgress();
        }

        internal void DisplayProgress(bool writeObject = true)
        {
            if (ProgressManager.GetRecordsWithVariableProgress)
                UpdatePreviousAndCurrentVariableProgressOperations(writeObject);
            else if (ProgressManager.GetResultsWithProgress)
            {
                if (ProgressManager.PreviousContainsProgress)
                {
                    ProgressManager.SetPreviousOperation($"Retrieving all {GetTypePlural()}");
                }
            }
        }

        internal void WriteProcessProgressRecords(Func<Func<int, string, bool>, object> getItems)
        {
            var items = getItems(DisplayProgress);

            if (items.IsIEnumerable())
            {
                ProgressManager.TotalRecords = items.ToIEnumerable().Count();

                foreach (var obj in (IEnumerable) items)
                {
                    UpdateRecordsProcessed();

                    WriteObject(obj);
                }
            }
            else
            {
                ProgressManager.TotalRecords = 1;

                UpdateRecordsProcessed();

                WriteObject(items);
            }

            CompleteDisplayProcessProgress();
        }

        private void UpdateRecordsProcessed()
        {
            if (!ProgressManager.NormalSeemsLikePipeFromVariable)
            {
                if (ProgressManager.RecordsProcessed < 0)
                    ProgressManager.RecordsProcessed++;

                ProgressManager.RecordsProcessed++;
            }
        }

        private bool DisplayProgress(int percentage, string operation)
        {
            DisplayProcessProgress(percentage, operation, false);

            return true;
        }

        private void DisplayProcessProgress(int percentage, string operation, bool complete = true)
        {
            if (!ProgressManager.ProgressEnabled)
                return;

            if (ProgressManager.PreviousRecord != null)
                ProgressManager.RemovePreviousOperation();

            if (ProgressManager.GetRecordsWithVariableProgress)
            {
                UpdatePreviousAndCurrentVariableProgressOperations(percentage == 100, $"{operation}");
            }
            else
            {
                ProgressManager.CurrentRecord.PercentComplete = percentage;

                ProgressManager.WriteProgress($"PRTG {TypeDescription} Search", $"{operation}");
            }

            if (percentage == 100)
            {
                if (complete)
                    CompleteDisplayProcessProgress();
            }
            else
            {
                //Null out the previous record's operation so it doesn't get removed on the next request
                if (ProgressManager.PreviousRecord?.CurrentOperation != null)
                    ProgressManager.PreviousRecord.CurrentOperation = null;
            }
        }

        private void CompleteDisplayProcessProgress()
        {
            if (!ProgressManager.ProgressEnabled)
                return;

            //If we're Variable -> Action -> Me, we wrote all our progress to the previous cmdlet's CurrentOperation,
            //so we don't need to complete anything
            if (!(ProgressManager.PipeFromVariableWithProgress && ProgressManager.PipelineContainsOperation))
            {
                if (ProgressManager.ReadyToComplete())
                    ProgressManager.CompleteProgress(true);

                if (!ProgressManager.GetRecordsWithVariableProgress)
                    ProgressManager.MaybeCompletePreviousProgress();
            }
        }

        /// <summary>
        /// Updates the previous and current progress records for the current input object for scenarios in which a variable was piped into one or more cmdlets.
        /// </summary>
        protected void UpdatePreviousAndCurrentVariableProgressOperations(bool writeObject = true, string currentOperation = null)
        {
            //PrtgOperationCmdlets use TrySetPreviousOperation, so they won't be caught by the fact PipelineContainsOperation would be true for them
            //(causing them to SetPreviousOperation, which would fail)
            if (ProgressManager.PipelineContainsOperation == false || ProgressManager.PreviousOperationDestroyed || ProgressManager.PipeFromPrtgCmdletPostProcessMode)
            {
                if (ProgressManager.PipelineIsProgressPure)
                {
                    ProgressManager.CurrentRecord.Activity = $"PRTG {TypeDescription} Search";

                    var obj = ProgressManager.CmdletPipeline.Current;
                    var prtgObj = obj as IObject;

                    if (prtgObj != null)
                        ProgressManager.InitialDescription = $"Processing {prtgObj.GetTypeDescription().ToLower()}";
                    else
                        ProgressManager.InitialDescription = $"Processing all {IObjectExtensions.GetTypeDescription(obj.GetType()).ToLower()}s";

                    ProgressManager.CurrentRecord.CurrentOperation = currentOperation ?? $"Retrieving all {GetTypePlural()}";

                    ProgressManager.RemovePreviousOperation();
                    ProgressManager.UpdateRecordsProcessed(ProgressManager.CurrentRecord, prtgObj, writeObject);
                }
            }
            else
                ProgressManager.SetPreviousOperation(currentOperation ?? $"Retrieving all {GetTypePlural()}");
        }

        /// <summary>
        /// Updates progress before any of the objects in the current cmdlet are written to the pipeline.
        /// </summary>
        protected void PreUpdateProgress()
        {
            if (!ProgressManager.ProgressEnabled)
                return;

            switch (GetSwitchScenario())
            {
                case ProgressScenario.NoProgress:
                    break;
                case ProgressScenario.StreamProgress:
                case ProgressScenario.MultipleCmdlets:
                    UpdateScenarioProgress_MultipleCmdlets(ProgressStage.PreLoop, null);
                    break;
                case ProgressScenario.VariableToSingleCmdlet:
                    break;
                case ProgressScenario.VariableToMultipleCmdlets:
                case ProgressScenario.MultipleCmdletsFromBlockingSelect:
                    UpdateScenarioProgress_VariableToMultipleCmdlet(ProgressStage.PreLoop, null);
                    break;
                default:
                    throw new NotImplementedException($"Handler for ProgressScenario '{ProgressManager.Scenario}' is not implemented.");
            }
        }

        /// <summary>
        /// Updates progress as each object in the current cmdlet is written to the pipeline.
        /// </summary>
        protected void DuringUpdateProgress(object obj)
        {
            if (!ProgressManager.ProgressEnabled)
                return;

            ProgressManager.CurrentState.Add(obj);
            ProgressManager.CurrentState.Current = obj;

            var prtgObj = obj as IObject;

            switch (GetSwitchScenario())
            {
                case ProgressScenario.NoProgress:
                    break;
                case ProgressScenario.StreamProgress:
                case ProgressScenario.MultipleCmdlets:
                    UpdateScenarioProgress_MultipleCmdlets(ProgressStage.BeforeEach, prtgObj);
                    break;
                case ProgressScenario.VariableToSingleCmdlet:
                    ProgressManager.CompletePrematurely(ProgressManager);
                    break;
                case ProgressScenario.VariableToMultipleCmdlets:
                case ProgressScenario.MultipleCmdletsFromBlockingSelect:
                    UpdateScenarioProgress_VariableToMultipleCmdlet(ProgressStage.BeforeEach, prtgObj);
                    break;
                case ProgressScenario.SelectLast:
                case ProgressScenario.SelectSkipLast:
                    if (ProgressManager.PartOfChain)
                        UpdateScenarioProgress_VariableToMultipleCmdlet(ProgressStage.BeforeEach, prtgObj);
                    break;
                default:
                    throw new NotImplementedException($"Handler for ProgressScenario '{ProgressManager.Scenario}' is not implemented.");
            }
        }

        /// <summary>
        /// Updates progress after all objects in the current cmdlet have been written to the pipeline.
        /// </summary>
        protected void PostUpdateProgress()
        {
            if (!ProgressManager.ProgressEnabled)
                return;

            switch (GetSwitchScenario())
            {
                case ProgressScenario.NoProgress:
                    break;
                case ProgressScenario.StreamProgress:
                case ProgressScenario.MultipleCmdlets:
                    UpdateScenarioProgress_MultipleCmdlets(ProgressStage.PostLoop, null);
                    break;
                case ProgressScenario.VariableToSingleCmdlet:
                    UpdateScenarioProgress_VariableToSingleCmdlet(ProgressStage.PostLoop);
                    break;
                case ProgressScenario.VariableToMultipleCmdlets:
                case ProgressScenario.MultipleCmdletsFromBlockingSelect:
                    UpdateScenarioProgress_VariableToMultipleCmdlet(ProgressStage.PostLoop, null);
                    break;
                default:
                    throw new NotImplementedException($"Handler for ProgressScenario '{ProgressManager.Scenario}' is not implemented.");
            }
        }

        private ProgressScenario GetSwitchScenario()
        {
            var scenario = ProgressManager.Scenario;

            if (scenario == ProgressScenario.SelectLast || scenario == ProgressScenario.SelectSkipLast)
            {
                scenario = ProgressManager.CalculateNonBlockingProgressScenario();

                if (scenario == ProgressScenario.NoProgress)
                    scenario = ProgressScenario.VariableToSingleCmdlet;
                else if (scenario == ProgressScenario.MultipleCmdlets) //We don't want MultipleCmdlets, because that makes us responsible for updating the records processed on each iteration
                    scenario = ProgressScenario.VariableToMultipleCmdlets;

                if (ProgressManager.Scenario == ProgressScenario.SelectSkipLast)
                {
                    //We can't trust PartOfChain as the cmdlets before us haven't been destroyed yet. So instead,
                    //the question is are there any cmdlets after us?

                    if (ProgressManager.LastInChain)
                        scenario = ProgressScenario.VariableToSingleCmdlet;
                }
            }

            return scenario;
        }

        private void UpdateScenarioProgress_MultipleCmdlets(ProgressStage stage, IObject obj)
        {
            if (ProgressManager.ExpectsContainsProgress)
            {
                if (stage == ProgressStage.PreLoop)
                {
                    ProgressManager.RemovePreviousOperation();
                }
                else if (stage == ProgressStage.BeforeEach)
                {
                    if (!ProgressManager.PipeFromPrtgCmdletPostProcessMode)
                        ProgressManager.UpdateRecordsProcessed(ProgressManager.CurrentRecord, obj);
                }
                else //PostLoop
                {
                    ProgressManager.CompleteProgress();
                }
            }

            if (stage == ProgressStage.PostLoop)
                ProgressManager.MaybeCompletePreviousProgress();
        }

        private void UpdateScenarioProgress_VariableToSingleCmdlet(ProgressStage stage)
        {
            if (stage == ProgressStage.PreLoop)
            {
            }
            else if (stage == ProgressStage.BeforeEach)
            {
            }
            else //PostLoop
            {
                if (ProgressManager.ExpectsContainsProgress)
                    ProgressManager.CompleteProgress();
            }
        }

        private void UpdateScenarioProgress_VariableToMultipleCmdlet(ProgressStage stage, IObject obj)
        {
            if (stage == ProgressStage.PreLoop)
            {
                if (ProgressManager.PipelineContainsOperation)
                {
                    if (!ProgressManager.LastInChain)
                        SetObjectSearchProgress(ProcessingOperation.Processing);

                    if (ProgressManager.ExpectsContainsProgress)
                        ProgressManager.RemovePreviousOperation();
                }
            }
            else if (stage == ProgressStage.BeforeEach)
            {
                //When a variable to cmdlet chain contains an operation, responsibility of updating the number of records processed
                //"resets", and we become responsible for updating our own count again. If that operation was a multi operation cmdlet
                //operating in both batch and paass through mode however, we're still acting as if we've piped from a variable
                if (ProgressManager.PipelineContainsOperation && ProgressManager.ExpectsContainsProgress && !ProgressManager.PipeFromPrtgCmdletPostProcessMode)
                    ProgressManager.UpdateRecordsProcessed(ProgressManager.CurrentRecord, obj);
            }
            else //PostLoop
            {
                UpdateScenarioProgress_VariableToSingleCmdlet(stage);
            }
        }

        internal void UpdatePreviousProgressAndSetCount(int count)
        {
            ProgressManager.TotalRecords = count;

            if (!ProgressManager.LastInChain)
            {
                SetObjectSearchProgress(ProcessingOperation.Processing);
            }
        }

        /// <summary>
        /// Set the progress activity, initial description and total number of records (where applicable) for the current cmdlet.
        /// </summary>
        /// <param name="operation">The type of processing that is being performed by this cmdlet.</param>
        internal void SetObjectSearchProgress(ProcessingOperation operation)
        {
            ProgressManager.CurrentRecord.Activity = $"PRTG {TypeDescription} {(ProgressManager.WatchStream ? "Watcher" : "Search")}";

            if (operation == ProcessingOperation.Processing)
                ProgressManager.InitialDescription = $"Processing {TypeDescription.ToLower()}";
            else
                ProgressManager.InitialDescription = $"Retrieving all {GetTypePlural()}";
        }

        /// <summary>
        /// Writes a list to the output pipeline.
        /// </summary>
        /// <param name="sendToPipeline">The list that will be output to the pipeline.</param>
        internal void WriteList<T>(IEnumerable<T> sendToPipeline)
        {
            PreUpdateProgress();

            foreach (var item in sendToPipeline)
            {
                DuringUpdateProgress(item);

                WriteObject(item);
            }

            PostUpdateProgress();
        }

        private string GetTypePlural()
        {
            var str = OperationTypeDescription?.ToLower() ?? TypeDescription.ToLower();

            if (str.EndsWith("ies"))
                return str;

            return $"{str}s";
        }

        /// <summary>
        /// Create a sequence of progress tasks for executing a process containing two or more operations.
        /// </summary>
        /// <typeparam name="TResult">The type of object returned by the first operation.</typeparam>
        /// <param name="func">The first operation to execute.</param>
        /// <param name="typeDescription">The type description use for the progress.</param>
        /// <param name="operationDescription">The progress description to use for the first operation.</param>
        /// <returns></returns>
        protected ProgressTask<TResult> First<TResult>(Func<List<TResult>> func, string typeDescription, string operationDescription)
        {
            return ProgressTask<TResult>.Create(func, this, typeDescription, operationDescription);
        }
    }
}
