using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using PrtgAPI.Parameters;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// Create a new set of notification trigger parameters for adding or editing a notification trigger.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "NotificationTriggerParameter")]
    public class NewNotificationTriggerParameter : PSCmdlet
    {
        /// <summary>
        /// The ID of the object the notification trigger will be created for.
        /// </summary>
        [Parameter(ParameterSetName = "Add", Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "The ID of the object the notification trigger will be created for.")]
        [Parameter(ParameterSetName = "Edit", Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "The ID of the object the notification trigger will be created for.")]
        [Parameter(ParameterSetName = "From", Mandatory = false, Position = 0, HelpMessage = "The ID of the object the notification trigger will be created for.")]
        public int? Id { get; set; }

        /// <summary>
        /// The notification trigger import trigger parameters from.
        /// </summary>
        [Parameter(ParameterSetName = "From", Mandatory = true, ValueFromPipeline = true, HelpMessage = "The notification trigger whose properties will be used as the basis of creating a new trigger.")]
        public NotificationTrigger Source { get; set; }

        /// <summary>
        /// The Sub ID of the trigger to manipulate.
        /// </summary>
        [Parameter(ParameterSetName = "Edit", Mandatory = true, Position = 1, HelpMessage = "The sub ID of the notification trigger to edit.")]
        public int? TriggerId { get; set; }

        /// <summary>
        /// The type of notification trigger to manipulate.
        /// </summary>
        [Parameter(ParameterSetName = "Add", Mandatory = true, Position = 1, HelpMessage = "The type of notification trigger to create.")]
        [Parameter(ParameterSetName = "Edit", Mandatory = true, Position = 2, HelpMessage = "The type of notification trigger to edit.")]
        public TriggerType? Type { get; set; }

        /// <summary>
        /// Provides a record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            ModifyAction? action = null;

            //we need to write tests for all the different valid and invalid argument scenarios and try and break it

            switch (ParameterSetName)
            {
                case "Add":
                    action = ModifyAction.Add;
                    break;
                case "Edit":
                    action = ModifyAction.Edit;
                    break;
                case "From":
                    action = Id != null ? ModifyAction.Add : ModifyAction.Edit;
                    break;
                default:
                    throw new NotImplementedException();
            }

            //need to update the trigger constructor to allow edit mode. also need to update readme
            //write some unit/integration tests. the unit tests will just

            //var action = ParameterSetName == "Edit" ? ModifyAction.Edit : ModifyAction.Add;

            if (Source != null)
                Type = Source.Type;

            switch (Type)
            {
                case TriggerType.Change:
                    WriteObject(CreateParameters<ChangeTriggerParameters>(action.Value));
                    break;
                case TriggerType.Speed:
                    WriteObject(CreateParameters<SpeedTriggerParameters>(action.Value));
                    break;
                case TriggerType.State:
                    WriteObject(CreateParameters<StateTriggerParameters>(action.Value));
                    break;
                case TriggerType.Threshold:
                    WriteObject(CreateParameters<ThresholdTriggerParameters>(action.Value));
                    break;
                case TriggerType.Volume:
                    WriteObject(CreateParameters<VolumeTriggerParameters>(action.Value));
                    break;
                default:
                    throw new NotImplementedException($"Handler of trigger type '{Type}' is not implemented.");
            }
        }

        private T CreateParameters<T>(ModifyAction action)
        {
            if (Source != null)
                return (T) Activator.CreateInstance(typeof (T), Id, Source, action); //Use an existing notification trigger
            if (TriggerId != null)
                return (T) Activator.CreateInstance(typeof (T), Id, TriggerId); //Edit a notification trigger
            else
                return (T) Activator.CreateInstance(typeof (T), Id); //Create a new notification trigger
        }
    }
}
