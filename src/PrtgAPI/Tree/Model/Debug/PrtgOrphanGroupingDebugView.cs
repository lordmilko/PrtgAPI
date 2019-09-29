using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree.Internal
{
    [ExcludeFromCodeCoverage]
    internal sealed class PrtgOrphanGroupingDebugView : PrtgOrphanCollectionDebugView
    {
        private PrtgOrphanGrouping orphan;

        public PrtgOrphanGroupingDebugView(PrtgOrphanGrouping orphan) : base(orphan)
        {
            this.orphan = orphan;
        }

        public IReadOnlyList<PrtgOrphan> Group => orphan.Group;
    }
}
