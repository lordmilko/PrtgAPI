using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree.Internal
{
    /// <summary>
    /// Represents an abstract visitor capable of accessing the orphans of a <see cref="PrtgOrphan"/> that
    /// performs a common action for all orphans by default.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal abstract class PrtgOrphanDefaultVisitor : PrtgOrphanVisitor
    {
        /// <summary>
        /// Visits a single <see cref="SensorOrphan"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        protected internal override void VisitSensor(SensorOrphan orphan) => DefaultVisit(orphan);

        /// <summary>
        /// Visits a single <see cref="DeviceOrphan"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        protected internal override void VisitDevice(DeviceOrphan orphan) => DefaultVisit(orphan);

        /// <summary>
        /// Visits a single <see cref="GroupOrphan"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        protected internal override void VisitGroup(GroupOrphan orphan) => DefaultVisit(orphan);

        /// <summary>
        /// Visits a single <see cref="ProbeOrphan"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        protected internal override void VisitProbe(ProbeOrphan orphan) => DefaultVisit(orphan);

        /// <summary>
        /// Visits a single <see cref="TriggerOrphan"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        protected internal override void VisitTrigger(TriggerOrphan orphan) => DefaultVisit(orphan);

        /// <summary>
        /// Visits a single <see cref="PropertyOrphan"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        /// <returns>The result of visitng the orphan.</returns>
        protected internal override void VisitProperty(PropertyOrphan orphan) => DefaultVisit(orphan);

        /// <summary>
        /// Visits a single <see cref="PrtgOrphanCollection"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        protected internal override void VisitCollection(PrtgOrphanCollection orphan) => DefaultVisit(orphan);

        /// <summary>
        /// The action to perform for all orphans whose visitor method is not overridden.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        protected abstract void DefaultVisit(PrtgOrphan orphan);
    }
}
