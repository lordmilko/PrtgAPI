using System;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.Parameters;
using PrtgAPI.PowerShell.Base;
using System.Reflection;
using PrtgAPI.Attributes;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Edits a notification trigger on a PRTG Server.</para>
    /// 
    /// <para type="description">The Edit-NotificationTriggerProperty cmdlet allows a single notification trigger property to be modified.
    /// Typically when you wish to modify the properties of an existing notification trigger, a TriggerParameters object must be constructed
    /// and then passed to the Set-NotificationTrigger cmdlet. Edit-NotificationTriggerProperty simplifies the common case of only wanting to
    /// modify a single property of the trigger.</para>
    /// 
    /// <example>
    ///     <code>Get-Sensor -Id 1044 | Get-Trigger | Edit-TriggerProperty OnNotificationAction $null</code>
    ///     <para>Remove the OnNotificationAction of all triggers defined on the object with ID 1044</para>
    /// </example>
    /// 
    /// <para type="link">Get-NotificationTrigger</para>
    /// <para type="link">Set-NotificationTrigger</para>
    /// <para type="link">New-NotificationTriggerParameter</para> 
    /// </summary>
    [Cmdlet(VerbsData.Edit, "NotificationTriggerProperty", SupportsShouldProcess = true)]
    public class EditNotificationTriggerProperty : PrtgOperationCmdlet
    {
        /// <summary>
        /// <para type="description">Notification Trigger to edit.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public NotificationTrigger Trigger { get; set; }

        /// <summary>
        /// <para type="description">The property to modify.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public TriggerProperty Property { get; set; }

        /// <summary>
        /// <para type="description">Value to set the property to.</para>
        /// </summary>
        [Parameter(Mandatory = false, Position = 1)]
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
                    SetProperty(new ChangeTriggerParameters(Trigger));
                    break;
                case TriggerType.Speed:
                    SetProperty(new SpeedTriggerParameters(Trigger));
                    break;
                case TriggerType.State:
                    SetProperty(new StateTriggerParameters(Trigger));
                    break;
                case TriggerType.Threshold:
                    SetProperty(new ThresholdTriggerParameters(Trigger));
                    break;
                case TriggerType.Volume:
                    SetProperty(new VolumeTriggerParameters(Trigger));
                    break;
                default:
                    throw new NotImplementedException($"Handler of trigger type '{Trigger.Type}' is not implemented.");
            }
        }

        private void SetProperty(TriggerParameters parameters)
        {
            //Get the TriggerParameters PropertyInfo that corresponds to the specified TriggerProperty
            var property = parameters.GetType().GetProperties().First(p => p.GetCustomAttribute<PropertyParameterAttribute>()?.Name == Property.ToString());

            Value = ParseValueIfRequired(property, Value);

            //how are we going to handle setting the object property when we need to set the scanninginterval?

            property.SetValue(parameters, Value);

            if (ShouldProcess($"{Trigger.OnNotificationAction} (Object ID: {Trigger.ObjectId})", $"Edit-NotificationTriggerProperty {Property} = '{Value}'"))
            {
                ExecuteOperation(() => client.SetNotificationTrigger(parameters), "Edit Notification Triggers", $"Setting trigger property {Property} to value '{Value}'");
            }
        }
    }
}
