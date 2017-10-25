using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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

        #region Get Objects

        internal List<T> GetObjects<T>(Parameters.Parameters parameters) => GetObjectsRaw<T>(parameters).Items;

        private Data<T> GetObjectsRaw<T>(Parameters.Parameters parameters)
        {
            var response = requestEngine.ExecuteRequest(XmlFunction.TableData, parameters);

            return Data<T>.DeserializeList(response);
        }

        private async Task<List<T>> GetObjectsAsync<T>(Parameters.Parameters parameters) => (await GetObjectsRawAsync<T>(parameters).ConfigureAwait(false)).Items;

        private async Task<Data<T>> GetObjectsRawAsync<T>(Parameters.Parameters parameters)
        {
            var response = await requestEngine.ExecuteRequestAsync(XmlFunction.TableData, parameters).ConfigureAwait(false);

            return Data<T>.DeserializeList(response);
        }

        private T GetObject<T>(XmlFunction function, Parameters.Parameters parameters, Action<string> responseValidator = null)
        {
            var response = requestEngine.ExecuteRequest(function, new Parameters.Parameters(), responseValidator);

            return Data<T>.DeserializeType(response);
        }

        private async Task<T> GetObjectAsync<T>(XmlFunction function, Parameters.Parameters parameters)
        {
            var response = await requestEngine.ExecuteRequestAsync(function, new Parameters.Parameters()).ConfigureAwait(false);

            return Data<T>.DeserializeType(response);
        }

        private T GetObject<T>(JsonFunction function, Parameters.Parameters parameters, Func<HttpResponseMessage, string> responseParser = null)
        {
            var response = requestEngine.ExecuteRequest(function, parameters, responseParser);

            var data = JsonDeserializer<T>.DeserializeType(response);

            return data;
        }

        private async Task<T> GetObjectAsync<T>(JsonFunction function, Parameters.Parameters parameters, Func<HttpResponseMessage, Task<string>> responseParser = null)
        {
            var response = await requestEngine.ExecuteRequestAsync(function, parameters, responseParser).ConfigureAwait(false);

            var data = JsonDeserializer<T>.DeserializeType(response);

            return data;
        }

        #endregion

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

        internal IEnumerable<T> SerialStreamObjects<T>(ContentParameters<T> parameters)
        {
            var totalObjects = GetTotalObjects(parameters.Content);

            parameters.Count = 500;

            for (int i = 0; i < totalObjects;)
            {
                var response = GetObjects<T>(parameters);

                foreach (var obj in response)
                    yield return obj;

                i = i + parameters.Count;
                parameters.Page++;

                if (totalObjects - i < parameters.Count)
                    parameters.Count = totalObjects - i;
            }
        }

        /// <summary>
        /// Apply a modification function to each element of a response.
        /// </summary>
        /// <typeparam name="T">The type of objects returned by the response.</typeparam>
        /// <param name="objects">The collection of objects to amend.</param>
        /// <param name="action">A modification function to apply to each element of the collection.</param>
        /// <returns>A collection of modified objects.</returns>
        internal List<T> Amend<T>(List<T> objects, Action<T> action)
        {
            foreach (var obj in objects)
            {
                action(obj);
            }

            return objects;
        }

        /// <summary>
        /// Apply a modification function to the properties of a nobject.
        /// </summary>
        /// <typeparam name="T">The type of object returned by the response.</typeparam>
        /// <param name="obj">The object to amend.</param>
        /// <param name="action">A modification function to apply to the object.</param>
        /// <returns></returns>
        internal T Amend<T>(T obj, Action<T> action)
        {
            action(obj);

            return obj;
        }

        /// <summary>
        /// Apply a modification action to a response, transforming the response to another type.
        /// </summary>
        /// <typeparam name="TSource">The type of object to transform.</typeparam>
        /// <typeparam name="TRet">The type of object to return.</typeparam>
        /// <param name="obj">The object to transform.</param>
        /// <param name="action">A modification function that transforms the response from one type to another.</param>
        /// <returns></returns>
        internal TRet Amend<TSource, TRet>(TSource obj, Func<TSource, TRet> action)
        {
            var val = action(obj);

            return val;
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
        /// <param name="statuses">A list of sensor statuses to filter for.</param>
        /// <returns></returns>
        public List<Sensor> GetSensors(params Status[] statuses) => GetSensors(new SensorParameters { StatusFilter = statuses });

        /// <summary>
        /// Asynchronously retrieve sensors from a PRTG Server of one or more statuses.
        /// </summary>
        /// <param name="statuses">A list of sensor statuses to filter for.</param>
        /// <returns></returns>
        public async Task<List<Sensor>> GetSensorsAsync(params Status[] statuses) => await GetSensorsAsync(new SensorParameters { StatusFilter = statuses }).ConfigureAwait(false);

        /// <summary>
        /// Stream sensors from a PRTG Server of one or more statuses. When this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <param name="statuses">A list of sensor statuses to filter for.</param>
        /// <returns></returns>
        public IEnumerable<Sensor> StreamSensors(params Status[] statuses) => StreamSensors(new SensorParameters { StatusFilter = statuses });

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
        /// <returns>The total number of sensors of each <see cref="Status"/> type.</returns>
        public SensorTotals GetSensorTotals() =>
            GetObject<SensorTotals>(XmlFunction.GetTreeNodeStats, new Parameters.Parameters());

        /// <summary>
        /// Asynchronously retrieve the number of sensors of each sensor type in the system.
        /// </summary>
        /// <returns>The total number of sensors of each <see cref="Status"/> type.</returns>
        public async Task<SensorTotals> GetSensorTotalsAsync() =>
            await GetObjectAsync<SensorTotals>(XmlFunction.GetTreeNodeStats, new Parameters.Parameters());

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
        public List<Channel> GetChannels(int sensorId) => GetChannelsInternal(sensorId);

        /// <summary>
        /// Retrieve all channels of a sensor that match the specified name.
        /// </summary>
        /// <param name="sensorId">The ID of the sensor to retrieve channels for.</param>
        /// <param name="channelName">The name of the channel to retrieve.</param>
        /// <returns></returns>
        public List<Channel> GetChannels(int sensorId, string channelName) => GetChannelsInternal(sensorId, name => name == channelName);

        /// <summary>
        /// Asynchronously retrieve all channels of a sensor.
        /// </summary>
        /// <param name="sensorId">The ID of the sensor to retrieve channels for.</param>
        /// <returns></returns>
        public async Task<List<Channel>> GetChannelsAsync(int sensorId) => await GetChannelsInternalAsync(sensorId).ConfigureAwait(false);

        /// <summary>
        /// Asynchronously retrieve all channels of a sensor that match the specified name.
        /// </summary>
        /// <param name="sensorId">The ID of the sensor to retrieve channels for.</param>
        /// <param name="channelName">The name of the channel to retrieve.</param>
        /// <returns></returns>
        public async Task<List<Channel>> GetChannelsAsync(int sensorId, string channelName) => await GetChannelsInternalAsync(sensorId, name => name == channelName).ConfigureAwait(false);

        internal XElement GetChannelProperties(int sensorId, int channelId)
        {
            var parameters = new ChannelPropertiesParameters(sensorId, channelId);

            var response = requestEngine.ExecuteRequest(HtmlFunction.ChannelEdit, parameters);

            return ChannelSettings.GetChannelXml(response, channelId);
        }

        internal async Task<XElement> GetChannelPropertiesAsync(int sensorId, int channelId)
        {
            var parameters = new ChannelPropertiesParameters(sensorId, channelId);

            var response = await requestEngine.ExecuteRequestAsync(HtmlFunction.ChannelEdit, parameters);

            return ChannelSettings.GetChannelXml(response, channelId);
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
            var xmlResponse = requestEngine.ExecuteRequest(XmlFunction.TableData, new NotificationTriggerParameters(objectId));

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
            var xmlResponse = await requestEngine.ExecuteRequestAsync(XmlFunction.TableData, new NotificationTriggerParameters(objectId)).ConfigureAwait(false);

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

        /// <summary>
        /// Retrieve all notification trigger types supported by a PRTG Object.
        /// </summary>
        /// <param name="objectId">The object to retrieve supported trigger types for.</param>
        /// <returns>The trigger types supported by the object.</returns>
        public List<TriggerType> GetNotificationTriggerTypes(int objectId) =>
            GetNotificationTriggerData(objectId).SupportedTypes.ToList();

        /// <summary>
        /// Asynchronously retrieve all notification trigger types supported by a PRTG Object.
        /// </summary>
        /// <param name="objectId">The object to retrieve supported trigger types for.</param>
        /// <returns>The trigger types supported by the object.</returns>
        public async Task<List<TriggerType>> GetNotificationTriggerTypesAsync(int objectId) =>
            (await GetNotificationTriggerDataAsync(objectId)).SupportedTypes.ToList();

        private NotificationTriggerData GetNotificationTriggerData(int objectId) =>
            GetObject<NotificationTriggerData>(
                JsonFunction.Triggers,
                new BaseActionParameters(objectId),
                ParseNotificationTriggerTypes
            );

        private async Task<NotificationTriggerData> GetNotificationTriggerDataAsync(int objectId) =>
            await GetObjectAsync<NotificationTriggerData>(
                JsonFunction.Triggers,
                new BaseActionParameters(objectId),
                ParseNotificationTriggerTypesAsync
            ).ConfigureAwait(false);

        #endregion

        #endregion
        #region Object Manipulation
        #region Sensor State

        /// <summary>
        /// Mark a <see cref="Status.Down"/> sensor as <see cref="Status.DownAcknowledged"/>. If an acknowledged sensor returns to <see cref="Status.Up"/>, it will not be acknowledged when it goes down again.
        /// </summary>
        /// <param name="objectId">ID of the sensor to acknowledge.</param>
        /// <param name="duration">Duration (in minutes) to acknowledge the object for. If null, sensor will be paused indefinitely.</param>
        /// <param name="message">Message to display on the acknowledged sensor.</param>
        public void AcknowledgeSensor(int objectId, int? duration = null, string message = null) =>
            requestEngine.ExecuteRequest(CommandFunction.AcknowledgeAlarm, new AcknowledgeSensorParameters(objectId, duration, message));

        /// <summary>
        /// Asynchronously mark a <see cref="Status.Down"/> sensor as <see cref="Status.DownAcknowledged"/>. If an acknowledged sensor returns to <see cref="Status.Up"/>, it will not be acknowledged when it goes down again.
        /// </summary>
        /// <param name="objectId">ID of the sensor to acknowledge.</param>
        /// <param name="duration">Duration (in minutes) to acknowledge the object for. If null, sensor will be paused indefinitely.</param>
        /// <param name="message">Message to display on the acknowledged sensor.</param>
        public async Task AcknowledgeSensorAsync(int objectId, int? duration = null, string message = null) =>
            await requestEngine.ExecuteRequestAsync(CommandFunction.AcknowledgeAlarm, new AcknowledgeSensorParameters(objectId, duration, message)).ConfigureAwait(false);

        /// <summary>
        /// Pause a PRTG Object.
        /// </summary>
        /// <param name="objectId">ID of the object to pause.</param>
        /// <param name="durationMinutes">Duration (in minutes) to pause the object for. If null, object will be paused indefinitely.</param>
        /// <param name="pauseMessage">Message to display on the paused object.</param>
        public void PauseObject(int objectId, int? durationMinutes = null, string pauseMessage = null)
        {
            var parameters = new PauseRequestParameters(objectId, durationMinutes, pauseMessage);

            requestEngine.ExecuteRequest(parameters.Function, parameters.Parameters);
        }

        /// <summary>
        /// Asynchronously pause a PRTG Object.
        /// </summary>
        /// <param name="objectId">ID of the object to pause.</param>
        /// <param name="durationMinutes">Duration (in minutes) to pause the object for. If null, object will be paused indefinitely.</param>
        /// <param name="pauseMessage">Message to display on the paused object.</param>
        public async Task PauseObjectAsync(int objectId, int? durationMinutes = null, string pauseMessage = null)
        {
            var parameters = new PauseRequestParameters(objectId, durationMinutes, pauseMessage);

            await requestEngine.ExecuteRequestAsync(parameters.Function, parameters.Parameters).ConfigureAwait(false);
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
        public async Task ResumeObjectAsync(int objectId) => await requestEngine.ExecuteRequestAsync(CommandFunction.Pause, new PauseParameters(objectId, PauseAction.Resume)).ConfigureAwait(false);

        /// <summary>
        /// Simulate an error state for a sensor.
        /// </summary>
        /// <param name="sensorId">ID of the sensor to simulate an error for.</param>
        public void SimulateError(int sensorId) => requestEngine.ExecuteRequest(CommandFunction.Simulate, new SimulateErrorParameters(sensorId));

        /// <summary>
        /// Asynchronously simulate an error state for a sensor.
        /// </summary>
        /// <param name="sensorId">ID of the sensor to simulate an error for.</param>
        public async Task SimulateErrorAsync(int sensorId) => await requestEngine.ExecuteRequestAsync(CommandFunction.Simulate, new SimulateErrorParameters(sensorId)).ConfigureAwait(false);

        #endregion
        #region Notifications

        /// <summary>
        /// Add a notification trigger to a PRTG Server.
        /// </summary>
        /// <param name="parameters"></param>
        public void AddNotificationTrigger(TriggerParameters parameters) => SetNotificationTrigger(parameters);

        /// <summary>
        /// Asynchronously add a notification trigger to a PRTG Server.
        /// </summary>
        /// <param name="parameters"></param>
        public async Task AddNotificationTriggerAsync(TriggerParameters parameters) => await SetNotificationTriggerAsync(parameters).ConfigureAwait(false);

        /// <summary>
        /// Add or edit a notification trigger on a PRTG Server.
        /// </summary>
        /// <param name="parameters">A set of parameters describing the type of notification trigger and how to manipulate it.</param>
        public void SetNotificationTrigger(TriggerParameters parameters)
        {
            ValidateTriggerParameters(parameters);

            requestEngine.ExecuteRequest(HtmlFunction.EditSettings, parameters);
        }

        /// <summary>
        /// Asynchronously add or edit a notification trigger on a PRTG Server.
        /// </summary>
        /// <param name="parameters">A set of parameters describing the type of notification trigger and how to manipulate it.</param>
        public async Task SetNotificationTriggerAsync(TriggerParameters parameters)
        {
            ValidateTriggerParameters(parameters);

            await requestEngine.ExecuteRequestAsync(HtmlFunction.EditSettings, parameters).ConfigureAwait(false);
        }

        private TriggerChannel GetTriggerChannel(TriggerParameters parameters)
        {
            TriggerChannel channel = null;

            switch (parameters.Type)
            {
                case TriggerType.Speed:
                    channel = ((SpeedTriggerParameters) parameters).Channel;
                    break;
                case TriggerType.Volume:
                    channel = ((VolumeTriggerParameters) parameters).Channel;
                    break;
                case TriggerType.Threshold:
                    channel = ((ThresholdTriggerParameters) parameters).Channel;
                    break;
            }

            return channel;
        }

        /// <summary>
        /// Remove a notification trigger from an object.
        /// </summary>
        /// <param name="trigger">The notification trigger to remove.</param>
        public void RemoveNotificationTrigger(NotificationTrigger trigger) =>
            requestEngine.ExecuteRequest(HtmlFunction.RemoveSubObject, new RemoveTriggerParameters(trigger));

        /// <summary>
        /// Asynchronously remove a notification trigger from an object.
        /// </summary>
        /// <param name="trigger">The notification trigger to remove.</param>
        public async Task RemoveNotificationTriggerAsync(NotificationTrigger trigger) =>
            await requestEngine.ExecuteRequestAsync(HtmlFunction.RemoveSubObject, new RemoveTriggerParameters(trigger)).ConfigureAwait(false);

        #endregion
        #region Clone Object

        /// <summary>
        /// Clone a sensor or group to another device or group.
        /// </summary>
        /// <param name="sourceObjectId">The ID of a sensor or group to clone.</param>
        /// <param name="cloneName">The name that should be given to the cloned object.</param>
        /// <param name="targetLocationObjectId">If this is a sensor, the ID of the device to clone to. If this is a group, the ID of the group to clone to.</param>
        /// <returns>The ID of the object that was created</returns>
        public int CloneObject(int sourceObjectId, string cloneName, int targetLocationObjectId) =>
            CloneObject(new CloneSensorOrGroupParameters(sourceObjectId, cloneName, targetLocationObjectId));

        /// <summary>
        /// Clone a device to another group or probe.
        /// </summary>
        /// <param name="deviceId">The ID of the device to clone.</param>
        /// <param name="cloneName">The name that should be given to the cloned device.</param>
        /// <param name="host">The hostname or IP Address that should be assigned to the new device.</param>
        /// <param name="targetLocationObjectId">The group or probe the device should be cloned to.</param>
        public int CloneObject(int deviceId, string cloneName, string host, int targetLocationObjectId) =>
            CloneObject(new CloneDeviceParameters(deviceId, cloneName, targetLocationObjectId, host));

        private int CloneObject(CloneSensorOrGroupParameters parameters) =>
            Amend(requestEngine.ExecuteRequest(CommandFunction.DuplicateObject, parameters, CloneRequestParser), CloneResponseParser);

        /// <summary>
        /// Asynchronously clone a sensor or group to another device or group.
        /// </summary>
        /// <param name="sourceObjectId">The ID of a sensor or group to clone.</param>
        /// <param name="cloneName">The name that should be given to the cloned object.</param>
        /// <param name="targetLocationObjectId">If this is a sensor, the ID of the device to clone to. If this is a group, the ID of the group to clone to.</param>
        /// <returns>The ID of the object that was created</returns>
        public async Task<int> CloneObjectAsync(int sourceObjectId, string cloneName, int targetLocationObjectId) =>
            await CloneObjectAsync(new CloneSensorOrGroupParameters(sourceObjectId, cloneName, targetLocationObjectId)).ConfigureAwait(false);

        /// <summary>
        /// Asynchronously clone a device to another group or probe.
        /// </summary>
        /// <param name="deviceId">The ID of the device to clone.</param>
        /// <param name="cloneName">The name that should be given to the cloned device.</param>
        /// <param name="host">The hostname or IP Address that should be assigned to the new device.</param>
        /// <param name="targetLocationObjectId">The group or probe the device should be cloned to.</param>
        public async Task<int> CloneObjectAsync(int deviceId, string cloneName, string host, int targetLocationObjectId) =>
            await CloneObjectAsync(new CloneDeviceParameters(deviceId, cloneName, targetLocationObjectId, host)).ConfigureAwait(false);

        private async Task<int> CloneObjectAsync(CloneSensorOrGroupParameters parameters) =>
            Amend(
                await requestEngine.ExecuteRequestAsync(
                    CommandFunction.DuplicateObject,
                    parameters,
                    async r => await Task.FromResult(CloneRequestParser(r))
                ).ConfigureAwait(false), CloneResponseParser
            );

        private string CloneRequestParser(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.BadRequest)
                return null;
            
            var message = response.RequestMessage.RequestUri.ToString();

            if (message.Contains("the object is currently not valid"))
            {
                var searchText = "errorurl=";
                var expectedUrl = message.Substring(message.IndexOf(searchText, StringComparison.Ordinal) + searchText.Length);

                if (expectedUrl.EndsWith("%26"))
                    expectedUrl = expectedUrl.Substring(0, expectedUrl.LastIndexOf("%26"));
                else if (expectedUrl.EndsWith("&"))
                    expectedUrl = expectedUrl.Substring(0, expectedUrl.LastIndexOf("&"));

                searchText = "error.htm";

                var server = message.Substring(0, message.IndexOf(searchText));

                message = $"{server}public/login.htm?loginurl={expectedUrl}&errormsg=";
                response.RequestMessage.RequestUri = new Uri(message);
            }

            return message;
        }

        private int CloneResponseParser(string response)
        {
            var decodedResponse = HttpUtility.UrlDecode(response);

            var id = Convert.ToInt32(Regex.Replace(decodedResponse, "(.+id=)(\\d+)(&.*)?", "$2"));

            return id;
        }

        #endregion
        #region Get Object Properties

        /// <summary>
        /// Retrieve properties and settings of a PRTG Sensor.
        /// </summary>
        /// <param name="sensorId">ID of the sensor to retrieve settings for.</param>
        /// <returns>All settings of the specified sensor.</returns>
        public SensorSettings GetSensorProperties(int sensorId) =>
            GetObjectProperties<SensorSettings>(sensorId, BaseType.Sensor.ToString());

        /// <summary>
        /// Asynchronously retrieve properties and settings of a PRTG Sensor.
        /// </summary>
        /// <param name="sensorId">ID of the sensor to retrieve settings for.</param>
        /// <returns>All settings of the specified sensor.</returns>
        public async Task<SensorSettings> GetSensorPropertiesAsync(int sensorId) =>
            await GetObjectPropertiesAsync<SensorSettings>(sensorId, BaseType.Sensor.ToString()).ConfigureAwait(false);

        /// <summary>
        /// Retrieve properties and settings of a PRTG Device.
        /// </summary>
        /// <param name="deviceId">ID of the device to retrieve settings for.</param>
        /// <returns>All settings of the specified device.</returns>
        public DeviceSettings GetDeviceProperties(int deviceId) =>
            GetObjectProperties<DeviceSettings>(deviceId, BaseType.Device.ToString());

        /// <summary>
        /// Asynchronously retrieve properties and settings of a PRTG Device.
        /// </summary>
        /// <param name="deviceId">ID of the device to retrieve settings for.</param>
        /// <returns>All settings of the specified device.</returns>
        public async Task<DeviceSettings> GetDevicePropertiesAsync(int deviceId) =>
            await GetObjectPropertiesAsync<DeviceSettings>(deviceId, BaseType.Device.ToString()).ConfigureAwait(false);

        /// <summary>
        /// Retrieve properties and settings of a PRTG Group.
        /// </summary>
        /// <param name="groupId">ID of the group to retrieve settings for.</param>
        /// <returns>All settings of the specified group.</returns>
        public GroupSettings GetGroupProperties(int groupId) =>
            GetObjectProperties<GroupSettings>(groupId, BaseType.Group.ToString());

        /// <summary>
        /// Asynchronously retrieve properties and settings of a PRTG Group.
        /// </summary>
        /// <param name="groupId">ID of the group to retrieve settings for.</param>
        /// <returns>All settings of the specified group.</returns>
        public async Task<GroupSettings> GetGroupPropertiesAsync(int groupId) =>
            await GetObjectPropertiesAsync<GroupSettings>(groupId, BaseType.Group.ToString()).ConfigureAwait(false);

        /// <summary>
        /// Retrieve properties and settings of a PRTG Probe.
        /// </summary>
        /// <param name="probeId">ID of the probe to retrieve settings for.</param>
        /// <returns>All settings of the specified probe.</returns>
        public ProbeSettings GetProbeProperties(int probeId) =>
            GetObjectProperties<ProbeSettings>(probeId, Content.ProbeNode.ToString());

        /// <summary>
        /// Asynchronously retrieve properties and settings of a PRTG Probe.
        /// </summary>
        /// <param name="probeId">ID of the probe to retrieve settings for.</param>
        /// <returns>All settings of the specified probe.</returns>
        public async Task<ProbeSettings> GetProbePropertiesAsync(int probeId) =>
            await GetObjectPropertiesAsync<ProbeSettings>(probeId, Content.ProbeNode.ToString()).ConfigureAwait(false);

        /// <summary>
        /// Retrieve unsupported properties and settings of a PRTG Object.
        /// </summary>
        /// <param name="objectId">The ID of the object whose property should be retrieved.</param>
        /// <param name="property">The property of the object to retrieve. This can be typically discovered by inspecting the "name" attribute of the properties' &lt;input/&gt; tag on the Settings page of PRTG.<para/>
        /// If the properties name ends in an underscore, this must be included.</param>
        /// <returns>The raw value of the object's property.</returns>
        public string GetObjectPropertyRaw(int objectId, string property)
        {
            var parameters = new GetObjectPropertyRawParameters(objectId, property);

            var response = requestEngine.ExecuteRequest(XmlFunction.GetObjectProperty, parameters);

            return ValidateRawObjectProperty(response, parameters);
        }

        /// <summary>
        /// Asynchronously retrieve unsupported properties and settings of a PRTG Object.
        /// </summary>
        /// <param name="objectId">The ID of the object whose property should be retrieved.</param>
        /// <param name="property">The property of the object to retrieve. This can be typically discovered by inspecting the "name" attribute of the properties' &lt;input/&gt; tag on the Settings page of PRTG.<para/>
        /// If the properties name ends in an underscore, this must be included.</param>
        /// <returns>The raw value of the object's property.</returns>
        public async Task<string> GetObjectPropertyRawAsync(int objectId, string property)
        {
            var parameters = new GetObjectPropertyRawParameters(objectId, property);

            var response = await requestEngine.ExecuteRequestAsync(XmlFunction.GetObjectProperty, parameters).ConfigureAwait(false);

            return ValidateRawObjectProperty(response, parameters);
        }

        private string ValidateRawObjectProperty(XDocument response, GetObjectPropertyRawParameters parameters)
        {
            var value = response.Descendants("result").First().Value;

            if (value == "(Property not found)")
                throw new PrtgRequestException($"PRTG was unable to complete the request. A value for property '{parameters.Name}' could not be found.");

            return value;
        }

        private T GetObjectProperties<T>(int objectId, string objectType)
        {
            var response = requestEngine.ExecuteRequest(HtmlFunction.ObjectData, new GetObjectPropertyParameters(objectId, objectType));

            return GetObjectProperties<T>(response);
        }

        private async Task<T> GetObjectPropertiesAsync<T>(int objectId, string objectType)
        {
            var response = await requestEngine.ExecuteRequestAsync(HtmlFunction.ObjectData, new GetObjectPropertyParameters(objectId, objectType)).ConfigureAwait(false);

            return GetObjectProperties<T>(response);
        }

        private T GetObjectProperties<T>(string response)
        {
            var xml = ObjectSettings.GetXml(response);
            var xDoc = new XDocument(xml);

            var items = Data<T>.DeserializeType(xDoc);

            return items;
        }

        #endregion
        #region Set Object Properties

            #region Normal

        /// <summary>
        /// Modify properties and settings of a PRTG Object.<para/>
        /// Each <see cref="ObjectProperty"/> corresponds with a Property of a type derived from <see cref="ObjectSettings"/>.<para/>
        /// If PrtgAPI cannot convert the specified value to the type required by the property, PrtgAPI will throw an exception indicating the type that was expected.
        /// </summary>
        /// <param name="objectId">The ID of the object whose properties should be modified.</param>
        /// <param name="property">The property of the object to modify.</param>
        /// <param name="value">The value to set the object's property to.</param>
        public void SetObjectProperty(int objectId, ObjectProperty property, object value) =>
            SetObjectProperty(CreateSetObjectPropertyParameters(objectId, property, value));

        /// <summary>
        /// Asynchronously modify properties and settings of a PRTG Object.<para/>
        /// Each <see cref="ObjectProperty"/> corresponds with a Property of a type derived from <see cref="ObjectSettings"/>.<para/>
        /// If PrtgAPI cannot convert the specified value to the type required by the property, PrtgAPI will throw an exception indicating the type that was expected.
        /// </summary>
        /// <param name="objectId">The ID of the object whose properties should be modified.</param>
        /// <param name="property">The property of the object to modify.</param>
        /// <param name="value">The value to set the object's property to.</param>
        public async Task SetObjectPropertyAsync(int objectId, ObjectProperty property, object value) =>
            await SetObjectPropertyAsync(await CreateSetObjectPropertyParametersAsync(objectId, property, value).ConfigureAwait(false)).ConfigureAwait(false);

            #endregion Normal
            #region Channel

        /// <summary>
        /// Modify channel properties for a PRTG Sensor.
        /// </summary>
        /// <param name="sensorId">The ID of the sensor whose channels should be modified.</param>
        /// <param name="channelId">The ID of the channel to modify.</param>
        /// <param name="property">The property of the channel to modify</param>
        /// <param name="value">The value to set the channel's property to.</param>
        public void SetObjectProperty(int sensorId, int channelId, ChannelProperty property, object value) =>
            SetObjectProperty(new SetChannelPropertyParameters(sensorId, channelId, property, value));

        /// <summary>
        /// Asynchronously modify channel properties for a PRTG Sensor.
        /// </summary>
        /// <param name="sensorId">The ID of the sensor whose channels should be modified.</param>
        /// <param name="channelId">The ID of the channel to modify.</param>
        /// <param name="property">The property of the channel to modify</param>
        /// <param name="value">The value to set the channel's property to.</param>
        public async Task SetObjectPropertyAsync(int sensorId, int channelId, ChannelProperty property, object value) =>
            await SetObjectPropertyAsync(new SetChannelPropertyParameters(sensorId, channelId, property, value)).ConfigureAwait(false);

            #endregion Channel
            #region Custom

        /// <summary>
        /// Modify unsupported properties and settings of a PRTG Object.
        /// </summary>
        /// <param name="objectId">The ID of the object whose properties should be modified.</param>
        /// <param name="property">The property of the object to modify. This can be typically discovered by inspecting the "name" attribute of the properties' &lt;input/&gt; tag on the Settings page of PRTG.<para/>
        /// If the properties name ends in an underscore, this must be included.</param>
        /// <param name="value">The value to set the object's property to. For radio buttons and dropdown lists, this is the integer found in the 'value' attribute.</param>
        public void SetObjectPropertyRaw(int objectId, string property, string value) =>
            SetObjectProperty(new SetObjectPropertyParameters(objectId, property, value));

        /// <summary>
        /// Asynchronously modify unsupported properties and settings of a PRTG Object.
        /// </summary>
        /// <param name="objectId">The ID of the object whose properties should be modified.</param>
        /// <param name="property">The property of the object to modify. This can be typically discovered by inspecting the "name" attribute of the properties' &lt;input/&gt; tag on the Settings page of PRTG.<para/>
        /// If the properties name ends in an underscore, this must be included.</param>
        /// <param name="value">The value to set the object's property to. For radio buttons and dropdown lists, this is the integer found in the 'value' attribute.</param>
        public async Task SetObjectPropertyRawAsync(int objectId, string property, string value) =>
            await SetObjectPropertyAsync(new SetObjectPropertyParameters(objectId, property, value)).ConfigureAwait(false);

            #endregion

        private void SetObjectProperty<T>(BaseSetObjectPropertyParameters<T> parameters) =>
            requestEngine.ExecuteRequest(HtmlFunction.EditSettings, parameters);

        private async Task SetObjectPropertyAsync<T>(BaseSetObjectPropertyParameters<T> parameters) =>
            await requestEngine.ExecuteRequestAsync(HtmlFunction.EditSettings, parameters).ConfigureAwait(false);

        private SetObjectPropertyParameters CreateSetObjectPropertyParameters(int objectId, ObjectProperty property, object value)
        {
            var attrib = property.GetEnumAttribute<TypeAttribute>();

            if (attrib != null)
            {
                try
                {
                    var method = attrib.Class.GetMethod("Resolve", BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Static);

                    if (method != null)
                    {
                        value = method.Invoke(null, new[] { this, objectId, value });
                    }
                }
                catch (TargetInvocationException ex)
                {
                    throw ex.InnerException;
                }
            }
            var parameters = new SetObjectPropertyParameters(objectId, property, value);

            return parameters;
        }

        private async Task<SetObjectPropertyParameters> CreateSetObjectPropertyParametersAsync(int objectId, ObjectProperty property, object value)
        {
            var attrib = property.GetEnumAttribute<TypeAttribute>();

            if (attrib != null)
            {
                try
                {
                    var method = attrib.Class.GetMethod("ResolveAsync", BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Static);

                    if (method != null)
                    {
                        var task = ((Task)method.Invoke(null, new[] { this, objectId, value }));

                        await task.ConfigureAwait(false);

                        value = task.GetType().GetProperty("Result").GetValue(task);
                    }
                }
                catch (TargetInvocationException ex)
                {
                    throw ex.InnerException;
                }
            }

            var parameters = new SetObjectPropertyParameters(objectId, property, value);

            return parameters;
        }

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
        public async Task RefreshObjectAsync(int objectId) => await requestEngine.ExecuteRequestAsync(CommandFunction.ScanNow, new BaseActionParameters(objectId)).ConfigureAwait(false);

        /// <summary>
        /// Automatically create sensors under an object based on the object's (or it's children's) device type.
        /// </summary>
        /// <param name="objectId">The object to run Auto-Discovery for (such as a device or group).</param>
        public void AutoDiscover(int objectId) => requestEngine.ExecuteRequest(CommandFunction.DiscoverNow, new BaseActionParameters(objectId));

        /// <summary>
        /// Asynchronously automatically create sensors under an object based on the object's (or it's children's) device type.
        /// </summary>
        /// <param name="objectId">The object to run Auto-Discovery for (such as a device or group).</param>
        public async Task AutoDiscoverAsync(int objectId) => await requestEngine.ExecuteRequestAsync(CommandFunction.DiscoverNow, new BaseActionParameters(objectId)).ConfigureAwait(false);

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
        /// Asynchronously move the position of an object up or down under its parent within the PRTG User Interface.
        /// </summary>
        /// <param name="objectId">The object to reposition.</param>
        /// <param name="position">The direction to move in.</param>
        public async Task SetPositionAsync(int objectId, Position position) => await requestEngine.ExecuteRequestAsync(CommandFunction.SetPosition, new SetPositionParameters(objectId, position)).ConfigureAwait(false);

        /// <summary>
        /// Asynchronously set the absolute position of an object under its parent within the PRTG User Interface
        /// </summary>
        /// <param name="obj">The object to reposition.</param>
        /// <param name="position">The position to move the object to. If this value is higher than the total number of objects under the parent node, the object will be moved to the last possible position.</param>
        public async Task SetPositionAsync(SensorOrDeviceOrGroupOrProbe obj, int position) => await requestEngine.ExecuteRequestAsync(CommandFunction.SetPosition, new SetPositionParameters(obj, position)).ConfigureAwait(false);

        /// <summary>
        /// Move a device or group (excluding the root group) to another group or probe within PRTG.
        /// </summary>
        /// <param name="objectId">The ID of a device or group to move.</param>
        /// <param name="destinationId">The group or probe to move the object to.</param>
        public void MoveObject(int objectId, int destinationId) => requestEngine.ExecuteRequest(CommandFunction.MoveObjectNow, new MoveObjectParameters(objectId, destinationId));

        /// <summary>
        /// Asynchronously Move a device or group (excluding the root group) to another group or probe within PRTG.
        /// </summary>
        /// <param name="objectId">The ID of a device or group to move.</param>
        /// <param name="destinationId">The group or probe to move the object to.</param>
        public async Task MoveObjectAsync(int objectId, int destinationId) => await requestEngine.ExecuteRequestAsync(CommandFunction.MoveObjectNow, new MoveObjectParameters(objectId, destinationId)).ConfigureAwait(false);

        /// <summary>
        /// Sort the children of a device, group or probe alphabetically.
        /// </summary>
        /// <param name="objectId">The object to sort.</param>
        public void SortAlphabetically(int objectId) => requestEngine.ExecuteRequest(CommandFunction.SortSubObjects, new BaseActionParameters(objectId));

        /// <summary>
        /// Asynchronously sort the children of a device, group or probe alphabetically.
        /// </summary>
        /// <param name="objectId">The object to sort.</param>
        public async Task SortAlphabeticallyAsync(int objectId) => await requestEngine.ExecuteRequestAsync(CommandFunction.SortSubObjects, new BaseActionParameters(objectId)).ConfigureAwait(false);

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
        /// Rename a Sensor, Device or Group within PRTG. Renaming probes is not supported. To rename a probe, use <see cref="SetObjectProperty(int, ObjectProperty, object)"/> 
        /// </summary>
        /// <param name="objectId">ID of the object to rename.</param>
        /// <param name="name">New name to give the object.</param>
        public void RenameObject(int objectId, string name) => requestEngine.ExecuteRequest(CommandFunction.Rename, new RenameParameters(objectId, name));

        /// <summary>
        /// Asynchronously rename a Sensor, Device or Group within PRTG. Renaming probes is not supported. To rename a probe, use <see cref="SetObjectProperty(int, ObjectProperty, object)"/> 
        /// </summary>
        /// <param name="objectId">ID of the object to rename.</param>
        /// <param name="name">New name to give the object.</param>
        public async Task RenameObjectAsync(int objectId, string name) => await requestEngine.ExecuteRequestAsync(CommandFunction.Rename, new RenameParameters(objectId, name)).ConfigureAwait(false);

        #endregion
    #endregion
#endregion

        #region Unsorted

        /// <summary>
        /// Calcualte the total number of objects of a given type present on a PRTG Server.
        /// </summary>
        /// <param name="content">The type of object to total.</param>
        /// <returns>The total number of objects of a given type.</returns>
        public int GetTotalObjects(Content content) => Convert.ToInt32(GetObjectsRaw<PrtgObject>(new TotalObjectsParameters(content)).TotalCount);

        /// <summary>
        /// Asynchronously calcualte the total number of objects of a given type present on a PRTG Server.
        /// </summary>
        /// <param name="content">The type of object to total.</param>
        /// <returns>The total number of objects of a given type.</returns>
        public async Task<int> GetTotalObjectsAsync(Content content) => Convert.ToInt32((await GetObjectsRawAsync<PrtgObject>(new TotalObjectsParameters(content)).ConfigureAwait(false)).TotalCount);

        /// <summary>
        /// Retrieve the setting/state modification history of a PRTG Object.
        /// </summary>
        /// <param name="objectId">The ID of the object to retrieve historical records for.</param>
        /// <returns>A list of all setting/state modifications to the specified object.</returns>
        public List<ModificationEvent> GetModificationHistory(int objectId) => Amend(GetObjects<ModificationEvent>(new ModificationHistoryParameters(objectId)), e => e.ObjectId = objectId);

        /// <summary>
        /// Asynchronously retrieve the setting/state modification history of a PRTG Object.
        /// </summary>
        /// <param name="objectId">The ID of the object to retrieve historical records for.</param>
        /// <returns>A list of all setting/state modifications to the specified object.</returns>
        public async Task<List<ModificationEvent>> GetModificationHistoryAsync(int objectId) => Amend(await GetObjectsAsync<ModificationEvent>(new ModificationHistoryParameters(objectId)).ConfigureAwait(false), e => e.ObjectId = objectId);

        /// <summary>
        /// Retrieve the historical values of a sensor's channels from within a specified time period.
        /// </summary>
        /// <param name="sensorId">The ID of the sensor to retrieve historical data for.</param>
        /// <param name="average">The time span (in seconds) to average results up to. For example, a value of 300 shows the average of results every 5 minutes.</param>
        /// <param name="startDate">The start date and time to retrieve data from.</param>
        /// <param name="endDate">The end date and time to retrieve data to.</param>
        /// <returns>Historical data for the specified sensor within the desired date range.</returns>
        public List<SensorHistoryData> GetSensorHistory(int sensorId, int average = 300, DateTime? startDate = null, DateTime? endDate = null)
        {
            var parameters = new SensorHistoryParameters(sensorId, average, startDate, endDate);

            var response = requestEngine.ExecuteRequest(XmlFunction.HistoricData, parameters, ValidateSensorHistoryResponse);

            return ParseSensorHistoryResponse(response, sensorId);
        }

        /// <summary>
        /// Asynchronously retrieve the historical values of a sensor's channels from within a specified time period.
        /// </summary>
        /// <param name="sensorId">The ID of the sensor to retrieve historical data for.</param>
        /// <param name="average">The time span (in seconds) to average results up to. For example, a value of 300 shows the average of results every 5 minutes.</param>
        /// <param name="startDate">The start date and time to retrieve data from.</param>
        /// <param name="endDate">The end date and time to retrieve data to.</param>
        public async Task<List<SensorHistoryData>> GetSensorHistoryAsync(int sensorId, int average = 300, DateTime? startDate = null, DateTime? endDate = null)
        {
            var parameters = new SensorHistoryParameters(sensorId, average, startDate, endDate);

            var response = await requestEngine.ExecuteRequestAsync(XmlFunction.HistoricData, parameters, ValidateSensorHistoryResponse).ConfigureAwait(false);

            return ParseSensorHistoryResponse(response, sensorId);
        }

        private void ValidateSensorHistoryResponse(string response)
        {
            if (response == "Not enough monitoring data")
                throw new PrtgRequestException($"PRTG was unable to complete the request. The server responded with the following error: {response}");
        }

        private List<SensorHistoryData> ParseSensorHistoryResponse(XDocument response, int sensorId)
        {
            var items = Data<SensorHistoryData>.DeserializeList(response).Items;

            var regex = new Regex("^(.+)(\\(.+\\))$");

            foreach (var history in items)
            {
                history.SensorId = sensorId;

                foreach (var value in history.ChannelRecords)
                {
                    value.Name = value.Name.Replace(" ", "");

                    if (regex.Match(value.Name).Success)
                        value.Name = regex.Replace(value.Name, "$1");

                    value.DateTime = history.DateTime;
                    value.SensorId = sensorId;
                }
            }

            return items;
        }

        //todo: check all arguments we can in this file and make sure we validate input. when theres a chain of methods, validate on the inner most one except if we pass a parameter object, in which case validate both

        internal ServerStatus GetStatus()
        {
            //todo: complete, and then make =>, and also implement an async version
            return GetObject<ServerStatus>(XmlFunction.GetStatus, new BaseActionParameters(0));
        }

        /// <summary>
        /// Resolve an address to its latitudinal and longitudinal coordinates. May spuriously return no results.
        /// </summary>
        /// <param name="address">The address to resolve.</param>
        /// <returns></returns>
        internal List<Location> ResolveAddress(string address) =>
            GetObject<GeoResult>(JsonFunction.GeoLocator, new ResolveAddressParameters(address)).Results.ToList();

        /// <summary>
        /// Asynchronously resolve an address to its latitudinal and longitudinal coordinates. May spuriously return no results.
        /// </summary>
        /// <param name="address">The address to resolve.</param>
        /// <returns></returns>
        internal async Task<List<Location>> ResolveAddressAsync(string address) =>
            (await GetObjectAsync<GeoResult>(JsonFunction.GeoLocator, new ResolveAddressParameters(address)).ConfigureAwait(false)).Results.ToList();

        #endregion
    }
}
