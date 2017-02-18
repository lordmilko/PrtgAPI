using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies whether something is specified or done automatically or manually.
    /// </summary>
    public enum AutoMode
    {
        /// <summary>
        /// The item is specified or done automatically.
        /// </summary>
        [XmlEnum("0")]
        Automatic,

        /// <summary>
        /// The item must be specified or done manually.
        /// </summary>
        [XmlEnum("1")]
        Manual
    }
}
