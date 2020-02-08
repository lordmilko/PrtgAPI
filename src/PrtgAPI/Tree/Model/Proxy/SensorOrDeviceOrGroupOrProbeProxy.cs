using System;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree
{
    [ExcludeFromCodeCoverage]
    internal abstract class SensorOrDeviceOrGroupOrProbeProxy<T> : SensorOrDeviceOrGroupOrProbeOrTicketProxy<T>, ISensorOrDeviceOrGroupOrProbe
        where T : SensorOrDeviceOrGroupOrProbe
    {
        public string Schedule => Resolved.Schedule;

        public BaseType BaseType => Resolved.BaseType;

        public string Url => Resolved.Url;

        public NotificationTypes NotificationTypes => Resolved.NotificationTypes;

        public TimeSpan Interval => Resolved.Interval;

        public bool InheritInterval => Resolved.InheritInterval;

        public Access Access => Resolved.Access;

        public string Dependency => Resolved.Dependency;

        public int Position => Resolved.Position;

        public Status Status => Resolved.Status;

        public string Comments => Resolved.Comments;

        protected SensorOrDeviceOrGroupOrProbeProxy(Func<T> valueResolver) : base(valueResolver)
        {
        }

        protected SensorOrDeviceOrGroupOrProbeProxy(Func<int, T> parametersResolver) : base(parametersResolver)
        {
        }
    }
}
