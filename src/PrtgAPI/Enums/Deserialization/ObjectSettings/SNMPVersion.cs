using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies the version to use for SNMP.
    /// </summary>
    public enum SNMPVersion
    {
        /// <summary>
        /// Use SNMPv1.
        /// </summary>
        [XmlEnum("V1")]
        v1,

        /// <summary>
        /// Use SNMPv2c.
        /// </summary>
        [XmlEnum("V2")]
        v2c,

        /// <summary>
        /// Use SNMPv3.
        /// </summary>
        [XmlEnum("V3")]
        v3
    }
}
