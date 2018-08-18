using System.Xml.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI
{
    class Ticket : SensorOrDeviceOrGroupOrProbeOrTicket
    {
        /// <summary>
        /// Status of this ticket
        /// </summary>
        [XmlElement("status_raw")]
        [PropertyParameter(nameof(Property.Status))]
        public TicketStatus Status { get; set; }
    }
}
