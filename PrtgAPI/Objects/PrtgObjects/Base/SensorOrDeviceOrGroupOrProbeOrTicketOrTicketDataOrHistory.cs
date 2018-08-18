using System.Xml.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI
{
    /// <summary>
    /// Base class for Sensors, Devices, Groups, Probes, Logs, Tickets, TicketData and History, containing properties that apply to all eight object types.
    /// </summary>
    public class SensorOrDeviceOrGroupOrProbeOrTicketOrTicketDataOrHistory : PrtgObject
    {
        // ################################## Sensors, Devices, Groups, Probes, Messages, Tickets, TicketData, History ##################################

        /// <summary>
        /// Message or subject displayed on an object.
        /// </summary>
        [XmlElement("message_raw")]
        [PropertyParameter(nameof(Property.Message))]
        public string Message { get; set; }

        [XmlElement("message")]
        internal string DisplayMessage { get; set; }
    }
}
