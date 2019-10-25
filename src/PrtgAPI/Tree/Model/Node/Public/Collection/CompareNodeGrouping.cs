using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using PrtgAPI.Tree.Internal;

namespace PrtgAPI.Tree
{
    /// <summary>
    /// Represents a grouping of nodes based on a common node value.
    /// </summary>
    [DebuggerDisplay("{GetDebuggerDisplay,nq}")]
    [DebuggerTypeProxy(typeof(CompareNodeGroupingDebugView))]
    public class CompareNodeGrouping : CompareNodeCollection, INodeGrouping<CompareNode>
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
        /// Gets the nodes contained in this group.
        /// </summary>
        public IReadOnlyList<CompareNode> Group { get; }

        internal CompareNodeGrouping(CompareOrphanGrouping orphan, CompareNode parent) : base(orphan, parent)
        {
            //Extract the children from our orphan counterpart
            var orphanList = ((IOrphanListProvider)orphan.Group).GetOrphanList();
            var nodeList = orphanList.ToNode<NodeList>(this);

            var list = new NodeList<CompareNode>(nodeList);

            Group = list;
        }

        /// <summary>
        /// This method is not supported on this node type and will always throw a <see cref="NotSupportedException"/>.
        /// </summary>
        /// <typeparam name="T">The type of value to return.</typeparam>
        /// <param name="visitor">The visitor to visit this node with.</param>
        /// <exception cref="NotSupportedException"/>
        /// <returns>The result of visiting this node.</returns>
        [ExcludeFromCodeCoverage]
        public override T Accept<T>(CompareNodeVisitor<T> visitor)
        {
            throw new NotSupportedException($"{nameof(CompareNodeGrouping)} cannot be used with visitors of type {nameof(CompareNodeVisitor<T>)}.");
        }

        /// <summary>
        /// Dispatches each orphan in this grouping to the specific visit method for the orphan.
        /// </summary>
        /// <param name="visitor">The visitor to visit each orphan in this grouping with.</param>
        [ExcludeFromCodeCoverage]
        public override void Accept(CompareNodeVisitor visitor)
        {
            foreach (var item in Group)
                item.Accept(visitor);
        }
    }
}
