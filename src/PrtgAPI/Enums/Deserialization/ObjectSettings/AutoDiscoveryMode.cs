using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies how thoroughly PRTG should scan for compatible sensor types when performing an auto-discovery.
    /// </summary>
    public enum AutoDiscoveryMode
    {
        /// <summary>
        /// Do not perform auto-discovery; sensors will be added manually.
        /// </summary>
        [XmlEnum("0")]
        Manual,

        /// <summary>
        /// Perform a standard auto-discovery scan, creating the most common sensor types.
        /// </summary>
        [XmlEnum("1")]
        Automatic,

        /// <summary>
        /// Perform an exhaustive auto-discovery scan, identifying as many sensor types as possible.
        /// </summary>
        [XmlEnum("3")]
        AutomaticDetailed,

        /// <summary>
        /// Perform an auto-discovery using a specified device template.
        /// </summary>
        [XmlEnum("2")]
        AutomaticTemplate
    }
}
