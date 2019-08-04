using System.Linq;
using PrtgAPI.Request;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="PrtgRequestMessage"/> for retrieving <see cref="Device"/> objects.
    /// </summary>
    public class DeviceParameters : TableParameters<Device>, IShallowCloneable<DeviceParameters>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceParameters"/> class.
        /// </summary>
        public DeviceParameters() : base(Content.Devices)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceParameters"/> class with one or more conditions to filter results by.
        /// </summary>
        /// <param name="filters">A list of conditions to filter results by.</param>
        public DeviceParameters(params SearchFilter[] filters) : this()
        {
            SearchFilters = filters.ToList();
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
