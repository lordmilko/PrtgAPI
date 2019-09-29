using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree
{
    /// <summary>
    /// Represents a visitor capable of recursively accessing the nodes of a <see cref="PrtgNode"/> in depth-first order.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class PrtgNodeWalker : PrtgNodeVisitor
    {
        /// <summary>
        /// Visits the children of a <see cref="SensorNode"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        protected internal override void VisitSensor(SensorNode node) => DefaultVisit(node);

        /// <summary>
        /// Visits the children of a <see cref="DeviceNode"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        protected internal override void VisitDevice(DeviceNode node) => DefaultVisit(node);

        /// <summary>
        /// Visits the children of a <see cref="GroupNode"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        protected internal override void VisitGroup(GroupNode node) => DefaultVisit(node);

        /// <summary>
        /// Visits the children of a <see cref="ProbeNode"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        protected internal override void VisitProbe(ProbeNode node) => DefaultVisit(node);

        /// <summary>
        /// Visits the children of a <see cref="PropertyNode"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        protected internal override void VisitProperty(PropertyNode node) => DefaultVisit(node);

        /// <summary>
        /// Visits the children of a <see cref="TriggerNode"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        protected internal override void VisitTrigger(TriggerNode node) => DefaultVisit(node);

        /// <summary>
        /// Visits the children of a <see cref="PrtgNodeCollection"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        protected internal override void VisitCollection(PrtgNodeCollection node) => DefaultVisit(node);

        private void DefaultVisit(PrtgNode node)
        {
            foreach (var child in node.Children)
                Visit(child);
        }
    }
}
