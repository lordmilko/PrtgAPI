using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PrtgAPI.Linq;

namespace PrtgAPI.Tree.Internal
{
    /// <summary>
    /// Represents an abstract tree orphan that does not have a child -> parent relationship.
    /// </summary>
    internal abstract class TreeOrphan
    {
        /// <summary>
        /// Gets the children of this node.
        /// </summary>
        internal IReadOnlyList<TreeOrphan> Children { get; private set; }

        /// <summary>
        /// The number of times this object has been converted into a <see cref="TreeNode"/>.
        /// </summary>
        internal int Generation { get; private set; }

        /// <summary>
        /// Gets the raw type of this node.
        /// </summary>
        internal int RawType { get; }

        /// <summary>
        /// Gets the type of this node.
        /// </summary>
        internal TreeNodeType Type
        {
            get
            {
                if (RawType < (int) TreeNodeType.Collection)
                    return TreeNodeType.Node;

                return (TreeNodeType) RawType;
            }
        }

        [Obsolete("SetChildren should only be called from constructors where the type of the NodeList<T> must be of a more derived type.")]
        protected void SetChildren<TTreeOrphan>(IEnumerable<TTreeOrphan> children) where TTreeOrphan : TreeOrphan
        {
            var list = GetEnumerableChildren(children);

            Children = list;
        }

        protected static IReadOnlyList<TreeOrphan> GetEnumerableChildren<TTreeOrphan>(IEnumerable<TTreeOrphan> children) where TTreeOrphan : TreeOrphan
        {
            if (children == null)
                return new OrphanList<TTreeOrphan>();
            else
            {
                if (children is CachedEnumerableIterator<TTreeOrphan>)
                    return new OrphanList<TTreeOrphan>(new LazyOrphanList(children));
                else
                {
#if DEBUG
                    var orphanList = new OrphanList<TTreeOrphan>(children);

                    Debug.Assert(!orphanList.Contains(null), "TreeOrphans cannot contain null children");

                    return orphanList;
#else
                    return new OrphanList<TTreeOrphan>(children);
#endif
                }
            }
        }

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeOrphan"/> class with no children.
        /// </summary>
        /// <param name="rawType">The raw type of this orphan.</param>
        protected TreeOrphan(int rawType)
        {
            RawType = rawType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeOrphan"/> class with a collection of children.
        /// </summary>
        /// <param name="children">The children of the orphan.</param>
        /// <param name="rawType">The raw type of this orphan.</param>
        protected TreeOrphan(IReadOnlyList<TreeOrphan> children, int rawType)
        {
            Children = children;
            RawType = rawType;
        }

        #endregion

        /// <summary>
        /// Increments the number of generations that have been created from this orphan and returns a new <see cref="TreeNode"/>
        /// that encapsulates this orphan casted to a specified type.
        /// </summary>
        /// <typeparam name="TNode">The type to cast the node to.</typeparam>
        /// <param name="parent">The parent to use for the node.</param>
        /// <returns>A <see cref="TreeNode"/> of type <typeparamref name="TNode"/> that encapsulates this orphan.</returns>
        internal TNode ToNode<TNode>(TreeNode parent) where TNode : TreeNode => (TNode) ToNode(parent);

        /// <summary>
        /// Increments the number of generations that have been created from this orphan and returns a new <see cref="TreeNode"/>
        /// that encapsulates this orphan.
        /// </summary>
        /// <param name="parent">The parent to use for the node.</param>
        /// <returns>A node that encapsulates this orphan, with the <paramref name="parent"/> as its parent.</returns>
        internal TreeNode ToNode(TreeNode parent)
        {
            Generation++;

            return ToNodeCore(parent);
        }

        /// <summary>
        /// Increments the number of generations that have been created from this orphan and returns a new <see cref="TreeNode"/>
        /// that encapsulates this orphan that does not have a parent.
        /// </summary>
        /// <typeparam name="TNode">The type of node the result should be casted to.</typeparam>
        /// <returns>A <see cref="TreeNode"/> of type <typeparamref name="TNode"/> that encapsulates this orphan.</returns>
        internal TNode ToStandaloneNode<TNode>() where TNode : TreeNode => ToNode<TNode>(null);

        /// <summary>
        /// Implements the specific logic required to create a <see cref="TreeNode"/> from this orphan.
        /// </summary>
        /// <param name="parent">The parent to use for the node.</param>
        /// <returns>A node that encapsulates this orphan, with the <paramref name="parent"/> as its parent.</returns>
        protected abstract TreeNode ToNodeCore(TreeNode parent);
    }
}
