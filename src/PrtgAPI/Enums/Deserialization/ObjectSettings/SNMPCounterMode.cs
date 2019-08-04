using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies whether to allow the use of 64-bit SNMP counters or always force 32-bit.
    /// </summary>
    public enum SNMPCounterMode
    {
        /// <summary>
        /// Use 64-bit counters if available; otherwise use 32-bit.
        /// </summary>
        [XmlEnum("0")]
        Use64BitIfAvailable,

        /// <summary>
        /// Always use 32-bit counters. For some devices, this can assist with reliability.
        /// </summary>
        [XmlEnum("1")]
        Use32BitOnly
    }
}
