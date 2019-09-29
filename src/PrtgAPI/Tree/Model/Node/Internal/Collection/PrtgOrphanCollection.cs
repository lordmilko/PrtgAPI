using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PrtgAPI.Linq;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree.Internal
{
    /// <summary>
    /// Represents a collection of orphans found in the PRTG Object Tree.
    /// </summary>
    [DebuggerTypeProxy(typeof(PrtgOrphanCollectionDebugView))]
    internal abstract class PrtgOrphanCollection : PrtgOrphan
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PrtgOrphanCollection"/> collection class.
        /// </summary>
        /// <param name="children">The children of this collection.</param>
        /// <param name="type">The type of this collection.</param>
        protected PrtgOrphanCollection(IEnumerable<PrtgOrphan> children, PrtgNodeType type = PrtgNodeType.Collection) : base(null, FlattenCollections(children), type)
        {
        }

        private string name;

        [ExcludeFromCodeCoverage]
        internal override string Name
        {
            get
            {
                if (name == null)
                    name = GetCollectionName(Children);

                return name;
            }
        }

        /// <summary>
        /// Dispatches this orphan to the visitor's <see cref="PrtgOrphanVisitor{TResult}.VisitCollection(PrtgOrphanCollection)"/> method
        /// and returns a value of a type specified by the <paramref name="visitor"/>.
        /// </summary>
        /// <typeparam name="T">The type of value to return.</typeparam>
        /// <param name="visitor">The visitor to visit this orphan with.</param>
        /// <returns>The result of visiting this orphan.</returns>
        [ExcludeFromCodeCoverage]
        internal override T Accept<T>(PrtgOrphanVisitor<T> visitor) => visitor.VisitCollection(this);

        /// <summary>
        /// Dispatches this orphan to the visitor's <see cref="PrtgOrphanVisitor.VisitCollection(PrtgOrphanCollection)"/> method.
        /// </summary>
        /// <param name="visitor">The visitor to visit this orphan with.</param>
        [ExcludeFromCodeCoverage]
        internal override void Accept(PrtgOrphanVisitor visitor) => visitor.VisitCollection(this);

        internal override PrtgOrphan Update(ITreeValue value, IEnumerable<PrtgOrphan> children)
        {
            //By default we prohibit modifying the children of a collection. Specific overrides (such as PropertyOrphanCollection
            //and TriggerOrphanCollection) may override this method to re-allow this behavior.
            throw new NotSupportedException($"Modifying the children of a {GetType().Name} is not supported.");
        }

        protected override void ValidateChildren(IEnumerable<PrtgOrphan> children)
        {
            //Anything goes!
        }

        internal PrtgOrphan ValidateAndUpdate<T>(IEnumerable<PrtgOrphan> children, PrtgNodeType type, Func<IEnumerable<T>, PrtgOrphan> create)
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
