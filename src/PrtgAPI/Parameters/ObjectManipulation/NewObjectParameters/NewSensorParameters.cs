using System;
using PrtgAPI.Attributes;
using PrtgAPI.Request;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// <para type="description">Represents parameters used to construct a <see cref="PrtgRequestMessage"/> for adding new <see cref="Sensor"/> objects.</para>
    /// </summary>
    public abstract class NewSensorParameters : NewObjectParameters
    {
        internal override CommandFunction Function => CommandFunction.AddSensor5;

        /// <summary>
        /// Gets or sets the priority of the sensor, controlling how the sensor is displayed in table lists.<para/>
        /// If this value is null, the default value <see cref="Priority.Three"/> will be used.
        /// </summary>
        [PropertyParameter(ObjectProperty.Priority)]
        public Priority? Priority
        {
            get { return (Priority?)GetCustomParameterEnumXml<Priority?>(ObjectProperty.Priority); }
            set { SetCustomParameterEnumXml(ObjectProperty.Priority, value); }
        }

        /// <summary>
        /// Gets or sets whether to inherit notification triggers from the parent object.<para/>
        /// If this value is null, the default value "true" will be used.
        /// </summary>
        [PropertyParameter(ObjectProperty.InheritTriggers)]
        public bool? InheritTriggers
        {
            get { return GetCustomParameterBool(ObjectProperty.InheritTriggers); }
            set { SetCustomParameterBool(ObjectProperty.InheritTriggers, value); }
        }

        /// <summary>
        /// Gets or sets whether this sensor's scanning interval settings are inherited from its parent.<para/>
        /// If this value is null, the default value "true" will be used.
        /// </summary>
        [PropertyParameter(ObjectProperty.InheritInterval)]
        public bool? InheritInterval
        {
            get { return GetCustomParameterBool(ObjectProperty.InheritInterval); }
            set { SetCustomParameterBool(ObjectProperty.InheritInterval, value); }
        }

        /// <summary>
        /// Gets or sets the scanning interval of the sensor. Applies only when <see cref="InheritInterval"/> is false.<para/>
        /// If this value is null, the default value <see cref="ScanningInterval.SixtySeconds"/> will be used.
        /// </summary>
        [PropertyParameter(ObjectProperty.Interval)]
        public ScanningInterval Interval
        {
            get { return (ScanningInterval)GetCustomParameter(ObjectProperty.Interval); }
            set { SetCustomParameter(ObjectProperty.Interval, value); }
        }

        /// <summary>
        /// Gets or sets the number of scanning intervals the sensor will wait before entering a <see cref="Status.Down"/> state when the sensor reports an error.<para/>
        /// If this value is null, the default value <see cref="IntervalErrorMode.OneWarningThenDown"/> will be used.
        /// </summary>
        [PropertyParameter(ObjectProperty.IntervalErrorMode)]
        public IntervalErrorMode? IntervalErrorMode
        {
            get { return (IntervalErrorMode?)GetCustomParameterEnumXml<IntervalErrorMode?>(ObjectProperty.IntervalErrorMode); }
            set { SetCustomParameterEnumXml(ObjectProperty.IntervalErrorMode, value); }
        }

        /// <summary>
        /// Specifies whether the resulting sensor type is dynamically determined by the parameters included in the request.<para/>
        /// If this property is true, PrtgAPI will relax its sensor resolution mechanism to ensure the resultant object is retrieved.
        /// </summary>
        public bool DynamicType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewSensorParameters"/> class.
        /// </summary>
        /// <param name="name">The name to use for this sensor.</param>
        /// <param name="sensorType">The type of sensor these parameters will create.</param>
        protected NewSensorParameters(string name, object sensorType) : base(name)
        {
            if (string.IsNullOrEmpty(sensorType?.ToString()))
                throw new ArgumentException("SensorType cannot be null or empty.", nameof(sensorType));

            this[Parameter.SensorType] = sensorType;

            //All sensors default to these settings
            Priority = PrtgAPI.Priority.Three;
            InheritTriggers = true;
            InheritInterval = true;
            Interval = ScanningInterval.SixtySeconds;
            IntervalErrorMode = PrtgAPI.IntervalErrorMode.OneWarningThenDown;
        }
    }
}
