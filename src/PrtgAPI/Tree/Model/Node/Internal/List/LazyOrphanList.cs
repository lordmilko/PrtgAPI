using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PrtgAPI.Tree.Internal
{
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(LazyOrphanListDebugView))]
    class LazyOrphanList : OrphanList
    {
        /// <summary>
        /// The underlying store of orphans contained in this list.
        /// </summary>
        internal readonly IEnumerable<TreeOrphan> children;

        /// <summary>
        /// Gets the number of elements contained in the <see cref="LazyOrphanList"/>. Accessing this member may trigger an API call.
        /// </summary>
        internal override int Count => children.Count();

        /// <summary>
        /// Initializes a new instance of the <see cref="LazyOrphanList"/> class with a collection of children.
        /// </summary>
        /// <param name="children">The collection to encapsulate.</param>
        internal LazyOrphanList(IEnumerable<TreeOrphan> children)
        {
            this.children = children;
        }

        /// <summary>
        /// Retrieves the orphan at the specified index.
        /// </summary>
        /// <param name="index">The index of the orphan to retrieve.</param>
        /// <returns>The orphan at the specified index.</returns>
        internal override TreeOrphan Item(int index)
        {
            return children.ElementAt(index);
        }

        /// <summary>
        /// Returns a <see cref="LazyNodeList"/> that encapsulates this <see cref="LazyOrphanList"/>.
        /// </summary>
        /// <param name="parent">The parent to use for the node.</param>
        /// <returns>A <see cref="LazyNodeList"/> that encapsulates this <see cref="LazyOrphanList"/>.</returns>
        protected override TreeNode ToNodeCore(TreeNode parent)
        {
            return new LazyNodeList(this, parent);
        }
    }
}