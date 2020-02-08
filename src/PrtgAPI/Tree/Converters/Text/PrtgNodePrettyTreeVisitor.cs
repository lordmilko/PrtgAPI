using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree.Converters.Text
{
    internal class PrtgNodePrettyTreeVisitor : PrtgNodeDefaultVisitor
    {
        private InternalPrtgNodePrettyTreeVisitor innerVisitor;

        /// <summary>
        /// Gets the result of the pretty printed tree.
        /// </summary>
        public List<PrettyLine> Result => innerVisitor.result;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrtgNodePrettyTreeVisitor"/> class.
        /// </summary>
        internal PrtgNodePrettyTreeVisitor()
        {
            innerVisitor = new InternalPrtgNodePrettyTreeVisitor(this);
        }

        /// <summary>
        /// Visits a node via the <see cref="innerVisitor"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        protected override void DefaultVisit(PrtgNode node) => innerVisitor.Visit(node);
    }

    /// <summary>
    /// Defines the logic for visiting a <see cref="PrtgNode"/> for use with <see cref="PrtgNodePrettyTreeVisitor"/>.
    /// </summary>
    internal class InternalPrtgNodePrettyTreeVisitor : InternalPrettyTreeVisitorBase<PrtgNode>
    {
        /// <summary>
        /// The outer visitor responsible for dispatching each node to its appropriate visitor method.
        /// </summary>
        private PrtgNodePrettyTreeVisitor outerVisitor;

        /// <summary>
        /// Initializes a new instance of the <see cref="InternalPrtgNodePrettyTreeVisitor"/> class.
        /// </summary>
        /// <param name="outerVisitor">The outer visitor responsible for dispatching nodes to their respective visitor methods.</param>
        internal InternalPrtgNodePrettyTreeVisitor(PrtgNodePrettyTreeVisitor outerVisitor)
        {
            this.outerVisitor = outerVisitor;
        }

        protected override PrettyLine GetLineObject(PrtgNode node, string text) =>
            new PrettyColorLine(node, GetColor(node), text);

        [ExcludeFromCodeCoverage]
        private ConsoleColor? GetColor(PrtgNode node)
        {
            switch (node.Type)
            {
                case PrtgNodeType.Sensor:
                    return GetSensorColor((SensorNode) node);
                case PrtgNodeType.Property:
                    return ConsoleColor.Yellow;
                default:
                    return null;
            }
        }

        protected override string GetName(PrtgNode node)
        {
            switch (node.Type)
            {
                case PrtgNodeType.Property:
                    return $"{node.Name} ('{((PropertyValuePair) node.Value).Value}')";

                default:
                    return node.Name;
            }
        }

        [ExcludeFromCodeCoverage]
        private ConsoleColor? GetSensorColor(SensorNode node)
        {
            switch (node.Value.Status)
            {
                case Status.Up:
                    return ConsoleColor.Green;
                case Status.Down:
                case Status.DownPartial:
                    return ConsoleColor.Red;
                case Status.DownAcknowledged:
                    return ConsoleColor.Magenta;
                case Status.Warning:
                    return ConsoleColor.Yellow;
                case Status.PausedByDependency:
                case Status.PausedByLicense:
                case Status.PausedBySchedule:
                case Status.PausedByUser:
                case Status.PausedUntil:
                    return ConsoleColor.Cyan;
                case Status.Unknown:
                case Status.None:
                    return ConsoleColor.DarkGray;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Dispatches a node to its appropriate visitor method, via the <see cref="outerVisitor"/>.<para/>
        /// For most node types we want to perform a common action based on the type of the node, however for groups
        /// we need to iterate through each of their children and process them individually.
        /// </summary>
        /// <param name="node">The node to dispatch.</param>
        protected override void OuterVisit(PrtgNode node) => node.Accept(outerVisitor);
    }
}
