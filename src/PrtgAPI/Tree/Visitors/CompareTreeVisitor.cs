using System;
using System.Collections.Generic;
using System.Linq;
using PrtgAPI.Tree.Internal;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree
{
    internal class CompareTreeVisitor : PrtgNodeDefaultVisitor<CompareOrphan>
    {
        private PrtgNode first;

        internal CompareTreeVisitor(PrtgNode first)
        {
            this.first = first;
        }

        public override CompareOrphan Visit(PrtgNode second)
        {
            var result = VisitTwo(first, second).ToArray();

            switch(result.Length)
            {
                case 1:
                    return result[0];
                case 2:
                    return new CompareOrphanRoot(result[0], result[1]);
                default:
                    throw new NotImplementedException($"Don't know how to handle a collection of {result.Length} nodes.");
            }
        }

        private IEnumerable<CompareOrphan> VisitTwo(PrtgNode first, PrtgNode second)
        {
            //If the IDs of objects are different, the trees have effectively diverged. Model
            //these two branches by returning a collection of comparisons
            if (first != null && second != null && first.Value?.Id != second.Value?.Id)
            {
                var firstBranchChildren = VisitChildren(first, null);
                var firstBranchOrphan = new CompareOrphan(first, null, firstBranchChildren);

                yield return firstBranchOrphan;

                var secondBranchChildren = VisitChildren(null, second);
                var secondBranchOrphan = new CompareOrphan(null, second, secondBranchChildren);

                yield return secondBranchOrphan;
            }
            else
            {
                var children = VisitChildren(first, second);

                var compareOrphan = new CompareOrphan(first, second, children);

                yield return compareOrphan;
            }
        }

        private List<CompareOrphan> VisitChildren(PrtgNode first, PrtgNode second)
        {
            //Iterate through the most children of the two candidate nodes.
            //All children beyond mostChildren will be null in leastChildren,
            //indicating those nodes have either been added or removed.
            var mostChildren = Math.Max(first?.Children.Count ?? 0, second?.Children.Count ?? 0);

            var comparisons = new List<CompareOrphan>();

            for(var i = 0; i < mostChildren; i++)
            {
                var firstChild = first?.Children.Count > i ? first.Children[i] : null;
                var secondChild = second?.Children.Count > i ? second.Children[i] : null;

                var compareOrphan = VisitTwo(firstChild, secondChild);

                comparisons.AddRange(compareOrphan);
            }

            return comparisons;
        }

        /// <summary>
        /// <see cref="CompareTreeVisitor"/> implements its own custom visitors that visit two nodes at a time.
        /// As such, defer the first node that is visited externally to Visit(PrtgNode).
        /// </summary>
        /// <param name="node">The external node that was passed to the visitor.</param>
        /// <returns>The final result of the visitor.</returns>
        [ExcludeFromCodeCoverage]
        protected override CompareOrphan DefaultVisit(PrtgNode node) => Visit(node);
    }
}
