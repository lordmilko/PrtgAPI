using System.Collections.Generic;
using System.Diagnostics;
using PrtgAPI.Tree.Internal;

namespace PrtgAPI.Tree
{
    /// <summary>
    /// Represents a lazy collection of tree nodes capable of lazily converting each <see cref="TreeOrphan"/>
    /// in the underlying <see cref="LazyOrphanList"/> to a <see cref="TreeNode"/> as required.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(LazyNodeListDebugView))]
    internal class LazyNodeList : NodeList
    {
        /// <summary>
        /// The underlying store of nodes contained in this list. Nodes will only be created from orphans on demand as required;
        /// if a child is never accessed, the corresponding node for it in this tree will not be created.
        /// </summary>
        internal readonly List<TreeNode> children = new List<TreeNode>();

        /// <summary>
        /// Gets the number of elements contained in the <see cref="LazyOrphanList"/>. Accessing this member may trigger an API call.
        /// </summary>
        internal override int Count => ((LazyOrphanList) Orphan).Count;

        /// <summary>
        /// Initializes a new instance of the <see cref="LazyNodeList"/> class with the orphan list this node encapsulates and the parent of this node.
        /// </summary>
        /// <param name="orphanList">The orphan list this node list encapsulates.</param>
        /// <param name="parent">The parent of this node list.</param>
        internal LazyNodeList(LazyOrphanList orphanList, TreeNode parent) : base(orphanList, parent)
        {
        }

        /// <summary>
        /// Wraps a <see cref="TreeOrphan"/> at the specified <paramref name="index"/> as a <see cref="TreeNode"/>,
        /// or returns the existing <see cref="TreeNode"/> for that <paramref name="index"/> if it was already
        /// wrapped on a previous invocation.
        /// </summary>
        /// <param name="index">The index of the orphan to wrap.</param>
        /// <returns>The node at the specified index.</returns>
        internal override TreeNode ElementToNode(int index)
        {
            lock (children)
            {
                if (index < children.Count)
                {
                    var value = children[index];
                    var original = value;

                    var result = ElementToNode(ref value, index);

                    if (value != original)
                        children[index] = result;

                    return result;
                }
                else
                {
                    TreeNode value = null;

                    var result = ElementToNode(ref value, index);

                    //The index could be anything; add empty elements between the end of the array and the position we're after.
                    //e.g. if we wanted index 15 and the current length is 10, index 9 points to the last position
                    var spaces = index - children.Count;

                    for (var i = 0; i <= spaces; i++)
                        children.Add(null);

                    children[index] = result;

                    return result;
                }
            }
        }
    }
}
