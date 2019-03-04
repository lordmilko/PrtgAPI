using System;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.Parameters;
using PrtgAPI.PowerShell.Base;
using PrtgAPI.Attributes;
using PrtgAPI.Reflection;
using PrtgAPI.Utilities;

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
    ///     <code>C:\> Get-Sensor -Id 1044 | Get-Trigger | Edit-TriggerProperty OnNotificationAction $null</code>
    ///     <para>Remove the OnNotificationAction of all triggers defined on the object with ID 1044</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Notification-Triggers#modify-1">Online version:</para>
    /// <para type="link">Get-NotificationTrigger</para>
    /// <para type="link">Set-NotificationTrigger</para>
    /// <para type="link">New-NotificationTriggerParameters</para> 
    /// </summary>
    [Cmdlet(VerbsData.Edit, "NotificationTriggerProperty", SupportsShouldProcess = true)]
    public class EditNotificationTriggerProperty : PrtgPassThruCmdlet
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
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            Value = PSObjectUtilities.CleanPSObject(Value);

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
            var property = parameters.GetTypeCache().Properties.FirstOrDefault(p => p.GetAttribute<PropertyParameterAttribute>()?.Property.Equals(Property) == true);

            if (property == null)
                throw new InvalidOperationException($"Property '{Property}' does not exist on triggers of type '{parameters.Type}'");

            Value = ParseValueIfRequired(property.Property, Value);

            property.Property.SetValue(parameters, Value);

            if (ShouldProcess($"{Trigger.OnNotificationAction} (Object ID: {Trigger.ObjectId})", $"Edit-NotificationTriggerProperty {Property} = '{Value}'"))
            {
                ExecuteOperation(() => client.SetNotificationTrigger(parameters), $"Setting trigger property {Property} to value '{Value}'");
            }
        }

        internal override string ProgressActivity => "Edit Notification Triggers";

        /// <summary>
        /// Returns the current object that should be passed through this cmdlet.
        /// </summary>
        public override object PassThruObject => Trigger;
    }
}
