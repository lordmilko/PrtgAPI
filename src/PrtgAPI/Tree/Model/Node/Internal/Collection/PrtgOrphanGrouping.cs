using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace PrtgAPI.Tree.Internal
{
    /// <summary>
    /// Represents a grouping of <see cref="PrtgOrphan"/> objects based on a common orphan property.
    /// </summary>
    [DebuggerDisplay("{GetDebuggerDisplay,nq}")]
    [DebuggerTypeProxy(typeof(PrtgOrphanGroupingDebugView))]
    internal class PrtgOrphanGrouping : PrtgOrphanCollection, INodeGrouping<PrtgOrphan>
    {
        [ExcludeFromCodeCoverage]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string GetDebuggerDisplay
        {
            get
            {
                if (Group.Count == 0)
                    return Name;

                return $"{Name} ({Group.Count})";
            }
        }

        /// <summary>
        /// Gets the orphans contained in this group.
        /// </summary>
        public new IReadOnlyList<PrtgOrphan> Group { get; }

        internal PrtgOrphanGrouping(IEnumerable<PrtgOrphan> children) : base(children, PrtgNodeType.Grouping)
        {
            Group = Children;

#pragma warning disable 618
            SetChildren(Group.SelectMany(g => FlattenCollections(g.Children)));
#pragma warning restore 618
        }

        private string name;

        internal override string Name
        {
            get
            {
                if (name == null)
                    name = GetCollectionName(Group);

                return name;
            }
        }

        /// <summary>
        /// This method is not supported on this orphan type and will always throw a <see cref="NotSupportedException"/>.
        /// </summary>
        /// <typeparam name="T">The type of value to return.</typeparam>
        /// <param name="visitor">The visitor to visit this node with.</param>
        /// <exception cref="NotSupportedException"/>
        /// <returns>The result of visiting this node.</returns>
        [ExcludeFromCodeCoverage]
        internal override T Accept<T>(PrtgOrphanVisitor<T> visitor)
        {
            throw new NotSupportedException($"{nameof(PrtgOrphanGrouping)} cannot be used with visitors of type {nameof(PrtgOrphanVisitor<T>)}.");
        }

        /// <summary>
        /// Dispatches each orphan in this grouping to the specific visit method for the orphan.
        /// </summary>
        /// <param name="visitor">The visitor to visit each orphan in this grouping with.</param>
        [ExcludeFromCodeCoverage]
        internal override void Accept(PrtgOrphanVisitor visitor)
        {
            foreach (var item in Group)
                item.Accept(visitor);
        }

        protected override TreeNode ToNodeCore(TreeNode parent) => new PrtgNodeGrouping(this, (PrtgNode) parent);
    }
}
