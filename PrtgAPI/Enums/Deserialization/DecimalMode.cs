using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies how decimal places are displayed.
    /// </summary>
    public enum DecimalMode
    {
        /// <summary>
        /// The number of decimal places to display is determined automatically.
        /// </summary>
        [XmlEnum("0")]
        Automatic,

        /// <summary>
        /// All decimal places are displayed.
        /// </summary>
        [XmlEnum("1")]
        All,

        /// <summary>
        /// A specified number of decimal places are displayed.
        /// </summary>
        [XmlEnum("2")]
        Custom
    }
}