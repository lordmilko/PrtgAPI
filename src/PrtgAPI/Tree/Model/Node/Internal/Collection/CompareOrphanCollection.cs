using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree.Internal
{
    /// <summary>
    /// Represents a collection of <see cref="CompareOrphan"/> objects.
    /// </summary>
    [DebuggerTypeProxy(typeof(CompareOrphanCollectionDebugView))]
    internal abstract class CompareOrphanCollection : CompareOrphan
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompareOrphanCollection"/> collection class.
        /// </summary>
        /// <param name="children">The children of this collection.</param>
        /// <param name="type">The type of this collection.</param>
        protected CompareOrphanCollection(IEnumerable<CompareOrphan> children, TreeNodeType type = TreeNodeType.Collection) : base(children, type)
        {
        }

        private string name;

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
        /// Dispatches this orphan to the visitor's <see cref="CompareOrphanVisitor{TResult}.VisitCollection(CompareOrphanCollection)"/> method
        /// and returns a value of a type specified by the <paramref name="visitor"/>.
        /// </summary>
        /// <typeparam name="T">The type of value to return.</typeparam>
        /// <param name="visitor">The visitor to visit this node with.</param>
        /// <returns>The result of visiting this node.</returns>
        [ExcludeFromCodeCoverage]
        internal override T Accept<T>(CompareOrphanVisitor<T> visitor) => visitor.VisitCollection(this);

        /// <summary>
        /// Dispatches this orphan to the visitor's <see cref="CompareOrphanVisitor.VisitCollection(CompareOrphanCollection)"/> method.
        /// </summary>
        /// <param name="visitor">The visitor to visit this orphan with.</param>
        [ExcludeFromCodeCoverage]
        internal override void Accept(CompareOrphanVisitor visitor) => visitor.VisitCollection(this);

        [ExcludeFromCodeCoverage]
        internal override CompareOrphan Update(IEnumerable<CompareOrphan> children)
        {
            //By default we prohibit modifying the children of a collection. Specific overrides may override this method to re-allow this behavior.
            throw new NotSupportedException($"Modifying the children of a {GetType().Name} is not supported.");
        }
    }
}
