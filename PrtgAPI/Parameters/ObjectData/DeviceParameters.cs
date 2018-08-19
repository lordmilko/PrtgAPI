using PrtgAPI.Request;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="PrtgUrl"/> for retrieving <see cref="Device"/> objects.
    /// </summary>
    public class DeviceParameters : TableParameters<Device>, IShallowCloneable<DeviceParameters>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceParameters"/> class.
        /// </summary>
        public DeviceParameters() : base(Content.Devices)
        {
        }
        DeviceParameters IShallowCloneable<DeviceParameters>.ShallowClone()
        {
            var newParameters = new DeviceParameters();

            ShallowClone(newParameters);

            return newParameters;
        }

        object IShallowCloneable.ShallowClone() => ((IShallowCloneable<DeviceParameters>)this).ShallowClone();
    }
}
