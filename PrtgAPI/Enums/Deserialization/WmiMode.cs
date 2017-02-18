using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies the method PRTG uses to query a WMI sensor.
    /// </summary>
    public enum WmiMode
    {
        /// <summary>
        /// The default query method.
        /// </summary>
        [XmlEnum("0")]
        Default,

        /// <summary>
        /// The alternative query method. May resolve errors such as "class not valid" or "invalid data" at the cost of potentially less accurate results.
        /// </summary>
        [XmlEnum("1")]
        Alternative
    }
}
