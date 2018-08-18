using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies how graphs should be displayed.
    /// </summary>
    public enum GraphType
    {
        /// <summary>
        /// Each series in the graph is shown independently of one another.
        /// </summary>
        [XmlEnum("0")]
        Independent,

        /// <summary>
        /// Each series in the graph is stacked on top of each other.
        /// </summary>
        [XmlEnum("1")]
        Stacked
    }
}
