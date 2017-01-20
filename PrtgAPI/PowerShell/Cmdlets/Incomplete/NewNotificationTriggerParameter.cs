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
        [Parameter(ParameterSetName = "AddFrom", Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "The ID of the object the notification trigger will be created for.")]
        public int Id { get; set; }

        /// <summary>
        /// The notification trigger import trigger parameters from.
        /// </summary>
        [Parameter(ParameterSetName = "AddFrom", Mandatory = true)]
        public NotificationTrigger Source { get; set; }

        /// <summary>
        /// The Sub ID of the trigger to manipulate.
        /// </summary>
        [Parameter(ParameterSetName = "Edit", Mandatory = true, Position = 1)]
        public int? TriggerId { get; set; }

        /// <summary>
        /// The type of notification trigger to manipulate.
        /// </summary>
        [Parameter(ParameterSetName = "Add", Mandatory = true, Position = 1)]
        [Parameter(ParameterSetName = "Edit", Mandatory = true, Position = 2)]
        public TriggerType? Type { get; set; }

        /// <summary>
        /// Provides a record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            var action = ParameterSetName == "Edit" ? ModifyAction.Edit : ModifyAction.Add;

            if (Source != null)
                Type = Source.Type;

            switch (Type)
            {
                case TriggerType.Change:
                    WriteObject(CreateParameters<ChangeTriggerParameters>());
                    break;
                case TriggerType.Speed:
                    WriteObject(CreateParameters<SpeedTriggerParameters>());
                    break;
                case TriggerType.State:
                    WriteObject(CreateParameters<StateTriggerParameters>());
                    break;
                case TriggerType.Threshold:
                    WriteObject(CreateParameters<ThresholdTriggerParameters>());
                    break;
                case TriggerType.Volume:
                    WriteObject(CreateParameters<VolumeTriggerParameters>());
                    break;
                default:
                    throw new NotImplementedException($"Handler of trigger type '{Type}' is not implemented.");
            }



            //todo: have some tests that confirm when we create
            //a parameter from a trigger, ALL its properties have values that match
            //the properties of the input trigger

            //todo before making this public, we need to check that notificationaction's getserializedformat method does get called
        }

        private T CreateParameters<T>()
        {
            if (Source != null)
                return (T) Activator.CreateInstance(typeof (T), Id, Source); //Use an existing notification trigger
            if (TriggerId != null)
                return (T) Activator.CreateInstance(typeof (T), Id, TriggerId); //Edit a notification trigger
            else
                return (T) Activator.CreateInstance(typeof (T), Id); //Create a new notification trigger
        }
    }
}
