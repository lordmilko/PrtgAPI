using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies the number of packets to send in a single Ping Request.
    /// </summary>
    public enum PingMode
    {
        /// <summary>
        /// Perform a single Ping.
        /// </summary>
        [XmlEnum("0")]
        SinglePing,

        /// <summary>
        /// Perform multiple Pings.
        /// </summary>
        [XmlEnum("1")]
        MultiPing
    }
}
