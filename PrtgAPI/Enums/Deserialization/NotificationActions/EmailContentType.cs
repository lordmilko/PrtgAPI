using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies the content type an email should be formatted with.
    /// </summary>
    public enum EmailContentType
    {
        /// <summary>
        /// Format automatically with HTML.
        /// </summary>
        [XmlEnum("text/html")]
        HTML,

        /// <summary>
        /// Format automatically with text.
        /// </summary>
        [XmlEnum("text/plain")]
        Text,

        /// <summary>
        /// Provide a custom format using variable placeholders.
        /// </summary>
        [XmlEnum("text/custom")]
        Custom
    }
}
