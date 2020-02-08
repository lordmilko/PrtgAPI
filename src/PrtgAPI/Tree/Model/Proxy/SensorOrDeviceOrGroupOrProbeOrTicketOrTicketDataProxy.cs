using System;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree
{
    [ExcludeFromCodeCoverage]
    internal class SensorOrDeviceOrGroupOrProbeOrTicketOrTicketDataProxy<T> : PrtgObjectProxy<T>, ISensorOrDeviceOrGroupOrProbeOrTicketOrTicketData
        where T : SensorOrDeviceOrGroupOrProbeOrTicketOrTicketData
    {
        public string Message => Resolved.Message;

        protected SensorOrDeviceOrGroupOrProbeOrTicketOrTicketDataProxy(Func<T> valueResolver)
            : base(valueResolver)
        {
        }

        protected SensorOrDeviceOrGroupOrProbeOrTicketOrTicketDataProxy(Func<int, T> parametersResolver) : base(parametersResolver)
        {
        }
    }
}
