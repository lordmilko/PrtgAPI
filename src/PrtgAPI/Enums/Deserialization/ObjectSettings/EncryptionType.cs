using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies types of encryption that can be used.
    /// </summary>
    public enum EncryptionType
    {
        /// <summary>
        /// Use Data Encryption Standard (DES) (less secure).
        /// </summary>
        [XmlEnum("DESPrivProtocol")]
        DES,

        /// <summary>
        /// Use Advanced Encryption Standard (more secure).
        /// </summary>
        [XmlEnum("AESPrivProtocol")]
        AES,

        /// <summary>
        /// Use AES-192 (more secure).
        /// </summary>
        [XmlEnum("AES192PrivProtocol")]
        AES192,

        /// <summary>
        /// Use AES-256 (more secure).
        /// </summary>
        [XmlEnum("AES256PrivProtocol")]
        AES256
    }
}
