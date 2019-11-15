using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace PrtgAPI.Tree.Internal
{
    [ExcludeFromCodeCoverage]
    internal sealed class StrictOrphanListDebugView
    {
        private StrictOrphanList list;

        public StrictOrphanListDebugView(StrictOrphanList list)
        {
            this.list = list;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public TreeOrphan[] Items => list.children.Select(i => i.Value).ToArray();
    }

    [ExcludeFromCodeCoverage]
    internal sealed class LazyOrphanListDebugView
    {
        private LazyOrphanList list;

        public LazyOrphanListDebugView(LazyOrphanList list)
        {
            this.list = list;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public TreeOrphan[] Items => list.children.ToArray();
    }
}
