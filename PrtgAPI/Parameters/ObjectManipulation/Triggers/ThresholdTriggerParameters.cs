using System;
using PrtgAPI.Attributes;
using PrtgAPI.Request;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="PrtgUrl"/> for adding/modifying <see cref="TriggerType.Threshold"/> <see cref="NotificationTrigger"/> objects.
    /// </summary>
    public class ThresholdTriggerParameters : TriggerParameters
    {
        /// <summary>
        /// Gets or sets the <see cref="NotificationAction"/> to execute when the trigger's active state clears.
        /// </summary>
        [PropertyParameter(nameof(TriggerProperty.OffNotificationAction))]
        public NotificationAction OffNotificationAction
        {
            get { return GetNotificationAction(TriggerProperty.OffNotificationAction); }
            set { SetNotificationAction(TriggerProperty.OffNotificationAction, value); }
        }

        /// <summary>
        /// Gets or sets the delay (in seconds) this trigger should wait before executing its <see cref="TriggerParameters.OnNotificationAction"/> once activated.
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
        /// Gets or sets the value which, once reached, will cause this trigger will activate. Used in conjunction with <see cref="Condition"/>.
        /// </summary>
        [PropertyParameter(nameof(TriggerProperty.Threshold))]
        public int? Threshold
        {
            get { return (int?)GetCustomParameterValue(TriggerProperty.Threshold); }
            set { UpdateCustomParameter(TriggerProperty.Threshold, value); }
        }

        /// <summary>
        /// Gets or sets the condition that controls when the <see cref="Threshold"/> is activated.
        /// </summary>
        [RequireValue(true)]
        [PropertyParameter(nameof(TriggerProperty.Condition))]
        public TriggerCondition? Condition
        {
            get { return (TriggerCondition?) GetCustomParameterEnumInt<TriggerCondition>(TriggerProperty.Condition); }
            set { UpdateCustomParameter(TriggerProperty.Condition, (int?) value, true); }
        }

        /// <summary>
        /// Gets or sets the channel of the sensor this trigger should apply to.
        /// </summary>
        [RequireValue(true)]
        [PropertyParameter(nameof(TriggerProperty.Channel))]
        public TriggerChannel Channel
        {
            get { return TriggerChannel.ParseForRequest(GetCustomParameterValue(TriggerProperty.Channel)); }
            set
            {
                if(value == TriggerChannel.TrafficIn || value == TriggerChannel.TrafficOut)
                    throw new InvalidOperationException($"Only '{nameof(TriggerChannel.Primary)}', '{nameof(TriggerChannel.Total)}' and sensor specific channels are valid for threshold triggers");

                UpdateCustomParameter(TriggerProperty.Channel, value, true);
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
            Threshold = 0;
            Latency = 60;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThresholdTriggerParameters"/> class for editing an existing notification trigger.
        /// </summary>
        /// <param name="objectId">The object ID the trigger is applied to. Note: if the trigger is inherited, the ParentId should be specified.</param>
        /// <param name="triggerId">The sub ID of the trigger on its parent object.</param>
        public ThresholdTriggerParameters(int objectId, int triggerId) : base(TriggerType.Threshold, objectId, triggerId, ModifyAction.Edit)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThresholdTriggerParameters"/> class for creating a new trigger from an existing <see cref="TriggerType.Threshold"/> <see cref="NotificationTrigger"/>.
        /// </summary>
        /// <param name="objectId">The object ID the trigger will apply to.</param>
        /// <param name="sourceTrigger">The notification trigger whose properties should be used.</param>
        public ThresholdTriggerParameters(int objectId, NotificationTrigger sourceTrigger) : base(TriggerType.Threshold, objectId, sourceTrigger, ModifyAction.Add)
        {
            OffNotificationAction = sourceTrigger.OffNotificationAction;
            Latency = sourceTrigger.Latency;
            Threshold = sourceTrigger.ThresholdInternal;
            Condition = sourceTrigger.Condition;
            Channel = sourceTrigger.Channel;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThresholdTriggerParameters"/> class for editing an existing <see cref="TriggerType.Threshold"/> <see cref="NotificationTrigger"/>.
        /// </summary>
        /// <param name="sourceTrigger">The notification trigger to modify.</param>
        public ThresholdTriggerParameters(NotificationTrigger sourceTrigger) : base(TriggerType.Threshold, sourceTrigger.ObjectId, sourceTrigger, ModifyAction.Edit)
        {
        }
    }
}
