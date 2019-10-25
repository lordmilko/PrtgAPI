using System;
using System.Collections.Generic;
using System.Linq;
using PrtgAPI.Tree.Internal;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree
{
    /// <summary>
    /// Represents a comparison of two <see cref="PrtgNode"/> objects.
    /// </summary>
    [DebuggerDisplay("{ComparisonDebuggerDisplay,nq}")]
    [DebuggerTypeProxy(typeof(CompareNodeDebugView))]
    public class CompareNode : TreeNode<CompareNode>
    {
        [ExcludeFromCodeCoverage]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string ComparisonDebuggerDisplay
        {
            get
            {
                return $"{GenerationDebuggerDisplay}, Diff = {Difference}, TreeDiff = {TreeDifference}";
            }
        }

        /// <summary>
        /// The internal object this node encapsulates, storing a one-way parent -> child relationship.
        /// </summary>
        internal new CompareOrphan Orphan => (CompareOrphan) base.Orphan;

        /// <summary>
        /// Gets the node from the first tree that was compared against. If a node at <see cref="Second"/>'s position did not exist
        /// in the first tree, this value is null.
        /// </summary>
        public PrtgNode First => Orphan.First;

        /// <summary>
        /// Gets the node from the second tree that was compared against. If a node at <see cref="First"/>'s position did not exist
        /// in the second tree, this value is null.
        /// </summary>
        public PrtgNode Second => Orphan.Second;

        /// <summary>
        /// Gets the name of the node this object compares.
        /// </summary>
        public override string Name => Orphan.Name;

        /// <summary>
        /// Gets the differences between <see cref="First"/> and <see cref="Second"/>.
        /// </summary>
        public FlagEnum<TreeNodeDifference> Difference => Orphan.Difference;

        /// <summary>
        /// Gets the differences of this node and its descendants.
        /// </summary>
        public FlagEnum<TreeNodeDifference> TreeDifference => Orphan.TreeDifference;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompareNode"/> class with the orphan this node encapsulates and the parent to use for this node.
        /// </summary>
        /// <param name="orphan">The orphan this node encapsulates.</param>
        /// <param name="parent">The parent of this node.</param>
        internal CompareNode(CompareOrphan orphan, CompareNode parent) : base(orphan, parent)
        {
        }

        /// <summary>
        /// Dispatches this orphan to the visitor's <see cref="CompareNodeVisitor{TResult}.VisitNode(CompareNode)"/> method
        /// and returns a value of a type specified by the <paramref name="visitor"/>.
        /// </summary>
        /// <typeparam name="T">The type of value to return.</typeparam>
        /// <param name="visitor">The visitor to visit this node with.</param>
        /// <returns>The result of visiting this node.</returns>
        [ExcludeFromCodeCoverage]
        public virtual T Accept<T>(CompareNodeVisitor<T> visitor) => visitor.VisitNode(this);

        /// <summary>
        /// Dispatches this orphan to the visitor's <see cref="CompareNodeVisitor.VisitNode(CompareNode)"/> method.
        /// </summary>
        /// <param name="visitor">The visitor to visit this orphan with.</param>
        [ExcludeFromCodeCoverage]
        public virtual void Accept(CompareNodeVisitor visitor) => visitor.VisitNode(this);

        /// <summary>
        /// Creates a new <see cref="CompareNode"/> if the specified <paramref name="children"/>
        /// differ from the children stored in this object.
        /// </summary>
        /// <param name="children">The children to compare against.</param>
        /// <returns>If the children do not match those stored in this object, a new object containing the new children. Otherwise, this object.</returns>
        public virtual CompareNode Update(IEnumerable<CompareNode> children)
        {
            if (children != Children)
                return new CompareOrphan(First, Second, GetOrphans(children)).ToStandaloneNode<CompareNode>();

            return this;
        }

        internal override bool IsNameEqual(string name, StringComparison comparison, CompareNode node) =>
            CompareOrphan.IsCompareNameEqual(name, comparison, node.Orphan);

        internal override CompareNode CreateIndexerGrouping(TreeOrphan orphanGrouping) => new CompareNodeGrouping((CompareOrphanGrouping) orphanGrouping, this);

        private IEnumerable<CompareOrphan> GetOrphans(IEnumerable<CompareNode> children)
        {
            if (children == null)
                return null;

            return children.Select(c => c?.Orphan);
        }
    }
}
