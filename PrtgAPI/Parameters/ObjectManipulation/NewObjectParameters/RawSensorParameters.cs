using System.Collections.Generic;
using PrtgAPI.Attributes;
using PrtgAPI.Request;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents raw parameters used to construct a <see cref="PrtgUrl"/> for creating a new sensor.
    /// </summary>
    public class RawSensorParameters : NewSensorParameters
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RawSensorParameters"/> class.
        /// </summary>
        /// <param name="sensorName">The name to use for this sensor.</param>
        /// <param name="sensorType">The type of sensor these parameters will create.</param>
        /// <param name="priority">The priority of the sensor, controlling how the sensor is displayed in table lists.</param>
        /// <param name="inheritTriggers">Whether to inherit notification triggers from the parent object.</param>
        public RawSensorParameters(string sensorName, string sensorType, Priority priority = Priority.Three, bool inheritTriggers = true) : base(sensorName, priority, inheritTriggers, sensorType)
        {
        }

        /// <summary>
        /// The type of sensor these parameters will create.
        /// </summary>
        [RequireValue(true)]
        public string SensorType => (string)this[Parameter.SensorType];

        /// <summary>
        /// Provides access to the underlying custom parameters of this object.
        /// </summary>
        public List<CustomParameter> Parameters
        {
            get
            {
                if (this[Parameter.Custom] == null)
                    this[Parameter.Custom] = new List<CustomParameter>();

                return (List<CustomParameter>)this[Parameter.Custom];
            }
            set { this[Parameter.Custom] = value; }
        }
    }
}
