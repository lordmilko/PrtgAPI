using System.Xml.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI.Objects.Shared
{
    /// <summary>
    /// Properties that apply to Sensors, Devices, Groups, Probes, Logs, Tickets, TicketData and History.
    /// </summary>
    public class SensorOrDeviceOrGroupOrProbeOrLogOrTicketOrTicketDataOrHistory : ObjectTable
    {
        // ################################## Sensors, Devices, Groups, Probes, Messages, Tickets, TicketData, History ##################################

        internal string message;

        /// <summary>
        /// Message or subject displayed on an object.
        /// </summary>
        [XmlElement("message_raw")]
        [PropertyParameter(nameof(Property.Message))]
        public string Message
        {
            get { return GetMessage(); }
            set { message = value; }
        }

        [XmlElement("message")]
        internal string DisplayMessage { get; set; }

        internal virtual string GetMessage()
        {
            return message;
        }
    }
}
