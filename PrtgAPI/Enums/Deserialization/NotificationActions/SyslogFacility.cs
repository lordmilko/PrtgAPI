using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies Syslog Facility levels used to describe the type of program that is generating the message.
    /// </summary>
    public enum SyslogFacility
    {
        /// <summary>
        /// User-level messages.
        /// </summary>
        [XmlEnum("1")]
        UserLevel,

        /// <summary>
        /// Custom facility 0.
        /// </summary>
        [XmlEnum("16")]
        Local0,

        /// <summary>
        /// Custom facility 1.
        /// </summary>
        [XmlEnum("17")]
        Local1,

        /// <summary>
        /// Custom facility 2.
        /// </summary>
        [XmlEnum("18")]
        Local2,

        /// <summary>
        /// Custom facility 3.
        /// </summary>
        [XmlEnum("19")]
        Local3,

        /// <summary>
        /// Custom facility 4.
        /// </summary>
        [XmlEnum("20")]
        Local4,

        /// <summary>
        /// Custom facility 5.
        /// </summary>
        [XmlEnum("21")]
        Local5,

        /// <summary>
        /// Custom facility 6.
        /// </summary>
        [XmlEnum("22")]
        Local6,

        /// <summary>
        /// Custom facility 7.
        /// </summary>
        [XmlEnum("23")]
        Local7,
    }
}
