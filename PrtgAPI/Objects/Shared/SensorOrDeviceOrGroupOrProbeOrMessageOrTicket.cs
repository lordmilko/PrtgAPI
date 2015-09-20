using System.Xml.Serialization;
using Prtg.Attributes;

namespace Prtg.Objects.Shared
{
    /// <summary>
    /// Properties that apply to Sensors, Devices, Groups, Probes, Messages and Tickets.
    /// </summary>
    public class SensorOrDeviceOrGroupOrProbeOrMessageOrTicket : SensorOrDeviceOrGroupOrProbeOrMessageOrTicketOrTicketDataOrHistory
    {
        // ################################## Sensors, Devices, Groups, Probes, Messages, Tickets ##################################

        /// <summary>
        /// <see cref="T:Prtg.SensorStatus"/> of this object. If this object is a Message, this value contains the category of log message.
        /// </summary>
        [XmlElement("status_raw")]
        [PropertyParameter(nameof(Property.Status))]
        public SensorStatus? Status { get; set; }

        /// <summary>
        /// <see cref="T:Prtg.Priority"/> of this object.
        /// </summary>
        [XmlElement("priority")]
        [PropertyParameter(nameof(Property.Priority))]
        public Priority? Priority { get; set; }
    }
}
