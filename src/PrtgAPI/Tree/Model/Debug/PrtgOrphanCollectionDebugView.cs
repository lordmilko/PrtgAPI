using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree.Internal
{
    [ExcludeFromCodeCoverage]
    internal class PrtgOrphanCollectionDebugView
    {
        private PrtgOrphanCollection orphan;

        public PrtgOrphanCollectionDebugView(PrtgOrphanCollection orphan)
        {
            this.orphan = orphan;
        }

        public INodeList<PrtgOrphan> Children => orphan.Children;

        public string Name => orphan.Name;

        public int RawType => orphan.RawType;

        public PrtgNodeType Type => orphan.Type;

        public int Generation => orphan.Generation;
    }
}
