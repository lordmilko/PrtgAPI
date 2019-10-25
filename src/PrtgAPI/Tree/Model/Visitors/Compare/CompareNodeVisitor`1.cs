using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree
{
    /// <summary>
    /// Represents a fully abstract visitor capable of accessing the nodes of a <see cref="CompareNode"/>
    /// and produces a value of the type specified by the <typeparamref name="TResult"/> parameter.<para/>
    /// By default this class will only visit the single <see cref="CompareNode"/> passed into its Visit method.<para/>
    /// To utilize a visitor with a default recursive implementation on <see cref="CompareNode"/> objects please see <see cref="CompareNodeRewriter"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of value to return.</typeparam>
    [ExcludeFromCodeCoverage]
    public abstract class CompareNodeVisitor<TResult>
    {
        /// <summary>
        /// Visits a single <see cref="CompareNode"/> and produces a value of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        /// <returns>If the node was not null, the value produced by visiting it. Otherwise, the default value of <typeparamref name="TResult"/>.</returns>
        public virtual TResult Visit(CompareNode node)
        {
            if (node != null)
                return node.Accept(this);

            return default(TResult);
        }

        /// <summary>
        /// Visits a single <see cref="TreeNodeType.Node"/> <see cref="CompareNode"/> and produces a value of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        /// <returns>If the node was not null, the value produced by visiting it. Otherwise, the default value of <typeparamref name="TResult"/>.</returns>
        protected internal abstract TResult VisitNode(CompareNode node);

        /// <summary>
        /// Visits a single <see cref="CompareNodeCollection"/> and produces a value of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        /// <returns>If the node was not null, the value produced by visiting it. Otherwise, the default value of <typeparamref name="TResult"/>.</returns>
        protected internal abstract TResult VisitCollection(CompareNodeCollection node);
    }
}
