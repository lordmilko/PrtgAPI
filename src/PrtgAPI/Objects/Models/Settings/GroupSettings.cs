namespace PrtgAPI
{
    /// <summary>
    /// Settings that apply to Groups within PRTG.
    /// </summary>
    public class GroupSettings : DeviceOrGroupSettings
    {
        //todo: maybe rename ipselectionmethod to autodiscoveryaddressmode

        /*[XmlElement("injected_ipselectmethod")]
        IPSelectionMethod? DiscoveryIPSelectionMethod { get; set; }

        [XmlElement("injected_ipbase")]
        public string DiscoveryIPv4Base { get; set; }

        [XmlElement("injected_iprangebegin")]
        public int? DiscoveryIPv4RangeStart { get; set; }

        [XmlElement("injected_iprangeend")]
        public int? DiscoveryIPv4RangeEnd { get; set; }

        [XmlElement("injected_iplist")]
        public string DiscoveryIPv4AddressList { get; set; } //todo: how do we split each entry on a newline?

        [XmlElement("injected_ipsubnet")]
        public string DiscoveryIPv4Subnet { get; set; }

        [XmlElement("injected_ipoctetrange")]
        public string DiscoveryIPv4Octet { get; set; }

        [XmlElement("injected_usedns")]
        public bool? UseDNS { get; set; }

        [XmlElement("injected_skipknownips")]
        public DiscoveryRescanMode? DiscoveryRescanMode { get; set; } //todo: we can probably change this to a bool*/

        //number of sensors limitation
    }
}