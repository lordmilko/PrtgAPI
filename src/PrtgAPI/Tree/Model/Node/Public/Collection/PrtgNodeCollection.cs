using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PrtgAPI.Linq;
using PrtgAPI.Tree.Internal;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree
{
    /// <summary>
    /// Represents a collection of nodes found in the PRTG Object Tree.
    /// </summary>
    [DebuggerTypeProxy(typeof(PrtgNodeCollectionDebugView))]
    public abstract class PrtgNodeCollection : PrtgNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PrtgNodeCollection"/> class with the orphan this node encapsulates and the parent of this node.
        /// </summary>
        /// <param name="orphan">The orphan this node encapsulates.</param>
        /// <param name="parent">The parent of this node.</param>
        internal PrtgNodeCollection(PrtgOrphanCollection orphan, PrtgNode parent) : base(orphan, parent)
        {
        }

        /// <summary>
        /// Dispatches this node to the visitor's <see cref="PrtgNodeVisitor{TResult}.VisitCollection(PrtgNodeCollection)"/> method
        /// and returns a value of a type specified by the <paramref name="visitor"/>.
        /// </summary>
        /// <typeparam name="T">The type of value to return.</typeparam>
        /// <param name="visitor">The visitor to visit this node with.</param>
        /// <returns>The result of visiting this node.</returns>
        [ExcludeFromCodeCoverage]
        public override T Accept<T>(PrtgNodeVisitor<T> visitor) => visitor.VisitCollection(this);

        /// <summary>
        /// Dispatches this node to the visitor's <see cref="PrtgNodeVisitor.VisitCollection(PrtgNodeCollection)"/> method.
        /// </summary>
        /// <param name="visitor">The visitor to visit this node with.</param>
        [ExcludeFromCodeCoverage]
        public override void Accept(PrtgNodeVisitor visitor) => visitor.VisitCollection(this);

        internal override PrtgNode Update(ITreeValue value, IEnumerable<PrtgNode> children)
        {
            //By default we prohibit modifying the children of a collection. Specific overrides (such as PropertyNodeCollection
            //and TriggerNodeCollection) may override this method to re-allow this behavior.
            throw new NotSupportedException($"Modifying the children of a {GetType().Name} is not supported.");
        }

        internal PrtgNode ValidateAndUpdate<T>(IEnumerable<PrtgNode> children, PrtgNodeType type, Func<IEnumerable<T>, PrtgNode> create)
        {
            if (children != Children)
            {
                var collection = children.AsCollection();

                var invalid = collection.Where(c => !(c is T)).ToArray();

                if (invalid.Length > 0)
                    throw new InvalidOperationException($"Cannot add child of type '{invalid[0].Type}' to '{GetType().Name}': child must be of type '{type}'.");

                return create(children.Cast<T>());
            }

            return this;
        }
    }
}
