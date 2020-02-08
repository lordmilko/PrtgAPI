namespace PrtgAPI
{
    /// <summary>
    /// Specifies properties that apply to Devices.
    /// </summary>
    public interface IDevice : ISensorOrDevice, IDeviceOrGroup
    {
        /// <summary>
        /// Location of this object.
        /// </summary>
        string Location { get; }

        /// <summary>
        /// The Hostname or IP Address of this device.
        /// </summary>
        string Host { get; }
    }
}
