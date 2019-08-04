using System.Xml.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI
{
    /// <summary>
    /// Base class for Sensors, Devices, Groups, Probes, Logs and Tickets, containing properties that apply to all six object types.
    /// </summary>
    public class SensorOrDeviceOrGroupOrProbeOrTicket : SensorOrDeviceOrGroupOrProbeOrTicketOrTicketData
    {
        // ################################## Sensors, Devices, Groups, Probes, Messages, Tickets ##################################

        /// <summary>
        /// <see cref="Priority"/> of this object.
        /// </summary>
        [XmlElement("priority")]
        [XmlElement("priority_raw")]
        [PropertyParameter(Property.Priority)]
        public Priority Priority { get; set; }
    }
}
