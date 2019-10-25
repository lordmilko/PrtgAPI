using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree.Internal
{
    [ExcludeFromCodeCoverage]
    internal sealed class CompareOrphanGroupingDebugView : CompareOrphanCollectionDebugView
    {
        private CompareOrphanGrouping orphan;

        public CompareOrphanGroupingDebugView(CompareOrphanGrouping orphan) : base(orphan)
        {
            this.orphan = orphan;
        }

        public IReadOnlyList<CompareOrphan> Group => orphan.Group;
    }
}
