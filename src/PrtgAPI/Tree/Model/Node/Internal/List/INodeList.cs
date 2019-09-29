using System.Collections.Generic;

namespace PrtgAPI.Tree
{
    /// <summary>
    /// Represents a read-only collection of items that can be stored in a tree.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    public interface INodeList<T> : IReadOnlyList<T>
    {
        /// <summary>
        /// Creates a new list with the specified node appended to the end.
        /// </summary>
        /// <param name="node">The node to add.</param>
        /// <returns>A new list with the specified node appended.</returns>
        INodeList<T> Add(T node);

        /// <summary>
        /// Creates a new list with a collection of nodes appended to the end.
        /// </summary>
        /// <param name="nodes">The nodes to add.</param>
        /// <returns>A new list with the specified nodes appended</returns>
        INodeList<T> AddRange(IEnumerable<T> nodes);

        /// <summary>
        /// Creates a new list with a new node inserted at a specified index.
        /// </summary>
        /// <param name="index">The index to insert the node at.</param>
        /// <param name="node">The node to insert.</param>
        /// <returns>A new list with the new node inserted at the specified index.</returns>
        INodeList<T> Insert(int index, T node);

        /// <summary>
        /// Creates a new list with a collection of nodes inserted at a specified index.
        /// </summary>
        /// <param name="index">The index to insert the new nodes at.</param>
        /// <param name="nodes">The nodes to insert.</param>
        /// <returns>A new list with the new nodes inserted at the specified index.</returns>
        INodeList<T> InsertRange(int index, IEnumerable<T> nodes);

        /// <summary>
        /// Creates a new list with a specified node removed.
        /// </summary>
        /// <param name="index">The index of the node to remove.</param>
        /// <returns>A new list with the specified node removed.</returns>
        INodeList<T> RemoveAt(int index);

        /// <summary>
        /// Creates a new list with a specified node removed.
        /// </summary>
        /// <param name="node">The node to remove.</param>
        /// <returns>A new list with the specified node removed.</returns>
        INodeList<T> Remove(T node);

        /// <summary>
        /// Creates a new list with a specified node replaced with another node.
        /// </summary>
        /// <param name="oldNode">The existing node to replace.</param>
        /// <param name="newNode">The new node to replace the existing node with.</param>
        /// <returns>A new list with the specified node replaced.</returns>
        INodeList<T> Replace(T oldNode, T newNode);

        /// <summary>
        /// Creates a new list with a specified node replaced with a collection of nodes.
        /// </summary>
        /// <param name="oldNode">The existing node to replace.</param>
        /// <param name="newNodes">The new nodes to replace the existing node with.</param>
        /// <returns>A new list with the specified node replaced.</returns>
        INodeList<T> ReplaceRange(T oldNode, IEnumerable<T> newNodes);

        /// <summary>
        /// Returns the zero-based index of the node in the list, or -1 if the node could not be found.
        /// </summary>
        /// <param name="node">The node to search for.</param>
        /// <returns>If the node was found, the zero-based index of that node. Otherwise, -1.</returns>
        int IndexOf(T node);
    }
}