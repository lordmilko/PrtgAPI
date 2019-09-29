using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace PrtgAPI.Tree.Internal
{
    [ExcludeFromCodeCoverage]
    internal sealed class OrphanListDebugView<TOrphan> where TOrphan : TreeOrphan
    {
        private OrphanList<TOrphan> list;

        public OrphanListDebugView(OrphanList<TOrphan> list)
        {
            this.list = list;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public TOrphan[] Items
        {
            get
            {
                var items = list.ToArray();
                return items;
            }
        }
    }
}
