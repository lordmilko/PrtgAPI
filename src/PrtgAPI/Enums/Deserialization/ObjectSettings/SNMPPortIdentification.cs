using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies how an SNMP interface is identified if its port is changed after a reboot.
    /// </summary>
    public enum SNMPPortIdentification
    {
        /// <summary>
        /// Uses IfAlias first, followed by IfDescr if IfAlias is unsuccessful.
        /// </summary>
        [XmlEnum("10")]
        Automatic,

        /// <summary>
        /// Use the interface's alias.
        /// </summary>
        [XmlEnum("0")]
        UseIfAlias,

        /// <summary>
        /// Use the interface's description.
        /// </summary>
        [XmlEnum("1")]
        UseIfDescr,

        /// <summary>
        /// Use the interface's name.
        /// </summary>
        [XmlEnum("3")]
        UseIfName,

        /// <summary>
        /// Do not update the port's interface.
        /// </summary>
        [XmlEnum("100")]
        NoPortUpdate
    }
}
