using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies how many SNMP requests are bundled in each SNMP communication with the device.
    /// </summary>
    public enum SNMPRequestMode
    {
        /// <summary>
        /// Allow for multiple SNMP request in each request.
        /// </summary>
        [XmlEnum("0")]
        MultiGet,

        /// <summary>
        /// Include one SNMP request in each request. This can be required in older devices.
        /// </summary>
        [XmlEnum("1")]
        SingleGet
    }
}
