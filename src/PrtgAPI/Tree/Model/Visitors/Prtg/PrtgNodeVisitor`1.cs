namespace PrtgAPI.Tree
{
    /// <summary>
    /// Represents a fully abstract visitor capable of accessing the nodes of a <see cref="PrtgNode"/> tree
    /// and produces a value of the type specified by the <typeparamref name="TResult"/> parameter.<para/>
    /// By default this class will only visit the single <see cref="PrtgNode"/> passed into its Visit method.<para/>
    /// To utilize a visitor with a default recursive implementation on <see cref="PrtgNode"/> objects please see <see cref="PrtgNodeRewriter"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of value to return.</typeparam>
    public abstract class PrtgNodeVisitor<TResult>
    {
        /// <summary>
        /// Visits a single <see cref="PrtgNode"/> and produces a value of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        /// <returns>If the node was not null, the value produced by visiting it. Otherwise, the default value of <typeparamref name="TResult"/>.</returns>
        public virtual TResult Visit(PrtgNode node)
        {
            if (node != null)
                return node.Accept(this);

            return default(TResult);
        }

        /// <summary>
        /// Visits a single <see cref="SensorNode"/> and produces a value of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        /// <returns>The result of visitng the node.</returns>
        protected internal abstract TResult VisitSensor(SensorNode node);

        /// <summary>
        /// Visits a single <see cref="DeviceNode"/> and produces a value of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        /// <returns>The result of visitng the node.</returns>
        protected internal abstract TResult VisitDevice(DeviceNode node);

        /// <summary>
        /// Visits a single <see cref="GroupNode"/> and produces a value of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        /// <returns>The result of visitng the node.</returns>
        protected internal abstract TResult VisitGroup(GroupNode node);

        /// <summary>
        /// Visits a single <see cref="ProbeNode"/> and produces a value of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        /// <returns>The result of visitng the node.</returns>
        protected internal abstract TResult VisitProbe(ProbeNode node);

        /// <summary>
        /// Visits a single <see cref="TriggerNode"/> and produces a value of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        /// <returns>The result of visitng the node.</returns>
        protected internal abstract TResult VisitTrigger(TriggerNode node);

        /// <summary>
        /// Visits a single <see cref="PropertyNode"/> and produces a value of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        /// <returns>The result of visitng the node.</returns>
        protected internal abstract TResult VisitProperty(PropertyNode node);

        /// <summary>
        /// Visits a single <see cref="PrtgNodeCollection"/> and produces a value of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        /// <returns>The result of visitng the node.</returns>
        protected internal abstract TResult VisitCollection(PrtgNodeCollection node);
    }
}
