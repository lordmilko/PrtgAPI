using System.Xml.Serialization;

namespace PrtgAPI.Objects.Undocumented
{
    internal class ProbeSettings : ContainerSettings
    {
        //outgoing ipv4, outgoing ipv4, restart options

        [XmlElement("injected_outgoingipv4")]
        public string OutgoingIPv4 { get; set; }

        [XmlElement("injected_outgoingipv6")]
        public string OutgoingIPv6 { get; set; }

        [XmlElement("injected_restartoptions")]
        public ProbeRestartOption RestartOption { get; set; }
    }
}

