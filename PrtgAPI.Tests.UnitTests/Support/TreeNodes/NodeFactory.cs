namespace PrtgAPI.Tests.UnitTests.TreeNodes
{
    class NodeFactory
    {
        private int probeId = 1000;
        private int groupId = 2000;
        private int deviceId = 3000;
        private int sensorId = 4000;

        public int GetProbeId() => probeId++;
        public int GetGroupId() => groupId++;
        public int GetDeviceId() => deviceId++;
        public int GetSensorId() => sensorId++;
    }
}
