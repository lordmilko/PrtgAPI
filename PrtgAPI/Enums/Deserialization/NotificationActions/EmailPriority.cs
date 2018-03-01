using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies the priority with which a notification email is identified as.
    /// </summary>
    public enum EmailPriority
    {
        /// <summary>
        /// Email is identified as being of high importance.
        /// </summary>
        [XmlEnum("0")]
        Highest,

        /// <summary>
        /// Email is identified as being of high importance.
        /// </summary>
        [XmlEnum("1")]
        High,

        /// <summary>
        /// Email is sent with normal importance.
        /// </summary>
        [XmlEnum("2")]
        Normal,

        /// <summary>
        /// Email is identified as being of low importance.
        /// </summary>
        [XmlEnum("3")]
        Low,

        /// <summary>
        /// Email is identified as being of low importance.
        /// </summary>
        [XmlEnum("4")]
        Lowest
    }
}
