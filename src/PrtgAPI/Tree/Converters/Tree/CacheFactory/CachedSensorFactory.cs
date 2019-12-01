using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PrtgAPI.Tree.Converters.Tree
{
    class CachedSensorFactory : SensorFactory
    {
        private List<Sensor> sensors;

        public override List<ITreeValue> Objects(int parentId) =>
            sensors.Where(s => s.ParentId == parentId).Cast<ITreeValue>().ToList();

        public override Task<List<ITreeValue>> ObjectsAsync(int parentId, CancellationToken token) =>
            Task.FromResult(Objects(parentId));

        internal CachedSensorFactory(PrtgClient client, List<Sensor> sensors) : base(client)
        {
            this.sensors = sensors ?? new List<Sensor>();
        }
    }
}
