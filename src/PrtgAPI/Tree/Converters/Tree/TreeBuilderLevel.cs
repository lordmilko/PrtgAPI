using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PrtgAPI.Linq;
using PrtgAPI.Tree.Internal;
using PrtgAPI.Tree.Progress;

namespace PrtgAPI.Tree.Converters.Tree
{
    /// <summary>
    /// Constructs a <see cref="PrtgOrphan"/> encapsulating a specified <see cref="ITreeValue"/> and its children.
    /// </summary>
    [DebuggerDisplay("{Value} ({ValueType})")]
    class TreeBuilderLevel
    {
        private TreeBuilder builder;

        private TreeProgressManager ProgressManager => builder.ProgressManager;
        private ObjectManager ObjectManager => builder.ObjectManager;
        private FlagEnum<TreeBuilderOptions> Options => builder.Options;

        /// <summary>
        /// Gets the value whose children are being processed at the current level.
        /// </summary>
        ITreeValue Value { get; }

        /// <summary>
        /// Gets the type of the object whose children are being processed at the current level.
        /// </summary>
        PrtgNodeType ValueType { get; }

        private Func<ITreeValue, IEnumerable<PrtgOrphan>, PrtgOrphan> GetOrphan;

        internal TreeBuilderLevel(ITreeValue value, TreeBuilder builder) :
            this(value, GetNodeType(value), PrtgOrphan.Object, builder)
        {
        }

        internal TreeBuilderLevel(ITreeValue value, PrtgNodeType valueType, Func<ITreeValue, IEnumerable<PrtgOrphan>, PrtgOrphan> getOrphan, TreeBuilder builder)
        {
            Value = value;
            ValueType = valueType;
            GetOrphan = getOrphan;
            this.builder = builder;
        }

        internal PrtgOrphan ProcessObject()
        {
            using (ProgressManager.ProcessLevel())
            {
                ProgressManager.OnLevelBegin(Value, ValueType);

                var children = GetChildren();

                if (Options.Contains(TreeBuilderOptions.Lazy))
                    children = children.ToCached();

                return GetOrphan(Value, children);
            }
        }

        private IEnumerable<PrtgOrphan> GetChildren()
        {
            switch (ValueType)
            {
                case PrtgNodeType.Device:
                    foreach (var child in GetDeviceChildren((Device) Value))
                        yield return child;
                    break;

                case PrtgNodeType.Group:
                case PrtgNodeType.Probe:
                    foreach (var child in GetContainerChildren((GroupOrProbe) Value))
                        yield return child;
                    break;
            }

            foreach (var child in GetCommonChildren())
                yield return child;
        }

        private IEnumerable<PrtgOrphan> GetDeviceChildren(Device device)
        {
            if (device.TotalSensors > 0)
            {
                var sensors = GetOrphans(ObjectManager.Sensor);

                return sensors;
            }

            return Enumerable.Empty<PrtgOrphan>();
        }

        private List<PrtgOrphan> GetContainerChildren(GroupOrProbe parent)
        {
            List<ObjectFactory> factories = new List<ObjectFactory>();

            if (parent.Id == WellKnownId.Root)
                factories.Add(ObjectManager.Probe);
            else
            {
                if (parent.TotalDevices > 0)
                    factories.Add(ObjectManager.Device);

                if (parent.TotalGroups > 0)
                    factories.Add(ObjectManager.Group);
            }

            var results = GetOrphans(factories.ToArray());

            return results;
        }

        private IEnumerable<PrtgOrphan> GetCommonChildren()
        {
            var triggers = GetTriggers();

            if (triggers != null)
                yield return triggers;

            var properties = GetProperties();

            if (properties != null)
                yield return properties;
        }

        private TriggerOrphanCollection GetTriggers()
        {
            var obj = Value as SensorOrDeviceOrGroupOrProbe;

            if (obj != null && obj.NotificationTypes.TotalTriggers > 0)
            {
                var triggers = ObjectManager.Trigger.Objects(Value.Id.Value);
                var orphans = triggers.Select(t => ObjectManager.Trigger.Orphan(t, null)).Cast<TriggerOrphan>();

                return PrtgOrphan.TriggerCollection(orphans);
            }

            return null;
        }

        private PropertyOrphanCollection GetProperties()
        {
            var obj = Value as SensorOrDeviceOrGroupOrProbe;

            if (obj != null)
            {
                var properties = ObjectManager.Property.Objects(obj.Id);
                var orphans = properties.Select(p => ObjectManager.Property.Orphan(p, null)).Cast<PropertyOrphan>();

                return PrtgOrphan.PropertyCollection(orphans);
            }

            return null;
        }

        /// <summary>
        /// Retrieves <see cref="ITreeValue"/> values objects from a set of object factories, retrieves the children
        /// of each object from the next <see cref="TreeBuilderLevel"/> and encapsulates the object and its children in a <see cref="PrtgOrphan"/>.
        /// </summary>
        /// <param name="factories">The factories to retrieve objects from.</param>
        /// <returns>A list of <see cref="PrtgOrphan"/> objects encapsulating the values returnd from the factories and their respective children.</returns>
        private List<PrtgOrphan> GetOrphans(params ObjectFactory[] factories)
        {
            List<Tuple<ITreeValue, ObjectFactory>> results = new List<Tuple<ITreeValue, ObjectFactory>>();

            foreach (var factory in factories)
            {
                var objs = factory.Objects(Value.Id.Value);

                results.AddRange(objs.Select(o => Tuple.Create(o, factory)));
            }

            ProgressManager.OnLevelWidthKnown(Value, ValueType, results.Count);

            var orphans = new List<PrtgOrphan>();

            foreach (var item in results)
            {
                ProgressManager.OnProcessValue(item.Item1);

                var level = new TreeBuilderLevel(item.Item1, item.Item2.Type, item.Item2.Orphan, builder);

                orphans.Add(level.ProcessObject());
            }

            return orphans;
        }

        private static PrtgNodeType GetNodeType(ITreeValue value)
        {
            if (value is Sensor)
                return PrtgNodeType.Sensor;

            if (value is Device)
                return PrtgNodeType.Device;

            if (value is Group)
                return PrtgNodeType.Group;

            if (value is Probe)
                return PrtgNodeType.Probe;

            if (value is NotificationTrigger)
                return PrtgNodeType.Trigger;

            if (value is PropertyValuePair)
                return PrtgNodeType.Property;

            if (value.GetType() == typeof(PrtgObject))
                throw new NotSupportedException($"Cannot process value '{value}' of type '{value.GetType()}': value must be a specific type of {nameof(PrtgObject)}.");

            throw new NotImplementedException($"Don't know what type of object '{value}' is.");
        }
    }
}
