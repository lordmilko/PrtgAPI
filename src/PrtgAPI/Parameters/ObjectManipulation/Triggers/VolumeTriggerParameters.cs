using System;
using PrtgAPI.Attributes;
using System.Linq;
using System.Xml.Serialization;
using PrtgAPI.Request;
using PrtgAPI.Utilities;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="PrtgRequestMessage"/> for adding/modifying <see cref="TriggerType.Volume"/> <see cref="NotificationTrigger"/> objects.
    /// </summary>
    public class VolumeTriggerParameters : TriggerParameters
    {
        private static DataUnit[] validUnits = GetValidUnits();

        private static DataUnit[] GetValidUnits()
        {
            var dataUnits = Enum.GetValues(typeof(DataUnit)).Cast<DataUnit>().ToList();
            var dataVolumeUnits = Enum.GetValues(typeof(DataVolumeUnit)).Cast<DataVolumeUnit>().ToList();

            var dataUnitsTuples = dataUnits.Select(u => new
            {
                E = u,
                X = u.GetEnumAttribute<XmlEnumAttribute>().Name
            });

            var dataVolumeUnitsTuples = dataVolumeUnits.Select(u => new
            {
                E = u,
                X = u.GetEnumAttribute<XmlEnumAttribute>().Name
            });

            return dataUnitsTuples.Where(u => dataVolumeUnitsTuples.Any(v => v.X == u.X)).Select(a => a.E).ToArray();
        }

        /// <summary>
        /// Gets or sets the channel of the sensor this trigger should apply to.
        /// </summary>
        [RequireValue(true)]
        [PropertyParameter(TriggerProperty.Channel)]
        public TriggerChannel Channel
        {
            get { return (TriggerChannel) GetCustomParameterValue(TriggerProperty.Channel); }
            set { UpdateCustomParameter(TriggerProperty.Channel, value, true); }
        }

        /// <summary>
        /// Gets or sets the value which, once reached, will cause this trigger will activate.
        /// </summary>
        [PropertyParameter(TriggerProperty.Threshold)]
        public double? Threshold
        {
            get { return (double?)GetCustomParameterValue(TriggerProperty.Threshold); }
            set { UpdateCustomParameter(TriggerProperty.Threshold, value); }
        }

        /// <summary>
        /// Gets or sets the time component of the volume limit that causes this trigger to activate.
        /// </summary>
        [PropertyParameter(TriggerProperty.Period)]
        public TriggerPeriod? Period
        {
            get { return (TriggerPeriod?) GetCustomParameterEnumInt<TriggerPeriod>(TriggerProperty.Period); }
            set { UpdateCustomParameter(TriggerProperty.Period, (int?) value); }
        }

        /// <summary>
        /// Gets or sets the unit component of the volume limit that causes this trigger to activate.<para/>
        /// Only byte-centric values can be specified.
        /// </summary>
        [PropertyParameter(TriggerProperty.UnitSize)]
        public DataUnit? UnitSize
        {
            get { return (DataUnit?) GetCustomParameterEnumXml<DataUnit>(TriggerProperty.UnitSize); }
            set { UpdateCustomParameter(TriggerProperty.UnitSize, ValidateUnit(value)?.EnumToXml()); }
        }

        private DataUnit? ValidateUnit(DataUnit? unitSize)
        {
            if (unitSize != null)
            {
                if (!validUnits.Contains(unitSize.Value))
                    throw new InvalidOperationException($"UnitSize '{unitSize}' cannot be used with {nameof(VolumeTriggerParameters)}. Please specify one of {string.Join(", ", validUnits)}.");
            }

            return unitSize;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VolumeTriggerParameters"/> class for creating a new notification trigger.
        /// </summary>
        /// <param name="objectOrId">The object or object ID the trigger will apply to.</param>
        public VolumeTriggerParameters(Either<IPrtgObject, int> objectOrId) : base(TriggerType.Volume, objectOrId, (int?)null, ModifyAction.Add)
        {
            Channel = TriggerChannel.Primary;
            UnitSize = DataUnit.Byte;
            Period = TriggerPeriod.Hour;
            Threshold = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VolumeTriggerParameters"/> class for editing an existing notification trigger.
        /// </summary>
        /// <param name="objectOrId">The object or object ID the trigger is applied to. Note: if the trigger is inherited, the ParentId should be specified.</param>
        /// <param name="triggerId">The sub ID of the trigger on its parent object.</param>
        public VolumeTriggerParameters(Either<IPrtgObject, int> objectOrId, int triggerId) : base(TriggerType.Volume, objectOrId, triggerId, ModifyAction.Edit)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VolumeTriggerParameters"/> class for creating a new trigger from an existing <see cref="TriggerType.Volume"/> <see cref="NotificationTrigger"/>.
        /// </summary>
        /// <param name="objectOrId">The object or object ID the trigger will apply to.</param>
        /// <param name="sourceTrigger">The notification trigger whose properties should be used.</param>
        public VolumeTriggerParameters(Either<IPrtgObject, int> objectOrId, NotificationTrigger sourceTrigger) : base(TriggerType.Volume, objectOrId, sourceTrigger, ModifyAction.Add)
        {
            Channel = sourceTrigger.Channel;
            Period = sourceTrigger.Period;

            if (sourceTrigger.UnitSize == null)
                UnitSize = null;
            else
                UnitSize = sourceTrigger.UnitSize;

            Threshold = sourceTrigger.Threshold;
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
