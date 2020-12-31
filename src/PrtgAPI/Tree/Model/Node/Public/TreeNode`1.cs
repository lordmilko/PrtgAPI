using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using PrtgAPI.Tree.Internal;
using PrtgAPI.Dynamic;

namespace PrtgAPI.Tree
{
    /// <summary>
    /// Represents an abstract tree node that is aware of its derived type.
    /// </summary>
    /// <typeparam name="TTreeNode">The type of node that derives from this type.</typeparam>
    public abstract class TreeNode<TTreeNode> : TreeNode, IDynamicMetaObjectProvider where TTreeNode : TreeNode<TTreeNode>
    {
        /// <summary>
        /// Gets the parent of this node.
        /// </summary>
        public new TTreeNode Parent => (TTreeNode) base.Parent;

        /// <summary>
        /// Gets the child nodes of this node.
        /// </summary>
        public new INodeList<TTreeNode> Children => (INodeList<TTreeNode>) base.Children;

        /// <summary>
        /// Gets the name of this node.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeNode{TTreeNode}"/> class with the orphan this node encapsulates and the parent to use for this node.
        /// </summary>
        /// <param name="orphan">The orphan this node encapsulates.</param>
        /// <param name="parent">The parent of this node.</param>
        internal TreeNode(TreeOrphan orphan, TreeNode<TTreeNode> parent) : base(orphan, parent)
        {
            //Extract the children from our orphan counterpart.
            //Since we need a reference to "this", we have to do this in our constructor, not in a static
            //method to our base constructor
            var orphanList = ((IOrphanListProvider) orphan.Children).GetOrphanList();
            var nodeList = orphanList.ToNode<NodeList>(this);

            var list = new NodeList<TTreeNode>(nodeList);

#pragma warning disable 618 //Obsolete
            SetChildren(list);
#pragma warning restore 618 //Obsolete
        }

        #region Index

        /// <summary>
        /// Gets all children directly descended from this node with a specified name, case-sensitively.<para/>
        /// If multiple children are found, a node representing a grouping of these nodes will be returned.<para/>
        /// If no children exist with the specified name, returns null.
        /// </summary>
        /// <param name="name">The name of the children to retrieve.</param>
        /// <returns>If a single match was found, that match. If multiple matches were a found, a grouping containing all matches. Otherwise, null.</returns>
        public TTreeNode this[string name] => this[name, false];

        /// <summary>
        /// Gets all children directly descended from this node with a specified name case sensitively or insensitively.<para/>
        /// If multiple children are found, a node representing a grouping of these nodes will be returned.<para/>
        /// If no children exist with the specified name, returns null.
        /// </summary>
        /// <param name="name">The name of the children to retrieve.</param>
        /// <param name="ignoreCase">Whether to ignore case.</param>
        /// <returns>If a single match was found, that match. If multiple matches were a found, a grouping containing all matches. Otherwise, null.</returns>
        public TTreeNode this[string name, bool ignoreCase]
        {
            get
            {
                IEnumerable<TTreeNode> children = Children;

                var strComp = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

                //In simple scenarios (such as with TableNode objects) we can just look at the Name property,
                //however in more complex scenarios (such as CompareNode) we have to look at the comparees!
                //As such, we leave it up to the derived type to tell us whether their name is a match
                var matches = children.Where(c => IsNameEqual(name, strComp, c)).ToList();

                if (matches.Count > 1)
                {
                    var orphanGrouping = ((IOrphanGroupingProvider) Orphan).CreateIndexerGrouping(matches.Select(m => m.Orphan));
                    var nodeGrouping = CreateIndexerGrouping(orphanGrouping);

                    return nodeGrouping;
                }

                return matches.FirstOrDefault();
            }
        }

        /// <summary>
        /// Indicates whether a node's name is equal to a specified value with a specified string comparison type.
        /// </summary>
        /// <param name="name">The name to compare against.</param>
        /// <param name="comparison">The type of string comparison to perform.</param>
        /// <param name="node">The node whose name should be compared.</param>
        /// <returns>True if the name is considered equal. Otherwise, false.</returns>
        internal abstract bool IsNameEqual(string name, StringComparison comparison, TTreeNode node);

        /// <summary>
        /// Creates a grouping of multiple matches that should be returned from this node's indexer.
        /// </summary>
        /// <param name="orphanGrouping">The orphan grouping that should be encapsulated in a node.</param>
        /// <returns>A grouping of the matches that should be returned from the node's indexer.</returns>
        internal abstract TTreeNode CreateIndexerGrouping(TreeOrphan orphanGrouping);

        #endregion
        #region Traversal

        /// <summary>
        /// Gets a list of ancestor nodes.
        /// </summary>
        /// <returns>A list of ancestor nodes.</returns>
        public IEnumerable<TTreeNode> Ancestors()
        {
            if (Parent == null)
                return Enumerable.Empty<TTreeNode>();

            return Parent.AncestorsAndSelf();
        }

        /// <summary>
        /// Gets a list of ancestor nodes (including this node).
        /// </summary>
        /// <returns>A list containing all ancestor nodes as well as this node.</returns>
        public IEnumerable<TTreeNode> AncestorsAndSelf()
        {
            for (var node = (TTreeNode) this; node != null; node = node.Parent)
                yield return node;
        }

        /// <summary>
        /// Gets a list of descendant nodes.
        /// </summary>
        /// <returns>A list of descendant nodes.</returns>
        public IEnumerable<TTreeNode> DescendantNodes()
        {
            return DescendantNodesInternal(false);
        }

        /// <summary>
        /// Gets a list of descendant nodes (including this node).
        /// </summary>
        /// <returns>A list containing all descendant nodes as well as this node.</returns>
        public IEnumerable<TTreeNode> DescendantNodesAndSelf()
        {
            return DescendantNodesInternal(true);
        }

        /// <summary>
        /// Determines whether the specified node is a descendant of this node.
        /// </summary>
        /// <param name="node">The node to locate in the tree.</param>
        /// <returns>True if the specified node is descended from this node. Otherwise, false.</returns>
        public bool Contains(TTreeNode node)
        {
            if (node == null)
                return false;

            while (node != null)
            {
                if (node == this)
                    return true;

                //Continuously check the child's parents to see if we are one of its ancestors.
                //If so, it was descended from us.
                node = node.Parent;
            }

            return false;
        }

        private IEnumerable<TTreeNode> DescendantNodesInternal(bool includeSelf)
        {
            if (includeSelf)
                yield return (TTreeNode) this;

            foreach (var child in Children)
            {
                foreach (var grandChild in child.DescendantNodesAndSelf())
                    yield return grandChild;
            }
        }

        #endregion

        /// <summary>
        /// Provides access to the <see cref="DynamicMetaObject"/> that dispatches to this object's dynamic members.
        /// </summary>
        /// <param name="parameter">The expression that represents the <see cref="DynamicMetaObject"/> to dispatch to the dynamic members.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> that dispatches to this object's dynamic members.</returns>
        public DynamicMetaObject GetMetaObject(Expression parameter)
        {
            return new DynamicMetaObject<TreeNode<TTreeNode>>(parameter, this, new TreeNodeProxy<TTreeNode>());
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
