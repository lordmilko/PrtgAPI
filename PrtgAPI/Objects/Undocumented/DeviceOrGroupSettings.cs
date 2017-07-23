using System.Xml.Serialization;

namespace PrtgAPI.Objects.Undocumented
{
    internal class DeviceOrGroupSettings : ContainerSettings
    {
        [XmlElement("injected_discoverytype")]
        DiscoveryType DiscoveryType { get; set; }

        [XmlElement("injected_discoveryschedule")]
        DiscoverySchedule DiscoverySchedule { get; set; }
    }
}