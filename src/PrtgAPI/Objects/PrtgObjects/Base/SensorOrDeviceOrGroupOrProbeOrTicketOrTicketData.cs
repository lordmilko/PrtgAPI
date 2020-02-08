using System.Xml.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI
{
    /// <summary>
    /// Base class for Sensors, Devices, Groups, Probes, Tickets and TicketData, containing properties that apply to all seven object types.
    /// </summary>
    public class SensorOrDeviceOrGroupOrProbeOrTicketOrTicketData : PrtgObject, ISensorOrDeviceOrGroupOrProbeOrTicketOrTicketData
    {
        // ################################## Sensors, Devices, Groups, Probes, Tickets, TicketData ##################################

        /// <summary>
        /// Message or subject displayed on an object.
        /// </summary>
        [XmlElement("message_raw")]
        [PropertyParameter(Property.Message)]
        public string Message { get; set; }

        [XmlElement("message")]
        internal string DisplayMessage { get; set; }
    }
}
