using System.Diagnostics;
using PrtgAPI.Tree.Internal;

namespace PrtgAPI.Tree
{
    /// <summary>
    /// Represents a strict collection of tree nodes capable of lazily converting each <see cref="TreeOrphan"/>
    /// in the underlying <see cref="StrictOrphanList"/> to a <see cref="TreeNode"/> as required.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(StrictNodeListDebugView))]
    internal class StrictNodeList : NodeList
    {
        /// <summary>
        /// The underlying store of nodes contained in this list. Nodes will only be created from orphans on demand as required;
        /// if a child is never accessed, the corresponding node for it in this tree will not be created.
        /// </summary>
        internal readonly ArrayElement<TreeNode>[] children;

        /// <summary>
        /// Gets the number of elements contained in the <see cref="StrictNodeList"/>.
        /// </summary>
        internal override int Count => children.Length;

        /// <summary>
        /// Initializes a new instance of the <see cref="StrictNodeList"/> class with the orphan list this node encapsulates and the parent of this node.
        /// </summary>
        /// <param name="orphanList">The orphan list this node list encapsulates.</param>
        /// <param name="parent">The parent of this node list.</param>
        internal StrictNodeList(StrictOrphanList orphanList, TreeNode parent) : base(orphanList, parent)
        {
            children = new ArrayElement<TreeNode>[orphanList.Count];
        }

        /// <summary>
        /// Wraps a <see cref="TreeOrphan"/> at the specified <paramref name="index"/> as a <see cref="TreeNode"/>,
        /// or returns the existing <see cref="TreeNode"/> for that <paramref name="index"/> if it was already
        /// wrapped on a previous invocation.
        /// </summary>
        /// <param name="index">The index of the orphan to wrap.</param>
        /// <returns>The node at the specified index.</returns>
        internal override TreeNode ElementToNode(int index) => ElementToNode(ref children[index].Value, index);
    }
}
