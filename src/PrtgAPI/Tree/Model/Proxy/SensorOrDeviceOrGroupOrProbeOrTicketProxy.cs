using System;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree
{
    [ExcludeFromCodeCoverage]
    internal class SensorOrDeviceOrGroupOrProbeOrTicketProxy<T> : SensorOrDeviceOrGroupOrProbeOrTicketOrTicketDataProxy<T>, ISensorOrDeviceOrGroupOrProbeOrTicket
        where T : SensorOrDeviceOrGroupOrProbeOrTicket
    {
        public Priority Priority => Resolved.Priority;

        protected SensorOrDeviceOrGroupOrProbeOrTicketProxy(Func<T> valueResolver) : base(valueResolver)
        {
        }

        protected SensorOrDeviceOrGroupOrProbeOrTicketProxy(Func<int, T> parametersResolver) : base(parametersResolver)
        {
        }
    }
}
