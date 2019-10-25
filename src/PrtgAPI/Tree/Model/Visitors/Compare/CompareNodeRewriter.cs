using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree
{
    /// <summary>
    /// Represents a visitor capable of recursively rewriting the members of a <see cref="CompareNode"/> in depth-first order.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class CompareNodeRewriter : CompareNodeVisitor<CompareNode>
    {
        /// <summary>
        /// Visits the children of a <see cref="CompareNode"/> and replaces the node if any of its children are modified.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        /// <returns>If any children were modified, a new node with the modified children. Otherwise, the original node.</returns>
        protected internal override CompareNode VisitNode(CompareNode node)
        {
            return node.WithChildren(VisitList(node.Children));
        }

        /// <summary>
        /// Visits the children of a <see cref="CompareNodeCollection"/> and replaces the node if any of its children are modified.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        /// <returns>If any children were modified, a new node with the modified children. Otherwise, the original node.</returns>
        protected internal override CompareNode VisitCollection(CompareNodeCollection node)
        {
            return node.WithChildren(VisitList(node.Children));
        }

        #region Helpers

        /// <summary>
        /// Visit the nodes in a list. If any of node is modified after being visited, this method will return a new collection containing the updated nodes.<para/>
        /// If an node is null after being visited, it will be excluded from the returned list.
        /// </summary>
        /// <param name="nodes">The nodes to visit.</param>
        /// <returns>If any node was modified, a new list containing the updated nodes. Otherwise, the original list.</returns>
        protected virtual IReadOnlyList<CompareNode> VisitList(INodeList<CompareNode> nodes)
        {
            return VisitorUtilities.VisitList(nodes, Visit);
        }

        #endregion
    }
}
