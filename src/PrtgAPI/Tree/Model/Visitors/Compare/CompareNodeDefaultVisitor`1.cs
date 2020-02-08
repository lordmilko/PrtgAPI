namespace PrtgAPI.Tree
{
    /// <summary>
    /// Represents an abstract visitor capable of accessing the nodes of a <see cref="CompareNode"/> tree that
    /// performs a common action for all nodes by default.
    /// </summary>
    internal abstract class CompareNodeDefaultVisitor<TResult> : CompareNodeVisitor<TResult>
    {
        /// <summary>
        /// Visits a single <see cref="CompareNode"/> and produces a value of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        /// <returns>The result of visitng the node.</returns>
        protected internal override TResult VisitNode(CompareNode node) => DefaultVisit(node);

        /// <summary>
        /// Visits a single <see cref="CompareNodeCollection"/> and produces a value of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        /// <returns>The result of visitng the node.</returns>
        protected internal override TResult VisitCollection(CompareNodeCollection node) => DefaultVisit(node);

        /// <summary>
        /// The action to perform for all nodes whose visitor method is not overridden.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        /// <returns>The result of visiting the node.</returns>
        protected abstract TResult DefaultVisit(CompareNode node);
    }
}
