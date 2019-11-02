using System.Collections.Generic;
using System.Linq;

namespace PrtgAPI.Tree
{
    /// <summary>
    /// Reduces a <see cref="CompareNode"/> tree to only the nodes that contain a difference.
    /// </summary>
    internal class CompareNodeReducer : CompareNodeDefaultVisitor<CompareNode>
    {
        protected override CompareNode DefaultVisit(CompareNode node)
        {
            if (node.Children.Count > 0)
            {
                var newChildren = new List<CompareNode>();
                var ignoredChildren = new List<CompareNode>();

                foreach (var child in node.Children)
                {
                    var newChild = Visit(child);

                    if (newChild != child)
                    {
                        if (newChild == null)
                            ignoredChildren.Add(child);
                        else
                        {
                            if (newChild.Difference != TreeNodeDifference.None)
                                newChildren.Add(newChild);
                        }
                    }
                }

                if (node.Children.Count > 0)
                {
                    //If any of the children were changed, return a new node with the modified children.
                    if (newChildren.Count > 0)
                        return node.WithChildren(newChildren);

                    if(ignoredChildren.Count > 0)
                    {
                        //Some of the children were ignored
                        if (ignoredChildren.Count < node.Children.Count)
                        {
                            var remainingChildren = node.Children.Except(ignoredChildren);

                            return node.WithChildren(remainingChildren);
                        }
                        
                        //All of the children were ignored

                        if (node.Difference != TreeNodeDifference.None)
                        {
                            //If no children were converted to something and none of them became non-null,
                            //all of them MUST be now null - all children were removed.
                            return node.WithChildren(new CompareNode[] { });
                        }

                        //The node no longer has any children, and has no difference itself. Ignore it.
                        return null;
                    }
                }
            }
            else
            {
                //If the node has no children and no difference of its own,
                //ignore it.
                if (node.Difference == TreeNodeDifference.None)
                    return null;
            }

            //Either all of our children were the same, or we have no children but we ourselves have a difference.
            return node;
        }
    }
}
