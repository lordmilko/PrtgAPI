namespace PrtgAPI
{
    /// <summary>
    /// Specifies properties that apply to Groups and Probes.
    /// </summary>
    public interface IGroupOrProbe : IDeviceOrGroupOrProbe
    {
        /// <summary>
        /// Whether the object is currently expanded or collapsed in the PRTG Interface.
        /// </summary>
        bool Collapsed { get; }

        /// <summary>
        /// Number of groups contained under this object and all sub-objects.
        /// </summary>
        int TotalGroups { get; }

        /// <summary>
        /// Number of devices contained under this object and all sub-objects.
        /// </summary>
        int TotalDevices { get; }
    }
}
