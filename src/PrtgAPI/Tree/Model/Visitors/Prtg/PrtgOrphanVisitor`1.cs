using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree.Internal
{
    /// <summary>
    /// Represents a fully abstract visitor capable of accessing the orphans of a <see cref="PrtgOrphan"/> tree
    /// and produces a value of the type specified by the <typeparamref name="TResult"/> parameter.<para/>
    /// By default this class will only visit the single <see cref="PrtgOrphan"/> passed into its Visit method.<para/>
    /// To utilize a visitor with a default recursive implementation on <see cref="PrtgOrphan"/> objects please see <see cref="PrtgOrphanRewriter"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of value to return.</typeparam>
    [ExcludeFromCodeCoverage]
    internal abstract class PrtgOrphanVisitor<TResult>
    {
        /// <summary>
        /// Visits a single <see cref="PrtgOrphan"/> and produces a value of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        /// <returns>If the orphan was not null, the value produced by visiting it. Otherwise, the default value of <typeparamref name="TResult"/>.</returns>
        internal virtual TResult Visit(PrtgOrphan orphan)
        {
            if (orphan != null)
                return orphan.Accept(this);

            return default(TResult);
        }

        /// <summary>
        /// Visits a single <see cref="SensorOrphan"/> and produces a value of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        /// <returns>The result of visitng the orphan.</returns>
        protected internal abstract TResult VisitSensor(SensorOrphan orphan);

        /// <summary>
        /// Visits a single <see cref="DeviceOrphan"/> and produces a value of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        /// <returns>The result of visitng the orphan.</returns>
        protected internal abstract TResult VisitDevice(DeviceOrphan orphan);

        /// <summary>
        /// Visits a single <see cref="GroupOrphan"/> and produces a value of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        /// <returns>The result of visitng the orphan.</returns>
        protected internal abstract TResult VisitGroup(GroupOrphan orphan);

        /// <summary>
        /// Visits a single <see cref="ProbeOrphan"/> and produces a value of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        /// <returns>The result of visitng the orphan.</returns>
        protected internal abstract TResult VisitProbe(ProbeOrphan orphan);

        /// <summary>
        /// Visits a single <see cref="TriggerOrphan"/> and produces a value of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        /// <returns>The result of visitng the orphan.</returns>
        protected internal abstract TResult VisitTrigger(TriggerOrphan orphan);

        /// <summary>
        /// Visits a single <see cref="PropertyOrphan"/> and produces a value of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        /// <returns>The result of visitng the orphan.</returns>
        protected internal abstract TResult VisitProperty(PropertyOrphan orphan);

        /// <summary>
        /// Visits a single <see cref="PrtgOrphanCollection"/> and produces a value of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        /// <returns>The result of visitng the orphan.</returns>
        protected internal abstract TResult VisitCollection(PrtgOrphanCollection orphan);
    }
}
