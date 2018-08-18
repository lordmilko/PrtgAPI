using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies how long WMI sensors should wait before timing out.
    /// </summary>
    public enum WmiTimeoutMethod
    {
        /// <summary>
        /// Sensors will wait 1.5x their scanning interval before timing out.
        /// </summary>
        [XmlEnum("1")]
        OnePointFiveTimesInterval,

        /// <summary>
        /// Use a custom timeout for all sensors.
        /// </summary>
        [XmlEnum("0")]
        Manual
    }
}
