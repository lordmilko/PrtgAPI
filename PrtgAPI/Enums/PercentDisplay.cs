using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies how the value on a channel is displayed.
    /// </summary>
    public enum PercentDisplay
    {
        /// <summary>
        /// The actual value of the channel is displayed.
        /// </summary>
        [XmlEnum("0")]
        Actual,

        /// <summary>
        /// The value of the channel is displayed as a percentage of a specified maximum.
        /// </summary>
        [XmlEnum("1")]
        PercentOfMax
    }
}