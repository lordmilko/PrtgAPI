using System;
using System.Collections.Generic;
using System.Linq;
using PrtgAPI.Linq;

namespace PrtgAPI.Tree.Internal
{
    /// <summary>
    /// Represents a pair of <see cref="CompareOrphan"/> objects modelling a pair of completely different <see cref="PrtgNode"/> trees.
    /// </summary>
    internal class CompareOrphanRoot : CompareOrphanCollection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompareOrphanRoot"/> class.
        /// </summary>
        /// <param name="first">The first tree to encapsulate.</param>
        /// <param name="second">The second tree to encapsulate.</param>
        internal CompareOrphanRoot(CompareOrphan first, CompareOrphan second) : base(new[] { first, second })
        {
        }

        /// <summary>
        /// Creates a new <see cref="CompareOrphan"/> if the specified <paramref name="children"/>
        /// differ from the children stored in this object. If <paramref name="children"/> is null or empty, this method returns null.
        /// </summary>
        /// <param name="children">The children to compare against.</param>
        /// <returns>If no children are specified, null. If one or more children are specified and the children do not match those stored in this object, a new object containing the new children. Otherwise, this object.</returns>
        internal override CompareOrphan Update(IEnumerable<CompareOrphan> children)
        {
            if (children == null)
                children = Enumerable.Empty<CompareOrphan>();

            var collection = children.AsCollection();

            switch (collection.Count)
            {
                case 0:
                    return null;
                case 1:
                    return collection.Single();
                case 2:
                    if (collection != Children)
                        return new CompareOrphanRoot(collection.First(), collection.Last());
                    return this;
                default:
                    throw new InvalidOperationException($"A {GetType().Name} cannot have more than two children.");
            }
        }

        protected override TreeNode ToNodeCore(TreeNode parent) => new CompareNodeRoot(this, (CompareNode) parent);
    }
}
