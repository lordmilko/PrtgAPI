using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies sensor statuses that can cause a trigger to activate.
    /// </summary>
    public enum TriggerSensorState
    {
        /// <summary>
        /// Trigger when the sensor is <see cref="SensorStatus.Down"/>.
        /// </summary>
        Down = 0,

        /// <summary>
        /// Trigger when the sensor is <see cref="SensorStatus.Warning"/>.
        /// </summary>
        Warning = 1,

        /// <summary>
        /// Trigger when the sensor is <see cref="SensorStatus.Unusual"/>.
        /// </summary>
        Unusual = 2,

        /// <summary>
        /// Trigger when the sensor is <see cref="SensorStatus.DownPartial"/>.
        /// </summary>
        [XmlEnum("Partial Down")]
        PartialDown = 3
    }
}
