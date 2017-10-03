using System.Xml.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI.Objects.Undocumented
{
    /// <summary>
    /// Settings that apply to Devices and Groups within PRTG.
    /// </summary>
    public class DeviceOrGroupSettings : ContainerSettings
    {
        /// <summary>
        /// Tags that are inherited from this object's parent.
        /// </summary>
        [XmlElement("injected_parenttags")]
        [SplittableString(' ')]
        public string[] ParentTags { get; set; }

        /*[XmlElement("injected_discoverytype")]
        DiscoveryType DiscoveryType { get; set; }

        [XmlElement("injected_discoveryschedule")]
        DiscoverySchedule DiscoverySchedule { get; set; }*/
    }
}