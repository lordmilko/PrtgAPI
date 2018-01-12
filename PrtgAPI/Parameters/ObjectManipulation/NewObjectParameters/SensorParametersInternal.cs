using System.Diagnostics.CodeAnalysis;
using PrtgAPI.Attributes;
using PrtgAPI.Request;

namespace PrtgAPI.Parameters
{
    /// <summary>
    ///Base class for defining type-safe parameter types used to construct a <see cref="PrtgUrl"/> for adding new <see cref="Sensor"/> objects.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class SensorParametersInternal : NewSensorParameters
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SensorParametersInternal"/> class.
        /// </summary>
        /// <param name="sensorName">The name to use for this sensor.</param>
        /// <param name="priority">The priority of the sensor, controlling how the sensor is displayed in table lists.</param>
        /// <param name="inheritTriggers">Whether to inherit notification triggers from the parent object.</param>
        /// <param name="sensorType">The type of sensor these parameters will create.</param>
        internal SensorParametersInternal(string sensorName, Priority priority, bool inheritTriggers, SensorType sensorType) :
            base(sensorName, priority, inheritTriggers, sensorType)
        {
        }

        /// <summary>
        /// The type of sensor these parameters will create.
        /// </summary>
        [RequireValue(true)]
        public SensorType SensorType => (SensorType)this[Parameter.SensorType];
    }
}
