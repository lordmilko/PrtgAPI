namespace PrtgAPI
{
    /// <summary>
    /// Specifies properties that apply to Sensors, Devices, Groups, Probes or Tickets.
    /// </summary>
    public interface ISensorOrDeviceOrGroupOrProbeOrTicket : ISensorOrDeviceOrGroupOrProbeOrTicketOrTicketData
    {
        /// <summary>
        /// <see cref="Priority"/> of this object.
        /// </summary>
        Priority Priority { get; }
    }
}
