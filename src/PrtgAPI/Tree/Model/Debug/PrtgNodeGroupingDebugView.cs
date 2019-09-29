using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree
{
    [ExcludeFromCodeCoverage]
    internal sealed class PrtgNodeGroupingDebugView : PrtgNodeCollectionDebugView
    {
        private PrtgNodeGrouping node;

        public PrtgNodeGroupingDebugView(PrtgNodeGrouping node) : base(node)
        {
            this.node = node;
        }

        public IReadOnlyList<PrtgNode> Group => node.Group;
    }
}
