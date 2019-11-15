using System.Diagnostics;
using System.Threading;
using PrtgAPI.Tree.Internal;

namespace PrtgAPI.Tree
{
    /// <summary>
    /// Represents an abstract collection of tree nodes capable of lazily converting each <see cref="TreeOrphan"/>
    /// in the underlying <see cref="OrphanList"/> to a <see cref="TreeNode"/> as required.
    /// </summary>
    internal abstract class NodeList : TreeNode
    {
        /// <summary>
        /// Gets the number of elements contained in the <see cref="StrictNodeList"/>.
        /// </summary>
        internal abstract int Count { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeList"/> class with the orphan list this node encapsulates and the parent of this node.
        /// </summary>
        /// <param name="orphanList">The orphan list this node list encapsulates.</param>
        /// <param name="parent">The parent of this node list.</param>
        internal NodeList(OrphanList orphanList, TreeNode parent) : base(orphanList, parent)
        {
        }

        /// <summary>
        /// Wraps a <see cref="TreeOrphan"/> at the specified <paramref name="index"/> as a <see cref="TreeNode"/>,
        /// or returns the existing <see cref="TreeNode"/> for that <paramref name="index"/> if it was already
        /// wrapped on a previous invocation.
        /// </summary>
        /// <param name="index">The index of the orphan to wrap.</param>
        /// <returns>The node at the specified index.</returns>
        internal abstract TreeNode ElementToNode(int index);

        /// <summary>
        /// Wraps a <see cref="TreeOrphan"/> at the specified <paramref name="index"/> as a <see cref="TreeNode"/>,
        /// or returns the existing <see cref="TreeNode"/> for that <paramref name="index"/> if it was already
        /// wrapped on a previous invocation.
        /// </summary>
        /// <param name="element">The value of a field that will cache the wrapped value. If this value is null the node will be created.</param>
        /// <param name="index">The index of the orphan to wrap.</param>
        /// <returns>The node at the specified index.</returns>
        protected TreeNode ElementToNode(ref TreeNode element, int index)
        {
            var result = element;

            //If the element does not have a value
            if (result == null)
            {
                //Get the orphan from the orphan list we want to convert
                var orphan = ((OrphanList) Orphan).Item(index);

                Debug.Assert(orphan != null);

                //Wrap the orphan as a node
                Interlocked.CompareExchange(ref element, orphan.ToNode(Parent), null);

                result = element;
            }

            return result;
        }
    }
}
