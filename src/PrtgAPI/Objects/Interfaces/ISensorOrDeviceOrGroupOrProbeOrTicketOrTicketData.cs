using PrtgAPI.Tree;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies properties that apply to Sensors, Devices, Groups, Probes, Tickets and TicketData.
    /// </summary>
    public interface ISensorOrDeviceOrGroupOrProbeOrTicketOrTicketData : IPrtgObject
    {
        /// <summary>
        /// Message or subject displayed on an object.
        /// </summary>
        string Message { get; }
    }
}
