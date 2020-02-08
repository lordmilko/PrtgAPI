using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree
{
    /// <summary>
    /// Represents an abstract visitor capable of accessing the nodes of a <see cref="PrtgNode"/> tree that
    /// performs a common action for all nodes by default and and produces a value of the type
    /// specified by the <typeparamref name="TResult"/> parameter.
    /// </summary>
    /// <typeparam name="TResult">The type of value to return.</typeparam>
    [ExcludeFromCodeCoverage]
    public abstract class PrtgNodeDefaultVisitor<TResult> : PrtgNodeVisitor<TResult>
    {
        /// <summary>
        /// Visits a single <see cref="SensorNode"/> and produces a value of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        /// <returns>The result of visitng the node.</returns>
        protected internal override TResult VisitSensor(SensorNode node) => DefaultVisit(node);

        /// <summary>
        /// Visits a single <see cref="DeviceNode"/> and produces a value of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        /// <returns>The result of visitng the node.</returns>
        protected internal override TResult VisitDevice(DeviceNode node) => DefaultVisit(node);

        /// <summary>
        /// Visits a single <see cref="GroupNode"/> and produces a value of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        /// <returns>The result of visitng the node.</returns>
        protected internal override TResult VisitGroup(GroupNode node) => DefaultVisit(node);

        /// <summary>
        /// Visits a single <see cref="ProbeNode"/> and produces a value of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        /// <returns>The result of visitng the node.</returns>
        protected internal override TResult VisitProbe(ProbeNode node) => DefaultVisit(node);

        /// <summary>
        /// Visits a single <see cref="TriggerNode"/> and produces a value of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        /// <returns>The result of visitng the node.</returns>
        protected internal override TResult VisitTrigger(TriggerNode node) => DefaultVisit(node);

        /// <summary>
        /// Visits a single <see cref="PropertyNode"/> and produces a value of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        /// <returns>The result of visitng the node.</returns>
        protected internal override TResult VisitProperty(PropertyNode node) => DefaultVisit(node);

        /// <summary>
        /// Visits a single <see cref="PrtgNodeCollection"/> and produces a value of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        /// <returns>The result of visitng the node.</returns>
        protected internal override TResult VisitCollection(PrtgNodeCollection node) => DefaultVisit(node);

        /// <summary>
        /// The action to perform for all nodes whose visitor method is not overridden.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        /// <returns>The result of visiting the node.</returns>
        protected abstract TResult DefaultVisit(PrtgNode node);
    }
}
