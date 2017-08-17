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

            ParseValueIfRequired(property);

            property.SetValue(parameters, Value);

            if (ShouldProcess($"{Trigger.OnNotificationAction} (Object ID: {Trigger.ObjectId})", $"Edit-NotificationTriggerProperty {Property} = '{Value}'"))
            {
                ExecuteOperation(() => client.SetNotificationTrigger(parameters), "Edit Notification Triggers", $"Setting trigger property {Property} to value '{Value}'");
            }
        }

        private void ParseValueIfRequired(PropertyInfo property)
        {
            //Types that can have possible enum values (such as TriggerChannel) possess a static Parse method for type conversion by the PowerShell runtime.
            //Only parse types that are defined in the PrtgAPI assembly.
            if (property.PropertyType.Assembly.FullName == GetType().Assembly.FullName)
            {
                var method = property.PropertyType.GetMethod("Parse", BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static);

                if (method != null)
                {
                    try
                    {
                        var newValue = method.Invoke(null, new[] { Value });
                        Value = newValue;
                    }
                    catch (Exception ex)
                    {
                        //Don't care if our value wasn't parsable
                    }
                }
            }
        }
    }
}
