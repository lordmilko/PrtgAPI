using System.Collections.Generic;
using System.Linq;
using PrtgAPI.Tree.Internal;

namespace PrtgAPI.Tree
{
    public abstract partial class PrtgNode
    {
        #region Sensor

        /// <summary>
        /// Creates a new <see cref="SensorNode"/> from a sensor and its children.
        /// </summary>
        /// <param name="sensor">The sensor this node represents.</param>
        /// <param name="children">The children of this node.</param>
        /// <returns>A node containing the specified sensor and a copy of the children that point to this node as their parent.</returns>
        public static SensorNode Sensor(ISensor sensor, params PrtgNode[] children) => Sensor(sensor, (IEnumerable<PrtgNode>) children);

        /// <summary>
        /// Creates a new <see cref="SensorNode"/> from a sensor and its children.
        /// </summary>
        /// <param name="sensor">The sensor this node represents.</param>
        /// <param name="children">The children of this node.</param>
        /// <returns>A node containing the specified sensor and a copy of the children that point to this node as their parent.</returns>
        public static SensorNode Sensor(ISensor sensor, IEnumerable<PrtgNode> children) =>
            PrtgOrphan.Sensor(sensor, GetOrphans(children)).ToStandaloneNode<SensorNode>();

        #endregion
        #region Device

        /// <summary>
        /// Creates a new <see cref="DeviceNode"/> from a device and its children.
        /// </summary>
        /// <param name="device">The device this node represents.</param>
        /// <param name="children">The children of this node.</param>
        /// <returns>A node containing the specified device and a copy of the children that point to this node as their parent.</returns>
        public static DeviceNode Device(IDevice device, params PrtgNode[] children) => Device(device, (IEnumerable<PrtgNode>) children);

        /// <summary>
        /// Creates a new <see cref="DeviceNode"/> from a device and its children.
        /// </summary>
        /// <param name="device">The device this node represents.</param>
        /// <param name="children">The children of this node.</param>
        /// <returns>A node containing the specified device and a copy of the children that point to this node as their parent.</returns>
        public static DeviceNode Device(IDevice device, IEnumerable<PrtgNode> children) =>
            PrtgOrphan.Device(device, GetOrphans(children)).ToStandaloneNode<DeviceNode>();

        #endregion
        #region Group

        /// <summary>
        /// Creates a new <see cref="GroupNode"/> from a group and its children.
        /// </summary>
        /// <param name="group">The group this node represents.</param>
        /// <param name="children">The children of this node.</param>
        /// <returns>A node containing the specified group and a copy of the children that point to this node as their parent.</returns>
        public static GroupNode Group(IGroup group, params PrtgNode[] children) => Group(group, (IEnumerable<PrtgNode>) children);

        /// <summary>
        /// Creates a new <see cref="GroupNode"/> from a group and its children.
        /// </summary>
        /// <param name="group">The group this node represents.</param>
        /// <param name="children">The children of this node.</param>
        /// <returns>A node containing the specified group and a copy of the children that point to this node as their parent.</returns>
        public static GroupNode Group(IGroup group, IEnumerable<PrtgNode> children) =>
            PrtgOrphan.Group(group, GetOrphans(children)).ToStandaloneNode<GroupNode>();

        #endregion
        #region Probe

        /// <summary>
        /// Creates a new <see cref="ProbeNode"/> from a probe and its children.
        /// </summary>
        /// <param name="probe">The probe this node represents.</param>
        /// <param name="children">The children of this node.</param>
        /// <returns>A node containing the specified probe and a copy of the children that point to this node as their parent.</returns>
        public static ProbeNode Probe(IProbe probe, params PrtgNode[] children) => Probe(probe, (IEnumerable<PrtgNode>) children);

        /// <summary>
        /// Creates a new <see cref="ProbeNode"/> from a probe and its children.
        /// </summary>
        /// <param name="probe">The probe this node represents.</param>
        /// <param name="children">The children of this node.</param>
        /// <returns>A node containing the specified probe and a copy of the children that point to this node as their parent.</returns>
        public static ProbeNode Probe(IProbe probe, IEnumerable<PrtgNode> children) =>
            PrtgOrphan.Probe(probe, GetOrphans(children)).ToStandaloneNode<ProbeNode>();

        #endregion
        #region Notification Trigger

        /// <summary>
        /// Creates a new <see cref="TriggerNode"/> from a non-inherited notification trigger.
        /// </summary>
        /// <param name="trigger">The notification trigger this node represents.</param>
        /// <returns>A node containing the specified notification trigger.</returns>
        public static TriggerNode Trigger(NotificationTrigger trigger) =>
            PrtgOrphan.Trigger(trigger).ToStandaloneNode<TriggerNode>();

        #endregion
        #region Property

        /// <summary>
        /// Creates a new <see cref="PropertyNode"/> from a property, value and the object they apply to.
        /// </summary>
        /// <param name="parentOrId">The object or ID of the object the property applies to.</param>
        /// <param name="property">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <returns>A node containing the specified property.</returns>
        public static PropertyNode Property(Either<IPrtgObject, int> parentOrId, Either<ObjectProperty, string> property, object value) =>
            Property(new PropertyValuePair(parentOrId, property, value));

        /// <summary>
        /// Creates a new <see cref="PropertyNode"/> from a property.
        /// </summary>
        /// <param name="property">The property this node represents.</param>
        /// <returns>A node containing the specified property.</returns>
        public static PropertyNode Property(PropertyValuePair property) =>
            PrtgOrphan.Property(property).ToStandaloneNode<PropertyNode>();

        #endregion
        #region PropertyCollection

        /// <summary>
        /// Creates a new <see cref="PropertyNodeCollection"/> for representing a collection of property nodes.
        /// </summary>
        /// <param name="children">The children to use for this collection.</param>
        /// <returns>A collection that encapsulates the specified children.</returns>
        public static PropertyNodeCollection PropertyCollection(params PropertyNode[] children) =>
            PropertyCollection((IEnumerable<PropertyNode>) children);

        /// <summary>
        /// Creates a new <see cref="PropertyNodeCollection"/> for representing a collection of property nodes.
        /// </summary>
        /// <param name="children">The children to use for this collection.</param>
        /// <returns>A collection that encapsulates the specified children.</returns>
        public static PropertyNodeCollection PropertyCollection(IEnumerable<PropertyNode> children) =>
            PrtgOrphan.PropertyCollection(GetOrphans(children)?.Cast<PropertyOrphan>()).ToStandaloneNode<PropertyNodeCollection>();

        #endregion
        #region TriggerCollection

        /// <summary>
        /// Creates a new <see cref="TriggerNodeCollection"/> for representing a collection of trigger nodes.
        /// </summary>
        /// <param name="children">The children to use for this collection.</param>
        /// <returns>A collection that encapsulates the specified children.</returns>
        public static TriggerNodeCollection TriggerCollection(params TriggerNode[] children) =>
            TriggerCollection((IEnumerable<TriggerNode>) children);

        /// <summary>
        /// Creates a new <see cref="TriggerNodeCollection"/> for representing a collection of trigger nodes.
        /// </summary>
        /// <param name="children">The children to use for this collection.</param>
        /// <returns>A collection that encapsulates the specified children.</returns>
        public static TriggerNodeCollection TriggerCollection(IEnumerable<TriggerNode> children) =>
            PrtgOrphan.TriggerCollection(GetOrphans(children)?.Cast<TriggerOrphan>()).ToStandaloneNode<TriggerNodeCollection>();

        #endregion
        #region Grouping

        internal static PrtgNodeGrouping Grouping(params PrtgNode[] children) =>
            Grouping((IEnumerable<PrtgNode>) children);

        internal static PrtgNodeGrouping Grouping(IEnumerable<PrtgNode> children) =>
            PrtgOrphan.Grouping(GetOrphans(children)).ToStandaloneNode<PrtgNodeGrouping>();

        #endregion

        private static IEnumerable<PrtgOrphan> GetOrphans(IEnumerable<PrtgNode> children)
        {
            if (children == null)
                return null;

            return children.Select(c => c?.Orphan);
        }
    }
}
