using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies the interval at which a sensor initiates a scan to refresh its data.
    /// </summary>
    public enum ScanningInterval
    {
        /// <summary>
        /// Scan every 30 seconds.
        /// </summary>
        [XmlEnum("30|30 seconds")]
        ThirtySeconds,

        /// <summary>
        /// Scan every 60 seconds.
        /// </summary>
        [XmlEnum("60|60 seconds")]
        SixtySeconds,

        /// <summary>
        /// Scan every 5 minutes.
        /// </summary>
        [XmlEnum("300|5 minutes")]
        FiveMinutes,

        /// <summary>
        /// Scan every 10 minutes.
        /// </summary>
        [XmlEnum("600|10 minutes")]
        TenMinutes,

        /// <summary>
        /// Scan every 15 minutes.
        /// </summary>
        [XmlEnum("900|15 minutes")]
        FifteenMinutes,

        /// <summary>
        /// Scan every 30 minutes.
        /// </summary>
        [XmlEnum("1800|30 minutes")]
        ThirtyMinutes,

        /// <summary>
        /// Scan every 1 hour.
        /// </summary>
        [XmlEnum("3600|1 hour")]
        OneHour,

        /// <summary>
        /// Scan every 4 hours.
        /// </summary>
        [XmlEnum("14400|4 hours")]
        FourHours,

        /// <summary>
        /// Scan every 6 hours.
        /// </summary>
        [XmlEnum("21600|6 hours")]
        SixHours,

        /// <summary>
        /// Scan every 12 hours.
        /// </summary>
        [XmlEnum("43200|12 hours")]
        TwelveHours,

        /// <summary>
        /// Scan every 24 hours.
        /// </summary>
        [XmlEnum("86400|24 hours")]
        TwentyFourHours,

        //todo: what if a custom scanninginterval has been specified
    }
}
