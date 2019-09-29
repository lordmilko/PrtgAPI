using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree
{
    [ExcludeFromCodeCoverage]
    internal class PrtgNodeCollectionDebugView
    {
        private PrtgNodeCollection node;

        public PrtgNodeCollectionDebugView(PrtgNodeCollection node)
        {
            this.node = node;
        }

        public PrtgNode Parent => node.Parent;

        public INodeList<PrtgNode> Children => node.Children;

        public string Name => node.Name;

        public PrtgNodeType Type => node.Type;
    }
}