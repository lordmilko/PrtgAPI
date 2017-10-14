using PrtgAPI.Attributes;
using PrtgAPI.Helpers;
using PrtgAPI.Request;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="PrtgUrl"/> for adding/modifying <see cref="TriggerType.Volume"/> <see cref="NotificationTrigger"/> objects.
    /// </summary>
    public class VolumeTriggerParameters : TriggerParameters
    {
        /// <summary>
        /// The channel of the sensor this trigger should apply to.
        /// </summary>
        [RequireValue(true)]
        [PropertyParameter(nameof(TriggerProperty.Channel))]
        public TriggerChannel Channel
        {
            get { return (TriggerChannel) GetCustomParameterValue(TriggerProperty.Channel); }
            set { UpdateCustomParameter(TriggerProperty.Channel, value, true); }
        }

        /// <summary>
        /// The value which, once reached, will cause this trigger will activate.
        /// </summary>
        [PropertyParameter(nameof(TriggerProperty.Threshold))]
        public int? Threshold
        {
            get { return (int?)GetCustomParameterValue(TriggerProperty.Threshold); }
            set { UpdateCustomParameter(TriggerProperty.Threshold, value); }
        }

        /// <summary>
        /// The time component of the volume limit that causes this trigger to activate.
        /// </summary>
        [PropertyParameter(nameof(TriggerProperty.Period))]
        public TriggerPeriod? Period
        {
            get { return (TriggerPeriod?) GetCustomParameterEnumInt<TriggerPeriod>(TriggerProperty.Period); }
            set { UpdateCustomParameter(TriggerProperty.Period, (int?) value); }
        }

        /// <summary>
        /// The unit component of the volume limit that causes this trigger to activate.
        /// </summary>
        [PropertyParameter(nameof(TriggerProperty.UnitSize))]
        public TriggerVolumeUnitSize? UnitSize
        {
            get { return (TriggerVolumeUnitSize?) GetCustomParameterEnumXml<TriggerVolumeUnitSize>(TriggerProperty.UnitSize); }
            set { UpdateCustomParameter(TriggerProperty.UnitSize, value?.EnumToXml()); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VolumeTriggerParameters"/> class for creating a new notification trigger.
        /// </summary>
        /// <param name="objectId">The object ID the trigger will apply to.</param>
        public VolumeTriggerParameters(int objectId) : base(TriggerType.Volume, objectId, (int?)null, ModifyAction.Add)
        {
            Channel = TriggerChannel.Primary;
            UnitSize = TriggerVolumeUnitSize.Byte;
            Period = TriggerPeriod.Hour;
            Threshold = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VolumeTriggerParameters"/> class for editing an existing notification trigger.
        /// </summary>
        /// <param name="objectId">The object ID the trigger is applied to. Note: if the trigger is inherited, the ParentId should be specified.</param>
        /// <param name="triggerId">The sub ID of the trigger on its parent object.</param>
        public VolumeTriggerParameters(int objectId, int triggerId) : base(TriggerType.Volume, objectId, triggerId, ModifyAction.Edit)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VolumeTriggerParameters"/> class for creating a new trigger from an existing <see cref="TriggerType.Volume"/> <see cref="NotificationTrigger"/>.
        /// </summary>
        /// <param name="objectId">The object ID the trigger will apply to.</param>
        /// <param name="sourceTrigger">The notification trigger whose properties should be used.</param>
        public VolumeTriggerParameters(int objectId, NotificationTrigger sourceTrigger) : base(TriggerType.Volume, objectId, sourceTrigger, ModifyAction.Add)
        {
            Channel = sourceTrigger.Channel;
            Period = sourceTrigger.Period;

            if (sourceTrigger.UnitSize == null)
                UnitSize = null;
            else
                UnitSize = sourceTrigger.UnitSize.ToString().ToEnum<TriggerVolumeUnitSize>();

            Threshold = sourceTrigger.ThresholdInternal;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VolumeTriggerParameters"/> class for editing an existing <see cref="TriggerType.Volume"/> <see cref="NotificationTrigger"/>.
        /// </summary>
        /// <param name="sourceTrigger">The notification trigger to modify.</param>
        public VolumeTriggerParameters(NotificationTrigger sourceTrigger) : base(TriggerType.Volume, sourceTrigger.ObjectId, sourceTrigger, ModifyAction.Edit)
        {
        }
    }
}
