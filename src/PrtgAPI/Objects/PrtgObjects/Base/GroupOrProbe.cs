using System.Xml.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI
{
    /// <summary>
    /// <para type="description">Base class for Groups and Probes, containing properties that apply to both object types.</para>
    /// </summary>
    public class GroupOrProbe : DeviceOrGroupOrProbe
    {
        /// <summary>
        /// Whether the object is currently expanded or collapsed in the PRTG Interface.
        /// </summary>
        [XmlElement("fold_raw")]
        [PropertyParameter(Property.Collapsed)]
        public bool Collapsed { get; set; }

        /// <summary>
        /// Number of groups contained under this object and all sub-objects.
        /// </summary>
        [XmlElement("groupnum")]
        [PropertyParameter(Property.TotalGroups)]
        public int TotalGroups { get; set; }

        /// <summary>
        /// Number of devices contained under this object and all sub-objects.
        /// </summary>
        [XmlElement("devicenum")]
        [PropertyParameter(Property.TotalDevices)]
        public int TotalDevices { get; set; }
    }
}
