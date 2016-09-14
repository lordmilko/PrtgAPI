namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="T:PrtgAPI.PrtgUrl"/> for retrieving <see cref="T:PrtgAPI.Device"/> objects.
    /// </summary>
    public class DeviceParameters : TableParameters<Device>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:PrtgAPI.Parameters.DeviceParameters"/> class.
        /// </summary>
        public DeviceParameters() : base(Content.Devices)
        {
        }
    }
}
