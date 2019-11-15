using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree.Internal
{
    internal abstract partial class PrtgOrphan
    {
        #region Sensor

        internal static SensorOrphan Sensor(Sensor sensor, params PrtgOrphan[] children) => Sensor(sensor, (IEnumerable<PrtgOrphan>) children);

        internal static SensorOrphan Sensor(Sensor sensor, IEnumerable<PrtgOrphan> children) => new SensorOrphan(sensor, children);

        #endregion
        #region Device

        internal static DeviceOrphan Device(Device device, params PrtgOrphan[] children) => Device(device, (IEnumerable<PrtgOrphan>) children);

        internal static DeviceOrphan Device(Device device, IEnumerable<PrtgOrphan> children) => new DeviceOrphan(device, children);

        #endregion
        #region Group

        internal static GroupOrphan Group(Group group, params PrtgOrphan[] children) => Group(group, (IEnumerable<PrtgOrphan>) children);

        internal static GroupOrphan Group(Group group, IEnumerable<PrtgOrphan> children) => new GroupOrphan(group, children);

        #endregion
        #region Probe

        internal static ProbeOrphan Probe(Probe probe, params PrtgOrphan[] children) =>
            Probe(probe, (IEnumerable<PrtgOrphan>) children);

        internal static ProbeOrphan Probe(Probe probe, IEnumerable<PrtgOrphan> children) =>
            new ProbeOrphan(probe, children);

        #endregion
        #region Property

        internal static PropertyOrphan Property(PropertyValuePair property) =>
            new PropertyOrphan(property);

        internal static PropertyOrphan Property(Either<IPrtgObject, int> parentOrId, Either<ObjectProperty, string> property, string value) =>
            Property(new PropertyValuePair(parentOrId, property, value));

        #endregion
        #region Trigger

        internal static TriggerOrphan Trigger(NotificationTrigger trigger) =>
            new TriggerOrphan(trigger);

        #endregion
        #region PropertyCollection

        internal static PropertyOrphanCollection PropertyCollection(params PropertyOrphan[] children) =>
            PropertyCollection((IEnumerable<PropertyOrphan>) children);

        internal static PropertyOrphanCollection PropertyCollection(IEnumerable<PropertyOrphan> children) =>
            new PropertyOrphanCollection(children);

        #endregion
        #region TriggerCollection

        internal static TriggerOrphanCollection TriggerCollection(params TriggerOrphan[] children) =>
            TriggerCollection((IEnumerable<TriggerOrphan>) children);

        internal static TriggerOrphanCollection TriggerCollection(IEnumerable<TriggerOrphan> children) =>
            new TriggerOrphanCollection(children);

        #endregion
        #region Grouping

        internal static PrtgOrphanGrouping Grouping(params PrtgOrphan[] children) =>
            Grouping((IEnumerable<PrtgOrphan>) children);

        internal static PrtgOrphanGrouping Grouping(IEnumerable<PrtgOrphan> children) =>
            new PrtgOrphanGrouping(children);

        #endregion

        [ExcludeFromCodeCoverage]
        internal static PrtgOrphan Object(ITreeValue value, params PrtgOrphan[] children) =>
            Object(value, (IEnumerable<PrtgOrphan>) children);

        [ExcludeFromCodeCoverage]
        internal static PrtgOrphan Object(ITreeValue value, IEnumerable<PrtgOrphan> children)
        {
            if (value is Probe)
                return Probe((Probe) value, children);

            if (value is Group)
                return Group((Group) value, children);

            if (value is Device)
                return Device((Device) value, children);

            if (value is Sensor)
                return Sensor((Sensor) value, children);

            if (value is NotificationTrigger)
                return Trigger((NotificationTrigger) value);

            if (value is PropertyValuePair)
                return Property((PropertyValuePair) value);

            throw new NotImplementedException($"Don't know what type of orphan object of type '{value.GetType().FullName}' should be.");
        }
    }
}
