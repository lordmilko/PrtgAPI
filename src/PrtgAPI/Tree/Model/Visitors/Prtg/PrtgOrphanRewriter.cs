using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree.Internal
{
    /// <summary>
    /// Represents a visitor capable of recursively rewriting the members of a <see cref="PrtgOrphan"/> in depth-first order.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal abstract class PrtgOrphanRewriter : PrtgOrphanVisitor<PrtgOrphan>
    {
        /// <summary>
        /// Visits the children of a <see cref="SensorOrphan"/> and replaces the orphan if any of its children are modified.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        /// <returns>If any children were modified, a new orphan with the modified children. Otherwise, the original orphan.</returns>
        protected internal override PrtgOrphan VisitSensor(SensorOrphan orphan)
        {
            return orphan.WithChildren(VisitList(orphan.Children));
        }

        /// <summary>
        /// Visits the children of a <see cref="DeviceOrphan"/> and replaces the orphan if any of its children are modified.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        /// <returns>If any children were modified, a new orphan with the modified children. Otherwise, the original orphan.</returns>
        protected internal override PrtgOrphan VisitDevice(DeviceOrphan orphan)
        {
            return orphan.WithChildren(VisitList(orphan.Children));
        }

        /// <summary>
        /// Visits the children of a <see cref="GroupOrphan"/> and replaces the orphan if any of its children are modified.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        /// <returns>If any children were modified, a new orphan with the modified children. Otherwise, the original orphan.</returns>
        protected internal override PrtgOrphan VisitGroup(GroupOrphan orphan)
        {
            return orphan.WithChildren(VisitList(orphan.Children));
        }

        /// <summary>
        /// Visits the children of a <see cref="ProbeOrphan"/> and replaces the orphan if any of its children are modified.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        /// <returns>If any children were modified, a new orphan with the modified children. Otherwise, the original orphan.</returns>
        protected internal override PrtgOrphan VisitProbe(ProbeOrphan orphan)
        {
            return orphan.WithChildren(VisitList(orphan.Children));
        }

        /// <summary>
        /// Visits the children of a <see cref="TriggerOrphan"/> and replaces the orphan if any of its children are modified.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        /// <returns>If any children were modified, a new orphan with the modified children. Otherwise, the original orphan.</returns>
        protected internal override PrtgOrphan VisitTrigger(TriggerOrphan orphan)
        {
            Debug.Assert(orphan.Children.Count == 0, "Trigger had a child. Need to implement WithChildren support for triggers");

            return orphan;
        }

        /// <summary>
        /// Visits the children of a <see cref="PropertyOrphan"/> and replaces the orphan if any of its children are modified.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        /// <returns>If any children were modified, a new orphan with the modified children. Otherwise, the original orphan.</returns>
        protected internal override PrtgOrphan VisitProperty(PropertyOrphan orphan)
        {
            Debug.Assert(orphan.Children.Count == 0, "Trigger had a child. Need to implement WithChildren support for properties");

            return orphan;
        }

        /// <summary>
        /// Visits the children of a <see cref="PrtgOrphanCollection"/> and replaces the orphan if any of its children are modified.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        /// <returns>If any children were modified, a new orphan with the modified children. Otherwise, the original orphan.</returns>
        protected internal override PrtgOrphan VisitCollection(PrtgOrphanCollection orphan)
        {
            return orphan.WithChildren(VisitList(orphan.Children));
        }

        #region Helpers

        /// <summary>
        /// Visit the orphans in a list. If any of orphan is modified after being visited, this method will return a new collection containing the updated orphans.<para/>
        /// If an orphan is null after being visited, it will be excluded from the returned list.
        /// </summary>
        /// <param name="orphans">The orphans to visit.</param>
        /// <returns>If any orphan was modified, a new list containing the updated orphans. Otherwise, the original list.</returns>
        protected virtual IReadOnlyList<PrtgOrphan> VisitList(INodeList<PrtgOrphan> orphans)
        {
            return VisitorUtilities.VisitList(orphans, Visit);
        }

        #endregion
    }
}
