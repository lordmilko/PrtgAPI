using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace PrtgAPI.Tree.Converters.Text
{
    /// <summary>
    /// Defines common logic for pretty printing a <see cref="TreeNode"/>.
    /// </summary>
    /// <typeparam name="TNode">The type of node to pretty print.</typeparam>
    internal abstract class InternalPrettyTreeVisitorBase<TNode> where TNode : TreeNode<TNode>
    {
        protected int currentLevel = -1;

#if DEBUG
        [ExcludeFromCodeCoverage]
        private string DebugView => string.Join(string.Empty, result.Select(r => r.Text)).ToString();
#endif

        internal List<PrettyLine> result = new List<PrettyLine>();

        internal void Visit(TNode node)
        {
            currentLevel++;

            AddLine(node);

            foreach (var child in node.Children)
                OuterVisit(child);

            currentLevel--;
        }

        private void AddLine(TNode node)
        {
            var text = GetLineText(node);

            var lineObject = GetLineObject(node, text);

            result.Add(lineObject);
        }

        private string GetLineText(TNode node)
        {
            var lineBuilder = new StringBuilder();

            var flattenedChildren = FlattenChildren(node.Parent).Where(IncludeChild).ToList();

            var currentChild = flattenedChildren.IndexOf(node);

            for (var i = currentLevel - 1; i > 0; i--)
            {
                if (!IsLastChild(node, i))
                    lineBuilder.Append(PrettyPrintConstants.DeepChild);
                else
                    lineBuilder.Append(" ");

                lineBuilder.Append("  ");
            }

            if (currentLevel >= 1)
            {
                if (currentChild >= flattenedChildren.Count - 1)
                    lineBuilder.Append(PrettyPrintConstants.LastChild);
                else
                    lineBuilder.Append(PrettyPrintConstants.MiddleChild);

                lineBuilder.Append(PrettyPrintConstants.ConnectChild).Append(PrettyPrintConstants.ConnectChild);
            }

            var nodeName = GetName(node);

            lineBuilder.Append(nodeName);

            return lineBuilder.ToString();
        }

        protected virtual string GetName(TNode node)
        {
            return node.Name;
        }

        protected abstract PrettyLine GetLineObject(TNode node, string text);

        private List<TNode> FlattenChildren(TNode parent)
        {
            if (parent == null || parent.Children.Count == 0)
                return new List<TNode>();

            var nodes = new List<TNode>();

            foreach (var child in parent.Children)
            {
                //A child can never be a Grouping, so we can ignore that scenario
                if (child.Type == TreeNodeType.Collection)
                    nodes.AddRange(FlattenChildren(child));
                else
                    nodes.Add(child);
            }

            return nodes;
        }

        /// <summary>
        /// Specifies whether a child should be included as part of the tree. By default all children are included.<para/>
        /// Note: if a child is excluded as part of this method, the child's visitor should also be modified as to not call <see cref="Visit(TNode)"/>.
        /// </summary>
        /// <param name="node">The node to analyze.</param>
        /// <returns>True if the node should be considered as part of the tree. Otherwise, false.</returns>
        protected virtual bool IncludeChild(TNode node)
        {
            return true;
        }

        private bool IsLastChild(TNode node, int level)
        {
            TNode originalNode = node;
            TNode parent = node;

            //Technically speaking its impossible for the node parent type to be Grouping; Grouping
            //can only be the top level node, which means our level with a Grouping for a parent will
            //never be deep enough to get into the for loop and call this method
            if (node.Parent.Type == TreeNodeType.Collection || node.Parent.Type == TreeNodeType.Grouping)
                level++;

            for (int i = 0; i <= level; i++)
            {
                node = parent;
                parent = node.Parent;
            }

            Debug.Assert(node.Type == TreeNodeType.Node, $"Should have skipped over a {node.Type}");

            var flattenedChildren = FlattenChildren(parent).Where(IncludeChild).ToList();

            var index = flattenedChildren.IndexOf(node);

            Debug.Assert(index != -1, "Node was not a valid child");

            return index == flattenedChildren.Count - 1;
        }

        protected abstract void OuterVisit(TNode node);
    }
}
