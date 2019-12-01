using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PrtgAPI.Tree.Converters.Tree
{
    class CachedGroupFactory : GroupFactory
    {
        private List<Group> groups;

        public override List<ITreeValue> Objects(int parentId) =>
            groups.Where(s => s.ParentId == parentId).Cast<ITreeValue>().ToList();

        public override Task<List<ITreeValue>> ObjectsAsync(int parentId, CancellationToken token) =>
            Task.FromResult(Objects(parentId));

        internal CachedGroupFactory(PrtgClient client, List<Group> groups) : base(client)
        {
            this.groups = groups ?? new List<Group>();
        }
    }
}
