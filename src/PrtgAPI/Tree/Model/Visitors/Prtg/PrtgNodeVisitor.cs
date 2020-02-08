using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree
{
    /// <summary>
    /// Represents a fully abstract visitor capable of accessing the nodes of a <see cref="PrtgNode"/> tree.<para/>
    /// By default this class will only visit the single <see cref="PrtgNode"/> passed into its Visit method.<para/>
    /// To utilize a visitor with a default recursive implementation please see <see cref="PrtgNodeWalker"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class PrtgNodeVisitor
    {
        /// <summary>
        /// Visits a single <see cref="PrtgNode"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        public virtual void Visit(PrtgNode node) => node.Accept(this);

        /// <summary>
        /// Visits a single <see cref="SensorNode"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        protected internal abstract void VisitSensor(SensorNode node);

        /// <summary>
        /// Visits a single <see cref="DeviceNode"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        protected internal abstract void VisitDevice(DeviceNode node);

        /// <summary>
        /// Visits a single <see cref="GroupNode"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        protected internal abstract void VisitGroup(GroupNode node);

        /// <summary>
        /// Visits a single <see cref="ProbeNode"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        protected internal abstract void VisitProbe(ProbeNode node);

        /// <summary>
        /// Visits a single <see cref="TriggerNode"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        protected internal abstract void VisitTrigger(TriggerNode node);

        /// <summary>
        /// Visits a single <see cref="PropertyNode"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        protected internal abstract void VisitProperty(PropertyNode node);

        /// <summary>
        /// Visits a single <see cref="PrtgNodeCollection"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        protected internal abstract void VisitCollection(PrtgNodeCollection node);
    }
}
