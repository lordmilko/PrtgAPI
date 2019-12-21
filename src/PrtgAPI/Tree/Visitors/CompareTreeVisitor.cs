using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            //these two branches by returning a collection of comparisons.
            //While we should probably be using EqualsIdentity here, we don't so that we can
            //catch differences between types with the same ID but different values, which is done in
            //CompareOrphan's constructor. We know we're safe though from the prior work that was done in GetChildrenTuples()
            if (first != null && second != null && first?.Value?.Id != second?.Value?.Id)
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
            var firstChildren = first?.Children.Count ?? 0;
            var secondChildren = second?.Children.Count ?? 0;

            //Iterate through the most children of the two candidate nodes.
            //All children beyond mostChildren will be null in the comparison tuple
            var mostChildren = Math.Max(firstChildren, secondChildren);
            var leastChildren = Math.Min(firstChildren, secondChildren);

            //Get the child nodes to compare for each parent. Any nodes in one tree
            //that do not have a corresponding item in the other will be compared with null
            //(indicating either they've been newly created or removed)
            var tuples = GetCompareTuples(first, second, mostChildren, leastChildren);

            var comparisons = new List<CompareOrphan>();

            for (var i = 0; i < tuples.Count; i++)
            {
                var compareOrphan = VisitTwo(tuples[i].Item1, tuples[i].Item2);

                comparisons.AddRange(compareOrphan);
            }

            return comparisons;
        }

        private List<Tuple<PrtgNode, PrtgNode>> GetCompareTuples(PrtgNode first, PrtgNode second, int mostChildren, int leastChildren)
        {
            //Return a list of tuples containing the nodes that should be encapsulated in each comparison.
            //Note: we do not currently support reordering nodes; we only support nodes that have changed position
            //due to a prior node having been removed or inserted

            var firstChildren = new List<PrtgNode>();
            var secondChildren = new List<PrtgNode>();

            if (first != null)
                firstChildren.AddRange(first.Children);

            if (second != null)
                secondChildren.AddRange(second.Children);

            if (firstChildren.Count != secondChildren.Count)
            {
                var smallerList = firstChildren.Count < secondChildren.Count ? firstChildren : secondChildren;

                var diff = Math.Abs(firstChildren.Count - secondChildren.Count);

                smallerList.AddRange(Enumerable.Range(0, diff).Select(i => (PrtgNode) null));
            }

            var pairs = new List<Tuple<PrtgNode, PrtgNode>>();

            for (var i = 0; i < mostChildren; i++)
            {
                var firstChild = firstChildren[i];
                var secondChild = secondChildren[i];

                if (firstChild == null && secondChild == null)
                    continue;

                if (mostChildren == leastChildren||
                    i < leastChildren - 1 ||
                    firstChild?.EqualsIdentity(secondChild) == true)
                {
                    firstChildren[i] = null;
                    secondChildren[i] = null;

                    //Whatever; we don't currently support reordering, so as far as we're concerned its been removed and re-added
                    pairs.Add(Tuple.Create(firstChild, secondChild));
                }
                else
                {
                    //Either first or second has a different number of children. If the position we're at is beyond
                    //the length of the smaller list, do a search to see whether our node has been added, removed,
                    //pushed back or pulled forward due to another node being added or removed

                    //e.g. 1,2,3 -> 1,3. If we are at index 2, we now need to go looking for moved nodes
                    //If we were at index 0, i < leastChildren - 1 we do the normal path above.
                    //For index 1 and 2 we grab a node and see whether have a match
                    AddMatchOrNew(firstChild, null, i, ref firstChildren, ref secondChildren, pairs);
                    AddMatchOrNew(null, secondChild, i, ref secondChildren, ref firstChildren, pairs);
                }
            }

            return pairs;
        }

        private void AddMatchOrNew(PrtgNode node, PrtgNode other, int nodeIndex, ref List<PrtgNode> nodeList, ref List<PrtgNode> otherList, List<Tuple<PrtgNode, PrtgNode>> pairs)
        {
            Debug.Assert(node == null || other == null, $"Both nodes passed to {nameof(AddMatchOrNew)} were not null.");

            var target = node ?? other;

            if (target == null)
                return;

            var match = otherList.FirstOrDefault(c =>
            {
                //EqualsIdentity will check for both value ID and type, which we don't want
                //for PrtgNode<T> types which may have the same ID but a different value. So
                //we check against ID if we can; otherwise we fall back to EqualsIdentity
                if (target.Value != null && target.Value.Id != null)
                    return target.Value.Id == c?.Value?.Id;

                return c?.EqualsIdentity(target) == true;
            });

            if (match != null)
            {
                var matchIndex = otherList.IndexOf(match);
                otherList[matchIndex] = null;
            }

            nodeList[nodeIndex] = null;

            if (node == null)
                pairs.Add(Tuple.Create(match, other));
            else
                pairs.Add(Tuple.Create(node, match));
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
