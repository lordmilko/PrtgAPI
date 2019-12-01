using System.Collections.Generic;

namespace PrtgAPI.Tree.Converters.Tree
{
    class CachedObjectManager : ObjectManager
    {
        internal CachedObjectManager(PrtgClient client, List<Sensor> sensors, List<Device> devices, List<Group> groups, List<Probe> probes)
        {
            Sensor = new CachedSensorFactory(client, sensors);
            Device = new CachedDeviceFactory(client, devices);
            Group = new CachedGroupFactory(client, groups);
            Probe = new CachedProbeFactory(client, probes);

            Trigger = new TriggerFactory(client);
            Property = new PropertyFactory(client);
        }
    }
}
