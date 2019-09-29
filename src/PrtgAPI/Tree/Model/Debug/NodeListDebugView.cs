using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace PrtgAPI.Tree
{
    [ExcludeFromCodeCoverage]
    internal sealed class NodeListDebugView
    {
        private NodeList list;

        public NodeListDebugView(NodeList list)
        {
            this.list = list;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public TreeNode[] Items => list.children.Select(i => i.Value).ToArray();
    }
}
