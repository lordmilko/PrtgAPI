using System;
using System.Collections.Generic;
using System.Linq;

namespace PrtgAPI.Tree.Internal
{
    interface IOrphanGroupingProvider
    {
        TreeOrphan CreateIndexerGrouping(IEnumerable<TreeOrphan> orphans);
    }

    /// <summary>
    /// Represents an abstract tree orphan that is aware of its derived type.
    /// </summary>
    /// <typeparam name="TTreeOrphan">The type of orphan that derives from this type.</typeparam>
    internal abstract class TreeOrphan<TTreeOrphan> : TreeOrphan, IOrphanGroupingProvider where TTreeOrphan : TreeOrphan<TTreeOrphan>
    {
        /// <summary>
        /// Gets the child orphans of this orphan.
        /// </summary>
        internal new INodeList<TTreeOrphan> Children => (INodeList<TTreeOrphan>) base.Children;

        /// <summary>
        /// Gets the name of this orphan.
        /// </summary>
        internal abstract string Name { get; }

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeOrphan"/> class.
        /// </summary>
        /// <param name="children">The children of this orphan.</param>
        /// <param name="rawType">The raw type of this orphan.</param>
        protected TreeOrphan(IEnumerable<TTreeOrphan> children, int rawType) : base(GetEnumerableChildren(children), rawType)
        {
        }

        #endregion
        #region Index

        internal TTreeOrphan this[string name] => this[name, false];

        internal TTreeOrphan this[string name, bool ignoreCase]
        {
            get
            {
                IEnumerable<TTreeOrphan> children = Children;

                var strComp = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

                var matches = children.Where(c => IsNameEqual(name, strComp, c)).ToList();

                if (matches.Count > 1)
                {
                    var orphanGrouping = (TTreeOrphan) CreateIndexerGrouping(matches);

                    return orphanGrouping;
                }

                return matches.FirstOrDefault();
            }
        }

        #endregion
        #region Traversal

        /// <summary>
        /// Gets a list of descendant orphans.
        /// </summary>
        /// <returns>A list of descendant orphans.</returns>
        internal IEnumerable<TTreeOrphan> DescendantOrphans()
        {
            return DescendantOrphansInternal(false);
        }

        /// <summary>
        /// Gets a list of descendant orphans (including this orphan).
        /// </summary>
        /// <returns>A list containing all descendant orphans as well as this orphan.</returns>
        internal IEnumerable<TTreeOrphan> DescendantOrphansAndSelf()
        {
            return DescendantOrphansInternal(true);
        }

        private IEnumerable<TTreeOrphan> DescendantOrphansInternal(bool includeSelf)
        {
            if (includeSelf)
                yield return (TTreeOrphan) this;

            foreach (var child in Children)
            {
                foreach (var grandChild in child.DescendantOrphansAndSelf())
                    yield return grandChild;
            }
        }

        #endregion

        protected abstract bool IsNameEqual(string name, StringComparison comparison, TTreeOrphan orphan);

        public abstract TreeOrphan CreateIndexerGrouping(IEnumerable<TreeOrphan> orphans);

        protected string GetCollectionName(IReadOnlyList<TTreeOrphan> collection)
        {
            if (collection.Count == 0)
                return "Empty";
            else
            {
                var first = collection.First();

                if (collection.All(c => c.Name == first.Name))
                    return first.Name;
                else
                {
                    var names = collection.Select(c => c.Name).Distinct();

                    return string.Join(" / ", names);
                }
            }
        }

        protected static IEnumerable<TTreeOrphan> FlattenCollections(IEnumerable<TTreeOrphan> children)
        {
            if (children == null)
                yield break;

            foreach (var child in children)
            {
                if (child == null)
                    yield return child;
                else
                {
                    switch ((TreeNodeType) child.RawType)
                    {
                        case TreeNodeType.Collection:
                            foreach (var subChild in FlattenCollections(child.Children))
                                yield return subChild;
                            break;
                        case TreeNodeType.Grouping:
                            foreach (var item in FlattenCollections(((INodeGrouping<TTreeOrphan>) child).Group))
                                yield return item;
                            break;
                        default:
                            yield return child;
                            break;
                    }
                }
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
