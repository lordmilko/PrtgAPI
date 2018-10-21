using System.Xml.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies HTTP protocols.
    /// </summary>
    public enum HttpMode
    {
        /// <summary>
        /// Secure HTTP.
        /// </summary>
        [XmlEnum("0")]
        [XmlEnumAlternateName("https")]
        HTTPS,

        /// <summary>
        /// Regular HTTP.
        /// </summary>
        [XmlEnum("1")]
        [XmlEnumAlternateName("http")]
        HTTP
    }
}
