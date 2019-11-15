using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PrtgAPI.Linq;

namespace PrtgAPI.Tree.Internal
{
    /// <summary>
    /// Represents an orphan found in the PRTG Object Tree.
    /// </summary>
    internal abstract partial class PrtgOrphan : TreeOrphan<PrtgOrphan>
    {
        internal static bool IsTableNameEqual(string name, StringComparison comparison, PrtgOrphan orphan) => orphan.Name.Equals(name, comparison);

        /// <summary>
        /// Gets the value from the PRTG Object Tree that this orphan encapsulates.
        /// </summary>
        internal ITreeValue Value { get; }

        /// <summary>
        /// Gets the name of this orphan.
        /// </summary>
        internal override string Name => Value.Name;

        /// <summary>
        /// Gets the type of this orphan.
        /// </summary>
        internal new PrtgNodeType Type => (PrtgNodeType) RawType;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PrtgOrphan"/> class.
        /// </summary>
        /// <param name="value">The value this orphan encapsulates.</param>
        /// <param name="children">The children of this orphan.</param>
        /// <param name="type">The type of this orphan.</param>
        internal PrtgOrphan(ITreeValue value, IEnumerable<PrtgOrphan> children, PrtgNodeType type) : base(ReduceChildren(children), (int) type)
        {
            Value = value;

            //We only have a cached enumeration of children when we lazily retrieved them from PRTG in a TreeLevelBuilder.
            //As such, skip validating the children.
            if (!(children is CachedEnumerableIterator<PrtgOrphan>))
                ValidateChildren(Children);
        }

        private static IEnumerable<PrtgOrphan> ReduceChildren(IEnumerable<PrtgOrphan> children)
        {
            //If we have a CachedEnumerableIterator we implicitly are a safe collection that doesn't need reducing
            if (children is CachedEnumerableIterator<PrtgOrphan>)
                return children;

            return ReduceChildrenInternal(children);
        }

        private static IEnumerable<PrtgOrphan> ReduceChildrenInternal(IEnumerable<PrtgOrphan> children)
        {
            if (children == null)
                yield break;

            foreach (var child in children)
            {
                if (child == null)
                    yield return null;
                else if (child.Type != PrtgNodeType.Grouping)
                    yield return child;
                else
                {
                    foreach (var g in ((PrtgOrphanGrouping) child).Group)
                        yield return g;
                }
            }
        }

        #endregion

        /// <summary>
        /// Dispatches to the specific visit method for this orphan type and returns a value of a type specified by the <paramref name="visitor"/>.<para/>
        /// For example, a <see cref="SensorOrphan"/> will call the <see cref="PrtgOrphanVisitor{TResult}.VisitSensor(SensorOrphan)"/> method. 
        /// </summary>
        /// <typeparam name="T">The type of value to return.</typeparam>
        /// <param name="visitor">The visitor to visit this orphan with.</param>
        /// <returns>The result of visiting this orphan.</returns>
        internal abstract T Accept<T>(PrtgOrphanVisitor<T> visitor);

        /// <summary>
        /// Dispatches to the specific visit method for this orphan.<para/>
        /// For example, a <see cref="SensorOrphan"/> will call the <see cref="PrtgOrphanVisitor.VisitSensor(SensorOrphan)"/> method.
        /// </summary>
        /// <param name="visitor">The visitor to visit this orphan with.</param>
        internal abstract void Accept(PrtgOrphanVisitor visitor);

        internal abstract PrtgOrphan Update(ITreeValue value, IEnumerable<PrtgOrphan> children);

        /// <summary>
        /// Validates that the children of this orphan are valid for its <see cref="PrtgNodeType"/>.
        /// </summary>
        protected abstract void ValidateChildren(IEnumerable<PrtgOrphan> children);

        protected void ValidateCommonChild(PrtgOrphan child)
        {
            Debug.Assert(child.Type != PrtgNodeType.Grouping, "Groupings should have been reduced to their grouped orphans.");

            if (child.Type == PrtgNodeType.Collection)
                ValidateChildren(child.Children);
            else
            {
                switch (child.Type)
                {
                    case PrtgNodeType.Property:
                    case PrtgNodeType.Trigger:
                        break;
                    default:
                        throw GetInvalidChildException(child);
                }
            }
        }

        protected void ValidateNoChildren(IEnumerable<PrtgOrphan> children)
        {
            if (children != null && children.Count() > 0)
                throw new InvalidOperationException($"Cannot add children to orphan of type '{Type}': orphan does not support children.");
        }

        protected Exception GetInvalidChildException(PrtgOrphan child)
        {
            return new InvalidOperationException($"Node '{child} (ID: {child.Value.Id})' of type '{child.Type}' cannot be a child of a node of type '{Type}'.");
        }

        protected override bool IsNameEqual(string name, StringComparison comparison, PrtgOrphan orphan) =>
            IsTableNameEqual(name, comparison, orphan);

        public override TreeOrphan CreateIndexerGrouping(IEnumerable<TreeOrphan> orphans) => new PrtgOrphanGrouping(orphans.Cast<PrtgOrphan>());
    }
}
