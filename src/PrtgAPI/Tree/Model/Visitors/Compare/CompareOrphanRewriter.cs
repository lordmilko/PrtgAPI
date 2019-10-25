using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree.Internal
{
    /// <summary>
    /// Represents a visitor capable of recursively rewriting the members of a <see cref="CompareOrphan"/> in depth-first order.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal abstract class CompareOrphanRewriter : CompareOrphanVisitor<CompareOrphan>
    {
        /// <summary>
        /// Visits the children of a <see cref="CompareOrphan"/> and replaces the orphan if any of its children are modified.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        /// <returns>If any children were modified, a new orphan with the modified children. Otherwise, the original orphan.</returns>
        protected internal override CompareOrphan VisitOrphan(CompareOrphan orphan)
        {
            return orphan.WithChildren(VisitList(orphan.Children));
        }

        /// <summary>
        /// Visits the children of a <see cref="CompareOrphanCollection"/> and replaces the orphan if any of its children are modified.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        /// <returns>If any children were modified, a new orphan with the modified children. Otherwise, the original orphan.</returns>
        protected internal override CompareOrphan VisitCollection(CompareOrphanCollection orphan)
        {
            return orphan.WithChildren(VisitList(orphan.Children));
        }

        #region Helpers

        /// <summary>
        /// Visit the orphans in a list. If any of orphan is modified after being visited, this method will return a new collection containing the updated orphans.<para/>
        /// If an orphan is null after being visited, it will be excluded from the returned list.
        /// </summary>
        /// <param name="orphans">The orphans to visit.</param>
        /// <returns>If any orphan was modified, a new list containing the updated orphans. Otherwise, the original list.</returns>
        protected virtual IReadOnlyList<CompareOrphan> VisitList(INodeList<CompareOrphan> orphans)
        {
            return VisitorUtilities.VisitList(orphans, Visit);
        }

        #endregion
    }
}
