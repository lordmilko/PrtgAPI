using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using PrtgAPI.PowerShell.Progress;

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
        protected string TypeDescription;

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
        protected void WriteObjectWithProgress(object obj)
        {
            if (ProgressManager.PipeFromVariableWithProgress && PrtgSessionState.EnableProgress)
                UpdatePreviousAndCurrentVariableProgressOperations();
            else if (ProgressManager.PartOfChain && PrtgSessionState.EnableProgress)
            {
                if (ProgressManager.PreviousContainsProgress)
                {
                    ProgressManager.SetPreviousOperation($"Retrieving all {GetTypePlural()}");
                }
            }

            WriteObject(obj);

            PostUpdateProgress();
        }

        /// <summary>
        /// Updates the previous and current progress records for the current input object for scenarios in which a variable was piped into one or more cmdlets.
        /// </summary>
        protected void UpdatePreviousAndCurrentVariableProgressOperations()
        {
            //PrtgOperationCmdlets use TrySetPreviousOperation, so they won't be caught by the fact PipelineContainsOperation would be true for them
            //(causing them to SetPreviousOperation, which would fail)
            if (!ProgressManager.PipelineContainsOperation)
            {
                if (ProgressManager.PipelineIsProgressPure)
                {
                    ProgressManager.CurrentRecord.Activity = $"PRTG {TypeDescription} Search";

                    ProgressManager.InitialDescription = $"Processing all {GetTypeDescription(ProgressManager.CmdletPipeline.List.First().GetType()).ToLower()}s";
                    ProgressManager.CurrentRecord.CurrentOperation = $"Retrieving all {GetTypePlural()}";

                    ProgressManager.RemovePreviousOperation();
                    ProgressManager.UpdateRecordsProcessed(ProgressManager.CurrentRecord);
                }
            }
            else
                ProgressManager.SetPreviousOperation($"Retrieving all {GetTypePlural()}");
        }

        /// <summary>
        /// Updates progress before any of the objects in the current cmdlet are written to the pipeline.
        /// </summary>
        protected void PreUpdateProgress()
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

        /// <summary>
        /// Updates progress as each object in the current cmdlet is written to the pipeline.
        /// </summary>
        protected void DuringUpdateProgress()
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

        /// <summary>
        /// Updates progress after all objects in the current cmdlet have been written to the pipeline.
        /// </summary>
        protected void PostUpdateProgress()
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
                        SetObjectSearchProgress(ProcessingOperation.Processing);

                    if (ProgressManager.ContainsProgress)
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
        /// Set the progress activity, initial description and total number of records (where applicable) for the current cmdlet.
        /// </summary>
        /// <param name="operation">The type of processing that is being performed by this cmdlet.</param>
        protected void SetObjectSearchProgress(ProcessingOperation operation)
        {
            ProgressManager.CurrentRecord.Activity = $"PRTG {TypeDescription} Search";

            if (operation == ProcessingOperation.Processing)
                ProgressManager.InitialDescription = $"Processing {TypeDescription.ToLower()}";
            else
                ProgressManager.InitialDescription = $"Retrieving all {GetTypePlural()}";
        }

        /// <summary>
        /// Retrieves the value of a <see cref="DescriptionAttribute"/> of the specified type. If the type does not have a <see cref="DescriptionAttribute"/>, its name is used instead.
        /// </summary>
        /// <param name="type">The type whose description should be retrieved.</param>
        /// <returns>The type's name or description.</returns>
        protected static string GetTypeDescription(Type type)
        {
            var attribute = type.GetCustomAttribute<DescriptionAttribute>();

            if (attribute != null)
                return attribute.Description;

            return type.Name;
        }

        private string GetTypePlural()
        {
            var str = TypeDescription.ToLower();

            if (str.EndsWith("ies"))
                return str;

            return $"{str}s";
        }
    }
}
