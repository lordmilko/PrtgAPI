using System.Diagnostics.CodeAnalysis;
using PrtgAPI.Attributes;
using PrtgAPI.Helpers;
using PrtgAPI.Request;

namespace PrtgAPI.Parameters
{
    /// <summary>
    ///Base class for defining type-safe parameter types used to construct a <see cref="PrtgUrl"/> for adding new <see cref="Sensor"/> objects.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class SensorParametersInternal : NewSensorParameters, ISourceParameters<Device>
    {
        /// <summary>
        /// Source device these parameters were derived from. If these parameters were not derived from a specific device this value is null.
        /// </summary>
        public Device Source { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SensorParametersInternal"/> class.
        /// </summary>
        /// <param name="sensorName">The name to use for this sensor.</param>
        /// <param name="sensorType">The type of sensor these parameters will create.</param>
        internal SensorParametersInternal(string sensorName, SensorType sensorType) : base(sensorName, "fake_type")
        {
            SensorType = sensorType;
        }

        /// <summary>
        /// The type of sensor these parameters will create.
        /// </summary>
        [RequireValue(true)]
        public SensorType SensorType
        {
            get { return this[Parameter.SensorType].ToString().XmlToEnum<SensorType>(); }
            set { this[Parameter.SensorType] = value.EnumToXml().ToLower(); }
        }
    }
}
