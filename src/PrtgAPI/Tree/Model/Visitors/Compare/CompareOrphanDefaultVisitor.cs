using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree.Internal
{
    /// <summary>
    /// Represents an abstract visitor capable of accessing the orphans of a <see cref="CompareOrphan"/> that
    /// performs a common action for all orphans by default.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal abstract class CompareOrphanDefaultVisitor : CompareOrphanVisitor
    {
        /// <summary>
        /// Visits a single <see cref="CompareOrphan"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        protected internal override void VisitOrphan(CompareOrphan orphan) => DefaultVisit(orphan);

        /// <summary>
        /// Visits a single <see cref="CompareOrphanCollection"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        protected internal override void VisitCollection(CompareOrphanCollection orphan) => DefaultVisit(orphan);

        /// <summary>
        /// The action to perform for all orphans whose visitor method is not overridden.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        protected abstract void DefaultVisit(CompareOrphan orphan);
    }
}
