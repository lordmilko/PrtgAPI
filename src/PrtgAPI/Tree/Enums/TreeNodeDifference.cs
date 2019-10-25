using System;

namespace PrtgAPI.Tree
{
    /// <summary>
    /// Specifies differences a <see cref="TreeNode"/> can have when compared with another node.
    /// </summary>
    [Flags]
    public enum TreeNodeDifference
    {
        /// <summary>
        /// There are no differences.
        /// </summary>
        None = 0,
        
        /// <summary>
        /// The type of the nodes are different.
        /// </summary>
        Type = 1,

        /// <summary>
        /// The Parent ID of the node is different.
        /// </summary>
        ParentId = 2,

        /// <summary>
        /// The name of the node is different.
        /// </summary>
        Name = 4,

        /// <summary>
        /// One of the nodes has children while the other does not have any at all.
        /// </summary>
        HasChildren = 8,

        /// <summary>
        /// Both nodes have children however the exact number if different.
        /// </summary>
        NumberOfChildren = 16,

        /// <summary>
        /// The node was added in the second tree.
        /// </summary>
        Added = 32,

        /// <summary>
        /// The node was removed from the first tree.
        /// </summary>
        Removed = 64
    }
}
