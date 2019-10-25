using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree
{
    /// <summary>
    /// Represents a fully abstract visitor capable of accessing the nodes of a <see cref="CompareNode"/>.<para/>
    /// By default this class will only visit the single <see cref="CompareNode"/> passed into its Visit method.<para/>
    /// To utilize a visitor with a default recursive implementation please see <see cref="CompareNodeWalker"/>.
    /// </summary>
    public abstract class CompareNodeVisitor
    {
        /// <summary>
        /// Visits a single <see cref="CompareNode"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        [ExcludeFromCodeCoverage]
        public virtual void Visit(CompareNode node) => node.Accept(this);

        /// <summary>
        /// Visits a single <see cref="TreeNodeType.Node"/> <see cref="CompareNode"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        protected internal abstract void VisitNode(CompareNode node);

        /// <summary>
        /// Visits a single <see cref="CompareNodeCollection"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        protected internal abstract void VisitCollection(CompareNodeCollection node);
    }
}
