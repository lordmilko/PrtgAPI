using System.Xml.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI
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

        /// <summary>
        /// How thoroughly PRTG should scan for compatible sensor types when performing an auto-discovery. Corresponds to:<para/>
        ///     Group Type -> Sensor Management<para/>
        ///     Device Type -> Sensor Management
        /// </summary>
        [XmlElement("injected_discoverytype")]
        public AutoDiscoveryMode? AutoDiscoveryMode { get; set; }

        /// <summary>
        /// How often auto-discovery operations should be performed to create new sensors. Corresponds to:<para/>
        ///     Group Type -> Discovery Schedule<para/>
        ///     Device Type -> Discovery Schedule
        /// </summary>
        [XmlElement("injected_discoveryschedule")]
        public AutoDiscoverySchedule? AutoDiscoverySchedule { get; set; }
    }
}