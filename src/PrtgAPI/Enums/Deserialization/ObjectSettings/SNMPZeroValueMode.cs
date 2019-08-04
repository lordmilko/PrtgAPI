using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies whether spurious "0" values should be ignored for delta (difference) SNMP sensors.
    /// </summary>
    public enum SNMPZeroValueMode
    {
        /// <summary>
        /// Ignore 0 values.
        /// </summary>
        [XmlEnum("1")]
        Ignore,

        /// <summary>
        /// Handle 0 values as valid results.
        /// </summary>
        [XmlEnum("0")]
        Handle
    }
}
