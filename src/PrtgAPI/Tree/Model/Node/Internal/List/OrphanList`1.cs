using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree.Internal
{
    interface IOrphanListProvider
    {
        OrphanList GetOrphanList();
    }

    /// <summary>
    /// Provides an interface for indexing and enumerating the elements of a <see cref="OrphanList"/>.
    /// </summary>
    /// <typeparam name="TOrphan">The type of tree orphan this list encapsulates.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(OrphanListDebugView<>))]
    internal class OrphanList<TOrphan> : ListBase<TOrphan>, IOrphanListProvider where TOrphan : TreeOrphan
    {
        private OrphanList list;

        public override int Count => list.Count;

        public override TOrphan this[int index]
        {
            get
            {
                Debug.Assert(index >= 0);
                Debug.Assert(index <= list.Count);

                return (TOrphan) list.Item(index);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrphanList{TOrphan}"/> class as an empty list.
        /// </summary>
        [ExcludeFromCodeCoverage]
        internal OrphanList() : this(new StrictOrphanList())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrphanList{TOrphan}"/> class with a single orphan.
        /// </summary>
        /// <param name="orphan">The orphan to contain in the list.</param>
        [ExcludeFromCodeCoverage]
        internal OrphanList(TreeOrphan orphan) : this(new StrictOrphanList(orphan))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrphanList{TOrphan}"/> class with an orphan list.
        /// </summary>
        /// <param name="list">The list this type will encapsulate.</param>
        internal OrphanList(OrphanList list)
        {
            this.list = list;
        }

        internal OrphanList(IEnumerable<TOrphan> orphans) : this(new StrictOrphanList(orphans))
        {
        }

        [ExcludeFromCodeCoverage]
        protected override ListBase<TOrphan> CreateList(IEnumerable<TOrphan> nodes)
        {
            return new OrphanList<TOrphan>(nodes);
        }

        public OrphanList GetOrphanList()
        {
            return list;
        }
    }
}
