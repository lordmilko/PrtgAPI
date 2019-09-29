using System.Diagnostics;
using System.Threading;
using PrtgAPI.Tree.Internal;

namespace PrtgAPI.Tree
{
    /// <summary>
    /// Represents a collection of tree nodes capable of lazily converting each <see cref="TreeOrphan"/>
    /// in the underlying <see cref="OrphanList"/> to a <see cref="TreeNode"/> as required.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(NodeListDebugView))]
    internal class NodeList : TreeNode
    {
        /// <summary>
        /// The underlying store of nodes contained in this list. Nodes will only be created from orphans on demand as required;
        /// if a child is never accessed, the corresponding node for it in this tree will not be created.
        /// </summary>
        internal readonly ArrayElement<TreeNode>[] children;

        /// <summary>
        /// Gets the number of elements contained in the <see cref="NodeList"/>.
        /// </summary>
        public int Count => children.Length;

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeList"/> class with the orphan list this node encapsulates and the parent of this node.
        /// </summary>
        /// <param name="orphanList"></param>
        /// <param name="parent"></param>
        internal NodeList(OrphanList orphanList, TreeNode parent) : base(orphanList, parent)
        {
            children = new ArrayElement<TreeNode>[orphanList.Count];
        }

        /// <summary>
        /// Wraps a <see cref="TreeOrphan"/> at the specified <paramref name="index"/> as a <see cref="TreeNode"/>,
        /// or returns the existing <see cref="TreeNode"/> for that <paramref name="index"/> if it was already
        /// wrapped on a previous invocation.
        /// </summary>
        /// <param name="index">The index of the orphan to wrap.</param>
        /// <returns></returns>
        internal TreeNode ElementToNode(int index) => ElementToNode(ref children[index].Value, index);

        /// <summary>
        /// Wraps a <see cref="TreeOrphan"/> at the specified <paramref name="index"/> as a <see cref="TreeNode"/>,
        /// or returns the existing <see cref="TreeNode"/> for that <paramref name="index"/> if it was already
        /// wrapped on a previous invocation.
        /// </summary>
        /// <param name="element">The value of a field that will cache the wrapped value. If this value is null the node will be created.</param>
        /// <param name="index">The index of the orphan to wrap.</param>
        /// <returns></returns>
        private TreeNode ElementToNode(ref TreeNode element, int index)
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
