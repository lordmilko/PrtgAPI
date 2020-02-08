using System;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree
{
    [ExcludeFromCodeCoverage]
    internal class DeviceOrGroupOrProbeProxy<T> : SensorOrDeviceOrGroupOrProbeProxy<T>, IDeviceOrGroupOrProbe where T : DeviceOrGroupOrProbe
    {
        public int UpSensors => Resolved.UpSensors;

        public int DownSensors => Resolved.DownSensors;

        public int DownAcknowledgedSensors => Resolved.DownAcknowledgedSensors;

        public int PartialDownSensors => Resolved.PartialDownSensors;

        public int WarningSensors => Resolved.WarningSensors;

        public int PausedSensors => Resolved.PausedSensors;

        public int UnusualSensors => Resolved.UnusualSensors;

        public int UnknownSensors => Resolved.UnknownSensors;

        public int TotalSensors => Resolved.TotalSensors;

        protected DeviceOrGroupOrProbeProxy(Func<T> valueResolver) : base(valueResolver)
        {
        }

        protected DeviceOrGroupOrProbeProxy(Func<int, T> parametersResolver) : base(parametersResolver)
        {
        }
    }
}
