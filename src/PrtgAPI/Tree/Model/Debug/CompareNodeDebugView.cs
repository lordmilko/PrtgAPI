using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree
{
    [ExcludeFromCodeCoverage]
    internal sealed class CompareNodeDebugView
    {
        private CompareNode node;

        public CompareNodeDebugView(CompareNode node)
        {
            this.node = node;
        }

        public CompareNode Parent => node.Parent;

        public INodeList<CompareNode> Children => node.Children;

        public string Name => node.Name;

        public TreeNodeType Type => node.Type;

        public PrtgNode First => node.First;

        public PrtgNode Second => node.Second;

        public FlagEnum<TreeNodeDifference> Difference => node.Difference;

        public FlagEnum<TreeNodeDifference> TreeDifference => node.TreeDifference;
    }
}
