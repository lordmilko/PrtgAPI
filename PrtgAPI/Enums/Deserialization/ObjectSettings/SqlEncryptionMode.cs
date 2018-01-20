using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies whethet encryption is used when communicating with a SQL Server.
    /// </summary>
    public enum SqlEncryptionMode
    {
        /// <summary>
        /// The level of encryption used is determined by the target SQL Server.
        /// </summary>
        [XmlEnum("0")]
        Default,

        /// <summary>
        /// Enforce encryption but do not validate the server certificate.
        /// </summary>
        [XmlEnum("2")]
        Encrypt,

        /// <summary>
        /// Enforce encryption and validate the server certificate.
        /// </summary>
        [XmlEnum("1")]
        EncryptAndValidate
    }
}
