using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace PrtgAPI.Tree.Internal
{
    [ExcludeFromCodeCoverage]
    internal sealed class OrphanListDebugView
    {
        private OrphanList list;

        public OrphanListDebugView(OrphanList list)
        {
            this.list = list;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public TreeOrphan[] Items => list.children.Select(i => i.Value).ToArray();
    }
}
