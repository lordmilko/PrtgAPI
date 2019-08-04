using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies whether a PRTG Probe should periodically restart certain components to improve system performance/reliability.
    /// </summary>
    public enum ProbeRestartOption
    {
        /// <summary>
        /// Do not perform scheduled service reboots/system restarts.
        /// </summary>
        [XmlEnum("0")]
        DoNothing,

        /// <summary>
        /// Periodically restart all PRTG Services.
        /// </summary>
        [XmlEnum("1")]
        RestartServices,

        /// <summary>
        /// Periodically restart the entire PRTG Probe Server.
        /// </summary>
        [XmlEnum("2")]
        RebootSystem
    }
}
