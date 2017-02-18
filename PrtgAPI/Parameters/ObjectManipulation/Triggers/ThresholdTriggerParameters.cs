using System;
using PrtgAPI.Attributes;
using PrtgAPI.Helpers;

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
        [PropertyParameter(nameof(TriggerProperty.OffNotificationAction))]
        public NotificationAction OffNotificationAction
        {
            get { return GetNotificationAction(TriggerProperty.OffNotificationAction); }
            set { SetNotificationAction(TriggerProperty.OffNotificationAction, value); }
        }

        /// <summary>
        /// The delay (in seconds) this trigger should wait before executing its <see cref="TriggerParameters.OnNotificationAction"/> once activated.
        /// </summary>
        [PropertyParameter(nameof(TriggerProperty.Latency))]
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
        [PropertyParameter(nameof(TriggerProperty.Threshold))]
        public int? Threshold
        {
            get { return (int?)GetCustomParameterValue(TriggerProperty.Threshold); }
            set { UpdateCustomParameter(TriggerProperty.Threshold, value); }
        }

        /// <summary>
        /// The condition that controls when the <see cref="Threshold"/> is activated.
        /// </summary>
        [RequireValue(true)]
        [PropertyParameter(nameof(TriggerProperty.Condition))]
        public TriggerCondition? Condition
        {
            get { return (TriggerCondition?) GetCustomParameterEnumInt<TriggerCondition>(TriggerProperty.Condition); }
            set { UpdateCustomParameter(TriggerProperty.Condition, (int?) value, true); }
        }

        /// <summary>
        /// The channel of the sensor this trigger should apply to.
        /// </summary>
        [RequireValue(true)]
        [PropertyParameter(nameof(TriggerProperty.Channel))]
        public TriggerChannel? Channel
        {
            get { return (TriggerChannel?) GetCustomParameterEnumXml<TriggerChannel>(TriggerProperty.Channel); }
            set
            {
                if(value != null && value != TriggerChannel.Primary && value != TriggerChannel.Total)
                    throw new InvalidOperationException($"Only '{nameof(TriggerChannel.Primary)}' and '{nameof(TriggerChannel.Total)}' channels are valid for threshold triggers");

                UpdateCustomParameter(TriggerProperty.Channel, value?.EnumToXml(), true);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThresholdTriggerParameters"/> class for creating a new notification trigger.
        /// </summary>
        /// <param name="objectId">The object ID the trigger will apply to.</param>
        public ThresholdTriggerParameters(int objectId) : base(TriggerType.Threshold, objectId, (int?)null, ModifyAction.Add)
        {
            OffNotificationAction = null;
            Channel = TriggerChannel.Primary;
            Condition = TriggerCondition.Above;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThresholdTriggerParameters"/> class for editing an existing notification trigger.
        /// </summary>
        /// <param name="objectId">The object ID the trigger is applied to.</param>
        /// <param name="triggerId">The sub ID of the trigger on its parent object.</param>
        public ThresholdTriggerParameters(int objectId, int triggerId) : base(TriggerType.Threshold, objectId, triggerId, ModifyAction.Edit)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThresholdTriggerParameters"/> class for creating a new trigger from an existing <see cref="TriggerType.Threshold"/> <see cref="NotificationTrigger"/>.
        /// </summary>
        /// <param name="objectId">The object ID the trigger will apply to.</param>
        /// <param name="sourceTrigger">The notification trigger whose properties should be used.</param>
        /// <param name="action">Whether these parameters will create a new trigger or edit an existing one.</param>
        public ThresholdTriggerParameters(int objectId, NotificationTrigger sourceTrigger, ModifyAction action) : base(TriggerType.Threshold, objectId, sourceTrigger, action)
        {
            if (action == ModifyAction.Add)
            {
                OffNotificationAction = sourceTrigger.OffNotificationAction;
                Latency = sourceTrigger.Latency;
                Threshold = sourceTrigger.ThresholdInternal;
                Condition = sourceTrigger.Condition;
                Channel = sourceTrigger.Channel;
            }
        }
    }
}
