using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies how often auto-discovery operations should be perform to create new sensors.
    /// </summary>
    public enum AutoDiscoverySchedule
    {
        /// <summary>
        /// Perform auto-discovery once, only when activated.
        /// </summary>
        [XmlEnum("0")]
        Once,

        /// <summary>
        /// Perform auto-discovery once an hour.
        /// </summary>
        [XmlEnum("1")]
        Hourly,

        /// <summary>
        /// Perform auto-discovery once a day.
        /// </summary>
        [XmlEnum("2")]
        Daily,

        /// <summary>
        /// Perform auto-discovery once a week.
        /// </summary>
        [XmlEnum("3")]
        Weekly
    }
}
