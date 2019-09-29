using System.Collections.Generic;
using PrtgAPI.Tree.Internal;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree
{
    /// <summary>
    /// Represents a property of an <see cref="IPrtgObject"/> in the PRTG Object Tree.
    /// </summary>
    public class PropertyNode : PrtgNode<PropertyValuePair>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyNode"/> class with the orphan this node encapsulates and the parent of this node.
        /// </summary>
        /// <param name="orphan">The orphan this node encapsulates.</param>
        /// <param name="parent">The parent of this node.</param>
        internal PropertyNode(PropertyOrphan orphan, PrtgNode parent) : base(orphan, parent)
        {
        }

        /// <summary>
        /// Dispatches this node to the visitor's <see cref="PrtgNodeVisitor{TResult}.VisitProperty(PropertyNode)"/> method
        /// and returns a value of a type specified by the <paramref name="visitor"/>.
        /// </summary>
        /// <typeparam name="T">The type of value to return.</typeparam>
        /// <param name="visitor">The visitor to visit this node with.</param>
        /// <returns>The result of visiting this node.</returns>
        [ExcludeFromCodeCoverage]
        public override T Accept<T>(PrtgNodeVisitor<T> visitor) => visitor.VisitProperty(this);

        /// <summary>
        /// Dispatches this node to the visitor's <see cref="PrtgNodeVisitor.VisitProperty(PropertyNode)"/> method.
        /// </summary>
        /// <param name="visitor">The visitor to visit this node with.</param>
        [ExcludeFromCodeCoverage]
        public override void Accept(PrtgNodeVisitor visitor) => visitor.VisitProperty(this);

        /// <summary>
        /// Creates a new <see cref="PropertyNode"/> if the specified <paramref name="property"/> and <paramref name="children"/>
        /// differ from the values stored in this object.<para/>
        /// </summary>
        /// <param name="property">The property object to compare against.</param>
        /// <param name="children">The children to compare against.</param>
        /// <returns>If the value or children do not match those stored in this object, a new object containing those values. Otherwise, this object.</returns>
        public override PrtgNode<PropertyValuePair> Update(PropertyValuePair property, IEnumerable<PrtgNode> children)
        {
            ValidateNoChildren(children);

            if (property != Value)
                return Property(property);

            return this;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return Value.ToString();
        }

        [ExcludeFromCodeCoverage]
        internal override string GetDebuggerName()
        {
            return Value.Property.ToString();
        }
    }
}
