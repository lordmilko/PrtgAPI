using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies whether PRTG should automatically update sensor names when names change in the target device.
    /// </summary>
    public enum SNMPPortNameUpdateMode
    {
        /// <summary>
        /// Do not automatically update sensor names.
        /// </summary>
        [XmlEnum("0")]
        Manual,

        /// <summary>
        /// Automatically update sensor names.
        /// </summary>
        [XmlEnum("1")]
        Automatic
    }
}
