using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrtgAPI.Attributes;
using PrtgAPI.Helpers;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="PrtgUrl"/> for adding/modifying <see cref="TriggerType.Speed"/> <see cref="NotificationTrigger"/> objects.
    /// </summary>
    public class SpeedTriggerParameters : TriggerParameters
    {
        /// <summary>
        /// The <see cref="NotificationAction"/> to execute when the trigger's active state clears.
        /// </summary>
        [RequireValue(false)]
        [PropertyParameter(nameof(TriggerProperty.OffNotificationAction))]
        public NotificationAction OffNotificationAction
        {
            get { return GetNotificationAction(TriggerProperty.OffNotificationAction); }
            set { SetNotificationAction(TriggerProperty.OffNotificationAction, value); }
        }

        /// <summary>
        /// The channel of the sensor this trigger should apply to.
        /// </summary>
        [RequireValue(true)]
        [PropertyParameter(nameof(TriggerProperty.Channel))]
        public TriggerChannel? Channel
        {
            get { return (TriggerChannel?) GetCustomParameterEnumXml<TriggerChannel>(TriggerProperty.Channel); }
            set { UpdateCustomParameter(TriggerProperty.Channel, value?.EnumToXml(), true); }
        }

        /// <summary>
        /// The delay (in seconds) this trigger should wait before executing its <see cref="TriggerParameters.OnNotificationAction"/> once activated.
        /// </summary>
        [PropertyParameter(nameof(TriggerProperty.Latency))]
        public int? Latency
        {
            get { return (int?) GetCustomParameterValue(TriggerProperty.Latency); }
            set { UpdateCustomParameter(TriggerProperty.Latency, value); }
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
        /// The value which, once reached, will cause this trigger will activate. Used in conjunction with <see cref="Condition"/>.
        /// </summary>
        [PropertyParameter(nameof(TriggerProperty.Threshold))]
        public int? Threshold
        {
            get { return (int?) GetCustomParameterValue(TriggerProperty.Threshold); }
            set { UpdateCustomParameter(TriggerProperty.Threshold, value); }
        }

        /// <summary>
        /// The time component of the data rate that causes this trigger to activate.
        /// </summary>
        [RequireValue(true)]
        [PropertyParameter(nameof(TriggerProperty.UnitTime))]
        public TriggerUnitTime? UnitTime
        {
            get { return (TriggerUnitTime?) GetCustomParameterEnumXml<TriggerUnitTime>(TriggerProperty.UnitTime); }
            set { UpdateCustomParameter(TriggerProperty.UnitTime, value?.EnumToXml(), true); }
        }

        /// <summary>
        /// The unit component of the data rate that causes this trigger to activate.
        /// </summary>
        [RequireValue(true)]
        [PropertyParameter(nameof(TriggerProperty.UnitSize))]
        public TriggerUnitSize? UnitSize
        {
            get { return (TriggerUnitSize?) GetCustomParameterEnumXml<TriggerUnitSize>(TriggerProperty.UnitSize); }
            set { UpdateCustomParameter(TriggerProperty.UnitSize, value?.EnumToXml(), true); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpeedTriggerParameters"/> class for creating a new notification trigger.
        /// </summary>
        /// <param name="objectId">The object ID the trigger will apply to.</param>
        public SpeedTriggerParameters(int objectId) : base(TriggerType.Speed, objectId, null, ModifyAction.Add)
        {
            OffNotificationAction = null;
            Channel = TriggerChannel.Primary;
            Condition = TriggerCondition.Above;
            UnitSize = TriggerUnitSize.Byte;
            UnitTime = TriggerUnitTime.Hour;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpeedTriggerParameters"/> class for editing an existing notification trigger.
        /// </summary>
        /// <param name="objectId">The object ID the trigger is applied to.</param>
        /// <param name="triggerId">The sub ID of the trigger on its parent object.</param>
        public SpeedTriggerParameters(int objectId, int triggerId) : base(TriggerType.Speed, objectId, triggerId, ModifyAction.Edit)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpeedTriggerParameters"/> class for creating a new trigger from an existing <see cref="TriggerType.Speed"/> <see cref="NotificationTrigger"/>.
        /// </summary>
        /// <param name="objectId">The object ID the trigger will apply to.</param>
        /// <param name="sourceTrigger">The notification trigger whose properties should be used.</param>
        public SpeedTriggerParameters(int objectId, NotificationTrigger sourceTrigger) : base(TriggerType.Speed, objectId, sourceTrigger)
        {
            OffNotificationAction = sourceTrigger.OffNotificationAction;
            Channel = sourceTrigger.Channel;
            Latency = sourceTrigger.Latency;
            Condition = sourceTrigger.Condition;
            Threshold = sourceTrigger.ThresholdInternal;
            UnitTime = sourceTrigger.UnitTime;
            UnitSize = sourceTrigger.UnitSize;
        }
    }
}
