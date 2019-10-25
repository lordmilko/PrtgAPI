using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree
{
    [ExcludeFromCodeCoverage]
    internal sealed class CompareNodeGroupingDebugView : CompareNodeCollectionDebugView
    {
        private CompareNodeGrouping node;

        public CompareNodeGroupingDebugView(CompareNodeGrouping node) : base(node)
        {
            this.node = node;
        }

        public IReadOnlyList<CompareNode> Group => node.Group;
    }
}
