using System;
using System.Collections.Generic;

namespace PrtgAPI.Tree.Converters.Text
{
    internal class CompareNodePrettyTreeVisitor : CompareNodeVisitor
    {
        /// <summary>
        /// As we cannot derive from both our node's visitor type and the base class for
        /// pretty printing visitors, we define an internal visitor class that we then defer to
        /// from our outer class (this).
        /// </summary>
        private InternalCompareNodePrettyTreeVisitor innerVisitor;

        /// <summary>
        /// Gets the result of the pretty printed tree.
        /// </summary>
        public List<PrettyLine> Result => innerVisitor.result;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompareNodePrettyTreeVisitor"/> class.
        /// </summary>
        internal CompareNodePrettyTreeVisitor()
        {
            innerVisitor = new InternalCompareNodePrettyTreeVisitor(this);
        }

        /// <summary>
        /// Visits a node via the <see cref="innerVisitor"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        protected internal override void VisitNode(CompareNode node) => innerVisitor.Visit(node);

        /// <summary>
        /// Dispatches the children of a collection to their respective visit methods for eventual dispatchment to the <see cref="innerVisitor"/>.
        /// </summary>
        /// <param name="node">The node whose children should be visited.</param>
        protected internal override void VisitCollection(CompareNodeCollection node)
        {
            foreach (var child in node.Children)
                child.Accept(this);
        }
    }

    /// <summary>
    /// Defines the logic for visiting a <see cref="CompareNode"/> for use with <see cref="CompareNodePrettyTreeVisitor"/>.
    /// </summary>
    internal class InternalCompareNodePrettyTreeVisitor : InternalPrettyTreeVisitorBase<CompareNode>
    {
        /// <summary>
        /// The outer visitor responsible for dispatching each node to its appropriate visitor method.
        /// </summary>
        private CompareNodePrettyTreeVisitor outerVisitor;

        /// <summary>
        /// Initializes a new instance of the <see cref="InternalCompareNodePrettyTreeVisitor"/> class.
        /// </summary>
        /// <param name="outerVisitor">The outer visitor responsible for dispatching nodes to their respective visitor methods.</param>
        internal InternalCompareNodePrettyTreeVisitor(CompareNodePrettyTreeVisitor outerVisitor)
        {
            this.outerVisitor = outerVisitor;
        }

        protected override PrettyLine GetLineObject(CompareNode node, string text) =>
            new PrettyColorLine(GetColor(node), text);

        private ConsoleColor? GetColor(CompareNode node)
        {
            if (node.Difference.Contains(TreeNodeDifference.Added))
                return ConsoleColor.Green;

            if (node.Difference.Contains(TreeNodeDifference.Removed))
                return ConsoleColor.Red;

            if (node.Difference != TreeNodeDifference.None)
                return ConsoleColor.Yellow;

            return null;
        }

        protected override string GetName(CompareNode node)
        {
            if (node.Difference.Contains(TreeNodeDifference.Name))
                return $"{node.First.Name} (Renamed '{node.Second.Name}')";

            return node.Name;
        }

        /// <summary>
        /// Dispatches a node to its appropriate visitor method, via the <see cref="outerVisitor"/>.<para/>
        /// For most node types we want to perform a common action based on the type of the node, however for groups
        /// we need to iterate through each of their children and process them individually.
        /// </summary>
        /// <param name="node">The node to dispatch.</param>
        protected override void OuterVisit(CompareNode node) => node.Accept(outerVisitor);
    }
}
