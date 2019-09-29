using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Tree
{
    internal class PrtgNodeReplacer : PrtgNodeRewriter
    {
        private PrtgNode[] nodes;
        private Func<PrtgNode, PrtgNode, PrtgNode> computeReplacementNode;

        internal static TRoot Replace<TRoot>(TRoot root, IEnumerable<PrtgNode> nodes,
            Func<PrtgNode, PrtgNode, PrtgNode> computeReplacementNode) where TRoot : PrtgNode
        {
            if (computeReplacementNode == null)
                return root;

            var replacer = new PrtgNodeReplacer(nodes, computeReplacementNode);

            return (TRoot) replacer.Visit(root);
        }

        private PrtgNodeReplacer(IEnumerable<PrtgNode> nodes, Func<PrtgNode, PrtgNode, PrtgNode> computeReplacementNode)
        {
            this.nodes = nodes.ToArray();
            this.computeReplacementNode = computeReplacementNode;
        }

        public override PrtgNode Visit(PrtgNode node)
        {
            var rewritten = base.Visit(node);

            if (nodes.Contains(node))
                rewritten = computeReplacementNode(node, rewritten);

            return rewritten;
        }
    }
}
