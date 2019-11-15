using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace PrtgAPI.Tree.Internal
{
    /// <summary>
    /// Represents a collection of tree orphans and serves as the underlying store for a <see cref="StrictNodeList"/>.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(StrictOrphanListDebugView))]
    class StrictOrphanList : OrphanList
    {
        /// <summary>
        /// The underlying store of orphans contained in this list.
        /// </summary>
        internal readonly ArrayElement<TreeOrphan>[] children;

        /// <summary>
        /// Gets the number of elements contained in the <see cref="StrictOrphanList"/>.
        /// </summary>
        internal override int Count => children.Length;

        /// <summary>
        /// Initializes a new instance of the <see cref="StrictOrphanList"/> class with no children.
        /// </summary>
        [ExcludeFromCodeCoverage]
        internal StrictOrphanList()
        {
            children = new ArrayElement<TreeOrphan>[0];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StrictOrphanList"/> class with a single child.
        /// </summary>
        /// <param name="orphan">The orphan to encapsulate.</param>
        [ExcludeFromCodeCoverage]
        internal StrictOrphanList(TreeOrphan orphan)
        {
            if (orphan == null)
                throw new ArgumentNullException(nameof(orphan));

            children = new ArrayElement<TreeOrphan>[1];
            children[0].Value = orphan;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StrictOrphanList"/> class from an existing collection of orphans.
        /// </summary>
        /// <param name="orphans">The collection to encapsulate.</param>
        internal StrictOrphanList(IEnumerable<TreeOrphan> orphans)
        {
            if (orphans == null)
                throw new ArgumentNullException(nameof(orphans));

            var list = orphans as IList<TreeOrphan>;

            if (list == null)
                list = orphans.ToList();

            children = new ArrayElement<TreeOrphan>[list.Count];

            for (var i = 0; i < list.Count; i++)
            {
                if (list[i] == null)
                    throw new ArgumentNullException("List contained a null element.", (Exception) null);

                children[i].Value = list[i];
            }
        }

        /// <summary>
        /// Retrieves the orphan at the specified index.
        /// </summary>
        /// <param name="index">The index of the orphan to retrieve.</param>
        /// <returns>The orphan at the specified index.</returns>
        internal override TreeOrphan Item(int index)
        {
            return children[index];
        }

        /// <summary>
        /// Returns a <see cref="StrictNodeList"/> that encapsulates this <see cref="StrictOrphanList"/>.
        /// </summary>
        /// <param name="parent">The parent to use for the node.</param>
        /// <returns>A <see cref="StrictNodeList"/> that encapsulates this <see cref="StrictOrphanList"/>.</returns>
        protected override TreeNode ToNodeCore(TreeNode parent)
        {
            return new StrictNodeList(this, parent);
        }
    }
}
