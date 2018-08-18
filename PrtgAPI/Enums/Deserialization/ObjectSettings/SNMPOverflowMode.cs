using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies how PRTG should process overflow values.
    /// </summary>
    public enum SNMPOverflowMode
    {
        /// <summary>
        /// Ignore overflow values.
        /// </summary>
        [XmlEnum("0")]
        Ignore,
        
        /// <summary>
        /// Treat overflow values as valid results.
        /// </summary>
        [XmlEnum("1")]
        Handle
    }
}
