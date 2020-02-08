using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree
{
    /// <summary>
    /// Represents an abstract visitor capable of accessing the nodes of a <see cref="CompareNode"/> tree that
    /// performs a common action for all nodes by default.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal abstract class CompareNodeDefaultVisitor : CompareNodeVisitor
    {
        /// <summary>
        /// Visits a single <see cref="CompareNode"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        protected internal override void VisitNode(CompareNode node) => DefaultVisit(node);

        /// <summary>
        /// Visits a single <see cref="CompareNodeCollection"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        protected internal override void VisitCollection(CompareNodeCollection node) => DefaultVisit(node);

        /// <summary>
        /// The action to perform for all nodes whose visitor method is not overridden.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        protected abstract void DefaultVisit(CompareNode node);
    }
}
