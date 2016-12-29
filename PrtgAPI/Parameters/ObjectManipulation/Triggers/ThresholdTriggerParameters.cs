using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="PrtgUrl"/> for adding/modifying <see cref="TriggerType.Threshold"/> <see cref="NotificationTrigger"/> objects.
    /// </summary>
    public class ThresholdTriggerParameters : TriggerParameters
    {
        /// <summary>
        /// The <see cref="NotificationAction"/> to execute when the trigger's active state clears.
        /// </summary>
        public NotificationAction OffNotificationAction
        {
            get { return GetNotificationAction(TriggerProperty.OffNotificationAction); }
            set { SetNotificationAction(TriggerProperty.OffNotificationAction, value); }
        }

        /// <summary>
        /// The delay (in seconds) this trigger should wait before executing its <see cref="TriggerParameters.OnNotificationAction"/> once activated.
        /// </summary>
        public int? Latency
        {
            get { return (int?)GetCustomParameterValue(TriggerProperty.Latency); }
            set
            {
                if (value != null)
                    UpdateCustomParameter(TriggerProperty.Latency, value);
            }
        }

        /// <summary>
        /// The value which, once reached, will cause this trigger will activate. Used in conjunction with <see cref="Condition"/>.
        /// </summary>
        public int? Threshold
        {
            get { return (int?)GetCustomParameterValue(TriggerProperty.Threshold); }
            set
            {
                if (value != null)
                    UpdateCustomParameter(TriggerProperty.Threshold, value);
            }
        }

        /// <summary>
        /// The condition that controls when the <see cref="Threshold"/> is activated.
        /// </summary>
        public TriggerCondition? Condition
        {
            get { return (TriggerCondition?)GetCustomParameterValue(TriggerProperty.Condition); }
            set
            {
                if (value != null)
                    UpdateCustomParameter(TriggerProperty.Condition, value);
            }
        }

        /// <summary>
        /// The channel of the sensor this trigger should apply to.
        /// </summary>
        public TriggerChannel? Channel
        {
            get { return (TriggerChannel?)GetCustomParameterValue(TriggerProperty.Channel); }
            set
            {
                if (value != null)
                    UpdateCustomParameter(TriggerProperty.Channel, value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThresholdTriggerParameters"/> class.
        /// </summary>
        /// <param name="objectId">The object ID the trigger will apply to.</param>
        /// <param name="triggerId">If this trigger is being edited, the trigger's sub ID. If this trigger is being added, this value is null.</param>
        /// <param name="action">Whether to add a new trigger or modify an existing one.</param>
        public ThresholdTriggerParameters(int objectId, int? triggerId, ModifyAction action) : base(TriggerType.Volume, objectId, triggerId, action)
        {
            if (action == ModifyAction.Add)
                OffNotificationAction = null;
        }
    }
}
