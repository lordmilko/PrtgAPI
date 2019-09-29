using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree.Internal
{
    /// <summary>
    /// Represents an abstract visitor capable of accessing the orphans of a <see cref="PrtgOrphan"/> that
    /// performs a common action for all orphans by default and and produces a value of the type
    /// specified by the <typeparamref name="TResult"/> parameter.
    /// </summary>
    /// <typeparam name="TResult">The type of value to return.</typeparam>
    [ExcludeFromCodeCoverage]
    internal abstract class PrtgOrphanDefaultVisitor<TResult> : PrtgOrphanVisitor<TResult>
    {
        /// <summary>
        /// Visits a single <see cref="SensorOrphan"/> and produces a value of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        /// <returns>The result of visitng the orphan.</returns>
        protected internal override TResult VisitSensor(SensorOrphan orphan) => DefaultVisit(orphan);

        /// <summary>
        /// Visits a single <see cref="DeviceOrphan"/> and produces a value of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        /// <returns>The result of visitng the orphan.</returns>
        protected internal override TResult VisitDevice(DeviceOrphan orphan) => DefaultVisit(orphan);

        /// <summary>
        /// Visits a single <see cref="GroupOrphan"/> and produces a value of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        /// <returns>The result of visitng the orphan.</returns>
        protected internal override TResult VisitGroup(GroupOrphan orphan) => DefaultVisit(orphan);

        /// <summary>
        /// Visits a single <see cref="ProbeOrphan"/> and produces a value of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        /// <returns>The result of visitng the orphan.</returns>
        protected internal override TResult VisitProbe(ProbeOrphan orphan) => DefaultVisit(orphan);

        /// <summary>
        /// Visits a single <see cref="TriggerOrphan"/> and produces a value of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        /// <returns>The result of visitng the orphan.</returns>
        protected internal override TResult VisitTrigger(TriggerOrphan orphan) => DefaultVisit(orphan);

        /// <summary>
        /// Visits a single <see cref="PropertyOrphan"/> and produces a value of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        /// <returns>The result of visitng the orphan.</returns>
        protected internal override TResult VisitProperty(PropertyOrphan orphan) => DefaultVisit(orphan);

        /// <summary>
        /// Visits a single <see cref="PrtgOrphanCollection"/> and produces a value of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        /// <returns>The result of visitng the orphan.</returns>
        protected internal override TResult VisitCollection(PrtgOrphanCollection orphan) => DefaultVisit(orphan);

        /// <summary>
        /// The action to perform for all orphans whose visitor method is not overridden.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        /// <returns>The result of visiting the orphan.</returns>
        protected abstract TResult DefaultVisit(PrtgOrphan orphan);
    }
}
