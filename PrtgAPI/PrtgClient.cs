using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Helpers;
using PrtgAPI.Objects.Deserialization;
using PrtgAPI.Objects.Shared;
using PrtgAPI.Objects.Undocumented;
using PrtgAPI.Parameters;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using PrtgAPI.Exceptions.Internal;
using PrtgAPI.Request;

namespace PrtgAPI
{
    /// <summary>
    /// <para type="description">Provides methods for generating API requests against a PRTG Network Monitor server.</para>
    /// </summary>
    public partial class PrtgClient
    {
        /// <summary>
        /// Gets the PRTG server API requests will be made against.
        /// </summary>
        public string Server { get; }

        /// <summary>
        /// Gets the Username that will be used to authenticate against PRTG.
        /// </summary>
        public string UserName { get; }

        /// <summary>
        /// The PassHash that will be used to authenticate with, in place of a password.
        /// </summary>
        public string PassHash { get; }

        /// <summary>
        /// The number of times to retry a request that times out while communicating with PRTG.
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        /// The base delay (in seconds) between retrying a timed out request. Each successive failure of a given request will wait an additional multiple of this value.
        /// </summary>
        public int RetryDelay { get; set; }

        internal EventHandler<RetryRequestEventArgs> retryRequest;

        /// <summary>
        /// Occurs when a request times out while communicating with PRTG.
        /// </summary>
        public event EventHandler<RetryRequestEventArgs> RetryRequest
        {
            add { retryRequest += value; }
            remove { retryRequest -= value; }
        }

        internal EventHandler<LogVerboseEventArgs> logVerbose;

        /// <summary>
        /// Occurs when a request times out while communicating with PRTG.
        /// </summary>
        public event EventHandler<LogVerboseEventArgs> LogVerbose
        {
            add { logVerbose += value; }
            remove { logVerbose -= value; }
        }

        internal void Log(string message)
        {
            HandleEvent(logVerbose, new LogVerboseEventArgs(message));
        }

        internal void HandleEvent<T>(EventHandler<T> handler, T args)
        {
            handler?.Invoke(this, args);
        }

        private RequestEngine requestEngine;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrtgClient"/> class.
        /// </summary>
        public PrtgClient(string server, string username, string pass, AuthMode authMode = AuthMode.Password)
            : this(server, username, pass, authMode, new PrtgWebClient())
        {
        }

        internal PrtgClient(string server, string username, string pass, AuthMode authMode, IWebClient client)
        {
            if (server == null)
                throw new ArgumentNullException(nameof(server));

            if (username == null)
                throw new ArgumentNullException(nameof(username));

            if (pass == null)
                throw new ArgumentNullException(nameof(pass));

            requestEngine = new RequestEngine(this, client);
            
            Server = server;
            UserName = username;

            PassHash = authMode == AuthMode.Password ? GetPassHash(pass) : pass;
        }

#region Requests
    #region Object Data

        private string GetPassHash(string password)
        {
            var parameters = new Parameters.Parameters()
            {
                [Parameter.Password] = password
            };

            var response = requestEngine.ExecuteRequest(JsonFunction.GetPassHash, parameters);

            if(!Regex.Match(response, "^[0-9]+$").Success)
                throw new PrtgRequestException($"Could not retrieve PassHash from PRTG Server. PRTG responded '{response}'");

            return response;
        }

        internal List<T> GetObjects<T>(Parameters.Parameters parameters)
        {
            var response = requestEngine.ExecuteRequest(XmlFunction.TableData, parameters);

            var data = Data<T>.DeserializeList(response);

            return data.Items;
        }

        internal IEnumerable<T> StreamObjects<T>(ContentParameters<T> parameters)
        {
            Log("Preparing to stream objects");
            Log("Requesting total number of objects");

            var totalObjects = GetTotalObjects(parameters.Content);

            var tasks = new List<Task<List<T>>>();

            parameters.Count = 500;

            for (int i = 0; i < totalObjects;)
            {
                tasks.Add(GetObjectsAsync<T>(parameters));

                i = i + parameters.Count;
                parameters.Page++;

                if (totalObjects - i < parameters.Count)
                    parameters.Count = totalObjects - i;
            }

            Log($"Requesting {totalObjects} objects from PRTG over {tasks.Count} tasks");

            var result = new ParallelObjectGenerator<List<T>>(tasks.WhenAnyForAll()).SelectMany(m => m);

            return result;
        }

        #region Sensors
            #region Default

        /// <summary>
        /// Retrieve all sensors from a PRTG Server.
        /// </summary>
        /// <returns>A list of all sensors on a PRTG Server.</returns>
        public List<Sensor> GetSensors() => GetSensors(new SensorParameters());

        /// <summary>
        /// Asynchronously retrieve all sensors from a PRTG Server.
        /// </summary>
        /// <returns>A task that returns a list of all sensors on a PRTG Server.</returns>
        public async Task<List<Sensor>> GetSensorsAsync() => await GetSensorsAsync(new SensorParameters()).ConfigureAwait(false);

        /// <summary>
        /// Stream all sensors from a PRTG Server. When this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.<para/>
        /// </summary>
        /// <returns>A generator encapsulating a series of <see cref="Task"/> objects to capable of streaming a response from a PRTG Server.</returns>
        public IEnumerable<Sensor> StreamSensors() => StreamSensors(new SensorParameters());

            #endregion
            #region Sensor Status
   
        /// <summary>
        /// Retrieve sensors from a PRTG Server of one or more statuses.
        /// </summary>
        /// <param name="sensorStatuses">A list of sensor statuses to filter for.</param>
        /// <returns></returns>
        public List<Sensor> GetSensors(params SensorStatus[] sensorStatuses) => GetSensors(new SensorParameters { StatusFilter = sensorStatuses });

        /// <summary>
        /// Asynchronously retrieve sensors from a PRTG Server of one or more statuses.
        /// </summary>
        /// <param name="sensorStatuses">A list of sensor statuses to filter for.</param>
        /// <returns></returns>
        public async Task<List<Sensor>> GetSensorsAsync(params SensorStatus[] sensorStatuses) => await GetSensorsAsync(new SensorParameters { StatusFilter = sensorStatuses }).ConfigureAwait(false);

        /// <summary>
        /// Stream sensors from a PRTG Server of one or more statuses. When this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <param name="sensorStatuses">A list of sensor statuses to filter for.</param>
        /// <returns></returns>
        public IEnumerable<Sensor> StreamSensors(params SensorStatus[] sensorStatuses) => StreamSensors(new SensorParameters { StatusFilter = sensorStatuses });

            #endregion
            #region Filter (Property, Value)

        /// <summary>
        /// Retrieve sensors from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns></returns>
        public List<Sensor> GetSensors(Property property, object value) => GetSensors(new SearchFilter(property, value));

        /// <summary>
        /// Asynchronously retrieve sensors from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns></returns>
        public async Task<List<Sensor>> GetSensorsAsync(Property property, object value) => await GetSensorsAsync(new SearchFilter(property, value)).ConfigureAwait(false);

        /// <summary>
        /// Stream sensors from a PRTG Server based on the value of a certain property. When this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns></returns>
        public IEnumerable<Sensor> StreamSensors(Property property, object value) => StreamSensors(new SearchFilter(property, value));

            #endregion
            #region Filter (Property, Operator, Value)

        /// <summary>
        /// Retrieve sensors from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="operator">Operator to compare value and property value with.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns></returns>
        public List<Sensor> GetSensors(Property property, FilterOperator @operator, object value) => GetSensors(new SearchFilter(property, @operator, value));

        /// <summary>
        /// Asynchronously retrieve sensors from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="operator">Operator to compare value and property value with.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns></returns>
        public async Task<List<Sensor>> GetSensorsAsync(Property property, FilterOperator @operator, object value) => await GetSensorsAsync(new SearchFilter(property, @operator, value)).ConfigureAwait(false);

        /// <summary>
        /// Stream sensors from a PRTG Server based on the value of a certain property. When this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="operator">Operator to compare value and property value with.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns></returns>
        public IEnumerable<Sensor> StreamSensors(Property property, FilterOperator @operator, object value) => StreamSensors(new SearchFilter(property, @operator, value));

            #endregion
            #region Filter (Array)

        /// <summary>
        /// Retrieve sensors from a PRTG Server based on the values of multiple properties.
        /// </summary>
        /// <param name="filters">One or more filters used to limit search results.</param>
        /// <returns></returns>
        public List<Sensor> GetSensors(params SearchFilter[] filters) => GetSensors(new SensorParameters { SearchFilter = filters });

        /// <summary>
        /// Asynchronously retrieve sensors from a PRTG Server based on the values of multiple properties.
        /// </summary>
        /// <param name="filters">One or more filters used to limit search results.</param>
        /// <returns></returns>
        public async Task<List<Sensor>> GetSensorsAsync(params SearchFilter[] filters) => await GetSensorsAsync(new SensorParameters { SearchFilter = filters }).ConfigureAwait(false);

        /// <summary>
        /// Stream sensors from a PRTG Server based on the values of multiple properties. When this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <param name="filters">One or more filters used to limit search results.</param>
        /// <returns></returns>
        public IEnumerable<Sensor> StreamSensors(params SearchFilter[] filters) => StreamSensors(new SensorParameters { SearchFilter = filters });

            #endregion
            #region Parameters

        /// <summary>
        /// Retrieve sensors from a PRTG Server using a custom set of parameters.
        /// </summary>
        /// <param name="parameters">A custom set of parameters used to retrieve PRTG Sensors.</param>
        /// <returns>A list of sensors that match the specified parameters.</returns>
        public List<Sensor> GetSensors(SensorParameters parameters) => GetObjects<Sensor>(parameters);

        /// <summary>
        /// Asynchronously retrieve sensors from a PRTG Server using a custom set of parameters.
        /// </summary>
        /// <param name="parameters">A custom set of parameters used to retrieve PRTG Sensors.</param>
        /// <returns>A list of sensors that match the specified parameters.</returns>
        public async Task<List<Sensor>> GetSensorsAsync(SensorParameters parameters) => await GetObjectsAsync<Sensor>(parameters).ConfigureAwait(false);

        /// <summary>
        /// Stream sensors from a PRTG Server using a custom set of parameters. When this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <param name="parameters">A custom set of parameters used to retrieve PRTG Sensors.</param>
        /// <returns>A list of sensors that match the specified parameters.</returns>
        public IEnumerable<Sensor> StreamSensors(SensorParameters parameters) => StreamObjects<Sensor>(parameters);

            #endregion

        /// <summary>
        /// Retrieve the number of sensors of each sensor type in the system.
        /// </summary>
        /// <returns></returns>
        public SensorTotals GetSensorTotals()
        {
            var response = requestEngine.ExecuteRequest(XmlFunction.GetTreeNodeStats, new Parameters.Parameters());

            return Data<SensorTotals>.DeserializeType(response);
        }

        #endregion
        #region Devices
            #region Default

        /// <summary>
        /// Retrieve all devices from a PRTG Server.
        /// </summary>
        /// <returns></returns>
        public List<Device> GetDevices() => GetDevices(new DeviceParameters());

        /// <summary>
        /// Asynchronously retrieve all devices from a PRTG Server.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Device>> GetDevicesAsync() => await GetDevicesAsync(new DeviceParameters()).ConfigureAwait(false);

        /// <summary>
        /// Stream all devices from a PRTG Server. When this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Device> StreamDevices() => StreamDevices(new DeviceParameters());

            #endregion
            #region Filter (Property, Value)

        /// <summary>
        /// Retrieve devices from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns></returns>
        public List<Device> GetDevices(Property property, object value) => GetDevices(new SearchFilter(property, value));

        /// <summary>
        /// Asynchronously retrieve devices from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns></returns>
        public async Task<List<Device>> GetDevicesAsync(Property property, object value) => await GetDevicesAsync(new SearchFilter(property, value)).ConfigureAwait(false);

        /// <summary>
        /// Stream devices from a PRTG Server based on the value of a certain property. When this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns></returns>
        public IEnumerable<Device> StreamDevices(Property property, object value) => StreamDevices(new SearchFilter(property, value));

            #endregion
            #region Filter (Property, Operator, Value)

        /// <summary>
        /// Retrieve devices from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="operator">Operator to compare value and property value with.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns></returns>
        public List<Device> GetDevices(Property property, FilterOperator @operator, string value) => GetDevices(new SearchFilter(property, @operator, value));

        /// <summary>
        /// Asynchronously retrieve devices from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="operator">Operator to compare value and property value with.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns></returns>
        public async Task<List<Device>> GetDevicesAsync(Property property, FilterOperator @operator, string value) => await GetDevicesAsync(new SearchFilter(property, @operator, value)).ConfigureAwait(false);

        /// <summary>
        /// Stream devices from a PRTG Server based on the value of a certain property. When this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="operator">Operator to compare value and property value with.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns></returns>
        public IEnumerable<Device> StreamDevices(Property property, FilterOperator @operator, string value) => StreamDevices(new SearchFilter(property, @operator, value));

            #endregion
            #region Filter (Array)

        /// <summary>
        /// Retrieve devices from a PRTG Server based on the values of multiple properties.
        /// </summary>
        /// <param name="filters">One or more filters used to limit search results.</param>
        /// <returns></returns>
        public List<Device> GetDevices(params SearchFilter[] filters) => GetDevices(new DeviceParameters { SearchFilter = filters });

        /// <summary>
        /// Asynchronously retrieve devices from a PRTG Server based on the values of multiple properties.
        /// </summary>
        /// <param name="filters">One or more filters used to limit search results.</param>
        /// <returns></returns>
        public async Task<List<Device>> GetDevicesAsync(params SearchFilter[] filters) => await GetDevicesAsync(new DeviceParameters { SearchFilter = filters }).ConfigureAwait(false);

        /// <summary>
        /// Stream devices from a PRTG Server based on the values of multiple properties. When this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <param name="filters">One or more filters used to limit search results.</param>
        /// <returns></returns>
        public IEnumerable<Device> StreamDevices(params SearchFilter[] filters) => StreamDevices(new DeviceParameters { SearchFilter = filters });

            #endregion
            #region Parameters

        /// <summary>
        /// Retrieve devices from a PRTG Server using a custom set of parameters.
        /// </summary>
        /// <param name="parameters">A custom set of parameters used to retrieve PRTG Devices.</param>
        /// <returns></returns>
        public List<Device> GetDevices(DeviceParameters parameters) => GetObjects<Device>(parameters);

        /// <summary>
        /// Asynchronously retrieve devices from a PRTG Server using a custom set of parameters.
        /// </summary>
        /// <param name="parameters">A custom set of parameters used to retrieve PRTG Devices.</param>
        /// <returns></returns>
        public async Task<List<Device>> GetDevicesAsync(DeviceParameters parameters) => await GetObjectsAsync<Device>(parameters).ConfigureAwait(false);

        /// <summary>
        /// Stream devices from a PRTG Server using a custom set of parameters. When this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <param name="parameters">A custom set of parameters used to retrieve PRTG Devices.</param>
        /// <returns></returns>
        public IEnumerable<Device> StreamDevices(DeviceParameters parameters) => StreamObjects<Device>(parameters);

        #endregion
        #endregion
        #region Groups
            #region Default

        /// <summary>
        /// Retrieve all groups from a PRTG Server.
        /// </summary>
        /// <returns></returns>
        public List<Group> GetGroups() => GetGroups(new GroupParameters());

        /// <summary>
        /// Asynchronously retrieve all groups from a PRTG Server.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Group>> GetGroupsAsync() => await GetGroupsAsync(new GroupParameters()).ConfigureAwait(false);

        /// <summary>
        /// Stream all groups from a PRTG Server. When this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Group> StreamGroups() => StreamGroups(new GroupParameters());

            #endregion
            #region Filter (Property, Value)

        /// <summary>
        /// Retrieve groups from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns></returns>
        public List<Group> GetGroups(Property property, object value) => GetGroups(new SearchFilter(property, value));

        /// <summary>
        /// Asynchronously retrieve groups from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns></returns>
        public async Task<List<Group>> GetGroupsAsync(Property property, object value) => await GetGroupsAsync(new SearchFilter(property, value)).ConfigureAwait(false);

        /// <summary>
        /// Stream groups from a PRTG Server based on the value of a certain property. When this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns></returns>
        public IEnumerable<Group> StreamGroups(Property property, object value) => StreamGroups(new SearchFilter(property, value));

            #endregion
            #region Filter (Property, Operator, Group)

        /// <summary>
        /// Retrieve groups from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="operator">Operator to compare value and property value with.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns></returns>
        public List<Group> GetGroups(Property property, FilterOperator @operator, string value) => GetGroups(new SearchFilter(property, @operator, value));

        /// <summary>
        /// Asynchronously retrieve groups from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="operator">Operator to compare value and property value with.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns></returns>
        public async Task<List<Group>> GetGroupsAsync(Property property, FilterOperator @operator, string value) => await GetGroupsAsync(new SearchFilter(property, @operator, value)).ConfigureAwait(false);

        /// <summary>
        /// Stream groups from a PRTG Server based on the value of a certain property. When this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="operator">Operator to compare value and property value with.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns></returns>
        public IEnumerable<Group> StreamGroups(Property property, FilterOperator @operator, string value) => StreamGroups(new SearchFilter(property, @operator, value));

            #endregion
            #region Filter (Array)

        /// <summary>
        /// Retrieve groups from a PRTG Server based on the values of multiple properties.
        /// </summary>
        /// <param name="filters">One or more filters used to limit search results.</param>
        /// <returns></returns>
        public List<Group> GetGroups(params SearchFilter[] filters) => GetGroups(new GroupParameters { SearchFilter = filters });

        /// <summary>
        /// Asynchronously retrieve groups from a PRTG Server based on the values of multiple properties.
        /// </summary>
        /// <param name="filters">One or more filters used to limit search results.</param>
        /// <returns></returns>
        public async Task<List<Group>> GetGroupsAsync(params SearchFilter[] filters) => await GetGroupsAsync(new GroupParameters { SearchFilter = filters }).ConfigureAwait(false);

        /// <summary>
        /// Stream groups from a PRTG Server based on the values of multiple properties. When this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <param name="filters">One or more filters used to limit search results.</param>
        /// <returns></returns>
        public IEnumerable<Group> StreamGroups(params SearchFilter[] filters) => StreamGroups(new GroupParameters { SearchFilter = filters });

            #endregion
            #region Parameters

        /// <summary>
        /// Retrieve groups from a PRTG Server using a custom set of parameters.
        /// </summary>
        /// <param name="parameters">A custom set of parameters used to retrieve PRTG Groups.</param>
        public List<Group> GetGroups(GroupParameters parameters) => GetObjects<Group>(parameters);

        /// <summary>
        /// Asynchronously retrieve groups from a PRTG Server using a custom set of parameters.
        /// </summary>
        public async Task<List<Group>> GetGroupsAsync(GroupParameters parameters) => await GetObjectsAsync<Group>(parameters).ConfigureAwait(false);

        /// <summary>
        /// Stream groups from a PRTG Server using a custom set of parameters. When this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        public IEnumerable<Group> StreamGroups(GroupParameters parameters) => StreamObjects<Group>(parameters);

            #endregion
        #endregion
        #region Probes
            #region Default

        /// <summary>
        /// Retrieve all probes from a PRTG Server.
        /// </summary>
        /// <returns></returns>
        public List<Probe> GetProbes() => GetProbes(new ProbeParameters());

        /// <summary>
        /// Asynchronously retrieve all probes from a PRTG Server.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Probe>> GetProbesAsync() => await GetProbesAsync(new ProbeParameters()).ConfigureAwait(false);

        /// <summary>
        /// Stream all probes from a PRTG Server. When this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Probe> StreamProbes() => StreamProbes(new ProbeParameters());

            #endregion
            #region Filter (Property, Value)

        /// <summary>
        /// Retrieve probes from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns></returns>
        public List<Probe> GetProbes(Property property, object value) => GetProbes(new SearchFilter(property, value));

        /// <summary>
        /// Asynchronously retrieve probes from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns></returns>
        public async Task<List<Probe>> GetProbesAsync(Property property, object value) => await GetProbesAsync(new SearchFilter(property, value)).ConfigureAwait(false);

        /// <summary>
        /// Stream probes from a PRTG Server based on the value of a certain property. When this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns></returns>
        public IEnumerable<Probe> StreamProbes(Property property, object value) => StreamProbes(new SearchFilter(property, value));

            #endregion
            #region Filter (Property, Operator, Value)

        /// <summary>
        /// Retrieve probes from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="operator">Operator to compare value and property value with.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns></returns>
        public List<Probe> GetProbes(Property property, FilterOperator @operator, string value) => GetProbes(new SearchFilter(property, @operator, value));

        /// <summary>
        /// Asynchronously retrieve probes from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="operator">Operator to compare value and property value with.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns></returns>
        public async Task<List<Probe>> GetProbesAsync(Property property, FilterOperator @operator, string value) => await GetProbesAsync(new SearchFilter(property, @operator, value)).ConfigureAwait(false);

        /// <summary>
        /// Stream probes from a PRTG Server based on the value of a certain property. When this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="operator">Operator to compare value and property value with.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns></returns>
        public IEnumerable<Probe> StreamProbes(Property property, FilterOperator @operator, string value) => StreamProbes(new SearchFilter(property, @operator, value));

            #endregion
            #region Filter (Array)

        /// <summary>
        /// Retrieve probes from a PRTG Server based on the values of multiple properties.
        /// </summary>
        /// <param name="filters">One or more filters used to limit search results.</param>
        /// <returns></returns>
        public List<Probe> GetProbes(params SearchFilter[] filters) => GetProbes(new ProbeParameters { SearchFilter = filters });

        /// <summary>
        /// Asynchronously retrieve probes from a PRTG Server based on the values of multiple properties.
        /// </summary>
        /// <param name="filters">One or more filters used to limit search results.</param>
        /// <returns></returns>
        public async Task<List<Probe>> GetProbesAsync(params SearchFilter[] filters) => await GetProbesAsync(new ProbeParameters { SearchFilter = filters }).ConfigureAwait(false);

        /// <summary>
        /// Stream probes from a PRTG Server based on the values of multiple properties. When this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <param name="filters">One or more filters used to limit search results.</param>
        /// <returns></returns>
        public IEnumerable<Probe> StreamProbes(params SearchFilter[] filters) => StreamProbes(new ProbeParameters { SearchFilter = filters });

            #endregion
            #region Parameters

        /// <summary>
        /// Retrieve probes from a PRTG Server using a custom set of parameters.
        /// </summary>
        /// <param name="parameters">A custom set of parameters used to retrieve PRTG Probes.</param>
        public List<Probe> GetProbes(ProbeParameters parameters) => GetObjects<Probe>(parameters);

        /// <summary>
        /// Asynchronously retrieve probes from a PRTG Server using a custom set of parameters.
        /// </summary>
        /// <param name="parameters">A custom set of parameters used to retrieve PRTG Probes.</param>
        public async Task<List<Probe>> GetProbesAsync(ProbeParameters parameters) => await GetObjectsAsync<Probe>(parameters).ConfigureAwait(false);

        /// <summary>
        /// Stream probes from a PRTG Server using a custom set of parameters. When this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <param name="parameters">A custom set of parameters used to retrieve PRTG Probes.</param>
        public IEnumerable<Probe> StreamProbes(ProbeParameters parameters) => StreamObjects<Probe>(parameters);

            #endregion
        #endregion
        #region Channel

        /// <summary>
        /// Retrieve all channels of a sensor.
        /// </summary>
        /// <param name="sensorId">The ID of the sensor to retrieve channels for.</param>
        /// <returns></returns>
        public List<Channel> GetChannels(int sensorId)
        {
            return GetChannelsInternal(sensorId, true);
        }

        private List<Channel> GetChannelsInternal(int sensorId, bool includeAdvancedProperties)
        {
            var response = requestEngine.ExecuteRequest(XmlFunction.TableData, new ChannelParameters(sensorId));

            response.Descendants("item").Where(item => item.Element("objid").Value == "-4").Remove();

            var items = response.Descendants("item").ToList();

            foreach (var item in items)
            {
                var id = Convert.ToInt32(item.Element("objid").Value);

                var properties = GetChannelProperties(sensorId, id);

                item.Add(properties.Nodes());
                item.Add(new XElement("injected_sensorId", sensorId));
            }

            return Data<Channel>.DeserializeList(response).Items;
        }

        //TODO: how do we abstract this stuff into a single function for sync and async versions

        public async Task<List<Channel>> GetChannelsAsync(int sensorId)
        {
            return await GetChannelsInternalAsync(sensorId, true).ConfigureAwait(false);
        }

        private async Task<List<Channel>> GetChannelsInternalAsync(int sensorId, bool includeAdvancedProperties)
        {
            var response = await requestEngine.ExecuteRequestAsync(XmlFunction.TableData, new ChannelParameters(sensorId)).ConfigureAwait(false);

            response.Descendants("item").Where(item => item.Element("objid").Value == "-4").Remove();

            var items = response.Descendants("item").ToList();

            foreach (var item in items)
            {
                var id = Convert.ToInt32(item.Element("objid").Value);

                var properties = await GetChannelPropertiesAsync(sensorId, id).ConfigureAwait(false);

                item.Add(properties.Nodes());
                item.Add(new XElement("injected_sensorId", sensorId));
            }

            return Data<Channel>.DeserializeList(response).Items;
        }

        internal XElement GetChannelProperties(int sensorId, int channelId)
        {
            var parameters = new ChannelPropertiesParameters(sensorId, channelId);

            var response = requestEngine.ExecuteRequest(HtmlFunction.ChannelEdit, parameters);

            return ChannelSettings.GetXml(response, channelId);
        }

        internal async Task<XElement> GetChannelPropertiesAsync(int sensorId, int channelId)
        {
            var parameters = new ChannelPropertiesParameters(sensorId, channelId);

            var response = await requestEngine.ExecuteRequestAsync(HtmlFunction.ChannelEdit, parameters);

            return ChannelSettings.GetXml(response, channelId);
        }

        #endregion
        #region Notifications

        /// <summary>
        /// Retrieve all notification actions on a PRTG Server.
        /// </summary>
        /// <returns></returns>
        public List<NotificationAction> GetNotificationActions() => GetObjects<NotificationAction>(new NotificationActionParameters());

        /// <summary>
        /// Asynchronously retrieve all notification actions on a PRTG Server.
        /// </summary>
        /// <returns></returns>
        public Task<List<NotificationAction>> GetNotificationActionsAsync() => GetObjectsAsync<NotificationAction>(new NotificationActionParameters());

        /// <summary>
        /// Retrieve all notification triggers of a PRTG Object.
        /// </summary>
        /// <param name="objectId">The object to retrieve triggers for.</param>
        /// <returns>A list of notification triggers that apply to the specified object.</returns>
        public List<NotificationTrigger> GetNotificationTriggers(int objectId)
        {
            var parameters = new NotificationTriggerParameters(objectId);

            var xmlResponse = requestEngine.ExecuteRequest(XmlFunction.TableData, parameters);

            var parsed = ParseNotificationTriggerResponse(objectId, xmlResponse);

            UpdateTriggerChannels(parsed);

            return parsed;
        }

        /// <summary>
        /// Asynchronously retrieve all notification triggers of a PRTG Object.
        /// </summary>
        /// <param name="objectId">The object to retrieve triggers for.</param>
        /// <returns>A list of notification triggers that apply to the specified object.</returns>
        public async Task<List<NotificationTrigger>> GetNotificationTriggersAsync(int objectId)
        {
            var parameters = new NotificationTriggerParameters(objectId);

            var xmlResponse = await requestEngine.ExecuteRequestAsync(XmlFunction.TableData, parameters).ConfigureAwait(false);

            var parsed = ParseNotificationTriggerResponse(objectId, xmlResponse);

            await UpdateTriggerChannelsAsync(parsed);

            return parsed;
        }

        private List<NotificationTrigger> ParseNotificationTriggerResponse(int objectId, XDocument xmlResponse)
        {
            var xmlResponseContent = xmlResponse.Descendants("item").Select(x => new
            {
                Content = x.Element("content").Value,
                Id = Convert.ToInt32(x.Element("objid").Value)
            }).ToList();

            var triggers = JsonDeserializer<NotificationTrigger>.DeserializeList(xmlResponseContent, e => e.Content,
                (e, o) =>
                {
                    o.SubId = e.Id;
                    o.ObjectId = objectId;
                }
            );

            return triggers;
        }

        private void UpdateTriggerChannels(List<NotificationTrigger> triggers)
        {
            foreach (var trigger in triggers)
            {
                if (trigger.RequiresChannelId())
                {
                    Log("Retrieving Channel ID for sensor specific channel based Notification Trigger");
                    trigger.channelId = GetChannelsInternal(trigger.ObjectId, false).First(t => t.Name == trigger.channel).Id;
                }
            }
        }

        private async Task UpdateTriggerChannelsAsync(List<NotificationTrigger> triggers)
        {
            foreach (var trigger in triggers)
            {
                if (trigger.RequiresChannelId())
                {
                    Log("Asynchronously retrieving Channel ID for sensor specific channel based Notification Trigger");
                    trigger.channelId = (await GetChannelsInternalAsync(trigger.ObjectId, false).ConfigureAwait(false)).First(t => t.Name == trigger.channel).Id;
                }
            }
        }

        /// <summary>
        /// Retrieve all notification trigger types supported by a PRTG Object.
        /// </summary>
        /// <param name="objectId">The object to retrieve supported trigger types for.</param>
        /// <returns></returns>
        public List<TriggerType> GetNotificationTriggerTypes(int objectId)
        {
            var response = GetNotificationTriggerData(objectId);

            var data = JsonDeserializer<NotificationTriggerData>.DeserializeType(response);

            return data.SupportedTypes.ToList();
        }

        private string GetNotificationTriggerData(int objectId)
        {
            var parameters = new Parameters.Parameters
            {
                [Parameter.Id] = objectId
            };

            var response = requestEngine.ExecuteRequest(JsonFunction.Triggers, parameters);
            response = response.Replace("\"data\": \"(no triggers defined)\",", "");

            return response;
        }

        #endregion

    #endregion
    #region Object Manipulation
        #region Sensor State

        /// <summary>
        /// Mark a <see cref="SensorStatus.Down"/> sensor as <see cref="SensorStatus.DownAcknowledged"/>. If an acknowledged sensor returns to <see cref="SensorStatus.Up"/>, it will not be acknowledged when it goes down again.
        /// </summary>
        /// <param name="objectId">ID of the sensor to acknowledge.</param>
        /// <param name="duration">Duration (in minutes) to acknowledge the object for. If null, sensor will be paused indefinitely.</param>
        /// <param name="message">Message to display on the acknowledged sensor.</param>
        public void AcknowledgeSensor(int objectId, int? duration = null, string message = null) => requestEngine.ExecuteRequest(CommandFunction.AcknowledgeAlarm, new AcknowledgeSensorParameters(objectId, duration, message));

        /// <summary>
        /// Asynchronously mark a <see cref="SensorStatus.Down"/> sensor as <see cref="SensorStatus.DownAcknowledged"/>. If an acknowledged sensor returns to <see cref="SensorStatus.Up"/>, it will not be acknowledged when it goes down again.
        /// </summary>
        /// <param name="objectId">ID of the sensor to acknowledge.</param>
        /// <param name="duration">Duration (in minutes) to acknowledge the object for. If null, sensor will be paused indefinitely.</param>
        /// <param name="message">Message to display on the acknowledged sensor.</param>
        public async void AcknowledgeSensorAsync(int objectId, int? duration = null, string message = null) => await requestEngine.ExecuteRequestAsync(CommandFunction.AcknowledgeAlarm, new AcknowledgeSensorParameters(objectId, duration, message)).ConfigureAwait(false);

        /// <summary>
        /// Pause a PRTG Object (sensor, device, etc).
        /// </summary>
        /// <param name="objectId">ID of the object to pause.</param>
        /// <param name="durationMinutes">Duration (in minutes) to pause the object for. If null, object will be paused indefinitely.</param>
        /// <param name="pauseMessage">Message to display on the paused object.</param>
        public void PauseObject(int objectId, int? durationMinutes = null, string pauseMessage = null)
        {
            PauseParametersBase parameters;
            CommandFunction? function = null;

            if (durationMinutes == null)
            {
                parameters = new PauseParameters(objectId);
                function = CommandFunction.Pause;
            }
            else
            {
                parameters = new PauseForDurationParameters(objectId, (int)durationMinutes);
                function = CommandFunction.PauseObjectFor;
            }
                
            if (pauseMessage != null)
                parameters.PauseMessage = pauseMessage;

            requestEngine.ExecuteRequest(function.Value, parameters);
        }

        /// <summary>
        /// Resume a PRTG Object (e.g. sensor or device) from a Paused or Simulated Error state.
        /// </summary>
        /// <param name="objectId">ID of the object to resume.</param>
        public void ResumeObject(int objectId) => requestEngine.ExecuteRequest(CommandFunction.Pause, new PauseParameters(objectId, PauseAction.Resume));

        /// <summary>
        /// Asynchronously resume a PRTG Object (e.g. sensor or device) from a Paused or Simulated Error state.
        /// </summary>
        /// <param name="objectId">ID of the object to resume.</param>
        public async void ResumeObjectAsync(int objectId) => await requestEngine.ExecuteRequestAsync(CommandFunction.Pause, new PauseParameters(objectId, PauseAction.Resume)).ConfigureAwait(false);

        /// <summary>
        /// Simulate an error state for a sensor.
        /// </summary>
        /// <param name="sensorId">ID of the sensor to simulate an error for.</param>
        public void SimulateError(int sensorId) => requestEngine.ExecuteRequest(CommandFunction.Simulate, new SimulateErrorParameters(sensorId));

        /// <summary>
        /// Asynchronously simulate an error state for a sensor.
        /// </summary>
        /// <param name="sensorId">ID of the sensor to simulate an error for.</param>
        public async void SimulateErrorAsync(int sensorId) => await requestEngine.ExecuteRequestAsync(CommandFunction.Simulate, new SimulateErrorParameters(sensorId)).ConfigureAwait(false);

        #endregion
        #region Notifications

        /// <summary>
        /// Add a notification trigger to a PRTG Server.
        /// </summary>
        /// <param name="parameters"></param>
        public void AddNotificationTrigger(TriggerParameters parameters) => SetNotificationTrigger(parameters);

        /// <summary>
        /// Add or edit a notification trigger on a PRTG Server.
        /// </summary>
        /// <param name="parameters">A set of parameters describing the type of notification trigger and how to manipulate it.</param>
        public void SetNotificationTrigger(TriggerParameters parameters)
        {
            if (parameters.Action == ModifyAction.Add)
            {
                var response = GetNotificationTriggerData(parameters.ObjectId);

                var data = JsonDeserializer<NotificationTriggerData>.DeserializeType(response);

                if (!data.SupportedTypes.Contains(parameters.Type))
                    throw new InvalidTriggerTypeException(parameters.ObjectId, parameters.Type, data.SupportedTypes.ToList());
            }
            else if (parameters.Action == ModifyAction.Edit)
            {
                var properties = parameters.GetType().GetProperties().Where(p => p.GetCustomAttribute<RequireValueAttribute>()?.ValueRequired == true).Where(p => p.GetValue(parameters) == null).ToList();

                //if(properties.Count > 0)
                //    throw new Exception
            }
            else
                throw new NotImplementedException($"Handler missing for modify action '{parameters.Action}'");

            requestEngine.ExecuteRequest(HtmlFunction.EditSettings, parameters);
        }

        //todo make a note this doesnt check for inheritance. find out what happens if you try and remove an inherited trigger using this? whats the http response?
        public void RemoveNotificationTrigger(int objectId, int triggerId) => requestEngine.ExecuteRequest(HtmlFunction.RemoveSubObject, new RemoveTriggerParameters(objectId, triggerId));

        /// <summary>
        /// Remove a notification trigger from an object.
        /// </summary>
        /// <param name="trigger">The notification trigger to remove.</param>
        public void RemoveNotificationTrigger(NotificationTrigger trigger) => requestEngine.ExecuteRequest(HtmlFunction.RemoveSubObject, new RemoveTriggerParameters(trigger));

        #endregion
        #region Miscellaneous

        /// <summary>
        /// Request an object or any children of an object refresh themselves immediately.
        /// </summary>
        /// <param name="objectId">The ID of the sensor, or the ID of a Probe, Group or Device whose child sensors should be refreshed.</param>
        public void RefreshObject(int objectId) => requestEngine.ExecuteRequest(CommandFunction.ScanNow, new BaseActionParameters(objectId));

        /// <summary>
        /// Asynchronously request an object or any children of an object refresh themselves immediately.
        /// </summary>
        /// <param name="objectId">The ID of the sensor, or the ID of a Probe, Group or Device whose child sensors should be refreshed.</param>
        public async void RefreshObjectAsync(int objectId) => await requestEngine.ExecuteRequestAsync(CommandFunction.ScanNow, new BaseActionParameters(objectId)).ConfigureAwait(false);

        /// <summary>
        /// Automatically create sensors under an object based on the object's (or it's children's) device type.
        /// </summary>
        /// <param name="objectId">The object to run Auto-Discovery for (such as a device or group).</param>
        public void AutoDiscover(int objectId) => requestEngine.ExecuteRequest(CommandFunction.DiscoverNow, new BaseActionParameters(objectId));

        /// <summary>
        /// Asynchronously automatically create sensors under an object based on the object's (or it's children's) device type.
        /// </summary>
        /// <param name="objectId">The object to run Auto-Discovery for (such as a device or group).</param>
        public async void AutoDiscoverAsync(int objectId) => await requestEngine.ExecuteRequestAsync(CommandFunction.DiscoverNow, new BaseActionParameters(objectId)).ConfigureAwait(false);

        /// <summary>
        /// Move the position of an object up or down under its parent within the PRTG User Interface.
        /// </summary>
        /// <param name="objectId">The object to reposition.</param>
        /// <param name="position">The direction to move in.</param>
        public void SetPosition(int objectId, Position position) => requestEngine.ExecuteRequest(CommandFunction.SetPosition, new SetPositionParameters(objectId, position));

        /// <summary>
        /// Set the absolute position of an object under its parent within the PRTG User Interface
        /// </summary>
        /// <param name="obj">The object to reposition.</param>
        /// <param name="position">The position to move the object to. If this value is higher than the total number of objects under the parent node, the object will be moved to the last possible position.</param>
        public void SetPosition(SensorOrDeviceOrGroupOrProbe obj, int position) => requestEngine.ExecuteRequest(CommandFunction.SetPosition, new SetPositionParameters(obj, position));

        /// <summary>
        /// Move a device or group (excluding the root group) to another group or probe within PRTG.
        /// </summary>
        /// <param name="objectId">The ID of a device or group to move.</param>
        /// <param name="destinationId">The group or probe to move the object to.</param>
        public void MoveObject(int objectId, int destinationId)
        {
            var parameters = new Parameters.Parameters
            {
                [Parameter.Id] = objectId,
                [Parameter.TargetId] = destinationId
            };

            requestEngine.ExecuteRequest(CommandFunction.MoveObjectNow, parameters);
        }

        /// <summary>
        /// Sort the children of a device, group or probe alphabetically.
        /// </summary>
        /// <param name="objectId">The object to sort.</param>
        public void SortAlphabetically(int objectId) => requestEngine.ExecuteRequest(CommandFunction.SortSubObjects, new BaseActionParameters(objectId));

        /// <summary>
        /// Asynchronously sort the children of a device, group or probe alphabetically.
        /// </summary>
        /// <param name="objectId">The object to sort.</param>
        public async void SortAlphabeticallyAsync(int objectId) => await requestEngine.ExecuteRequestAsync(CommandFunction.SortSubObjects, new BaseActionParameters(objectId)).ConfigureAwait(false);

        /// <summary>
        /// Clone a sensor or group to another device or group respectively.
        /// </summary>
        /// <param name="sourceObjectId">The ID of a sensor or group to clone.</param>
        /// <param name="cloneName">The name that should be given to the cloned object.</param>
        /// <param name="targetLocationObjectId">If this is a sensor, the ID of the device to clone to. If this is a group, the ID of the group to clone to.</param>
        /// <returns>The ID of the object that was created</returns>
        public int CloneObject(int sourceObjectId, string cloneName, int targetLocationObjectId)
        {
            if (cloneName == null)
                throw new ArgumentNullException(nameof(cloneName));

            var parameters = new Parameters.Parameters()
            {
                [Parameter.Id] = sourceObjectId,
                [Parameter.Name] = cloneName,
                [Parameter.TargetId] = targetLocationObjectId
            };

            //todo: need to implement simulateerrorparameters or get rid of it?

            var response = HttpUtility.UrlDecode(requestEngine.ExecuteRequest(CommandFunction.DuplicateObject, parameters, r => r.RequestMessage.RequestUri.ToString()));

            var id = Convert.ToInt32(Regex.Replace(response, "(.+id=)(\\d+)(&.+)", "$2"));

            return id;

            //todo: apparently the server replies with the url of the new page, which we could parse into an object containing the id of the new object and return from this method

            //get-sensor|copy-object -target $devices
        }

        /// <summary>
        /// Clone a device to another group.
        /// </summary>
        /// <param name="deviceId">The ID of the device to clone.</param>
        /// <param name="cloneName">The name that should be given to the cloned device.</param>
        /// <param name="host">The hostname or IP Address that should be assigned to the new device.</param>
        /// <param name="targetGroupId">The group or probe the device should be cloned to.</param>
        public int CloneObject(int deviceId, string cloneName, string host, int targetGroupId)
        {
            if (cloneName == null)
                throw new ArgumentNullException(nameof(cloneName));

            if (host == null)
                throw new ArgumentNullException(nameof(host));

            var parameters = new Parameters.Parameters()
            {
                [Parameter.Id] = deviceId,
                [Parameter.Name] = cloneName,
                [Parameter.Host] = host,
                [Parameter.TargetId] = targetGroupId
            };

            var response = HttpUtility.UrlDecode(requestEngine.ExecuteRequest(CommandFunction.DuplicateObject, parameters, r => r.RequestMessage.RequestUri.ToString()));

            var id = Convert.ToInt32(Regex.Replace(response, "(.+id=)(\\d+)(&.+)", "$2"));

            return id;

            //todo: apparently the server replies with the url of the new page, which we could parse into an object containing the id of the new object and return from this method
        }

        /// <summary>
        /// Permanently delete an object from PRTG. This cannot be undone.
        /// </summary>
        /// <param name="objectId">ID of the object to delete.</param>
        public void DeleteObject(int objectId) => requestEngine.ExecuteRequest(CommandFunction.DeleteObject, new DeleteParameters(objectId));

        /// <summary>
        /// Asynchronously permanently delete an object from PRTG. This cannot be undone.
        /// </summary>
        /// <param name="objectId">ID of the object to delete.</param>
        public async Task DeleteObjectAsync(int objectId) => await requestEngine.ExecuteRequestAsync(CommandFunction.DeleteObject, new DeleteParameters(objectId)).ConfigureAwait(false);

        /// <summary>
        /// Rename an object.
        /// </summary>
        /// <param name="objectId">ID of the object to rename.</param>
        /// <param name="name">New name to give the object.</param>
        public void RenameObject(int objectId, string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            var parameters = new Parameters.Parameters
            {
                [Parameter.Id] = objectId,
                [Parameter.Value] = name
            };

            requestEngine.ExecuteRequest(CommandFunction.Rename, parameters);
        }

        #endregion
    #endregion
#endregion

        #region Unsorted

        private void a()
        {
            //wmi volume
            //add: name=WMI+Free+Disk+Space+(Single+Disk)&parenttags_=C_OS_Win&tags_=wmivolumesensor+diskspacesensor&priority_=3&deviceidlist_=1&deviceidlist_=1&deviceidlist__check=%5C%5C%5C%5C%3F%5C%5CVolume%7B33bf348c-e1b7-11e3-80b5-806e6f6e6963%7D%5C%5C%7CC%3A%5C%5C%7Csy1-dc-01%7CLocal+Disk%7CNTFS%7CC%3A%7C%7C&deviceid=&drivetype=&wmialternative=0&driveletter_=&intervalgroup=0&intervalgroup=1&interval_=60%7C60+seconds&errorintervalsdown_=1&inherittriggers=1&id=2212&sensortype=wmivolume
            //enumerate: 
        }

        /// <summary>
        /// Calcualte the total number of objects of a given type present on a PRTG Server.
        /// </summary>
        /// <param name="content">The type of object to total.</param>
        /// <returns>The total number of objects of a given type.</returns>
        public int GetTotalObjects(Content content)
        {
            var parameters = new Parameters.Parameters()
            {
                [Parameter.Count] = 0,
                [Parameter.Content] = content
            };

            return Convert.ToInt32(GetObjectsRaw<PrtgObject>(parameters).TotalCount);
        }

        internal SensorSettings GetObjectProperties(int objectId)
        {
            var parameters = new Parameters.Parameters
            {
                [Parameter.Id] = objectId,
                [Parameter.ObjectType] = BaseType.Sensor
            };

            //we'll need to add support for dropdown lists too

            var response = requestEngine.ExecuteRequest(HtmlFunction.ObjectData, parameters);

            var blah = SensorSettings.GetXml(response, objectId);

            var doc = new XDocument(blah);

            var aaaa = Data<SensorSettings>.DeserializeType(doc);

            //maybe instead of having an enum for my schedule and scanninginterval we have a class with a special getter that removes the <num>|component when you try and retrieve the property
            //the thing is, the enum IS actually dynamic - we need a list of valid options

            //todo: whenever we use _raw attributes dont we need to add xmlignore on the one that accesses it/

            return aaaa;
        }

        //todo: move this
        private List<SensorHistory> GetSensorHistory(int sensorId)
        {
            Logger.DebugEnabled = false;

            //todo: add xml formatting
            //var awwww = typeof (SensorOrDeviceOrGroupOrProbe).GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            //var q = GetSensors(SensorStatus.Paused);

            //var filter = new SearchFilter(Property.Comments, SensorStatus.Paused);
            //filter.ToString();

            //myenum val = myenum.Val5;

            //var al= Enum.GetValues(typeof (myenum)).Cast<myenum>().Where(m => m != val && val.HasFlag(m)).ToList();

            //var enums = Enum.GetValues(typeof (myenum)).Cast<myenum>().ToList();


            //var result = myenum.Val5.GetUnderlyingFlags();

            //var result = outer(myenum.Val5, enums).ToList();

            //foreach (var a in al1)


            //loop over each element. if a value contains more than 1 element in it, its not real, so ignore it

            var parameters = new Parameters.Parameters
            {
                [Parameter.Columns] = new[] { Property.Datetime, Property.Value_, Property.Coverage },
                [Parameter.Id] = 2196,
                [Parameter.Content] = Content.Values
            };

            var items = GetObjects<SensorHistoryData>(parameters);

            foreach (var history in items)
            {
                foreach (var value in history.Values)
                {
                    value.DateTime = history.DateTime;
                    value.SensorId = sensorId;
                }
            }
            //todo: need to implement coverage column
            //todo: right now the count is just the default - 500. need to allow specifying bigger counts
            return items.SelectMany(i => i.Values).OrderByDescending(a => a.DateTime).Where(v => v.Value != null).ToList();
        }

        #region SetObjectProperty

        #region BasicObjectSetting

        /// <summary>
        /// Modify basic object settings (name, tags, priority, etc.) for a PRTG Object.
        /// </summary>
        /// <param name="objectId">ID of the object to modify.</param>
        /// <param name="name">The setting to whose value will be overwritten.</param>
        /// <param name="value">Value of the setting to apply.</param>
        public void SetObjectProperty(int objectId, BasicObjectSetting name, string value)
        {
            SetObjectProperty(new SetObjectSettingParameters<BasicObjectSetting>(objectId, name, value));
        }

        #endregion

        #region ScanningInterval

        /// <summary>
        /// Modify scanning interval settings for a PRTG Object.
        /// </summary>
        /// <param name="objectId">ID of the object to modify.</param>
        /// <param name="name">The setting to whose value will be overwritten.</param>
        /// <param name="value">Value of the setting to apply.</param>
        private void SetObjectProperty(int objectId, ScanningInterval name, object value)
        {
            SetObjectProperty(new SetObjectSettingParameters<ScanningInterval>(objectId, name, value));
        }

        #endregion

        #region SensorDisplay

        /// <summary>
        /// Modify sensor display settings for a PRTG Object.
        /// </summary>
        /// <param name="objectId">ID of the object to modify.</param>
        /// <param name="name">The setting to whose value will be overwritten.</param>
        /// <param name="value">Value of the setting to apply.</param>
        private void SetObjectProperty(int objectId, SensorDisplay name, object value)
        {
            SetObjectProperty(new SetObjectSettingParameters<SensorDisplay>(objectId, name, value));
        }

        #endregion

        #region ExeScriptSetting

        /// <summary>
        /// Modify EXE/Script settings for a PRTG Object.
        /// </summary>
        /// <param name="objectId">ID of the object to modify.</param>
        /// <param name="name">The setting to whose value will be overwritten.</param>
        /// <param name="value">Value of the setting to apply.</param>
        private void SetObjectProperty(int objectId, ExeScriptSetting name, object value)
        {
            SetObjectProperty(new SetObjectSettingParameters<ExeScriptSetting>(objectId, name, value));
        }

        #endregion

        #region Channel

        /// <summary>
        /// Modify channel properties for a PRTG Sensor.
        /// </summary>
        /// <param name="sensorId">The ID of the sensor whose channels should be modified.</param>
        /// <param name="channelId">The ID of the channel to modify.</param>
        /// <param name="property">The property of the channel to modify</param>
        /// <param name="value">The value to set the channel's property to.</param>
        public void SetObjectProperty(int sensorId, int channelId, ChannelProperty property, object value) => requestEngine.ExecuteRequest(HtmlFunction.EditSettings, new SetChannelSettingParameters(sensorId, channelId, property, value));

        /// <summary>
        /// Asynchronously modify channel properties for a PRTG Sensor.
        /// </summary>
        /// <param name="sensorId">The ID of the sensor whose channels should be modified.</param>
        /// <param name="channelId">The ID of the channel to modify.</param>
        /// <param name="property">The property of the channel to modify</param>
        /// <param name="value">The value to set the channel's property to.</param>
        public async void SetObjectPropertyAsync(int sensorId, int channelId, ChannelProperty property, object value) => await requestEngine.ExecuteRequestAsync(HtmlFunction.EditSettings, new SetChannelSettingParameters(sensorId, channelId, property, value)).ConfigureAwait(false);

        //move this
        public void SetObjectProperty(int objectId, ObjectProperty property, object value)
        {
            var prop = typeof (SensorSettings).GetProperties().FirstOrDefault(p => p.GetCustomAttribute<PropertyParameterAttribute>()?.Name == property.ToString());

            if (prop == null)
                throw new MissingAttributeException(typeof (SensorSettings), property.ToString(), typeof (PropertyParameterAttribute));

            var propertyType = prop.PropertyType;
            var valueType = value.GetType();

            object val = null;

            if (propertyType == typeof(bool))
            {
                if(valueType == typeof(bool))
                    val = ((bool)value) ? "1" : "0";
            }
            else if (propertyType.IsEnum)
            {
                if (propertyType == valueType)
                    val = ((Enum) value).GetEnumAttribute<XmlEnumAttribute>(true).Name;
                else
                {
                    if(Enum.GetNames(propertyType).Any(x => x.ToLower() == value.ToString().ToLower()))
                        val = ((Enum)Enum.Parse(propertyType, value.ToString(), true)).GetEnumAttribute<XmlEnumAttribute>(true).Name;
                }
            }
            else
                throw new InvalidTypeException(propertyType, valueType);

            if (val == null)
                throw new ArgumentException($"Value '{value}' could not be assigned to property '{prop.Name}'. Expected type: '{propertyType}'. Actual type: '{valueType}'.");

            //i dont care what the current type is, i just want to know if the type can be parsed to the type of the property type

            //maybe we'll create an instance of the property type and check we're assignable to it?


            //then we ultimately do need to serialize that property



            //i think the properties in sensorsetting need an enum that links them to an object property
            //and then we use reflection to get the param with a given property and then confirm
            //that the value we were given is convertable to the given type

            //we should then implement this type safety for setchannelproperty as well



            //we need to add handling for inherit error interval


            //GetEnumAttributepqp


            var parameters = new Parameters.Parameters
            {
                [Parameter.Custom] = ObjectSettings.CreateCustomParameter(property, val),
                [Parameter.Id] = objectId
            };

            requestEngine.ExecuteRequest(HtmlFunction.EditSettings, parameters);
        }

        #endregion

        private void SetObjectProperty<T>(SetObjectSettingParameters<T> parameters)
        {
            requestEngine.ExecuteRequest(CommandFunction.SetObjectProperty, parameters);
        }

        #endregion

        #region GetObjectProperty

        #region BasicObjectSetting

        /// <summary>
        /// Retrieves basic object settings (name, tags, priority, etc.) for a PRTG Object.
        /// </summary>
        /// <param name="objectId">ID of the object to retrieve settings for.</param>
        /// <param name="name">The setting to retrieve.</param>
        /// <returns>The value of the requested setting.</returns>
        public string GetObjectProperty(int objectId, BasicObjectSetting name)
        {
            return GetObjectProperty<string>(objectId, name);
        }

        /// <summary>
        /// Retrieves basic object settings (name, tags, priority, etc.) in their true data type for a PRTG Object.
        /// </summary>
        /// <typeparam name="T">The return type suggested by the documentation for the <see cref="BasicObjectSetting"/> specified in <paramref name="name"/>.</typeparam>
        /// <param name="objectId">ID of the object to retrieve settings for.</param>
        /// <param name="name">The setting to retrieve.</param>
        /// <returns>The value of the requested setting.</returns>
        public T GetObjectProperty<T>(int objectId, BasicObjectSetting name)
        {
            return GetObjectProperty<T, BasicObjectSetting>(new GetObjectSettingParameters<BasicObjectSetting>(objectId, name));
        }

        #endregion

        #region ScanningInterval

        /// <summary>
        /// Retrieves scanning interval related settings for a PRTG Object.
        /// </summary>
        /// <param name="objectId">ID of the object to retrieve settings for.</param>
        /// <param name="name">The setting to retrieve.</param>
        /// <returns>The value of the requested setting.</returns>
        private string GetObjectProperty(int objectId, ScanningInterval name)
        {
            return GetObjectProperty<string, ScanningInterval>(new GetObjectSettingParameters<ScanningInterval>(objectId, name));
        }

        /// <summary>
        /// Retrieves scanning interval related settings in their true data type for a PRTG Object.
        /// </summary>
        /// <typeparam name="T">The return type suggested by the documentation for the <see cref="ScanningInterval"/> specified in <paramref name="name"/>.</typeparam>
        /// <param name="objectId">ID of the object to retrieve settings for.</param>
        /// <param name="name">The setting to retrieve.</param>
        /// <returns>The value of the requested setting.</returns>
        private T GetObjectProperty<T>(int objectId, ScanningInterval name)
        {
            return GetObjectProperty<T, ScanningInterval>(new GetObjectSettingParameters<ScanningInterval>(objectId, name));
        }

        #endregion

        #region SensorDisplay

        /// <summary>
        /// Retrieves sensor display settings for a PRTG Object.
        /// </summary>
        /// <param name="objectId">ID of the object to retrieve settings for.</param>
        /// <param name="name">The setting to retrieve.</param>
        /// <returns>The value of the requested setting.</returns>
        private string GetObjectProperty(int objectId, SensorDisplay name)
        {
            return GetObjectProperty<string, SensorDisplay>(new GetObjectSettingParameters<SensorDisplay>(objectId, name));
        }

        /// <summary>
        /// Retrieves sensor display settings in their true data type for a PRTG Object.
        /// </summary>
        /// <typeparam name="T">The return type suggested by the documentation for the <see cref="SensorDisplay"/> specified in <paramref name="name"/>.</typeparam>
        /// <param name="objectId">ID of the object to retrieve settings for.</param>
        /// <param name="name">The setting to retrieve.</param>
        /// <returns>The value of the requested setting.</returns>
        private T GetObjectProperty<T>(int objectId, SensorDisplay name)
        {
            return GetObjectProperty<T, SensorDisplay>(new GetObjectSettingParameters<SensorDisplay>(objectId, name));
        }

        #endregion

        #region ExeScriptSetting

        /// <summary>
        /// Retrieves EXE/Script settings for a PRTG Object.
        /// </summary>
        /// <param name="objectId">ID of the object to retrieve settings for.</param>
        /// <param name="name">The setting to retrieve.</param>
        /// <returns>The value of the requested setting.</returns>
        private string GetObjectProperty(int objectId, ExeScriptSetting name)
        {
            return GetObjectProperty<string, ExeScriptSetting>(new GetObjectSettingParameters<ExeScriptSetting>(objectId, name));
        }

        /// <summary>
        /// Retrieves EXE/Script settings in their true data type for a PRTG Object.
        /// </summary>
        /// <typeparam name="T">The return type suggested by the documentation for the <see cref="ExeScriptSetting"/> specified in <paramref name="name"/>.</typeparam>
        /// <param name="objectId">ID of the object to retrieve settings for.</param>
        /// <param name="name">The setting to retrieve.</param>
        /// <returns>The value of the requested setting.</returns>
        private T GetObjectProperty<T>(int objectId, ExeScriptSetting name)
        {
            return GetObjectProperty<T, ExeScriptSetting>(new GetObjectSettingParameters<ExeScriptSetting>(objectId, name));
        }

        #endregion
        
        private TReturn GetObjectProperty<TReturn, TEnum>(GetObjectSettingParameters<TEnum> parameters)
        {
            var response = requestEngine.ExecuteRequest(XmlFunction.GetObjectProperty, parameters);

            var value = response.Descendants("result").First().Value;

            if (value == "(Property not found)")
                throw new PrtgRequestException("PRTG was unable to complete the request. A value for property '" + parameters.Name + "' could not be found.");

            if (typeof(TReturn).IsEnum)
            {
                return (TReturn)(object)Convert.ToInt32(value);
            }
            if (typeof (TReturn) == typeof (int))
            {
                return (TReturn)(object)Convert.ToInt32(value);
            }
            if (typeof (TReturn) == typeof (string))
            {
                return (TReturn) (object)value;
            }
            throw new UnknownTypeException(typeof(TReturn));
        }

        #endregion

        //todo: check all arguments we can in this file and make sure we validate input. when theres a chain of methods, validate on the inner most one except if we pass a parameter object, in which case validate both

        internal ServerStatus GetStatus()
        {
            var response = requestEngine.ExecuteRequest(XmlFunction.GetStatus, new BaseActionParameters(0));

            return Data<ServerStatus>.DeserializeType(response);
        }

        #endregion
    }
}
