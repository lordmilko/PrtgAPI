namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="PrtgUrl"/> for retrieving <see cref="Device"/> objects.
    /// </summary>
    public class DeviceParameters : TableParameters<Device>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceParameters"/> class.
        /// </summary>
        public DeviceParameters() : base(Content.Devices)
        {
        }
    }
}
