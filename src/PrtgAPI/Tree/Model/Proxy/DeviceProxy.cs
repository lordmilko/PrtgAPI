using System;
using System.Diagnostics.CodeAnalysis;
using PrtgAPI.Parameters;

namespace PrtgAPI.Tree
{
    [ExcludeFromCodeCoverage]
    internal class DeviceProxy : DeviceOrGroupOrProbeProxy<Device>, IDevice
    {
        public string Location => Resolved.Location;

        public string Host => Resolved.Host;

        public string Group => Resolved.Group;

        public bool Favorite => Resolved.Favorite;

        public string Probe => Resolved.Probe;

        public string Condition => Resolved.Condition;

        public DeviceProxy(Func<Device> valueResolver) : base(valueResolver)
        {
        }

        public DeviceProxy(NewDeviceParameters parameters, PrtgClient client) : base(i => client.AddDevice(i, parameters))
        {
        }
    }
}
