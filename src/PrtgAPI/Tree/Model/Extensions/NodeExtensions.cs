using System;
using System.Collections.Generic;
using System.Linq;
using PrtgAPI.Tree.Converters.Text;
using PrtgAPI.Tree.Converters.Text.Writers;

namespace PrtgAPI.Tree
{
    /// <summary>
    /// Provides a set of extension methods for interfacing with <see cref="TreeNode"/> objects.
    /// </summary>
    public static class NodeExtensions
    {
        #region Compare Extensions

        /// <summary>
        /// Compares a <see cref="PrtgNode"/> to another node and creates a tree that describes their differences.
        /// </summary>
        /// <param name="node">The root of the first tree to compare.</param>
        /// <param name="other">The root of the second tree to compare.</param>
        /// <param name="differences">The differences to consider. If no value is specified, all differences will be considered.
        /// To ignore certain differences specify the bitwise complement (~).</param>
        /// <returns>A tree that describes the comparison of both trees.</returns>
        public static CompareNode CompareTo(this PrtgNode node, PrtgNode other, params TreeNodeDifference[] differences)
        {
            var comparer = new CompareTreeVisitor(node, differences);

            var result = comparer.Visit(other);

            return result.ToStandaloneNode<CompareNode>();
        }

        /// <summary>
        /// Reduces a <see cref="CompareNode"/> to a new tree consisting of only the branches that contain differences.<para/>
        /// If every node in the specified tree has <see cref="TreeNodeDifference.None"/>, this method returns null.
        /// </summary>
        /// <param name="node">The root of the tree to reduce.</param>
        /// <returns>A tree with all branches consisting of <see cref="TreeNodeDifference.None"/> removed.</returns>
        public static CompareNode Reduce(this CompareNode node)
        {
            var reducer = new CompareNodeReducer();

            var reduced = reducer.Visit(node);

            return reduced;
        }

        #endregion
        #region FindNodes
            #region PrtgNode

        /// <summary>
        /// Retrieves the single node that matches a specified predicate from the descendants of a <see cref="PrtgNode"/>.<para/>
        /// If no matches are found, this method returns null. If multiple matches are found, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="root">The node whose tree should be searched.</param>
        /// <param name="predicate">The condition to filter by.</param>
        /// <exception cref="InvalidOperationException">Multiple nodes matched the specified predicate.</exception>
        /// <returns>If a single node matched the predicate, the node that matched the predicate. Otherwise, null.</returns>
        public static PrtgNode FindNode(this PrtgNode root, Func<PrtgNode, bool> predicate) =>
            FindNode<PrtgNode>(root, predicate);

        /// <summary>
        /// Retrieves all nodes that match a specified predicate from the descendants of a <see cref="PrtgNode"/>.
        /// </summary>
        /// <param name="root">The node whose tree should be searched.</param>
        /// <param name="predicate">The condition to filter by.</param>
        /// <returns>A collection of nodes that matched the specified predicate.</returns>
        public static IEnumerable<PrtgNode> FindNodes(this PrtgNode root, Func<PrtgNode, bool> predicate) =>
            FindNodes<PrtgNode>(root, predicate);

        /// <summary>
        /// Retrieves the single node that matches a specified type and predicate from the descendants of a <see cref="PrtgNode"/>.<para/>
        /// If no matches are found, this method returns null. If multiple matches are found, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <typeparam name="TNode">The type of node to retrieve.</typeparam>
        /// <param name="root">The node whose tree should be searched.</param>
        /// <param name="predicate">The condition to filter by.</param>
        /// <exception cref="InvalidOperationException">Multiple nodes matched the specified predicate.</exception>
        /// <returns>If a single node matched the predicate, the node that matched the predicate. Otherwise, null.</returns>
        public static TNode FindNode<TNode>(this PrtgNode root, Func<TNode, bool> predicate = null) where TNode : PrtgNode =>
            FindNodes(root, predicate)?.SingleOrDefault();

        /// <summary>
        /// Retrieves all nodes that match a specified predicate and type from the descendants of a <see cref="PrtgNode"/>.
        /// </summary>
        /// <typeparam name="TNode">The type of node to retrieve.</typeparam>
        /// <param name="root">The node whose tree should be searched.</param>
        /// <param name="predicate">The condition to filter by.</param>
        /// <returns>A collection of nodes that matched the specified predicate.</returns>
        public static IEnumerable<TNode> FindNodes<TNode>(this PrtgNode root, Func<TNode, bool> predicate = null)
            where TNode : PrtgNode
        {
            if (predicate == null)
                predicate = v => true;

            return root?.DescendantNodes().OfType<TNode>().Where(predicate);
        }

            #endregion
            #region CompareNode

        /// <summary>
        /// Retrieves the single node that matches a specified predicate from the descendants of a <see cref="CompareNode"/>.<para/>
        /// If no matches are found, this method returns null. If multiple matches are found, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="root">The node whose tree should be searched.</param>
        /// <param name="predicate">The condition to filter by.</param>
        /// <exception cref="InvalidOperationException">Multiple nodes matched the specified predicate.</exception>
        /// <returns>If a single node matched the predicate, the node that matched the predicate. Otherwise, null.</returns>
        public static CompareNode FindNode(this CompareNode root, Func<CompareNode, bool> predicate) =>
            FindNodes(root, predicate)?.SingleOrDefault();

        /// <summary>
        /// Retrieves all nodes that match a specified predicate from the descendants of a <see cref="CompareNode"/>.
        /// </summary>
        /// <param name="root">The node whose tree should be searched.</param>
        /// <param name="predicate">The condition to filter by.</param>
        /// <returns>A collection of nodes that matched the specified predicate.</returns>
        public static IEnumerable<CompareNode> FindNodes(this CompareNode root, Func<CompareNode, bool> predicate)
        {
            if (predicate == null)
                predicate = v => true;

            return root?.DescendantNodes().Where(predicate);
        }

            #endregion
        #endregion
        #region InsertNodesAfter

        /// <summary>
        /// Creates a new <see cref="PrtgNode"/> with a new node inserted after a specified node.
        /// </summary>
        /// <typeparam name="TRoot">The type of the root node.</typeparam>
        /// <param name="root">The root of the tree of nodes.</param>
        /// <param name="nodeInList">The descendant of <paramref name="root"/> the new node should be inserted after.</param>
        /// <param name="newNode">The new node to insert after the <paramref name="nodeInList"/> that will become a child of the <paramref name="nodeInList"/>'s parent.</param>
        /// <returns>A new node with the specified node inserted at the specified position.</returns>
        public static TRoot InsertNodeAfter<TRoot>(this TRoot root, PrtgNode nodeInList, PrtgNode newNode) where TRoot : PrtgNode =>
            root.InsertNodesAfter(nodeInList, new[] { newNode });

        /// <summary>
        /// Creates a new <see cref="PrtgNode"/> with a collection of nodes inserted after a specified node.
        /// </summary>
        /// <typeparam name="TRoot">The type of the root node.</typeparam>
        /// <param name="root">The root of the tree of nodes.</param>
        /// <param name="nodeInList">The descendant of <paramref name="root"/> the new nodes should be inserted after.</param>
        /// <param name="newNodes">The new nodes to insert after the <paramref name="nodeInList"/> that will become children of the <paramref name="nodeInList"/>'s parent.</param>
        /// <returns>A new node with the specified nodes inserted at the specified position.</returns>
        public static TRoot InsertNodesAfter<TRoot>(this TRoot root, PrtgNode nodeInList, params PrtgNode[] newNodes) where TRoot : PrtgNode =>
            root.InsertNodesAfter(nodeInList, (IEnumerable<PrtgNode>) newNodes);

        /// <summary>
        /// Creates a new <see cref="PrtgNode"/> with a collection of nodes inserted after a specified node.
        /// </summary>
        /// <typeparam name="TRoot">The type of the root node.</typeparam>
        /// <param name="root">The root of the tree of nodes.</param>
        /// <param name="nodeInList">The descendant of <paramref name="root"/> the new nodes should be inserted after.</param>
        /// <param name="newNodes">The new nodes to insert after the <paramref name="nodeInList"/> that will become children of the <paramref name="nodeInList"/>'s parent.</param>
        /// <returns>A new node with the specified nodes inserted at the specified position.</returns>
        public static TRoot InsertNodesAfter<TRoot>(this TRoot root, PrtgNode nodeInList, IEnumerable<PrtgNode> newNodes) where TRoot : PrtgNode =>
            (TRoot) new PrtgNodeListEditor(nodeInList, newNodes, ListEditType.InsertAfter).Visit(root);

        #endregion
        #region InsertNodesBefore

        /// <summary>
        /// Creates a new <see cref="PrtgNode"/> with a new node inserted before a specified node.
        /// </summary>
        /// <typeparam name="TRoot">The type of the root node.</typeparam>
        /// <param name="root">The root of the tree of nodes.</param>
        /// <param name="nodeInList">The descendant of <paramref name="root"/> the new node should be inserted before.</param>
        /// <param name="newNode">The new node to insert before the <paramref name="nodeInList"/> that will become a child of the <paramref name="nodeInList"/>'s parent.</param>
        /// <returns>A new node with the specified node inserted at the specified position.</returns>
        public static TRoot InsertNodeBefore<TRoot>(this TRoot root, PrtgNode nodeInList, PrtgNode newNode) where TRoot : PrtgNode =>
            root.InsertNodesBefore(nodeInList, new[] {newNode});

        /// <summary>
        /// Creates a new <see cref="PrtgNode"/> with a collection of nodes inserted before a specified node.
        /// </summary>
        /// <typeparam name="TRoot">The type of the root node.</typeparam>
        /// <param name="root">The root of the tree of nodes.</param>
        /// <param name="nodeInList">The descendant of <paramref name="root"/> the new nodes should be inserted before.</param>
        /// <param name="newNodes">The new nodes to insert before the <paramref name="nodeInList"/> that will become children of the <paramref name="nodeInList"/>'s parent.</param>
        /// <returns>A new node with the specified nodes inserted at the specified position.</returns>
        public static TRoot InsertNodesBefore<TRoot>(this TRoot root, PrtgNode nodeInList, params PrtgNode[] newNodes) where TRoot : PrtgNode =>
            root.InsertNodesBefore(nodeInList, (IEnumerable<PrtgNode>) newNodes);

        /// <summary>
        /// Creates a new <see cref="PrtgNode"/> with a collection of nodes inserted before a specified node.
        /// </summary>
        /// <typeparam name="TRoot">The type of the root node.</typeparam>
        /// <param name="root">The root of the tree of nodes.</param>
        /// <param name="nodeInList">The descendant of <paramref name="root"/> the new nodes should be inserted before.</param>
        /// <param name="newNodes">The new nodes to insert before the <paramref name="nodeInList"/> that will become children of the <paramref name="nodeInList"/>'s parent.</param>
        /// <returns>A new node with the specified nodes inserted at the specified position.</returns>
        public static TRoot InsertNodesBefore<TRoot>(this TRoot root, PrtgNode nodeInList, IEnumerable<PrtgNode> newNodes) where TRoot : PrtgNode =>
            (TRoot) new PrtgNodeListEditor(nodeInList, newNodes, ListEditType.InsertBefore).Visit(root);

        #endregion
        #region RemoveNodes

        /// <summary>
        /// Creates a new <see cref="PrtgNode"/> with specified descendant node removed.
        /// </summary>
        /// <typeparam name="TRoot">The type of the root node.</typeparam>
        /// <param name="root">The node to remove descendant nodes from.</param>
        /// <param name="node">The node to remove. If this value is empty or null, no node will be removed.</param>
        /// <returns>If any nodes were specified, a new node with those descendants removed. Otherwise, the original node.</returns>
        public static TRoot RemoveNode<TRoot>(this TRoot root, PrtgNode node) where TRoot : PrtgNode =>
            RemoveNodes(root, node);

        /// <summary>
        /// Creates a new <see cref="PrtgNode"/> with specified descendant nodes removed.
        /// </summary>
        /// <typeparam name="TRoot">The type of the root node.</typeparam>
        /// <param name="root">The node to remove descendant nodes from.</param>
        /// <param name="nodes">The nodes to remove. If this value is empty or null, no nodes will be removed.</param>
        /// <returns>If any nodes were specified, a new node with those descendants removed. Otherwise, the original node.</returns>
        public static TRoot RemoveNodes<TRoot>(this TRoot root, params PrtgNode[] nodes) where TRoot : PrtgNode =>
            RemoveNodes(root, (IEnumerable<PrtgNode>)nodes);

        /// <summary>
        /// Creates a new <see cref="PrtgNode"/> with specified descendant nodes removed.
        /// </summary>
        /// <typeparam name="TRoot">The type of the root node.</typeparam>
        /// <param name="root">The node to remove descendant nodes from.</param>
        /// <param name="nodes">The nodes to remove. If this value is empty or null, no nodes will be removed.</param>
        /// <returns>If any nodes were specified, a new node with those descendants removed. Otherwise, the original node.</returns>
        public static TRoot RemoveNodes<TRoot>(this TRoot root, IEnumerable<PrtgNode> nodes) where TRoot : PrtgNode =>
            PrtgNodeRemover.RemoveNodes(root, nodes);

        #endregion
        #region ReplaceNodes

        /// <summary>
        /// Creates a new <see cref="PrtgNode"/> with a single descendant <see cref="PrtgNode"/> replaced with another node.
        /// </summary>
        /// <typeparam name="TRoot">The type of the root node.</typeparam>
        /// <param name="root">The node to replace descendant nodes of.</param>
        /// <param name="oldNode">The existing node to replace.</param>
        /// <param name="newNode">The new node to replace the existing node with.</param>
        /// <returns>A new node with the specified descendant node replaced.</returns>
        public static TRoot ReplaceNode<TRoot>(this TRoot root, PrtgNode oldNode, PrtgNode newNode) where TRoot : PrtgNode
        {
            if (oldNode == newNode)
                return root;

            return root.ReplaceNodes(oldNode, newNode);
        }

        /// <summary>
        /// Creates a new <see cref="PrtgNode"/> with a single descendant <see cref="PrtgNode"/> replaced with a collection of nodes.
        /// </summary>
        /// <typeparam name="TRoot">The type of the root node.</typeparam>
        /// <param name="root">The node to replace descendant nodes of.</param>
        /// <param name="oldNode">The existing node to replace.</param>
        /// <param name="newNodes">The new nodes to replace the existing node with.</param>
        /// <returns>A new node with the specified descendant node replaced.</returns>
        public static TRoot ReplaceNodes<TRoot>(this TRoot root, PrtgNode oldNode, params PrtgNode[] newNodes) where TRoot : PrtgNode =>
            root.ReplaceNodes(oldNode, (IEnumerable<PrtgNode>) newNodes);

        /// <summary>
        /// Creates a new <see cref="PrtgNode"/> with a single descendant <see cref="PrtgNode"/> replaced with a collection of nodes.
        /// </summary>
        /// <typeparam name="TRoot">The type of the root node.</typeparam>
        /// <param name="root">The node to replace descendant nodes of.</param>
        /// <param name="oldNode">The existing node to replace.</param>
        /// <param name="newNodes">The new nodes to replace the existing node with.</param>
        /// <returns>A new node with the specified descendant node replaced.</returns>
        public static TRoot ReplaceNodes<TRoot>(this TRoot root, PrtgNode oldNode, IEnumerable<PrtgNode> newNodes) where TRoot : PrtgNode =>
            (TRoot) new PrtgNodeListEditor(oldNode, newNodes, ListEditType.Replace).Visit(root);

        /// <summary>
        /// Creates a new <see cref="PrtgNode"/> with multiple descendant <see cref="PrtgNode"/> objects replaced according to a callback function.
        /// </summary>
        /// <typeparam name="TRoot">The type of the root node.</typeparam>
        /// <param name="root">The node to replace descendant nodes of.</param>
        /// <param name="nodes">The existing nodes to be replaced.</param>
        /// <param name="computeReplacementNode">A function the computes the replacement node.<para/>
        /// If the descendants of a node to be replaced were was also replaced, this function is passed
        /// the original node and its calculated replacement reflecting the changes to its descendants.<para/>
        /// Otherwise, both arguments to this function will be the original node.</param>
        /// <returns>If <paramref name="computeReplacementNode"/> was null, the original node. Otherwise
        /// a new node with all replacements made.</returns>
        public static TRoot ReplaceNodes<TRoot>(this TRoot root, IEnumerable<PrtgNode> nodes,
            Func<PrtgNode, PrtgNode, PrtgNode> computeReplacementNode) where TRoot : PrtgNode
        {
            return PrtgNodeReplacer.Replace(root, nodes, computeReplacementNode);
        }

        #endregion
        #region WithChildren
            #region PrtgNode

        /// <summary>
        /// Creates a new <see cref="PrtgNode"/> with the specified <paramref name="children"/>, or returns the existing node if the specified
        /// collection of children is reference equal the current children of the <paramref name="node"/>.
        /// </summary>
        /// <typeparam name="TRoot">The type of the root node.</typeparam>
        /// <param name="node">The node to modify.</param>
        /// <param name="children">The children to be stored under the node.</param>
        /// <returns>If the children are different from the node's existing children, a new node that contains those children.
        /// Otherwise, the original node.</returns>
        public static TRoot WithChildren<TRoot>(this TRoot node, params PrtgNode[] children) where TRoot : PrtgNode =>
            node.WithChildren((IEnumerable<PrtgNode>) children);

        /// <summary>
        /// Creates a new <see cref="PrtgNode"/> with the specified <paramref name="children"/>, or returns the existing node if the specified
        /// collection of children is reference equal to the current children of the <paramref name="node"/>.
        /// </summary>
        /// <typeparam name="TRoot">The type of the root node.</typeparam>
        /// <param name="node">The node to modify.</param>
        /// <param name="children">The children to be stored under the node.</param>
        /// <returns>If the children are different from the node's existing children, a new node that contains those children.
        /// Otherwise, the original node.</returns>
        public static TRoot WithChildren<TRoot>(this TRoot node, IEnumerable<PrtgNode> children) where TRoot : PrtgNode =>
            (TRoot) node?.Update(node.Value, children);

            #endregion
            #region CompareNode

        /// <summary>
        /// Creates a new <see cref="CompareNode"/> with the specified <paramref name="children"/>, or returns the existing node if the specified
        /// collection of children is reference equal the current children of the <paramref name="node"/>.
        /// </summary>
        /// <typeparam name="TRoot">The type of the root node.</typeparam>
        /// <param name="node">The node to modify.</param>
        /// <param name="children">The children to be stored under the node.</param>
        /// <returns>If the children are different from the node's existing children, a new node that contains those children.
        /// Otherwise, the original node.</returns>
        public static TRoot WithChildren<TRoot>(this TRoot node, params CompareNode[] children) where TRoot : CompareNode =>
            node.WithChildren((IEnumerable<CompareNode>)children);

        /// <summary>
        /// Creates a new <see cref="CompareNode"/> with the specified <paramref name="children"/>, or returns the existing node if the specified
        /// collection of children is reference equal to the current children of the <paramref name="node"/>.
        /// </summary>
        /// <typeparam name="TRoot">The type of the root node.</typeparam>
        /// <param name="node">The node to modify.</param>
        /// <param name="children">The children to be stored under the node.</param>
        /// <returns>If the children are different from the node's existing children, a new node that contains those children.
        /// Otherwise, the original node.</returns>
        public static TRoot WithChildren<TRoot>(this TRoot node, IEnumerable<CompareNode> children) where TRoot : CompareNode =>
            (TRoot) node?.Update(children);

            #endregion
        #endregion
        #region PrettyPrint

        /// <summary>
        /// Pretty prints a <see cref="PrtgNode"/> to a specified writer.
        /// </summary>
        /// <param name="node">The root of the tree to print.</param>
        /// <param name="writer">A writer that will be used for displaying the resulting text.</param>
        public static void PrettyPrint(this PrtgNode node, PrettyWriter writer)
        {
            var visitor = new PrtgNodePrettyTreeVisitor();

            node.Accept(visitor);

            writer.Execute(visitor.Result);
        }

        /// <summary>
        /// Pretty prints a <see cref="CompareNode"/> to a specified writer.
        /// </summary>
        /// <param name="node">The root of the tree to print.</param>
        /// <param name="writer">A writer that will be used for displaying the resulting text.</param>
        public static void PrettyPrint(this CompareNode node, PrettyWriter writer)
        {
            var visitor = new CompareNodePrettyTreeVisitor();

            node.Accept(visitor);

            writer.Execute(visitor.Result);
        }

        #endregion
    }
}
