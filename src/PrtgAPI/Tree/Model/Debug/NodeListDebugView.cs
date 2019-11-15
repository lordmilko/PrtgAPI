using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace PrtgAPI.Tree
{
    [ExcludeFromCodeCoverage]
    internal sealed class StrictNodeListDebugView
    {
        private StrictNodeList list;

        public StrictNodeListDebugView(StrictNodeList list)
        {
            this.list = list;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public TreeNode[] Items => list.children.Select(i => i.Value).ToArray();
    }

    [ExcludeFromCodeCoverage]
    internal sealed class LazyNodeListDebugView
    {
        private LazyNodeList list;

        public LazyNodeListDebugView(LazyNodeList list)
        {
            this.list = list;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public TreeNode[] Items => list.children.ToArray();
    }
}
