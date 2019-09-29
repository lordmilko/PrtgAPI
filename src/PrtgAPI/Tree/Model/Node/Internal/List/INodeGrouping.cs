using System.Collections.Generic;

namespace PrtgAPI.Tree
{
    /// <summary>
    /// Represents a grouping or orphans or nodes.
    /// </summary>
    /// <typeparam name="T">The type of items contained in the group.</typeparam>
    internal interface INodeGrouping<T>
    {
        /// <summary>
        /// Gets the nodes contained in this group.
        /// </summary>
        IReadOnlyList<T> Group { get; }
    }
}
