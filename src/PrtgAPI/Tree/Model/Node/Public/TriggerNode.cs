using System.Collections.Generic;
using PrtgAPI.Tree.Internal;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree
{
    /// <summary>
    /// Represents a <see cref="NotificationTrigger"/> in the PRTG Object Tree.
    /// </summary>
    public class TriggerNode : PrtgNode<NotificationTrigger>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerNode"/> class with the orphan this node encapsulates and the parent of this node.
        /// </summary>
        /// <param name="orphan">The orphan this node encapsulates.</param>
        /// <param name="parent">The parent of this node.</param>
        internal TriggerNode(TriggerOrphan orphan, PrtgNode parent) : base(orphan, parent)
        {
        }

        /// <summary>
        /// Dispatches this node to the visitor's <see cref="PrtgNodeVisitor{TResult}.VisitTrigger(TriggerNode)"/> method
        /// and returns a value of a type specified by the <paramref name="visitor"/>.
        /// </summary>
        /// <typeparam name="T">The type of value to return.</typeparam>
        /// <param name="visitor">The visitor to visit this node with.</param>
        /// <returns>The result of visiting this node.</returns>
        [ExcludeFromCodeCoverage]
        public override T Accept<T>(PrtgNodeVisitor<T> visitor) => visitor.VisitTrigger(this);

        /// <summary>
        /// Dispatches this node to the visitor's <see cref="PrtgNodeVisitor.VisitTrigger(TriggerNode)"/> method.
        /// </summary>
        /// <param name="visitor">The visitor to visit this node with.</param>
        [ExcludeFromCodeCoverage]
        public override void Accept(PrtgNodeVisitor visitor) => visitor.VisitTrigger(this);

        /// <summary>
        /// Creates a new <see cref="TriggerNode"/> if the specified <paramref name="trigger"/> and <paramref name="children"/>
        /// differ from the values stored in this object.<para/>
        /// </summary>
        /// <param name="trigger">The notification trigger to compare against.</param>
        /// <param name="children">The children to compare against.</param>
        /// <returns>If the value or children do not match those stored in this object, a new object containing those values. Otherwise, this object.</returns>
        public override PrtgNode<NotificationTrigger> Update(NotificationTrigger trigger, IEnumerable<PrtgNode> children)
        {
            ValidateNoChildren(children);

            if (trigger != Value)
                return Trigger(trigger);

            return this;
        }
    }
}
