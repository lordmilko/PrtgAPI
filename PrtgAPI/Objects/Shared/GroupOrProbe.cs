using System;
using System.Xml.Serialization;
using Prtg.Attributes;

namespace Prtg.Objects.Shared
{
    /// <summary>
    /// Properties that apply to Groups and Probes
    /// </summary>
    public class GroupOrProbe : DeviceOrGroupOrProbe
    {
        /// <summary>
        /// Whether the object is currently expanded or collapsed in the PRTG Interface.
        /// </summary>
        [PropertyParameter(nameof(Property.Fold))]
        public bool Collapsed => Convert.ToBoolean(_RawCollapsed);

        private string rawcollapsed;

        /// <summary>
        /// Raw value used for <see cref="Collapsed"/> attribute. This property should not be used.
        /// </summary>
        [XmlElement("fold")]
        public string _RawCollapsed
        {
            get { return rawcollapsed; }
            set { rawcollapsed = Convert.ToBoolean(value).ToString(); }
        }

        /// <summary>
        /// Number of groups contained under this object.
        /// </summary>
        [XmlElement("groupnum")]
        [PropertyParameter(nameof(Property.GroupNum))]
        public int? TotalGroups { get; set; }

        /// <summary>
        /// Number of devices contained under this object.
        /// </summary>
        [XmlElement("devicenum")]
        [PropertyParameter(nameof(Property.DeviceNum))]
        public int? TotalDevices { get; set; }
    }
}
