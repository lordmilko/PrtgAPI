using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using PrtgAPI.PowerShell.Progress;

namespace PrtgAPI.PowerShell.Base
{
    /// <summary>
    /// Base class for all cmdlets that return a list of objects.
    /// </summary>
    /// <typeparam name="T">The type of objects that will be retrieved.</typeparam>
    public abstract class PrtgObjectCmdlet<T> : PrtgCmdlet
    {
        /// <summary>
        /// Retrieves all records of a specified type from a PRTG Server. Implementors can call different methods of a <see cref="PrtgClient"/> based on the type they wish to retrieve.
        /// </summary>
        /// <returns>A list of records relevant to the caller.</returns>
        protected abstract IEnumerable<T> GetRecords();

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            IEnumerable<T> records = null;

            if (ProgressManager.PipeFromVariable && PrtgSessionState.EnableProgress)
                records = GetResultsWithVariableProgress(GetRecords);
            else if (ProgressManager.PartOfChain && PrtgSessionState.EnableProgress)
                records = GetResultsWithProgress(GetRecords);
            else
                records = GetRecords();

            WriteList(records);
        }

        protected IEnumerable<T> GetResultsWithVariableProgress(Func<IEnumerable<T>> getResults)
        {
            //PrtgOperationCmdlets use TrySetPreviousOperation, so they won't be caught by the fact PipelineContainsOperation would be true for them
            //(causing them to SetPreviousOperation, which would fail)
            if (!ProgressManager.PipelineContainsOperation)
            {
                if(ProgressManager.PipelineIsPure)
                {
                    ProgressManager.CurrentRecord.Activity = $"PRTG {GetTypeDescription(typeof(T))} Search"; //moving this into the if statement caused the exception

                    ProgressManager.InitialDescription = $"Processing all {GetTypeDescription(ProgressManager.CmdletPipeline.List.First().GetType()).ToLower()}s";
                    ProgressManager.CurrentRecord.CurrentOperation = $"Retrieving all {GetTypeDescription(typeof(T)).ToLower()}s";

                    ProgressManager.RemovePreviousOperation();
                    ProgressManager.UpdateRecordsProcessed(ProgressManager.CurrentRecord);
                }
            }
            else
                ProgressManager.SetPreviousOperation($"Retrieving all {GetTypeDescription(typeof(T)).ToLower()}s");

            var records = getResults().ToList();

            ProgressManager.TotalRecords = records.Count;

            return records;
        }

        protected IEnumerable<T> GetResultsWithProgress(Func<IEnumerable<T>> getResults)
        {
            //May return the count of records we'll be retrieving if we're streaming (for example)
            int count = DisplayInitialProgress();

            var records = getResults();

            return UpdatePreviousProgressCount(records, count);
        }

        private int DisplayInitialProgress()
        {
            int count = -1;

            if (ProgressManager.FirstInChain)
            {
                //If DisplayFirstInChainMessage is overridden, count _may_ be set (e.g. if we are streaming records,
                //in which case we will get the total number of records we _will_ retrieve before returning)
                count = DisplayFirstInChainMessage();
            }
            else
            {
                if (ProgressManager.PreviousContainsProgress)
                {
                    ProgressManager.SetPreviousOperation($"Retrieving all {GetTypeDescription(typeof(T)).ToLower()}s");
                }
            }

            return count;
        }

        /// <summary>
        /// Retrieves the value of a <see cref="DescriptionAttribute"/> of the specified type. If the type does not have a <see cref="DescriptionAttribute"/>, its name is used instead.
        /// </summary>
        /// <param name="type">The type whose description should be retrieved.</param>
        /// <returns>The type's name or description.</returns>
        protected string GetTypeDescription(Type type)
        {
            var attribute = type.GetCustomAttribute<DescriptionAttribute>();

            if (attribute != null)
                return attribute.Description;

            return type.Name;
        }

        private IEnumerable<T> UpdatePreviousProgressCount(IEnumerable<T> records, int count) //todo: should we rename this method to make it more clear what it does?
        {
            records = GetCount(records, ref count);

            ProgressManager.TotalRecords = count;

            if (!ProgressManager.LastInChain)
            {
                SetObjectSearchProgress(ProcessingOperation.Processing, count);
            }

            return records;
        }

        protected virtual int DisplayFirstInChainMessage()
        {
            int count = -1;

            SetObjectSearchProgress(ProcessingOperation.Retrieving, null);

            ProgressManager.DisplayInitialProgress();

            return count;
        }

        protected virtual IEnumerable<T> GetCount(IEnumerable<T> records, ref int count)
        {
            var list = records.ToList();

            count = list.Count;

            return list.Select(s => s);
        }

        protected void SetObjectSearchProgress(ProcessingOperation operation, int? count)
        {
            ProgressManager.CurrentRecord.Activity = $"PRTG {GetTypeDescription(typeof(T))} Search";

            if (operation == ProcessingOperation.Processing)
                ProgressManager.InitialDescription = $"Processing {GetTypeDescription(typeof(T)).ToLower()}";
            else
                ProgressManager.InitialDescription = $"Retrieving all {GetTypeDescription(typeof(T)).ToLower()}s";

            if (count != null)
                ProgressManager.TotalRecords = count;
        }

        /// <summary>
        /// Writes a list to the output pipeline.
        /// </summary>
        /// <param name="sendToPipeline">The list that will be output to the pipeline.</param>
        internal void WriteList(IEnumerable<T> sendToPipeline)
        {
            PreUpdateProgress(sendToPipeline);

            foreach (var item in sendToPipeline)
            {
                DuringUpdateProgress();

                WriteObject(item);
            }

            PostUpdateProgress();
        }

        private void PreUpdateProgress(IEnumerable<T> sendToPipeline)
        {
            switch (ProgressManager.Scenario)
            {
                case ProgressScenario.NoProgress:
                    break;
                case ProgressScenario.StreamProgress:
                case ProgressScenario.MultipleCmdlets:
                    UpdateScenarioProgress_MultipleCmdlets(ProgressStage.PreLoop);
                    break;
                case ProgressScenario.VariableToSingleCmdlet:
                    break;
                case ProgressScenario.VariableToMultipleCmdlets:
                    UpdateScenarioProgress_VariableToMultipleCmdlet(ProgressStage.PreLoop);
                    break;
                default:
                    throw new NotImplementedException($"Handler for ProgressScenario '{ProgressManager.Scenario}' is not implemented");
            }
        }

        private void DuringUpdateProgress()
        {
            switch (ProgressManager.Scenario)
            {
                case ProgressScenario.NoProgress:
                    break;
                case ProgressScenario.StreamProgress:
                case ProgressScenario.MultipleCmdlets:
                    UpdateScenarioProgress_MultipleCmdlets(ProgressStage.BeforeEach);
                    break;
                case ProgressScenario.VariableToSingleCmdlet:
                    break;
                case ProgressScenario.VariableToMultipleCmdlets:
                    UpdateScenarioProgress_VariableToMultipleCmdlet(ProgressStage.BeforeEach);
                    break;
                default:
                    throw new NotImplementedException($"Handler for ProgressScenario '{ProgressManager.Scenario}' is not implemented");
            }
        }

        private void PostUpdateProgress()
        {
            switch (ProgressManager.Scenario)
            {
                case ProgressScenario.NoProgress:
                    break;
                case ProgressScenario.StreamProgress:
                case ProgressScenario.MultipleCmdlets:
                    UpdateScenarioProgress_MultipleCmdlets(ProgressStage.PostLoop);
                    break;
                case ProgressScenario.VariableToSingleCmdlet:
                    UpdateScenarioProgress_VariableToSingleCmdlet(ProgressStage.PostLoop);
                    break;
                case ProgressScenario.VariableToMultipleCmdlets:
                    UpdateScenarioProgress_VariableToMultipleCmdlet(ProgressStage.PostLoop);
                    break;

                default:
                    throw new NotImplementedException($"Handler for ProgressScenario '{ProgressManager.Scenario}' is not implemented");
            }
        }

        private void UpdateScenarioProgress_MultipleCmdlets(ProgressStage stage)
        {
            if (ProgressManager.ContainsProgress)
            {
                if (stage == ProgressStage.PreLoop)
                {
                    ProgressManager.RemovePreviousOperation();
                }
                else if (stage == ProgressStage.BeforeEach)
                {
                    ProgressManager.UpdateRecordsProcessed(ProgressManager.CurrentRecord);
                }
                else //PostLoop
                {
                    ProgressManager.CompleteProgress();
                }
            }
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
                if (ProgressManager.ContainsProgress)
                    ProgressManager.CompleteProgress();
            }
        }

        private void UpdateScenarioProgress_VariableToMultipleCmdlet(ProgressStage stage)
        {
            if (stage == ProgressStage.PreLoop)
            {
                if (ProgressManager.PipelineContainsOperation)
                {
                    if (!ProgressManager.LastInChain)
                        SetObjectSearchProgress(ProcessingOperation.Processing, ProgressManager.TotalRecords);

                    if(ProgressManager.ContainsProgress)
                        ProgressManager.RemovePreviousOperation();
                }
            }
            else if (stage == ProgressStage.BeforeEach)
            {
                if (ProgressManager.PipelineContainsOperation && ProgressManager.ContainsProgress)
                    ProgressManager.UpdateRecordsProcessed(ProgressManager.CurrentRecord);
            }
            else //PostLoop
            {
                UpdateScenarioProgress_VariableToSingleCmdlet(stage);
            }
        }

        /// <summary>
        /// Filter a response with a wildcard expression on a specified property.
        /// </summary>
        /// <param name="records">The records to filter.</param>
        /// <param name="pattern">The wildcard expression to filter with.</param>
        /// <param name="getProperty">A function that yields the property to filter on.</param>
        /// <returns>A list of records that match the specified filter.</returns>
        protected IEnumerable<T> FilterResponseRecords(IEnumerable<T> records, string pattern, Func<T, string> getProperty)
        {
            if (pattern != null)
            {
                var filter = new WildcardPattern(pattern.ToLower());
                records = records.Where(r => filter.IsMatch(getProperty(r).ToLower()));
            }

            return records;
        }
    }
}
