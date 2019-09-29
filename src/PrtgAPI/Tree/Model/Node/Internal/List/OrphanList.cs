using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace PrtgAPI.Tree.Internal
{
    /// <summary>
    /// Represents a collection of tree orphans and serves as the underlying store for a <see cref="NodeList"/>.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(OrphanListDebugView))]
    class OrphanList : TreeOrphan
    {
        /// <summary>
        /// The underlying store of orphans contained in this list.
        /// </summary>
        internal readonly ArrayElement<TreeOrphan>[] children;

        /// <summary>
        /// Gets the number of elements contained in the <see cref="OrphanList"/>.
        /// </summary>
        internal int Count => children.Length;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrphanList"/> class with no children.
        /// </summary>
        [ExcludeFromCodeCoverage]
        internal OrphanList() : base((int) TreeNodeType.Collection)
        {
            children = new ArrayElement<TreeOrphan>[0];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrphanList"/> class with a single child.
        /// </summary>
        /// <param name="orphan">The orphan to encapsulate.</param>
        [ExcludeFromCodeCoverage]
        internal OrphanList(TreeOrphan orphan) : base((int)TreeNodeType.Collection)
        {
            if (orphan == null)
                throw new ArgumentNullException(nameof(orphan));

            children = new ArrayElement<TreeOrphan>[1];
            children[0].Value = orphan;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrphanList"/> class from an existing collection of orphans.
        /// </summary>
        /// <param name="orphans">The collection to encapsulate.</param>
        internal OrphanList(IEnumerable<TreeOrphan> orphans) : base((int)TreeNodeType.Collection)
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

        internal TreeOrphan Item(int index)
        {
            return children[index];
        }

        /// <summary>
        /// Returns a <see cref="NodeList"/> that encapsulates this <see cref="OrphanList"/>.
        /// </summary>
        /// <param name="parent">The parent to use for the node.</param>
        /// <returns>A <see cref="NodeList"/> that encapsulates this <see cref="OrphanList"/>.</returns>
        protected override TreeNode ToNodeCore(TreeNode parent)
        {
            return new NodeList(this, parent);
        }
    }
}
