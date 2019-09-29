using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace PrtgAPI.Tree
{
    [ExcludeFromCodeCoverage]
    internal sealed class NodeListDebugView<TNode> where TNode : TreeNode
    {
        private NodeList<TNode> list;

        public NodeListDebugView(NodeList<TNode> list)
        {
            this.list = list;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public TNode[] Items
        {
            get
            {
                var items = list.ToArray();
                return items;
            }
        }
    }
}
