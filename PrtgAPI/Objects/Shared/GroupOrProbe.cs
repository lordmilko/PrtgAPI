using System.Xml.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI.Objects.Shared
{
    /// <summary>
    /// Properties that apply to Groups and Probes
    /// </summary>
    public class GroupOrProbe : DeviceOrGroupOrProbe
    {
        /// <summary>
        /// Whether the object is currently expanded or collapsed in the PRTG Interface.
        /// </summary>
        [XmlElement("fold")]
        [PropertyParameter(nameof(Property.Collapsed))]
        public bool Collapsed { get; set; }

        /// <summary>
        /// Number of groups contained under this object.
        /// </summary>
        [XmlElement("groupnum")]
        [PropertyParameter(nameof(Property.TotalGroups))]
        public int TotalGroups { get; set; }

        /// <summary>
        /// Number of devices contained under this object.
        /// </summary>
        [XmlElement("devicenum")]
        [PropertyParameter(nameof(Property.TotalDevices))]
        public int TotalDevices { get; set; }
    }
}
