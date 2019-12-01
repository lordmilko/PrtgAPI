using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PrtgAPI.Tree.Converters.Tree
{
    class CachedDeviceFactory : DeviceFactory
    {
        private List<Device> devices;

        public override List<ITreeValue> Objects(int parentId) =>
            devices.Where(s => s.ParentId == parentId).Cast<ITreeValue>().ToList();

        public override Task<List<ITreeValue>> ObjectsAsync(int parentId, CancellationToken token) =>
            Task.FromResult(Objects(parentId));

        internal CachedDeviceFactory(PrtgClient client, List<Device> devices) : base(client)
        {
            this.devices = devices ?? new List<Device>();
        }
    }
}
