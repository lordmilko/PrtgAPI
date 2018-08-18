using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies the data source to use for WMI sensors.
    /// </summary>
    public enum WmiDataSource
    {
        /// <summary>
        /// Use both Performance Counters and WMI.
        /// </summary>
        [XmlEnum("1")]
        PerformanceCountersAndWMI,

        /// <summary>
        /// Use Performance Counters only.
        /// </summary>
        [XmlEnum("2")]
        PerformanceCounters,

        /// <summary>
        /// Use WMI only.
        /// </summary>
        [XmlEnum("3")]
        WMI
    }
}
