using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PrtgAPI.Tree.Internal;

namespace PrtgAPI.Tree
{
    /// <summary>
    /// Provides an interface for indexing and enumerating the elements of a <see cref="NodeList"/>.
    /// </summary>
    /// <typeparam name="TNode">The type of tree node this list encapsulates.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(NodeListDebugView<>))]
    internal class NodeList<TNode> : ListBase<TNode> where TNode : TreeNode
    {
        private NodeList list;

        /// <summary>
        /// Gets the number of elements contained in the <see cref="NodeList{TNode}"/>.
        /// </summary>
        public override int Count => list.Count;

        /// <summary>
        /// Gets the node at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <returns>The node at the specified index.</returns>
        public override TNode this[int index]
        {
            get
            {
                if (index >= 0 && index < list.Count)
                    return (TNode) list.ElementToNode(index);

                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeList{TNode}"/> class with a specified list.
        /// </summary>
        /// <param name="list">The list this type will encapsulate.</param>
        internal NodeList(NodeList list)
        {
            this.list = list;
        }

        protected override ListBase<TNode> CreateList(IEnumerable<TNode> nodes)
        {
            return new NodeList<TNode>(new StrictOrphanList(nodes?.Select(n => n?.Orphan)).ToStandaloneNode<NodeList>());
        }
    }
}
