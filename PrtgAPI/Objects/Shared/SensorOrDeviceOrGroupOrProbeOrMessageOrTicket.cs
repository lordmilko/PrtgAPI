using System.Xml.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI.Objects.Shared
{
    /// <summary>
    /// Properties that apply to Sensors, Devices, Groups, Probes, Messages and Tickets.
    /// </summary>
    public class SensorOrDeviceOrGroupOrProbeOrMessageOrTicket : SensorOrDeviceOrGroupOrProbeOrMessageOrTicketOrTicketDataOrHistory
    {
        // ################################## Sensors, Devices, Groups, Probes, Messages, Tickets ##################################

        /// <summary>
        /// <see cref="PrtgAPI.Status"/> of this object. If this object is a Message, this value contains the category of log message.
        /// </summary>
        [XmlElement("status_raw")]
        [PropertyParameter(nameof(Property.Status))]
        public Status Status { get; set; }

        /// <summary>
        /// <see cref="Priority"/> of this object.
        /// </summary>
        [XmlElement("priority")]
        [XmlElement("priority_raw")]
        [PropertyParameter(nameof(Property.Priority))]
        public Priority Priority { get; set; }
    }
}
