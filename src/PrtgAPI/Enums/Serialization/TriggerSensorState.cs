using System.Xml.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies sensor statuses that can cause a trigger to activate.
    /// </summary>
    public enum TriggerSensorState
    {
        /// <summary>
        /// Trigger when the sensor is <see cref="Status.Down"/>.
        /// </summary>
        Down = 0,

        /// <summary>
        /// Trigger when the sensor is <see cref="Status.Warning"/>.
        /// </summary>
        Warning = 1,

        /// <summary>
        /// Trigger when the sensor is <see cref="Status.Unusual"/>.
        /// </summary>
        Unusual = 2,

        /// <summary>
        /// Trigger when the sensor is <see cref="Status.DownPartial"/>.
        /// </summary>
        [XmlEnum("Partial Down")]
        [XmlEnumAlternateName("Down (Partial)")]
        DownPartial = 3
    }
}
