using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree.Internal
{
    /// <summary>
    /// Represents a visitor capable of recursively accessing the orphans of a <see cref="PrtgOrphan"/> tree in depth-first order.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal abstract class PrtgOrphanWalker : PrtgOrphanVisitor
    {
        /// <summary>
        /// Visits the children of a <see cref="SensorOrphan"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        protected internal override void VisitSensor(SensorOrphan orphan) => DefaultVisit(orphan);

        /// <summary>
        /// Visits the children of a <see cref="DeviceOrphan"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        protected internal override void VisitDevice(DeviceOrphan orphan) => DefaultVisit(orphan);

        /// <summary>
        /// Visits the children of a <see cref="GroupOrphan"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        protected internal override void VisitGroup(GroupOrphan orphan) => DefaultVisit(orphan);

        /// <summary>
        /// Visits the children of a <see cref="ProbeOrphan"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        protected internal override void VisitProbe(ProbeOrphan orphan) => DefaultVisit(orphan);

        /// <summary>
        /// Visits the children of a <see cref="PropertyOrphan"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        protected internal override void VisitProperty(PropertyOrphan orphan) => DefaultVisit(orphan);

        /// <summary>
        /// Visits the children of a <see cref="TriggerOrphan"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        protected internal override void VisitTrigger(TriggerOrphan orphan) => DefaultVisit(orphan);

        /// <summary>
        /// Visits the children of a <see cref="PrtgOrphanCollection"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        protected internal override void VisitCollection(PrtgOrphanCollection orphan) => DefaultVisit(orphan);

        private void DefaultVisit(PrtgOrphan orphan)
        {
            foreach (var child in orphan.Children)
                Visit(child);
        }
    }
}
