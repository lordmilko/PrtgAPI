using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using PrtgAPI.Tree.Internal;

namespace PrtgAPI.Tree
{
    /// <summary>
    /// Represents an abstract tree node that may contain a parent and children.
    /// </summary>
    public abstract class TreeNode
    {
        /// <summary>
        /// Gets the internal orphan that this node encapsulates.
        /// </summary>
        internal TreeOrphan Orphan { get; }

        /// <summary>
        /// Gets the type of this node.
        /// </summary>
        public TreeNodeType Type => Orphan.Type;

        /// <summary>
        /// Gets the child nodes of this node.
        /// </summary>
        public IReadOnlyList<TreeNode> Children { get; private set; }

        [Obsolete("SetChildren should only be called from constructors where the type of the NodeList<T> must be of a more derived type.")]
        internal void SetChildren(IReadOnlyList<TreeNode> children)
        {
            Children = children;
        }

        /// <summary>
        /// Gets the parent of this node.
        /// </summary>
        public TreeNode Parent { get; }

        /// <summary>
        /// The generation of this node. If this object's <see cref="Orphan"/> is converted into a new <see cref="TreeNode"/> the generation of the underlying orphan
        /// will be incremented and applied to the new node.
        /// </summary>
        [ExcludeFromCodeCoverage]
        private int Generation { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeNode"/> class with the orphan this node encapsulates and the parent to use for this node.
        /// </summary>
        /// <param name="orphan">The orphan this node encapsulates.</param>
        /// <param name="parent">The parent of this node.</param>
        internal TreeNode(TreeOrphan orphan, TreeNode parent)
        {
            if (orphan == null)
                throw new ArgumentNullException(nameof(orphan));

            Orphan = orphan;
            Parent = parent;

            //Set the generation of this node; used to identify when a node has been reconstructed due to changes in its tree
            Generation = orphan.Generation;
        }

        #region Debugger

        [ExcludeFromCodeCoverage]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal string GenerationDebuggerDisplay
        {
            get
            {
                var type = Type.ToString();

                if (this is PrtgNode)
                    type = ((PrtgNode) this).Type.ToString();

                var str = $"Name = {GetDebuggerName()}, Type = {type}";

#if DEBUG
                if (Orphan.Generation > 1)
                    return $"{str}, Generation = {Generation}/{Orphan.Generation}";
                else
                    return $"{str}, Generation = {Generation}";
#else
                return str;
#endif
            }
        }

        [ExcludeFromCodeCoverage]
        internal virtual string GetDebuggerName()
        {
            return ToString();
        }

        #endregion
    }
}
