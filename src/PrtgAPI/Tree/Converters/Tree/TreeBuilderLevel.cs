using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using PrtgAPI.Linq;
using PrtgAPI.Tree.Internal;
using PrtgAPI.Tree.Progress;

namespace PrtgAPI.Tree.Converters.Tree
{
    /// <summary>
    /// Constructs a <see cref="PrtgOrphan"/> encapsulating a specified <see cref="ITreeValue"/> and its children.
    /// </summary>
    [DebuggerDisplay("{Value} ({ValueType})")]
    internal partial class TreeBuilderLevel
    {
        private TreeBuilder builder;

        private TreeProgressManager ProgressManager => builder.ProgressManager;
        private ObjectManager ObjectManager => builder.ObjectManager;
        private CancellationToken Token => builder.Token;
        private FlagEnum<TreeRequestType> RequestType => builder.RequestType;
        private FlagEnum<TreeParseOption> Options => builder.Options;

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
            Token.ThrowIfCancellationRequested();

            using (ProgressManager.ProcessLevel())
            {
                ProgressManager.OnLevelBegin(Value, ValueType);

                var children = GetChildren();

                if (RequestType.Contains(TreeRequestType.Lazy))
                    children = children.ToCached();

                return GetOrphan(Value, children);
            }
        }

        internal async Task<PrtgOrphan> ProcessObjectAsync()
        {
            Token.ThrowIfCancellationRequested();

            using (ProgressManager.ProcessLevel())
            {
                ProgressManager.OnLevelBegin(Value, ValueType);

                var children = await GetChildrenAsync().ConfigureAwait(false);

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

        private async Task<List<PrtgOrphan>> GetChildrenAsync()
        {
            var children = new List<PrtgOrphan>();

            switch (ValueType)
            {
                case PrtgNodeType.Device:
                    children.AddRange(await GetDeviceChildrenAsync((Device) Value).ConfigureAwait(false));
                    break;

                case PrtgNodeType.Group:
                case PrtgNodeType.Probe:
                    children.AddRange(await GetContainerChildrenAsync((GroupOrProbe) Value).ConfigureAwait(false));
                    break;
            }

            children.AddRange(await GetCommonChildrenAsync().ConfigureAwait(false));

            return children;
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

        private async Task<List<PrtgOrphan>> GetCommonChildrenAsync()
        {
            var children = new List<PrtgOrphan>();

            var triggers = GetTriggersAsync();
            var properties = GetPropertiesAsync();

            await Task.WhenAll(triggers, properties).ConfigureAwait(false);

            if (triggers.Result != null)
                children.Add(triggers.Result);

            if (properties.Result != null)
                children.Add(properties.Result);

            return children;
        }

        [ExcludeFromCodeCoverage]
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
