using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PrtgAPI.Tree.Converters.Tree
{
    class CachedProbeFactory : ProbeFactory
    {
        private List<Probe> probes;

        public override List<ITreeValue> Objects(int parentId) =>
            probes.Where(s => s.ParentId == parentId).Cast<ITreeValue>().ToList();

        public override Task<List<ITreeValue>> ObjectsAsync(int parentId, CancellationToken token) =>
            Task.FromResult(Objects(parentId));

        internal CachedProbeFactory(PrtgClient client, List<Probe> probes) : base(client)
        {
            this.probes = probes ?? new List<Probe>();
        }
    }
}
