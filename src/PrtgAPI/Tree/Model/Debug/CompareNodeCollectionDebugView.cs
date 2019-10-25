using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree
{
    [ExcludeFromCodeCoverage]
    internal class CompareNodeCollectionDebugView
    {
        private CompareNodeCollection node;

        public CompareNodeCollectionDebugView(CompareNodeCollection node)
        {
            this.node = node;
        }

        public CompareNode Parent => node.Parent;

        public INodeList<CompareNode> Children => node.Children;

        public string Name => node.Name;

        public TreeNodeType Type => node.Type;

        public FlagEnum<TreeNodeDifference> Difference => node.Difference;

        public FlagEnum<TreeNodeDifference> TreeDifference => node.TreeDifference;
    }
}
