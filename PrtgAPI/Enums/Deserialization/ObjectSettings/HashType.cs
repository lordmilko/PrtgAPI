using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies hash types that can be used for performing authentication.
    /// </summary>
    public enum HashType
    {
        /// <summary>
        /// Message-Digest Algorithm 5. MD5 is the standard authentication type.
        /// </summary>
        [XmlEnum("authpHMACMD596")]
        MD5,

        /// <summary>
        /// Secure hash Algorithm (SHA).
        /// </summary>
        [XmlEnum("authpHMACSHA96")]
        SHA
    }
}
