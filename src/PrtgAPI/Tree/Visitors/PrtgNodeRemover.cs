using System.Collections.Generic;
using System.Linq;

namespace PrtgAPI.Tree
{
    /// <summary>
    /// Represents a visitor capable of removing nodes from a <see cref="PrtgNode"/> tree.
    /// </summary>
    internal class PrtgNodeRemover : PrtgNodeRewriter
    {
        /// <summary>
        /// Remove descendant nodes from a parent <see cref="PrtgNode"/>.
        /// </summary>
        /// <typeparam name="TRoot">The type of the root node.</typeparam>

        /// <param name="root">The node to remove child nodes from.</param>
        /// <param name="nodes">The nodes to remove. If this value is null or empty, no nodes will be removed.</param>
        /// <returns></returns>
        internal static TRoot RemoveNodes<TRoot>(TRoot root, IEnumerable<PrtgNode> nodes) where TRoot : PrtgNode
        {
            if (nodes == null)
                return root;

            var list = nodes.ToArray();

            if (list.Length == 0)
                return root;

            var remover = new PrtgNodeRemover(list);

            return (TRoot) remover.Visit(root);
        }

        /// <summary>
        /// The nodes to remove.
        /// </summary>
        private PrtgNode[] nodes;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrtgNodeRemover"/> class.
        /// </summary>
        /// <param name="nodes">The nodes to remove.</param>
        private PrtgNodeRemover(PrtgNode[] nodes)
        {
            this.nodes = nodes;
        }

        public override PrtgNode Visit(PrtgNode node)
        {
            if (nodes.Contains(node))
                return null;

            return base.Visit(node);
        }
    }
}
