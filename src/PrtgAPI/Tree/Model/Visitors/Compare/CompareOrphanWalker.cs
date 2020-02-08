using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree.Internal
{
    /// <summary>
    /// Represents a visitor capable of recursively accessing the orphans of a <see cref="CompareOrphan"/> tree in depth-first order.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal abstract class CompareOrphanWalker : CompareOrphanVisitor
    {
        /// <summary>
        /// Visits the children of a <see cref="TreeNodeType.Node"/> <see cref="CompareOrphan"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        protected internal override void VisitOrphan(CompareOrphan orphan) => DefaultVisit(orphan);

        /// <summary>
        /// Visits the children of a <see cref="CompareOrphanCollection"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        protected internal override void VisitCollection(CompareOrphanCollection orphan) => DefaultVisit(orphan);

        private void DefaultVisit(CompareOrphan orphan)
        {
            foreach (var child in orphan.Children)
                Visit(child);
        }
    }
}
