using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using PrtgAPI.Parameters;
using PrtgAPI.PowerShell.Base;
using System.Reflection;
using PrtgAPI.Attributes;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// Edit a notification trigger on a PRTG Server.
    /// </summary>
    [Cmdlet(VerbsData.Edit, "NotificationTriggerProperty", SupportsShouldProcess = true)]
    public class EditNotificationTriggerProperty : PrtgCmdlet
    {
        /// <summary>
        /// Notification Trigger to edit.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public NotificationTrigger Trigger { get; set; }

        /// <summary>
        /// The property to modify.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public TriggerProperty Property { get; set; }

        /// <summary>
        /// Value to set the property to.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1)]
        public object Value { get; set; }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (Value is PSObject)
                Value = ((PSObject) Value).BaseObject;

            switch (Trigger.Type)
            {
                case TriggerType.Change:
                    SetProperty(new ChangeTriggerParameters(Trigger.ObjectId, Trigger.SubId));
                    break;
                case TriggerType.Speed:
                    SetProperty(new SpeedTriggerParameters(Trigger.ObjectId, Trigger.SubId));
                    break;
                case TriggerType.State:
                    SetProperty(new StateTriggerParameters(Trigger.ObjectId, Trigger.SubId));
                    break;
                case TriggerType.Threshold:
                    SetProperty(new ThresholdTriggerParameters(Trigger.ObjectId, Trigger.SubId));
                    break;
                case TriggerType.Volume:
                    SetProperty(new VolumeTriggerParameters(Trigger.ObjectId, Trigger.SubId));
                    break;
                default:
                    throw new NotImplementedException($"Handler of trigger type '{Trigger.Type}' is not implemented.");
            }
        }

        private void SetProperty(TriggerParameters parameters)
        {
            var property = parameters.GetType().GetProperties().First(p => p.GetCustomAttribute<PropertyParameterAttribute>()?.Name == Property.ToString());

            property.SetValue(parameters, Value);

            if(ShouldProcess($"{Trigger.OnNotificationAction} (Object ID: {Trigger.ObjectId})", $"Edit-NotificationTriggerProperty {Property} = '{Value}'"))
                client.SetNotificationTrigger(parameters);
        }
    }
}
