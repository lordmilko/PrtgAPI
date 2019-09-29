using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace PrtgAPI.Tree.Internal
{
    /// <summary>
    /// Represents a property of an <see cref="IPrtgObject"/> in the PRTG Object Tree that does not have a child -> parent relationship.
    /// </summary>
    internal class PropertyOrphan : PrtgOrphan<PropertyValuePair>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyOrphan"/> class with a specified property and no children.
        /// </summary>
        /// <param name="property">The property to encapsulate in this orphan.</param>
        internal PropertyOrphan(PropertyValuePair property) : base(property, null, PrtgNodeType.Property)
        {
        }

        /// <summary>
        /// Dispatches this orphan to the visitor's <see cref="PrtgOrphanVisitor{TResult}.VisitProperty(PropertyOrphan)"/> method
        /// and returns a value of a type specified by the <paramref name="visitor"/>.
        /// </summary>
        /// <typeparam name="T">The type of value to return.</typeparam>
        /// <param name="visitor">The visitor to visit this orphan with.</param>
        /// <returns>The result of visiting this orphan.</returns>
        [ExcludeFromCodeCoverage]
        internal override T Accept<T>(PrtgOrphanVisitor<T> visitor) => visitor.VisitProperty(this);

        /// <summary>
        /// Dispatches this orphan to the visitor's <see cref="PrtgOrphanVisitor.VisitProperty(PropertyOrphan)"/> method.
        /// </summary>
        /// <param name="visitor">The visitor to visit this orphan with.</param>
        [ExcludeFromCodeCoverage]
        internal override void Accept(PrtgOrphanVisitor visitor) => visitor.VisitProperty(this);

        /// <summary>
        /// Creates a new <see cref="PropertyOrphan"/> if the specified <paramref name="property"/> and <paramref name="children"/>
        /// differ from the values stored in this object.<para/>
        /// </summary>
        /// <param name="property">The property object to compare against.</param>
        /// <param name="children">The children to compare against.</param>
        /// <returns>If the value or children do not match those stored in this object, a new object containing those values. Otherwise, this object.</returns>
        internal override PrtgOrphan<PropertyValuePair> Update(PropertyValuePair property, IEnumerable<PrtgOrphan> children)
        {
            ValidateNoChildren(children);

            if (property != Value)
                return Property(property);

            return this;
        }

        /// <summary>
        /// Creates a new <see cref="PropertyOrphan"/> with the specified <paramref name="property"/> if this <paramref name="property"/> does not
        /// match this object's <see cref="PrtgNode.Value"/>.
        /// </summary>
        /// <param name="property">The value to compare with.</param>
        /// <returns>If the <paramref name="property"/> does not match this object's <see cref="PrtgNode.Value"/>,
        /// a new object encapsulating the value. Otherwise, this object.</returns>
        internal PropertyOrphan WithProperty(PropertyValuePair property) => (PropertyOrphan) WithValue(property);

        /// <summary>
        /// Creates a new <see cref="PropertyNode"/> from this orphan.
        /// </summary>
        /// <param name="parent">The parent of the node.</param>
        /// <returns>A <see cref="PropertyNode"/> that encapsulates this orphan.</returns>
        protected override TreeNode ToNodeCore(TreeNode parent) => new PropertyNode(this, (PrtgNode)parent);

        protected override void ValidateChildren(IEnumerable<PrtgOrphan> children)
        {
            foreach (var child in children)
                throw GetInvalidChildException(child);
        }
    }
}
