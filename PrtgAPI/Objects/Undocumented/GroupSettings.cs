using System.Xml.Serialization;

namespace PrtgAPI.Objects.Undocumented
{
    internal class GroupSettings : DeviceOrGroupSettings
    {
        [XmlElement("injected_injected_ipselectmethod")]
        IPSelectionMethod DiscoveryIPSelectionMethod { get; set; }

        [XmlElement("injected_ipbase")]
        public string DiscoveryIPv4Base { get; set; }

        [XmlElement("injected_iprangebegin")]
        public int DiscoveryIPv4RangeStart { get; set; }

        [XmlElement("injected_iprangeend")]
        public int DiscoveryIPv4RangeEnd { get; set; }

        [XmlElement("injected_iplist")]
        public string DiscoveryIPv4AddressList { get; set; } //todo: how do we split each entry on a newline?

        [XmlElement("injected_ipsubnet")]
        public string DiscoveryIPv4Subnet { get; set; }

        [XmlElement("injected_ipoctetrange")]
        public string DiscoveryIPv4Octet { get; set; }

        [XmlElement("injected_usedns")]
        public DiscoveryNameResolutionMode NameResolutionMode { get; set; }

        [XmlElement("injected_skipknownips")]
        public DiscoveryRescanMode DiscoveryRescanMode { get; set; }

        //number of sensors limitation
    }
}