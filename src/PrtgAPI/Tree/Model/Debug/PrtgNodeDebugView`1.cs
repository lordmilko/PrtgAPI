using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree
{
    [ExcludeFromCodeCoverage]
    internal sealed class PrtgNodeDebugView<TValue> where TValue : ITreeValue
    {
        private PrtgNode<TValue> node;

        public PrtgNodeDebugView(PrtgNode<TValue> node)
        {
            this.node = node;
        }

        public PrtgNode Parent => node.Parent;

        public INodeList<PrtgNode> Children => node.Children;

        public string Name => node.Name;

        public PrtgNodeType Type => node.Type;

        public TValue Value => node.Value;
    }
}
