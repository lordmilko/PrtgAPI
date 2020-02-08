using System;
using System.Linq;
using PrtgAPI.Parameters;

namespace PrtgAPI.Tree
{
    internal class SensorProxy : SensorOrDeviceOrGroupOrProbeProxy<Sensor>, ISensor
    {
        public string Probe => Resolved.Probe;

        public string Group => Resolved.Group;

        public bool Favorite => Resolved.Favorite;

        public string DisplayLastValue => Resolved.DisplayLastValue;

        public double? LastValue => Resolved.LastValue;

        public string Device => Resolved.Device;

        public double? Downtime => Resolved.Downtime;

        public TimeSpan? TotalDowntime => Resolved.TotalDowntime;

        public TimeSpan? DownDuration => Resolved.DownDuration;

        public double? Uptime => Resolved.Uptime;

        public TimeSpan? TotalUptime => Resolved.TotalUptime;

        public TimeSpan? UpDuration => Resolved.UpDuration;

        public TimeSpan TotalMonitorTime => Resolved.TotalMonitorTime;

        public DateTime? DataCollectedSince => Resolved.DataCollectedSince;

        public DateTime? LastCheck => Resolved.LastCheck;

        public DateTime? LastUp => Resolved.LastUp;

        public DateTime? LastDown => Resolved.LastDown;

        public string MiniGraph => Resolved.MiniGraph;

        public SensorProxy(Func<Sensor> valueResolver) : base(valueResolver)
        {
        }

        public SensorProxy(NewSensorParameters parameters, PrtgClient client) : base(i => client.AddSensor(i, parameters).First())
        {
        }

        public SensorProxy(string sensorType, PrtgClient client, Action<DynamicSensorParameters> callback = null) : base(GetDynamicSensorParametersResolver(sensorType, client, callback))
        {
        }

        private static Func<int, Sensor> GetDynamicSensorParametersResolver(string sensorType, PrtgClient client, Action<DynamicSensorParameters> callback)
        {
            Func<int, Sensor> func = parentId =>
            {
                var parameters = client.GetDynamicSensorParameters(parentId, sensorType);

                callback?.Invoke(parameters);

                var sensor = client.AddSensor(parentId, parameters).First();

                return sensor;
            };

            return func;
        }
    }
}
