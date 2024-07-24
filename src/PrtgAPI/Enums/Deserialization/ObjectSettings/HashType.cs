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
        SHA,

        /// <summary>
        /// SHA-224.
        /// </summary>
        [XmlEnum("authpHMACSHA224")]
        SHA224,
        
        /// <summary>
        /// SHA-256.
        /// </summary>
        [XmlEnum("authpHMACSHA256")]
        SHA256,

        /// <summary>
        /// SHA-384.
        /// </summary>
        [XmlEnum("authpHMACSHA384")]
        SHA384,

        /// <summary>
        /// SHA-512.
        /// </summary>
        [XmlEnum("authpHMACSHA512")]
        SHA512
    }
}
