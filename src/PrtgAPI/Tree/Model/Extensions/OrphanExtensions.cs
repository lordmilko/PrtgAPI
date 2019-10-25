using System;
using System.Collections.Generic;
using System.Linq;

namespace PrtgAPI.Tree.Internal
{
    /// <summary>
    /// Provides a set of extension methods for interfacing with <see cref="TreeOrphan"/> objects.
    /// </summary>
    internal static class OrphanExtensions
    {
        #region FindOrphans

        internal static PrtgOrphan FindOrphan(this PrtgOrphan root, Func<PrtgOrphan, bool> predicate) =>
            FindOrphan<PrtgOrphan>(root, predicate);

        internal static IEnumerable<PrtgOrphan> FindOrphans(this PrtgOrphan root, Func<PrtgOrphan, bool> predicate) =>
            FindOrphans<PrtgOrphan>(root, predicate);

        internal static TOrphan FindOrphan<TOrphan>(this PrtgOrphan root, Func<TOrphan, bool> predicate = null) where TOrphan : PrtgOrphan =>
            FindOrphans(root, predicate)?.SingleOrDefault();

        internal static IEnumerable<TOrphan> FindOrphans<TOrphan>(this PrtgOrphan root, Func<TOrphan, bool> predicate = null)
            where TOrphan : PrtgOrphan
        {
            if (predicate == null)
                predicate = v => true;

            return root?.DescendantOrphans().OfType<TOrphan>().Where(predicate);
        }

        #endregion
        #region WithChildren
            #region PrtgOrphan

        internal static TRoot WithChildren<TRoot>(this TRoot root, params PrtgOrphan[] children) where TRoot : PrtgOrphan =>
            WithChildren(root, (IEnumerable<PrtgOrphan>) children);

        internal static TRoot WithChildren<TRoot>(this TRoot root, IEnumerable<PrtgOrphan> children) where TRoot : PrtgOrphan =>
            (TRoot) root?.Update(root.Value, children);

            #endregion
            #region CompareOrphan

        internal static TRoot WithChildren<TRoot>(this TRoot root, params CompareOrphan[] children) where TRoot : CompareOrphan =>
            WithChildren(root, (IEnumerable<CompareOrphan>) children);

        internal static TRoot WithChildren<TRoot>(this TRoot root, IEnumerable<CompareOrphan> children) where TRoot : CompareOrphan =>
            (TRoot) root?.Update(children);

            #endregion
        #endregion
    }
}
