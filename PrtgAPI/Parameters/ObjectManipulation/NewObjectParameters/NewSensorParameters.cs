using System;
using PrtgAPI.Request;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// <para type="description">Represents parameters used to construct a <see cref="PrtgUrl"/> for adding new <see cref="Sensor"/> objects.</para>
    /// </summary>
    public abstract class NewSensorParameters : NewObjectParameters
    {
        /// <summary>
        /// Whether to inherit notification triggers from the parent object.
        /// </summary>
        public bool? InheritTriggers
        {
            get { return GetCustomParameterBool(ObjectProperty.InheritTriggers); }
            set { SetCustomParameterBool(ObjectProperty.InheritTriggers, value); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewSensorParameters"/> class.
        /// </summary>
        /// <param name="sensorName">The name to use for this sensor.</param>
        /// <param name="inheritTriggers">Whether to inherit notification triggers from the parent object.</param>
        /// <param name="sensorType">The type of sensor these parameters will create.</param>
        public NewSensorParameters(string sensorName, bool inheritTriggers, object sensorType) : base(sensorName)
        {
            if (string.IsNullOrEmpty(sensorType?.ToString()))
                throw new ArgumentException($"{nameof(sensorType)} cannot be null or empty", nameof(sensorType));

            InheritTriggers = inheritTriggers;
            this[Parameter.SensorType] = sensorType;
        }
    }

}
