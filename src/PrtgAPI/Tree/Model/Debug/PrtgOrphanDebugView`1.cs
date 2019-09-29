using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree.Internal
{
    [ExcludeFromCodeCoverage]
    internal class PrtgOrphanDebugView<TValue> where TValue : ITreeValue
    {
        private PrtgOrphan<TValue> orphan;

        public PrtgOrphanDebugView(PrtgOrphan<TValue> orphan)
        {
            this.orphan = orphan;
        }

        public INodeList<PrtgOrphan> Children => orphan.Children;

        public string Name => orphan.Name;

        public int RawType => orphan.RawType;

        public PrtgNodeType Type => orphan.Type;

        public int Generation => orphan.Generation;

        public TValue Value => orphan.Value;
    }
}
