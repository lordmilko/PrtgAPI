using System;
using System.Collections.Generic;
using System.Linq;
using PrtgAPI.Linq;
using PrtgAPI.Tree.Internal;

namespace PrtgAPI.Tree
{
    /// <summary>
    /// Represents a pair of <see cref="CompareNode"/> objects modelling a pair of completely different <see cref="PrtgNode"/> trees.
    /// </summary>
    public class CompareNodeRoot : CompareNodeCollection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompareNodeRoot"/> class with the orphan this node encapsulates and the parent of this node.
        /// </summary>
        /// <param name="orphan">The orphan this node encapsulates.</param>
        /// <param name="parent">The parent of this node.</param>
        internal CompareNodeRoot(CompareOrphanRoot orphan, CompareNode parent) : base(orphan, parent)
        {
        }

        /// <summary>
        /// Creates a new <see cref="CompareNode"/> if the specified <paramref name="children"/>
        /// differ from the children stored in this object. If <paramref name="children"/> is null or empty, this method returns null.
        /// </summary>
        /// <param name="children">The children to compare against.</param>
        /// <returns>If no children are specified, null. If one or more children are specified and the children do not match those stored in this object, a new object containing the new children. Otherwise, this object.</returns>
        public override CompareNode Update(IEnumerable<CompareNode> children)
        {
            if (children == null)
                children = Enumerable.Empty<CompareNode>();

            var collection = children.AsCollection();

            switch (collection.Count)
            {
                case 0:
                    return null;
                case 1:
                    return collection.Single()?.Orphan.ToStandaloneNode<CompareNode>();
                case 2:
                    if (collection != Children)
                        return new CompareOrphanRoot(collection.First()?.Orphan, collection.Last()?.Orphan).ToStandaloneNode<CompareNodeRoot>();
                    return this;
                default:
                    throw new InvalidOperationException($"A {GetType().Name} cannot have more than two children.");
            }
        }
    }
}
