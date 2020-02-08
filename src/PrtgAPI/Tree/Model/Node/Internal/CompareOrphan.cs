using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree.Internal
{
    /// <summary>
    /// Represents a comparison of two <see cref="PrtgNode"/> objects.
    /// </summary>
    [DebuggerTypeProxy(typeof(CompareOrphanDebugView))]
    internal class CompareOrphan : TreeOrphan<CompareOrphan>
    {
        internal static bool IsCompareNameEqual(string name, StringComparison comparison, CompareOrphan orphan)
        {
            if (orphan.First == null && orphan.Second == null)
                return false;

            //If Node is not null, Name is Node.Name
            if (orphan.Name.Equals(name, comparison))
                return true;

            //If both Node and Other were not null, we'll need to inspect Other directly
            //to see if its name potentially matches.
            if (orphan.Difference.Contains(TreeNodeDifference.Name))
            {
                if (orphan.Second != null && orphan.Second.Name.Equals(name, comparison))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the node from the first tree that was compared against. If a node at <see cref="Second"/>'s position did not exist
        /// in the first tree, this value is null.
        /// </summary>
        public virtual PrtgNode First { get; }

        /// <summary>
        /// Gets the node from the second tree that was compared against. If a node at <see cref="First"/>'s position did not exist
        /// in the second tree, this value is null.
        /// </summary>
        public virtual PrtgNode Second { get; }

        /// <summary>
        /// Gets the name of the node this object compares.
        /// </summary>
        internal override string Name => First?.Name ?? Second?.Name;

        /// <summary>
        /// Gets the differences between <see cref="First"/> and <see cref="Second"/>.
        /// </summary>
        internal FlagEnum<TreeNodeDifference> Difference { get; private set; }

        private FlagEnum<TreeNodeDifference>? treeDifference;

        /// <summary>
        /// Gets the differences of this node and its descendants.
        /// </summary>
        internal FlagEnum<TreeNodeDifference> TreeDifference
        {
            get
            {
                if (treeDifference == null)
                {
                    var differences = new List<TreeNodeDifference>();

                    differences.AddRange(Children.SelectMany(c => c.TreeDifference.GetValues()));

                    differences.AddRange(Difference.GetValues());

                    var distinct = differences.Distinct().ToList();

                    if (distinct.Count > 1 && distinct.Contains(TreeNodeDifference.None))
                        distinct.Remove(TreeNodeDifference.None);

                    treeDifference = new FlagEnum<TreeNodeDifference>(distinct.ToArray());
                }

                return treeDifference.Value;
            }
        }

        [Obsolete("SetDifference should only be called from constructors.")]
        protected void SetDifference(FlagEnum<TreeNodeDifference> value)
        {
            Difference = value;
        }

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CompareOrphan"/> class.
        /// </summary>
        /// <param name="first">The node from the first tree this object should compare.</param>
        /// <param name="second">The node from the second tree this object should compare.</param>
        /// <param name="children">The children of this orphan.</param>
        /// <param name="interestedDifferences">The differences to consider. If no value is specified, all differences will be considered.</param>
        internal CompareOrphan(PrtgNode first, PrtgNode second, IEnumerable<CompareOrphan> children, TreeNodeDifference[] interestedDifferences = null) : base(FlattenCollections(children), (int) TreeNodeType.Node)
        {
            if (first?.Type == PrtgNodeType.Grouping || second?.Type == PrtgNodeType.Grouping)
                throw new ArgumentException($"Cannot create a comparison containing a node of type {nameof(PrtgNodeType.Grouping)}. Please perform a comparison using a specific grouped node.");

            First = first;
            Second = second;

            Difference = GetDifference(first, second, interestedDifferences);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompareOrphan"/> class with a custom type.
        /// </summary>
        /// <param name="children">The children of this orphan.</param>
        /// <param name="type">The type of this orphan.</param>
        protected CompareOrphan(IEnumerable<CompareOrphan> children, TreeNodeType type) : base(FlattenCollections(children), (int) type)
        {
        }

        #endregion

        private FlagEnum<TreeNodeDifference> GetDifference(PrtgNode first, PrtgNode second, TreeNodeDifference[] interestedDifferences)
        {
            var differences = new List<TreeNodeDifference>();

            if (first == null && second == null)
                return TreeNodeDifference.None;

            if (first != null && second == null)
                differences.Add(TreeNodeDifference.Removed);

            if (first == null && second != null)
                differences.Add(TreeNodeDifference.Added);

            if (first != null && second != null)
            {
                if (first.GetType() != second.GetType())
                    differences.Add(TreeNodeDifference.Type);

                //Collections don't have values, so there's no point checking value properties for them
                if (first.Value != null && second.Value != null)
                {
                    Debug.Assert(first.Value.Id == second.Value.Id, "If two nodes had different IDs one of them should have been replaced in the comparison visitor with null.");

                    if (first.Value.ParentId != second.Value.ParentId)
                        differences.Add(TreeNodeDifference.ParentId);

                    if (first.Value.Name != second.Value.Name)
                        differences.Add(TreeNodeDifference.Name);

                    if (first.Value is ISensorOrDeviceOrGroupOrProbe && second.Value is ISensorOrDeviceOrGroupOrProbe)
                    {
                        if (((ISensorOrDeviceOrGroupOrProbe) first.Value).Position !=
                            ((ISensorOrDeviceOrGroupOrProbe) second.Value).Position)
                        {
                            differences.Add(TreeNodeDifference.Position);
                        }
                    }

                    if (first.Value is PropertyValuePair && second.Value is PropertyValuePair)
                    {
                        if (!string.Equals(((PropertyValuePair) first.Value).Value?.ToString(), ((PropertyValuePair) second.Value).Value?.ToString()))
                            differences.Add(TreeNodeDifference.Value);
                    }
                }

                if ((first.Children.Count > 0 && second.Children.Count == 0 || second.Children.Count > 0 && first.Children.Count == 0))
                    differences.Add(TreeNodeDifference.HasChildren);

                if (first.Children.Count != second.Children.Count)
                    differences.Add(TreeNodeDifference.NumberOfChildren);
            }

            Debug.Assert(
                (differences.Contains(TreeNodeDifference.Added) ||
                 differences.Contains(TreeNodeDifference.Removed)) &&
                 differences.Count > 1 ?
                    false :
                    true,
                 "Cannot contain more than one difference when the difference is Added or Removed"
            );

            if (interestedDifferences != null && interestedDifferences.Length > 0)
            {
                var interestedFlags = new FlagEnum<TreeNodeDifference>(interestedDifferences);

                if (interestedFlags != TreeNodeDifference.None)
                {
                    var proposedDifferences = new FlagEnum<TreeNodeDifference>(differences.ToArray());

                    var result = proposedDifferences & interestedFlags;

                    return result;
                }
            }

            return new FlagEnum<TreeNodeDifference>(differences.ToArray());
        }

        /// <summary>
        /// Dispatches this orphan to the visitor's <see cref="CompareOrphanVisitor{TResult}.VisitOrphan(CompareOrphan)"/> method
        /// and returns a value of a type specified by the <paramref name="visitor"/>.
        /// </summary>
        /// <typeparam name="T">The type of value to return.</typeparam>
        /// <param name="visitor">The visitor to visit this node with.</param>
        /// <returns>The result of visiting this node.</returns>
        [ExcludeFromCodeCoverage]
        internal virtual T Accept<T>(CompareOrphanVisitor<T> visitor) => visitor.VisitOrphan(this);

        /// <summary>
        /// Dispatches this orphan to the visitor's <see cref="CompareOrphanVisitor.VisitOrphan(CompareOrphan)"/> method.
        /// </summary>
        /// <param name="visitor">The visitor to visit this orphan with.</param>
        [ExcludeFromCodeCoverage]
        internal virtual void Accept(CompareOrphanVisitor visitor) => visitor.VisitOrphan(this);

        internal virtual CompareOrphan Update(IEnumerable<CompareOrphan> children)
        {
            if (children != Children)
                return new CompareOrphan(First, Second, children);

            return this;
        }

        /// <summary>
        /// Creates a new <see cref="CompareNode"/> from this orphan.
        /// </summary>
        /// <param name="parent">The parent of the node.</param>
        /// <returns>A <see cref="CompareNode"/> that encapsulates this orphan.</returns>
        protected override TreeNode ToNodeCore(TreeNode parent) => new CompareNode(this, (CompareNode) parent);

        protected override bool IsNameEqual(string name, StringComparison comparison, CompareOrphan orphan) =>
            IsCompareNameEqual(name, comparison, orphan);

        public override TreeOrphan CreateIndexerGrouping(IEnumerable<TreeOrphan> orphans) => new CompareOrphanGrouping(orphans.Cast<CompareOrphan>());
    }
}
