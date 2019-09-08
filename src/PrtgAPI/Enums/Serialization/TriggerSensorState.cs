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
        [XmlEnum("0")]
        Down,

        /// <summary>
        /// Trigger when the sensor is <see cref="Status.Warning"/>.
        /// </summary>
        [XmlEnum("1")]
        Warning,

        /// <summary>
        /// Trigger when the sensor is <see cref="Status.Unusual"/>.
        /// </summary>
        [XmlEnum("2")]
        Unusual,

        /// <summary>
        /// Trigger when the sensor is <see cref="Status.DownPartial"/>.
        /// </summary>
        [XmlEnum("3")]
        [XmlEnumAlternateName("Partial Down")]
        [XmlEnumAlternateName("Down (Partial)")]
        DownPartial,

        /// <summary>
        /// Trigger when the sensor is <see cref="Status.Up"/>.
        /// </summary>
        [XmlEnum("4")]
        Up,

        /// <summary>
        /// Trigger when the sensor is <see cref="Status.Unknown"/>.
        /// </summary>
        [XmlEnum("5")]
        Unknown
    }
}
