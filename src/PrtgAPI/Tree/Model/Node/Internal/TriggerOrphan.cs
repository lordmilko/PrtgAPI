using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree.Internal
{
    /// <summary>
    /// Represents a <see cref="NotificationTrigger"/> in the PRTG Object Tree that does not have a child -> parent relationship.
    /// </summary>
    internal class TriggerOrphan : PrtgOrphan<NotificationTrigger>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerOrphan"/> class with a specified notification trigger and no children.
        /// </summary>
        /// <param name="trigger">The notification trigger to encapsulate in this orphan.</param>
        internal TriggerOrphan(NotificationTrigger trigger) : base(trigger, null, PrtgNodeType.Trigger)
        {
            if (trigger.Inherited)
                throw new InvalidOperationException($"Cannot encapsulate notification trigger '{trigger}': trigger is inherited.");
        }

        /// <summary>
        /// Dispatches this orphan to the visitor's <see cref="PrtgOrphanVisitor{TResult}.VisitTrigger(TriggerOrphan)"/> method
        /// and returns a value of a type specified by the <paramref name="visitor"/>.
        /// </summary>
        /// <typeparam name="T">The type of value to return.</typeparam>
        /// <param name="visitor">The visitor to visit this orphan with.</param>
        /// <returns>The result of visiting this orphan.</returns>
        [ExcludeFromCodeCoverage]
        internal override T Accept<T>(PrtgOrphanVisitor<T> visitor) => visitor.VisitTrigger(this);

        /// <summary>
        /// Dispatches this orphan to the visitor's <see cref="PrtgOrphanVisitor.VisitTrigger(TriggerOrphan)"/> method.
        /// </summary>
        /// <param name="visitor">The visitor to visit this orphan with.</param>
        [ExcludeFromCodeCoverage]
        internal override void Accept(PrtgOrphanVisitor visitor) => visitor.VisitTrigger(this);

        /// <summary>
        /// Creates a new <see cref="TriggerOrphan"/> if the specified <paramref name="trigger"/> and <paramref name="children"/>
        /// differ from the values stored in this object.<para/>
        /// </summary>
        /// <param name="trigger">The notification trigger object to compare against.</param>
        /// <param name="children">The children to compare against.</param>
        /// <returns>If the value or children do not match those stored in this object, a new object containing those values. Otherwise, this object.</returns>
        internal override PrtgOrphan<NotificationTrigger> Update(NotificationTrigger trigger, IEnumerable<PrtgOrphan> children)
        {
            ValidateNoChildren(children);

            if (trigger != Value)
                return Trigger(trigger);

            return this;
        }

        /// <summary>
        /// Creates a new <see cref="TriggerOrphan"/> with the specified <paramref name="trigger"/> if this <paramref name="trigger"/> does not
        /// match this object's <see cref="PrtgNode.Value"/>.
        /// </summary>
        /// <param name="trigger">The value to compare with.</param>
        /// <returns>If the <paramref name="trigger"/> does not match this object's <see cref="PrtgNode.Value"/>,
        /// a new object encapsulating the value. Otherwise, this object.</returns>
        internal TriggerOrphan WithTrigger(NotificationTrigger trigger) => (TriggerOrphan) WithValue(trigger);

        /// <summary>
        /// Creates a new <see cref="TriggerNode"/> from this orphan.
        /// </summary>
        /// <param name="parent">The parent of the node.</param>
        /// <returns>A <see cref="TriggerNode"/> that encapsulates this orphan.</returns>
        protected override TreeNode ToNodeCore(TreeNode parent) => new TriggerNode(this, (PrtgNode) parent);

        protected override void ValidateChildren(IEnumerable<PrtgOrphan> children)
        {
            foreach (var child in children)
                throw GetInvalidChildException(child);
        }
    }
}
