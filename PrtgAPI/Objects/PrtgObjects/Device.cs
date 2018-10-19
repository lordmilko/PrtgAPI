using System.Xml.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI
{
    /// <summary>
    /// <para type="description">A computer or piece of equipment containing one or more sensors monitored by PRTG.</para>
    /// </summary>
    public class Device : DeviceOrGroupOrProbe, ISensorOrDevice
    {
        /// <summary>
        /// Location of this object.
        /// </summary>
        [XmlElement("location_raw")]
        [PropertyParameter(Property.Location)]
        public string Location { get; set; }

        /// <summary>
        /// The Hostname or IP Address of this device.
        /// </summary>
        [XmlElement("host")]
        [PropertyParameter(Property.Host)]
        public string Host { get; set; }

        // ################################## Sensors, Devices ##################################
        // There is a copy in both Sensor and Device

        /// <summary>
        /// Group this device is contained in.
        /// </summary>
        [XmlElement("group")]
        [PropertyParameter(Property.Group)]
        public string Group { get; set; }

        /// <summary>
        /// Probe that monitors this device's sensors.
        /// </summary>
        [XmlElement("probe")]
        [PropertyParameter(Property.Probe)]
        public string Probe { get; set; }

        /// <summary>
        /// Whether this object has been marked as a favorite.
        /// </summary>
        [XmlElement("favorite_raw")]
        [PropertyParameter(Property.Favorite)]
        public bool Favorite { get; set; }

        // ################################## Devices, Groups ##################################
        // There is a copy in both Device and Group

        /// <summary>
        /// Auto-discovery progress (if one is in progress). Otherwise, null.
        /// </summary>
        [XmlElement("condition")]
        [PropertyParameter(Property.Condition)]
        public string Condition { get; set; }

        //todo: put these where they need to be

        //deviceicon, host, icon, location
        //location applies to both groups and devices
        /*
        [XmlElement("deviceicon")]
        [PropertyParameter("deviceicon")]
        public string DeviceIcon { get; set; }

        [XmlElement("host")]
        [PropertyParameter("host")]
        public string Host { get; set; }

        [XmlElement("icon")]
        [PropertyParameter("icon")]
        public string Icon { get; set; }*/
    }
}
