using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace PrtgAPI.Tree.Internal
{
    /// <summary>
    /// Represents a grouping of orphans based on a common orphan value.
    /// </summary>
    [DebuggerDisplay("{GetDebuggerDisplay,nq}")]
    [DebuggerTypeProxy(typeof(CompareOrphanGroupingDebugView))]
    internal class CompareOrphanGrouping : CompareOrphanCollection, INodeGrouping<CompareOrphan>
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
        public IReadOnlyList<CompareOrphan> Group { get; }

        internal CompareOrphanGrouping(IEnumerable<CompareOrphan> children) : base(children, TreeNodeType.Grouping)
        {
            Group = Children;

#pragma warning disable 618
            SetChildren(Group.SelectMany(g => FlattenCollections(g.Children)));
            SetDifference(Group.Select(g => g.Difference).Aggregate((a, b) => a | b));
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
        internal override T Accept<T>(CompareOrphanVisitor<T> visitor)
        {
            throw new NotSupportedException($"{nameof(CompareOrphanGrouping)} cannot be used with visitors of type {nameof(CompareOrphanVisitor<T>)}.");
        }

        /// <summary>
        /// Dispatches each orphan in this grouping to the specific visit method for the orphan.
        /// </summary>
        /// <param name="visitor">The visitor to visit each orphan in this grouping with.</param>
        [ExcludeFromCodeCoverage]
        internal override void Accept(CompareOrphanVisitor visitor)
        {
            foreach (var item in Group)
                item.Accept(visitor);
        }

        protected override TreeNode ToNodeCore(TreeNode parent) => new CompareNodeGrouping(this, (CompareNode) parent);
    }
}
