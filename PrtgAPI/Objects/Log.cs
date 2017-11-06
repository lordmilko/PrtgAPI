using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Objects.Shared;

namespace PrtgAPI
{
    /// <summary>
    /// An informational event that has occurred on an object.
    /// </summary>
    public class Log : SensorOrDeviceOrGroupOrProbeOrLogOrTicket
    {
        /// <summary>
        /// The date and time the event occurred.
        /// </summary>
        [XmlElement("datetime_raw")]
        [PropertyParameter(nameof(Property.DateTime))]
        public DateTime DateTime { get; set; }

        /// <summary>
        /// The parent of the object the event pertained to.
        /// </summary>
        [XmlElement("parent")]
        [PropertyParameter(nameof(Property.Parent))]
        public string Parent { get; set; }

        /// <summary>
        /// Type of log record this object contains.
        /// </summary>
        [XmlElement("status_raw")]
        [PropertyParameter(nameof(Property.Status))]
        public LogStatus Status { get; set; }

        /// <summary>
        /// Sensor the event pertained to (if applicable)
        /// </summary>
        [XmlElement("sensor")]
        [PropertyParameter(nameof(Property.Sensor))]
        public string Sensor { get; set; }

        /// <summary>
        /// Device the event pertained to, or the device of the affected sensor.
        /// </summary>
        [XmlElement("device")]
        [PropertyParameter(nameof(Property.Device))]
        public string Device { get; set; }

        /// <summary>
        /// Group the event pertained to, or the group of the affected device or sensor.
        /// </summary>
        [XmlElement("group")]
        [PropertyParameter(nameof(Property.Group))]
        public string Group { get; set; }

        /// <summary>
        /// Probe the event pertained to, or the probe of the affected group, device or sensor.
        /// </summary>
        [XmlElement("probe")]
        [PropertyParameter(nameof(Property.Probe))]
        public string Probe { get; set; }

        internal override string GetMessage()
        {
            if (message != null && Regex.Match(message, "#[a-zA-Z].+").Success && DisplayMessage.StartsWith("<div"))
            {
                return Regex.Replace(DisplayMessage, "(<div.+?>)(.+?)(<.+>)", "$2");
            }                

            return message;
        }
    }
}
