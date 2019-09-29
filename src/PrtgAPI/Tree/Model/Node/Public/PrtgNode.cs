using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PrtgAPI.Tree.Internal;

namespace PrtgAPI.Tree
{
    /// <summary>
    /// Represents a node found in the PRTG Object Tree.
    /// </summary>
    [DebuggerDisplay("{GenerationDebuggerDisplay,nq}")]
    public abstract partial class PrtgNode : TreeNode<PrtgNode>
    {
        /// <summary>
        /// The internal object this node encapsulates, storing a one-way parent -> child relationship.
        /// </summary>
        internal new PrtgOrphan Orphan => (PrtgOrphan) base.Orphan;

        /// <summary>
        /// Gets the value from the PRTG Object Tree that this node encapsulates.
        /// </summary>
        public ITreeValue Value => Orphan.Value;

        /// <summary>
        /// Gets the name of this node.
        /// </summary>
        public override string Name => Orphan.Name;

        /// <summary>
        /// Gets the type of this node.
        /// </summary>
        public new PrtgNodeType Type => Orphan.Type;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrtgNode"/> class with the orphan this node encapsulates and the parent to use for this node.
        /// </summary>
        /// <param name="orphan">The orphan this node encapsulates.</param>
        /// <param name="parent">The parent of this node.</param>
        internal PrtgNode(PrtgOrphan orphan, PrtgNode parent) : base(orphan, parent)
        {
        }

        /// <summary>
        /// Dispatches to the specific visit method for this node type and returns a value of a type specified by the <paramref name="visitor"/>.<para/>
        /// For example, a <see cref="SensorNode"/> will call the <see cref="PrtgNodeVisitor{TResult}.VisitSensor(SensorNode)"/> method. 
        /// </summary>
        /// <typeparam name="T">The type of value to return.</typeparam>
        /// <param name="visitor">The visitor to visit this node with.</param>
        /// <returns>The result of visiting this node.</returns>
        public abstract T Accept<T>(PrtgNodeVisitor<T> visitor);

        /// <summary>
        /// Dispatches to the specific visit method for this node.<para/>
        /// For example, a <see cref="SensorNode"/> will call the <see cref="PrtgNodeVisitor.VisitSensor(SensorNode)"/> method.
        /// </summary>
        /// <param name="visitor">The visitor to visit this node with.</param>
        public abstract void Accept(PrtgNodeVisitor visitor);

        /// <summary>
        /// Creates a new <see cref="PrtgNode"/> if the specified <paramref name="value"/> and <paramref name="children"/>
        /// differ from the values stored in this object.
        /// </summary>
        /// <param name="value">The tree value to compare against.</param>
        /// <param name="children">The children to compare against.</param>
        /// <returns>If the value or children do not match those stored in this object, a new object containing those values. Otherwise, this object.</returns>
        internal abstract PrtgNode Update(ITreeValue value, IEnumerable<PrtgNode> children);

        internal void ValidateNoChildren(IEnumerable<PrtgNode> children)
        {
            if (children != null && children.Count() > 0)
                throw new InvalidOperationException($"Cannot add children to node of type '{Type}': node does not support children.");
        }

        internal override bool IsNameEqual(string name, StringComparison comparison, PrtgNode node) =>
            PrtgOrphan.IsTableNameEqual(name, comparison, node.Orphan);

        internal override PrtgNode CreateIndexerGrouping(TreeOrphan orphanGrouping) => new PrtgNodeGrouping((PrtgOrphanGrouping) orphanGrouping, this);
    }
}
