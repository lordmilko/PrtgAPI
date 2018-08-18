using System.Xml.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI.Objects.Shared
{
    /// <summary>
    /// Base class for Sensors, Devices, Groups, Probes, Logs and Tickets, containing properties that apply to all six object types.
    /// </summary>
    public class SensorOrDeviceOrGroupOrProbeOrLogOrTicket : SensorOrDeviceOrGroupOrProbeOrLogOrTicketOrTicketDataOrHistory
    {
        // ################################## Sensors, Devices, Groups, Probes, Messages, Tickets ##################################

        /// <summary>
        /// <see cref="Priority"/> of this object.
        /// </summary>
        [XmlElement("priority")]
        [XmlElement("priority_raw")]
        [PropertyParameter(nameof(Property.Priority))]
        public Priority Priority { get; set; }
    }
}
