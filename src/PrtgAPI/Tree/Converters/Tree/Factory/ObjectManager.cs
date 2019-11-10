namespace PrtgAPI.Tree.Converters.Tree
{
    /// <summary>
    /// Provides capabilities for managing different <see cref="ITreeValue"/> object types.
    /// </summary>
    internal class ObjectManager
    {
        /// <summary>
        /// Provides capabilities for managing <see cref="Sensor"/> objects.
        /// </summary>
        public SensorFactory Sensor { get; }

        /// <summary>
        /// Provides capabilities for managing <see cref="Device"/> objects.
        /// </summary>
        public DeviceFactory Device { get; }

        /// <summary>
        /// Provides capabilities for managing <see cref="Group"/> objects.
        /// </summary>
        public GroupFactory Group { get; }

        /// <summary>
        /// Provides capabilities for managing <see cref="Probe"/> objects.
        /// </summary>
        public ProbeFactory Probe { get; }

        /// <summary>
        /// Provides capabilities for managing <see cref="NotificationTrigger"/> objects.
        /// </summary>
        public TriggerFactory Trigger { get; }

        /// <summary>
        /// Provides capabilities for managing <see cref="PropertyValuePair"/> objects.
        /// </summary>
        public PropertyFactory Property { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectManager"/> class.
        /// </summary>
        /// <param name="client">The client to use for executing API requests.</param>
        internal ObjectManager(PrtgClient client)
        {
            Sensor = new SensorFactory(client);
            Device = new DeviceFactory(client);
            Group = new GroupFactory(client);
            Probe = new ProbeFactory(client);
            Trigger = new TriggerFactory(client);
            Property = new PropertyFactory(client);
        }
    }
}
