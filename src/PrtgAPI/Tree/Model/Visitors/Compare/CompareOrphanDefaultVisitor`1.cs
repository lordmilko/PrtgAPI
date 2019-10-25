namespace PrtgAPI.Tree.Internal
{
    /// <summary>
    /// Represents an abstract visitor capable of accessing the orphans of a <see cref="CompareOrphan"/> that
    /// performs a common action for all nodes by default.
    /// </summary>
    internal abstract class CompareOrphanDefaultVisitor<TResult> : CompareOrphanVisitor<TResult>
    {
        /// <summary>
        /// Visits a single <see cref="CompareOrphan"/> and produces a value of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        /// <returns>The result of visitng the orphan.</returns>
        protected internal override TResult VisitOrphan(CompareOrphan orphan) => DefaultVisit(orphan);

        /// <summary>
        /// Visits a single <see cref="CompareOrphanCollection"/> and produces a value of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        /// <returns>The result of visitng the orphan.</returns>
        protected internal override TResult VisitCollection(CompareOrphanCollection orphan) => DefaultVisit(orphan);

        /// <summary>
        /// The action to perform for all orphans whose visitor method is not overridden.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        /// <returns>The result of visiting the orphan.</returns>
        protected abstract TResult DefaultVisit(CompareOrphan orphan);
    }
}
