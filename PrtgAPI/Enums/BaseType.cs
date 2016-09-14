using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies the underlying type of a PRTG Object.
    /// </summary>
    public enum BaseType
    {
        /// <summary>
        /// An object used to monitor data on a system.
        /// </summary>
        [XmlEnum("sensor")]
        Sensor,

        /// <summary>
        /// A computer or piece of equipment containing one or more sensors monitored by PRTG.
        /// </summary>
        [XmlEnum("device")]
        Device,

        /// <summary>
        /// A group used to organize one or more devices.
        /// </summary>
        [XmlEnum("group")]
        Group,

        /// <summary>
        /// A device used to perform monitoring against a site or set of systems.
        /// </summary>
        [XmlEnum("probe")]
        Probe
    }
}
