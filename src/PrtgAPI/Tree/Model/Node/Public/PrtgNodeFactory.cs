using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using PrtgAPI.Linq;

namespace PrtgAPI.Tree
{
    /// <summary>
    /// Provides methods for generating <see cref="PrtgNode"/> objects.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class PrtgNodeFactory
    {
        private PrtgClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrtgNodeFactory"/> class.
        /// </summary>
        /// <param name="client">The client to use for executing API requests.</param>
        public PrtgNodeFactory(PrtgClient client)
        {
            this.client = client;
        }

        #region Sensor
            #region Id

        /// <summary>
        /// Creates a <see cref="SensorNode"/> for a sensor with a specified ID.<para/>
        /// If the sensor does not exist or an ambiguous match is found, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="id">The ID of the sensor to retrieve.</param>
        /// <param name="children">The children of this node.</param>
        /// <exception cref="InvalidOperationException">The specified sensor does not exist or multiple sensors were resolved with the specified ID.</exception>
        /// <returns>A <see cref="SensorNode"/> encapsulating the specified object.</returns>
        public SensorNode Sensor(int id, params PrtgNode[] children) =>
            Sensor(id, (IEnumerable<PrtgNode>) children);

        /// <summary>
        /// Creates a <see cref="SensorNode"/> for a sensor with a specified ID.<para/>
        /// If the sensor does not exist or an ambiguous match is found, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="id">The ID of the sensor to retrieve.</param>
        /// <param name="children">The children of this node.</param>
        /// <exception cref="InvalidOperationException">The specified sensor does not exist or multiple sensors were resolved with the specified ID.</exception>
        /// <returns>A <see cref="SensorNode"/> encapsulating the specified object.</returns>
        public SensorNode Sensor(int id, IEnumerable<PrtgNode> children) =>
            PrtgNode.Sensor(client.GetSensor(id), children);

            #endregion
            #region Name

        /// <summary>
        /// Creates a <see cref="SensorNode"/> for a sensor with a specified name.<para/>
        /// If the sensor does not exist or an ambiguous match is found, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="name">The name of the sensor to retrieve.</param>
        /// <param name="children">The children of this node.</param>
        /// <exception cref="InvalidOperationException">The specified sensor does not exist or multiple sensors were resolved with the specified name.</exception>
        /// <returns>A <see cref="SensorNode"/> encapsulating the specified object.</returns>
        public SensorNode Sensor(string name, params PrtgNode[] children) =>
            Sensor(name, (IEnumerable<PrtgNode>) children);

        /// <summary>
        /// Creates a <see cref="SensorNode"/> for a sensor with a specified name.<para/>
        /// If the sensor does not exist or an ambiguous match is found, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="name">The name of the sensor to retrieve.</param>
        /// <param name="children">The children of this node.</param>
        /// <exception cref="InvalidOperationException">The specified sensor does not exist or multiple sensors were resolved with the specified name.</exception>
        /// <returns>A <see cref="SensorNode"/> encapsulating the specified object.</returns>
        public SensorNode Sensor(string name, IEnumerable<PrtgNode> children) =>
            GetByName(name, client.GetSensors, PrtgNode.Sensor, children);

            #endregion
            #region Multiple

        /// <summary>
        /// Creates <see cref="SensorNode"/> objects for all sensors that match a specified filter.<para/>
        /// If no sensors match the specified filter, a <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="property">The property to search against.</param>
        /// <param name="value">The value to search for.</param>
        /// <exception cref="InvalidOperationException">No sensors matched the specified filter.</exception>
        /// <returns>A list of <see cref="SensorNode"/> objects encapsulating the returned sensors.</returns>
        public List<SensorNode> Sensors(Property property, object value) =>
            GetNodes(property, value, client.GetSensors, PrtgNode.Sensor);

            #endregion
        #endregion
        #region Device
            #region Id

        /// <summary>
        /// Creates a <see cref="DeviceNode"/> for a device with a specified ID.<para/>
        /// If the device does not exist or an ambiguous match is found, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="id">The ID of the device to retrieve.</param>
        /// <param name="children">The children of this node.</param>
        /// <exception cref="InvalidOperationException">The specified device does not exist or multiple devices were resolved with the specified ID.</exception>
        /// <returns>A <see cref="DeviceNode"/> encapsulating the specified object.</returns>
        public DeviceNode Device(int id, params PrtgNode[] children) =>
            Device(id, (IEnumerable<PrtgNode>) children);

        /// <summary>
        /// Creates a <see cref="DeviceNode"/> for a device with a specified ID.<para/>
        /// If the device does not exist or an ambiguous match is found, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="id">The ID of the device to retrieve.</param>
        /// <param name="children">The children of this node.</param>
        /// <exception cref="InvalidOperationException">The specified device does not exist or multiple devices were resolved with the specified ID.</exception>
        /// <returns>A <see cref="DeviceNode"/> encapsulating the specified object.</returns>
        public DeviceNode Device(int id, IEnumerable<PrtgNode> children) =>
            PrtgNode.Device(client.GetDevice(id), children);

            #endregion
            #region Name

        /// <summary>
        /// Creates a <see cref="DeviceNode"/> for a device with a specified name.<para/>
        /// If the device does not exist or an ambiguous match is found, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="name">The name of the device to retrieve.</param>
        /// <param name="children">The children of this node.</param>
        /// <exception cref="InvalidOperationException">The specified device does not exist or multiple devices were resolved with the specified name.</exception>
        /// <returns>A <see cref="DeviceNode"/> encapsulating the specified object.</returns>
        public DeviceNode Device(string name, params PrtgNode[] children) =>
            Device(name, (IEnumerable<PrtgNode>) children);

        /// <summary>
        /// Creates a <see cref="DeviceNode"/> for a device with a specified name.<para/>
        /// If the device does not exist or an ambiguous match is found, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="name">The name of the device to retrieve.</param>
        /// <param name="children">The children of this node.</param>
        /// <exception cref="InvalidOperationException">The specified device does not exist or multiple devices were resolved with the specified name.</exception>
        /// <returns>A <see cref="DeviceNode"/> encapsulating the specified object.</returns>
        public DeviceNode Device(string name, IEnumerable<PrtgNode> children) =>
            GetByName(name, client.GetDevices, PrtgNode.Device, children);

            #endregion
            #region Multiple

        /// <summary>
        /// Creates <see cref="DeviceNode"/> objects for all devices that match a specified filter.<para/>
        /// If no devices match the specified filter, a <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="property">The property to search against.</param>
        /// <param name="value">The value to search for.</param>
        /// <exception cref="InvalidOperationException">No devices matched the specified filter.</exception>
        /// <returns>A list of <see cref="DeviceNode"/> objects encapsulating the returned devices.</returns>
        public List<DeviceNode> Devices(Property property, object value) =>
            GetNodes(property, value, client.GetDevices, PrtgNode.Device);

            #endregion
        #endregion
        #region Group
            #region Id

        /// <summary>
        /// Creates a <see cref="GroupNode"/> for a group with a specified ID.<para/>
        /// If the group does not exist or an ambiguous match is found, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="id">The ID of the group to retrieve.</param>
        /// <param name="children">The children of this node.</param>
        /// <exception cref="InvalidOperationException">The specified group does not exist or multiple groups were resolved with the specified ID.</exception>
        /// <returns>A <see cref="GroupNode"/> encapsulating the specified object.</returns>
        public GroupNode Group(int id, params PrtgNode[] children) =>
            Group(id, (IEnumerable<PrtgNode>) children);

        /// <summary>
        /// Creates a <see cref="GroupNode"/> for a group with a specified ID.<para/>
        /// If the group does not exist or an ambiguous match is found, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="id">The ID of the group to retrieve.</param>
        /// <param name="children">The children of this node.</param>
        /// <exception cref="InvalidOperationException">The specified group does not exist or multiple groups were resolved with the specified ID.</exception>
        /// <returns>A <see cref="GroupNode"/> encapsulating the specified object.</returns>
        public GroupNode Group(int id, IEnumerable<PrtgNode> children) =>
            PrtgNode.Group(client.GetGroup(id), children);

            #endregion
            #region Name

        /// <summary>
        /// Creates a <see cref="GroupNode"/> for a group with a specified name.<para/>
        /// If the group does not exist or an ambiguous match is found, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="name">The name of the group to retrieve.</param>
        /// <param name="children">The children of this node.</param>
        /// <exception cref="InvalidOperationException">The specified group does not exist or multiple groups were resolved with the specified name.</exception>
        /// <returns>A <see cref="GroupNode"/> encapsulating the specified object.</returns>
        public GroupNode Group(string name, params PrtgNode[] children) =>
            Group(name, (IEnumerable<PrtgNode>) children);

        /// <summary>
        /// Creates a <see cref="GroupNode"/> for a group with a specified name.<para/>
        /// If the group does not exist or an ambiguous match is found, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="name">The name of the group to retrieve.</param>
        /// <param name="children">The children of this node.</param>
        /// <exception cref="InvalidOperationException">The specified group does not exist or multiple groups were resolved with the specified name.</exception>
        /// <returns>A <see cref="GroupNode"/> encapsulating the specified object.</returns>
        public GroupNode Group(string name, IEnumerable<PrtgNode> children) =>
            GetByName(name, client.GetGroups, PrtgNode.Group, children);

            #endregion
            #region Multiple

        /// <summary>
        /// Creates <see cref="GroupNode"/> objects for all groups that match a specified filter.<para/>
        /// If no groups match the specified filter, a <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="property">The property to search against.</param>
        /// <param name="value">The value to search for.</param>
        /// <exception cref="InvalidOperationException">No groups matched the specified filter.</exception>
        /// <returns>A list of <see cref="GroupNode"/> objects encapsulating the returned groups.</returns>
        public List<GroupNode> Groups(Property property, object value) =>
            GetNodes(property, value, client.GetGroups, PrtgNode.Group);

            #endregion
        #endregion
        #region Probe
            #region Id

        /// <summary>
        /// Creates a <see cref="ProbeNode"/> for a probe with a specified ID.<para/>
        /// If the probe does not exist or an ambiguous match is found, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="id">The ID of the probe to retrieve.</param>
        /// <param name="children">The children of this node.</param>
        /// <exception cref="InvalidOperationException">The specified probe does not exist or multiple probes were resolved with the specified ID.</exception>
        /// <returns>A <see cref="ProbeNode"/> encapsulating the specified object.</returns>
        public ProbeNode Probe(int id, params PrtgNode[] children) =>
            Probe(id, (IEnumerable<PrtgNode>) children);

        /// <summary>
        /// Creates a <see cref="ProbeNode"/> for a probe with a specified ID.<para/>
        /// If the probe does not exist or an ambiguous match is found, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="id">The ID of the probe to retrieve.</param>
        /// <param name="children">The children of this node.</param>
        /// <exception cref="InvalidOperationException">The specified probe does not exist or multiple probes were resolved with the specified ID.</exception>
        /// <returns>A <see cref="ProbeNode"/> encapsulating the specified object.</returns>
        public ProbeNode Probe(int id, IEnumerable<PrtgNode> children) =>
            PrtgNode.Probe(client.GetProbe(id), children);

            #endregion
            #region Name

        /// <summary>
        /// Creates a <see cref="ProbeNode"/> for a probe with a specified name.<para/>
        /// If the probe does not exist or an ambiguous match is found, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="name">The name of the probe to retrieve.</param>
        /// <param name="children">The children of this node.</param>
        /// <exception cref="InvalidOperationException">The specified probe does not exist or multiple probes were resolved with the specified name.</exception>
        /// <returns>A <see cref="ProbeNode"/> encapsulating the specified object.</returns>
        public ProbeNode Probe(string name, params PrtgNode[] children) =>
            Probe(name, (IEnumerable<PrtgNode>) children);

        /// <summary>
        /// Creates a <see cref="ProbeNode"/> for a probe with a specified name.<para/>
        /// If the probe does not exist or an ambiguous match is found, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="name">The name of the probe to retrieve.</param>
        /// <param name="children">The children of this node.</param>
        /// <exception cref="InvalidOperationException">The specified probe does not exist or multiple probes were resolved with the specified name.</exception>
        /// <returns>A <see cref="ProbeNode"/> encapsulating the specified object.</returns>
        public ProbeNode Probe(string name, IEnumerable<PrtgNode> children) =>
            GetByName(name, client.GetProbes, PrtgNode.Probe, children);

            #endregion
            #region Multiple

        /// <summary>
        /// Creates <see cref="ProbeNode"/> objects for all probes that match a specified filter.<para/>
        /// If no probes match the specified filter, a <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="property">The property to search against.</param>
        /// <param name="value">The value to search for.</param>
        /// <exception cref="InvalidOperationException">No probes matched the specified filter.</exception>
        /// <returns>A list of <see cref="ProbeNode"/> objects encapsulating the returned probes.</returns>
        public List<ProbeNode> Probes(Property property, object value) =>
            GetNodes(property, value, client.GetProbes, PrtgNode.Probe);

            #endregion
        #endregion
        #region Triggers

        /// <summary>
        /// Creates a <see cref="TriggerNode"/> for a notification trigger with a specified OnNotificationAction name.<para/>
        /// If the trigger does not exist, an ambiguous match is found, or the resulting notification trigger is inherited, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="objectOrId">The object to retrieve notification triggers from.</param>
        /// <param name="name">The OnNotificationAction name to retrieve.</param>
        /// <exception cref="InvalidOperationException">The specified notification trigger does not exist or multiple triggers were resolved with the specified name.</exception>
        /// <returns>A <see cref="TriggerNode"/> encapsulating the specified object.</returns>
        public TriggerNode Trigger(Either<IPrtgObject, int> objectOrId, string name)
        {
            var triggers = client.GetNotificationTriggers(objectOrId).Where(t => !t.Inherited);

            var trigger = triggers.Where(t => t.OnNotificationAction.Name == name).ToList().SingleObject(name, "name");

            return PrtgNode.Trigger(trigger);
        }

        /// <summary>
        /// Creates <see cref="TriggerNode"/> objects for all non-inherited notification triggers under a specified object.<para/>
        /// If no non-inherited notification triggers are found, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="objectOrId">The object to retrieve notification triggers from.</param>
        /// <exception cref="InvalidOperationException">No notification triggers were found.</exception>
        /// <returns>A list of <see cref="TriggerNode"/> objects encapsulating the returned triggers.</returns>
        public List<TriggerNode> Triggers(Either<IPrtgObject, int> objectOrId)
        {
            return GetNodes(
                PrtgAPI.Property.Id,
                objectOrId,
                (p, o) => client.GetNotificationTriggers((Either<IPrtgObject, int>) o).Where(t => !t.Inherited).ToList(),
                (v, c) => PrtgNode.Trigger(v)
            );
        }

        #endregion
        #region Properties

        /// <summary>
        /// Creates a <see cref="PropertyNode"/> for a specified object property.<para/>
        /// If an <see cref="ObjectProperty"/> is specified, the typed value of the property will be retrieved. Otherwise, the raw value will be used.
        /// </summary>
        /// <param name="objectOrId">The object to retrieve the property from.</param>
        /// <param name="property">The <see cref="ObjectProperty"/> or raw property name of the property to retrieve.</param>
        /// <returns>A <see cref="PropertyNode"/> encapsulating the specified property.</returns>
        public PropertyNode Property(Either<IPrtgObject, int> objectOrId, Either<ObjectProperty, string> property)
        {
            object value;

            if (property.IsLeft)
                value = client.GetObjectProperty(objectOrId, property.Left);
            else
                value = client.GetObjectPropertyRaw(objectOrId, property.Right);

            return PrtgNode.Property(objectOrId, property, value);
        }

        /// <summary>
        /// Creates a <see cref="PropertyNode"/> for all properties of a specified object.
        /// </summary>
        /// <param name="objectOrId">The object to retrieve the properties from.</param>
        /// <returns>A list of <see cref="PropertyNode"/> objects encapsulating the returned properties.</returns>
        public List<PropertyNode> Properties(Either<IPrtgObject, int> objectOrId) =>
            client.GetObjectPropertiesRaw(objectOrId).Select(kv => PrtgNode.Property(objectOrId, kv.Key, kv.Value)).ToList();

        #endregion


        private TNode GetByName<TNode, TObject>(
            string name,
            Func<Property, object, List<TObject>> getResults,
            Func<TObject, IEnumerable<PrtgNode>, TNode> getNode,
            IEnumerable<PrtgNode> children)
            where TNode : PrtgNode
            where TObject : IObject
        {
            var result = getResults(PrtgAPI.Property.Name, name).SingleObject(name, "name");

            return getNode(result, children);
        }

        private List<TNode> GetNodes<TNode, TObject>(
            Property property,
            object value,
            Func<Property, object, List<TObject>> getResults,
            Func<TObject, IEnumerable<PrtgNode>, TNode> getNode)
        {
            var results = getResults(property, value);

            if(results.Count == 0)
                throw new InvalidOperationException($"Failed to resolve any {IObjectExtensions.GetTypeDescription(typeof(TObject))} objects for query {property} = {value}.");

            return results.Select(r => getNode(r, null)).ToList();
        }
    }
}
