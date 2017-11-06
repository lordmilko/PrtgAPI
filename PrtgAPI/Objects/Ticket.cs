using System.Xml.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Objects.Shared;

namespace PrtgAPI.Objects
{
    class Ticket : SensorOrDeviceOrGroupOrProbeOrLogOrTicket
    {
        /// <summary>
        /// Status of this ticket
        /// </summary>
        [XmlElement("status_raw")]
        [PropertyParameter(nameof(Property.Status))]
        public TicketStatus Status { get; set; }
    }
}
