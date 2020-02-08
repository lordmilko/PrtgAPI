using System;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree
{
    [ExcludeFromCodeCoverage]
    internal class GroupOrProbeProxy<T> : DeviceOrGroupOrProbeProxy<T>, IGroupOrProbe where T : GroupOrProbe
    {
        public bool Collapsed => Resolved.Collapsed;

        public int TotalGroups => Resolved.TotalGroups;

        public int TotalDevices => Resolved.TotalDevices;

        protected GroupOrProbeProxy(Func<T> valueResolver) : base(valueResolver)
        {
        }

        protected GroupOrProbeProxy(Func<int, T> parametersResolver) : base(parametersResolver)
        {
        }
    }
}
