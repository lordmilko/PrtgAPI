namespace Prtg.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="T:Prtg.PrtgUrl"/> for retrieving <see cref="T:Prtg.Device"/> objects.
    /// </summary>
    public class DeviceParameters : TableParameters<Device>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Prtg.Parameters.DeviceParameters"/> class.
        /// </summary>
        public DeviceParameters() : base(Content.Devices)
        {
        }
    }
}
