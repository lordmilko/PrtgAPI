using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Settings that apply to Probes within PRTG.
    /// </summary>
    public class ProbeSettings : ContainerSettings
    {
        //outgoing ipv4, outgoing ipv4, restart options

        /*[XmlElement("injected_outgoingipv4")]
        public string OutgoingIPv4 { get; set; }

        [XmlElement("injected_outgoingipv6")]
        public string OutgoingIPv6 { get; set; }

        [XmlElement("injected_restartoptions")]
        public ProbeRestartOption RestartOption { get; set; }*/

        /// <summary>
        /// Whether this probe has been confirmed for use within this PRTG Core.<para/>If this value is false,
        /// the probe has neither been approved nor denied.<para/>If a probe is denied, its GID will be blacklisted
        /// and it will be removed from PRTG.
        /// </summary>
        [XmlElement("injected_authorized")]
        public bool ProbeApproved { get; set; }
    }
}

