using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies how values are displayed for aggregated timespans.
    /// </summary>
    public enum HistoricValueMode
    {
        /// <summary>
        /// The average of values in the timespan is used.
        /// </summary>
        [XmlEnum("0")]
        Average,

        /// <summary>
        /// The minimum value in the timespan is used.
        /// </summary>
        [XmlEnum("1")]
        Minimum,

        /// <summary>
        /// The maximum value in the timespan is used.
        /// </summary>
        [XmlEnum("2")]
        Maximum
    }
}
