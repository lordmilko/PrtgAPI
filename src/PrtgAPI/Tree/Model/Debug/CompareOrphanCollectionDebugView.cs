using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree.Internal
{
    [ExcludeFromCodeCoverage]
    internal class CompareOrphanCollectionDebugView
    {
        private CompareOrphanCollection orphan;

        public CompareOrphanCollectionDebugView(CompareOrphanCollection orphan)
        {
            this.orphan = orphan;
        }

        public INodeList<CompareOrphan> Children => orphan.Children;

        public string Name => orphan.Name;

        public int RawType => orphan.RawType;

        public TreeNodeType Type => orphan.Type;

        public FlagEnum<TreeNodeDifference> Difference => orphan.Difference;

        public FlagEnum<TreeNodeDifference> TreeDifference => orphan.TreeDifference;

        public int Generation => orphan.Generation;
    }
}
