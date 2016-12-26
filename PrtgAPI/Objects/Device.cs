using System.Xml.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Objects.Shared;

namespace PrtgAPI
{
    /// <summary>
    /// A computer or piece of equipment containing one or more sensors monitored by PRTG.
    /// </summary>
    public class Device : DeviceOrGroupOrProbe
    {
        // ################################## Sensors, Devices ##################################
        // There is a copy in both Sensor and Device

        /// <summary>
        /// Group this device is contained in.
        /// </summary>
        [XmlElement("group")]
        [PropertyParameter(nameof(Property.Group))]
        public string Group { get; set; }

        /// <summary>
        /// Probe that monitors this device's sensors.
        /// </summary>
        [XmlElement("probe")]
        [PropertyParameter(nameof(Property.Probe))]
        public string Probe { get; set; }

        [XmlElement("host")]
        [PropertyParameter(nameof(Property.Host))]
        public string Host { get; set; }

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
