using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree
{
    /// <summary>
    /// Represents an abstract visitor capable of accessing the nodes of a <see cref="PrtgNode"/> that
    /// performs a common action for all nodes by default.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class PrtgNodeDefaultVisitor : PrtgNodeVisitor
    {
        /// <summary>
        /// Visits a single <see cref="SensorNode"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        protected internal override void VisitSensor(SensorNode node) => DefaultVisit(node);

        /// <summary>
        /// Visits a single <see cref="DeviceNode"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        protected internal override void VisitDevice(DeviceNode node) => DefaultVisit(node);

        /// <summary>
        /// Visits a single <see cref="GroupNode"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        protected internal override void VisitGroup(GroupNode node) => DefaultVisit(node);

        /// <summary>
        /// Visits a single <see cref="ProbeNode"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        protected internal override void VisitProbe(ProbeNode node) => DefaultVisit(node);

        /// <summary>
        /// Visits a single <see cref="TriggerNode"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        protected internal override void VisitTrigger(TriggerNode node) => DefaultVisit(node);

        /// <summary>
        /// Visits a single <see cref="PropertyNode"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        /// <returns>The result of visitng the node.</returns>
        protected internal override void VisitProperty(PropertyNode node) => DefaultVisit(node);

        /// <summary>
        /// Visits a single <see cref="PrtgNodeCollection"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        protected internal override void VisitCollection(PrtgNodeCollection node) => DefaultVisit(node);

        /// <summary>
        /// The action to perform for all nodes whose visitor method is not overridden.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        protected abstract void DefaultVisit(PrtgNode node);
    }
}
