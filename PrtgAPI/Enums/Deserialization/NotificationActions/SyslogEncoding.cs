using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies the character encoding to use for syslog messages.
    /// </summary>
    public enum SyslogEncoding
    {
        /// <summary>
        /// ANSI encoding.
        /// </summary>
        [XmlEnum("0")]
        ANSI,

        /// <summary>
        /// Unicode encoding with UTF-8.
        /// </summary>
        [XmlEnum("1")]
        UTF8
    }
}
