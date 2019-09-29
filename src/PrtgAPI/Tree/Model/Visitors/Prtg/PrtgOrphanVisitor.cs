using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree.Internal
{
    /// <summary>
    /// Represents a fully abstract visitor capable of accessing the orphans of a <see cref="PrtgOrphan"/>.<para/>
    /// By default this class will only visit the single <see cref="PrtgOrphan"/> passed into its Visit method.<para/>
    /// To utilize a visitor with a default recursive implementation please see <see cref="PrtgOrphanWalker"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal abstract class PrtgOrphanVisitor
    {
        /// <summary>
        /// Visits a single <see cref="PrtgOrphan"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        public virtual void Visit(PrtgOrphan orphan) => orphan.Accept(this);

        /// <summary>
        /// Visits a single <see cref="SensorOrphan"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        protected internal abstract void VisitSensor(SensorOrphan orphan);

        /// <summary>
        /// Visits a single <see cref="DeviceOrphan"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        protected internal abstract void VisitDevice(DeviceOrphan orphan);

        /// <summary>
        /// Visits a single <see cref="GroupOrphan"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        protected internal abstract void VisitGroup(GroupOrphan orphan);

        /// <summary>
        /// Visits a single <see cref="ProbeOrphan"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        protected internal abstract void VisitProbe(ProbeOrphan orphan);

        /// <summary>
        /// Visits a single <see cref="TriggerOrphan"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        protected internal abstract void VisitTrigger(TriggerOrphan orphan);

        /// <summary>
        /// Visits a single <see cref="PropertyOrphan"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        protected internal abstract void VisitProperty(PropertyOrphan orphan);

        /// <summary>
        /// Visits a single <see cref="PrtgOrphanCollection"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        protected internal abstract void VisitCollection(PrtgOrphanCollection orphan);
    }
}
