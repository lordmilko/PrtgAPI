using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree
{
    /// <summary>
    /// Represents a visitor capable of recursively accessing the nodes of a <see cref="CompareNode"/> in depth-first order.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class CompareNodeWalker : CompareNodeVisitor
    {
        /// <summary>
        /// Visits the children of a <see cref="TreeNodeType.Node"/> <see cref="CompareNode"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        protected internal override void VisitNode(CompareNode node) => DefaultVisit(node);

        /// <summary>
        /// Visits the children of a <see cref="CompareNodeCollection"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        protected internal override void VisitCollection(CompareNodeCollection node) => DefaultVisit(node);

        private void DefaultVisit(CompareNode node)
        {
            foreach (var child in node.Children)
                Visit(child);
        }
    }
}
