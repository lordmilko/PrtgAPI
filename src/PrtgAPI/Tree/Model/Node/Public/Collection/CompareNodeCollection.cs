using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using PrtgAPI.Tree.Internal;

namespace PrtgAPI.Tree
{
    /// <summary>
    /// Represents a collection of <see cref="CompareNode"/> objects.
    /// </summary>
    [DebuggerTypeProxy(typeof(CompareNodeCollectionDebugView))]
    public abstract class CompareNodeCollection : CompareNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompareNodeCollection"/> class with the orphan this node encapsulates and the parent of this node.
        /// </summary>
        /// <param name="orphan">The orphan this node encapsulates.</param>
        /// <param name="parent">The parent of this node.</param>
        internal CompareNodeCollection(CompareOrphanCollection orphan, CompareNode parent) : base(orphan, parent)
        {
        }

        /// <summary>
        /// If this object supports modifying its children, creates a new <see cref="CompareNode"/> if the specified <paramref name="children"/>
        /// differ from the children stored in this object. Otherwise, throws a <see cref="NotSupportedException"/>.
        /// </summary>
        /// <param name="children">The children to compare against.</param>
        /// <exception cref="NotSupportedException">The object does not support updating its children.</exception>
        /// <returns>If the children do not match those stored in this object, a new object containing the new children. Otherwise, this object.</returns>
        [ExcludeFromCodeCoverage]
        public override CompareNode Update(IEnumerable<CompareNode> children)
        {
            //By default we prohibit modifying the children of a collection. Specific overrides may override this method to re-allow this behavior.
            throw new NotSupportedException($"Modifying the children of a {GetType().Name} is not supported.");
        }

        /// <summary>
        /// Dispatches this orphan to the visitor's <see cref="CompareNodeVisitor{TResult}.VisitCollection(CompareNodeCollection)"/> method
        /// and returns a value of a type specified by the <paramref name="visitor"/>.
        /// </summary>
        /// <typeparam name="T">The type of value to return.</typeparam>
        /// <param name="visitor">The visitor to visit this node with.</param>
        /// <returns>The result of visiting this node.</returns>
        [ExcludeFromCodeCoverage]
        public override T Accept<T>(CompareNodeVisitor<T> visitor) => visitor.VisitCollection(this);

        /// <summary>
        /// Dispatches this orphan to the visitor's <see cref="CompareNodeVisitor.VisitCollection(CompareNodeCollection)"/> method.
        /// </summary>
        /// <param name="visitor">The visitor to visit this orphan with.</param>
        [ExcludeFromCodeCoverage]
        public override void Accept(CompareNodeVisitor visitor) => visitor.VisitCollection(this);
    }
}
