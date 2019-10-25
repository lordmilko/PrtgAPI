using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree.Internal
{
    [ExcludeFromCodeCoverage]
    internal sealed class CompareOrphanDebugView
    {
        private CompareOrphan orphan;

        public CompareOrphanDebugView(CompareOrphan orphan)
        {
            this.orphan = orphan;
        }

        public INodeList<CompareOrphan> Children => orphan.Children;

        public string Name => orphan.Name;

        public int RawType => orphan.RawType;

        public TreeNodeType Type => orphan.Type;

        public PrtgNode First => orphan.First;

        public PrtgNode Second => orphan.Second;

        public FlagEnum<TreeNodeDifference> Difference => orphan.Difference;

        public FlagEnum<TreeNodeDifference> TreeDifference => orphan.TreeDifference;

        public int Generation => orphan.Generation;
    }
}
