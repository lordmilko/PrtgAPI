using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using PrtgAPI.Attributes;
using PrtgAPI.Helpers;
using PrtgAPI.Objects.Deserialization;
using PrtgAPI.Objects.Shared;
using PrtgAPI.Objects.Undocumented;
using PrtgAPI.Parameters;
using PrtgAPI.Request;

namespace PrtgAPI
{
    /// <summary>
    /// <para type="description">Provides methods for generating API requests against PRTG Network Monitor.</para>
    /// </summary>
    public partial class PrtgClient
    {
        /// <summary>
        /// Stores server and authentication details required to connect to a PRTG Server.
        /// </summary>
        internal readonly ConnectionDetails ConnectionDetails;

        /// <summary>
        /// Provides access to methods used for executing web requests against a PRTG Server.
        /// </summary>
        private RequestEngine RequestEngine { get; }

        /// <summary>
        /// Provides access to methods used for requesting and deserializing objects from a PRTG Server.
        /// </summary>
        internal ObjectEngine ObjectEngine { get; }

        /// <summary>
        /// Provides methods for retrieving dynamic sensor targets used for creating and modifying sensors.
        /// </summary>
        public PrtgTargetHelper Targets { get; }

        /// <summary>
        /// Gets the PRTG server API requests will be made against.
        /// </summary>
        public string Server => ConnectionDetails.Server;

        /// <summary>
        /// Gets the username that will be used for authenticating API requests.
        /// </summary>
        public string UserName => ConnectionDetails.UserName;

        /// <summary>
        /// Gets the passhash that will be used for authenticating API requests, in place of a password.
        /// </summary>
        public string PassHash => ConnectionDetails.PassHash;

        /// <summary>
        /// Gets or sets the number of times to retry a request that times out while communicating with the server.
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        /// Gets or sets the base delay (in seconds) between retrying a timed out request. Each successive failure of a given request will wait an additional multiple of this value.
        /// </summary>
        public int RetryDelay { get; set; }

        internal EventHandler<RetryRequestEventArgs> retryRequest;

        /// <summary>
        /// Occurs when a request times out while communicating with the server.
        /// </summary>
        public event EventHandler<RetryRequestEventArgs> RetryRequest
        {
            add { retryRequest += value; }
            remove { retryRequest -= value; }
        }

        internal EventHandler<LogVerboseEventArgs> logVerbose;

        /// <summary>
        /// Occurs when a PrtgAPI logs verbose processing information.
        /// </summary>
        public event EventHandler<LogVerboseEventArgs> LogVerbose
        {
            add { logVerbose += value; }
            remove { logVerbose -= value; }
        }

        /// <summary>
        /// Specifies the version of the PRTG Server this client is connected to.
        /// <summary>
        /// Gets the version of PRTG Network Monitor this client is connected to.
        /// </summary>
        public Version Version => version ?? (version = GetStatus().Version);

        internal Version version;

        internal void Log(string message)
        {
            HandleEvent(logVerbose, new LogVerboseEventArgs(message));
        }

        internal void HandleEvent<T>(EventHandler<T> handler, T args)
        {
            handler?.Invoke(this, args);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrtgClient"/> class.
        /// </summary>
        /// <param name="server">The server to connect to. If a protocol is not specified, HTTPS will be used.</param>
        /// <param name="username">The username to authenticate with.</param>
        /// <param name="password">The password or passhash to authenticate with.</param>
        /// <param name="authMode">Whether the <paramref name="password"/> refers to a password or passhash. If a password is specified,
        /// this will automatically be resolved to a passhash.</param>
        public PrtgClient(string server, string username, string password, AuthMode authMode = AuthMode.Password)
            : this(server, username, password, authMode, new PrtgWebClient())
        {
        }

        internal PrtgClient(string server, string username, string password, AuthMode authMode, IWebClient client)
        {
            if (server == null)
                throw new ArgumentNullException(nameof(server));

            if (username == null)
                throw new ArgumentNullException(nameof(username));

            if (password == null)
                throw new ArgumentNullException(nameof(password));

            RequestEngine = new RequestEngine(this, client);

            ConnectionDetails = new ConnectionDetails(server, username, password);
            Targets = new PrtgTargetHelper(this);

            if (authMode == AuthMode.Password)
                ConnectionDetails.PassHash = GetPassHash(password);

            ObjectEngine = new ObjectEngine(this, RequestEngine);
        }

#region Requests

        internal VersionClient GetVersionClient(object[] obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var enums = obj.Where(o => o is Enum).ToList();

            if (enums.Count == 0)
                throw new NotImplementedException($"Don't know how to get {nameof(VersionAttribute)} for '{string.Join(",", obj)}'");

            var result = obj.OfType<Enum>().Select(o => o.GetEnumAttribute<VersionAttribute>()).Where(a => a != null).OrderBy(a => a.Version).ToList();

            var attr = result.FirstOrDefault();
            var ver = attr?.Version ?? RequestVersion.v14_4;

            if (attr != null && attr.IsActive(Version))
            {
                switch (ver)
                {
                    case RequestVersion.v18_1:
                        return new VersionClient18_1(this);

                    default:
                        return new VersionClient(ver, this);
                }
            }
            else
                return new VersionClient(ver, this);
        }

        [ExcludeFromCodeCoverage]
        internal VersionClient GetVersionClient<T1, T2>(List<T1> parameters) where T1 : PropertyParameter<T2>
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            return GetVersionClient(parameters.Select(p => p.Property).Cast<object>().ToArray());
        }

    #region Object Data

        private string GetPassHash(string password)
        {
            var response = RequestEngine.ExecuteRequest(new PassHashParameters(password));

            if(!Regex.Match(response, "^[0-9]+$").Success)
                throw new PrtgRequestException($"Could not retrieve PassHash from PRTG Server. PRTG responded '{response}'");

            return response;
        }

        #region Objects
            #region Single

        /// <summary>
        /// Retrieves an object of an unspecified type based on its object ID. If an object with the specified object ID
        /// does not exist or an ambiguous match is found, an <see cref="InvalidOperationException"/> exception is thrown.
        /// </summary>
        /// <param name="objectId">The ID of the object to retrieve.</param>
        /// <param name="resolve">Whether to resolve the resultant object to its most derived <see cref="PrtgObject"/> type. If the object type
        /// is not supported by PrtgAPI, the original <see cref="PrtgObject"/> is returned.</param>
        /// <exception cref="InvalidOperationException">The specified object does not exist or multiple objects were resolved with the specified ID.</exception> 
        /// <returns>If the object with the specified ID.</returns>
        public PrtgObject GetObject(int objectId, bool resolve = false) =>
            GetObjectInternal(objectId, resolve);

        /// <summary>
        /// Asynchronously retrieves an object of an unspecified type based on its object ID. If an object with the specified object ID
        /// does not exist or an ambiguous match is found, an <see cref="InvalidOperationException"/> exception is thrown.
        /// </summary>
        /// <param name="objectId">The ID of the object to retrieve.</param>
        /// <param name="resolve">Whether to resolve the resultant object to its most derived <see cref="PrtgObject"/> type. If the object type
        /// is not supported by PrtgAPI, the original <see cref="PrtgObject"/> is returned.</param>
        /// <exception cref="InvalidOperationException">The specified object does not exist or multiple objects were resolved with the specified ID.</exception> 
        /// <returns>If the object with the specified ID.</returns>
        public async Task<PrtgObject> GetObjectAsync(int objectId, bool resolve = false) =>
            await GetObjectInternalAsync(objectId, resolve).ConfigureAwait(false);

            #endregion
            #region Multiple

        /// <summary>
        /// Retrieves all uniquely identifiable objects from a PRTG Server.
        /// </summary>
        /// <returns>A list of all objects on a PRTG Server.</returns>
        public List<PrtgObject> GetObjects() => GetObjects(new PrtgObjectParameters());

        /// <summary>
        /// Asynchronously retrieves all uniquely identifiable objects from a PRTG Server.
        /// </summary>
        /// <returns>A list of all objects on a PRTG Server.</returns>
        public async Task<List<PrtgObject>> GetObjectsAsync() => await GetObjectsAsync(new PrtgObjectParameters()).ConfigureAwait(false);

        /// <summary>
        /// Streams all uniquely identifiable objects from a PRTG Server. If <paramref name="serial"/> is false, when this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <returns>If <paramref name="serial"/> is false, a generator encapsulating a series of <see cref="Task"/> objects capable of streaming a response from a PRTG Server. Otherwise, an enumeration that when iterated retrieves the specified objects.</returns>
        public IEnumerable<PrtgObject> StreamObjects(bool serial = false) => StreamObjects(new PrtgObjectParameters(), serial);

            #endregion
            #region Filter (Property, Value)

        /// <summary>
        /// Retrieves uniquely identifiable objects from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns>A list of objects that match the specified search criteria.</returns>
        public List<PrtgObject> GetObjects(Property property, object value) =>
            GetObjects(new SearchFilter(property, value));

        /// <summary>
        /// Asynchronously retrieves uniquely identifiable objects from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns>A list of objects that match the specified search criteria.</returns>
        public async Task<List<PrtgObject>> GetObjectsAsync(Property property, object value) =>
            await GetObjectsAsync(new SearchFilter(property, value)).ConfigureAwait(false);

        /// <summary>
        /// Streams uniquely identifiable objects from a PRTG Server based on the value of a certain property. When this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns>A generator encapsulating a series of <see cref="Task"/> objects capable of streaming a response from a PRTG Server.</returns>
        public IEnumerable<PrtgObject> StreamObjects(Property property, object value) =>
            StreamObjects(new SearchFilter(property, value));

            #endregion
            #region Filter (Property, Operator, Value)

        /// <summary>
        /// Retrieves uniquely identifiable objects from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="operator">Operator to compare value and property value with.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns>A list of objects that match the specified search criteria.</returns>
        public List<PrtgObject> GetObjects(Property property, FilterOperator @operator, object value) =>
            GetObjects(new SearchFilter(property, @operator, value));

        /// <summary>
        /// Asynchronously retrieves uniquely identifiable objects from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="operator">Operator to compare value and property value with.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns>A list of objects that match the specified search criteria.</returns>
        public async Task<List<PrtgObject>> GetObjectsAsync(Property property, FilterOperator @operator, object value) =>
            await GetObjectsAsync(new SearchFilter(property, @operator, value)).ConfigureAwait(false);

        /// <summary>
        /// Streams uniquely identifiable objects from a PRTG Server based on the value of a certain property. When this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="operator">Operator to compare value and property value with.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns>A generator encapsulating a series of <see cref="Task"/> objects capable of streaming a response from a PRTG Server.</returns>
        public IEnumerable<PrtgObject> StreamObjects(Property property, FilterOperator @operator, object value) =>
            StreamObjects(new SearchFilter(property, @operator, value));

            #endregion
            #region Filter (Array)

        /// <summary>
        /// Retrieves uniquely identifiable objects from a PRTG Server based on the values of multiple properties.
        /// </summary>
        /// <param name="filters">One or more filters used to limit search results.</param>
        /// <returns>A list of objects that match the specified search criteria.</returns>
        public List<PrtgObject> GetObjects(params SearchFilter[] filters) =>
            GetObjects(new PrtgObjectParameters(filters));

        /// <summary>
        /// Asynchronously retrieves uniquely identifiable objects from a PRTG Server based on the values of multiple properties.
        /// </summary>
        /// <param name="filters">One or more filters used to limit search results.</param>
        /// <returns>A list of objects that match the specified search criteria.</returns>
        public async Task<List<PrtgObject>> GetObjectsAsync(params SearchFilter[] filters) =>
            await GetObjectsAsync(new PrtgObjectParameters(filters)).ConfigureAwait(false);

        /// <summary>
        /// Streams uniquely identifiable objects from a PRTG Server based on the values of multiple properties. When this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <param name="filters">One or more filters used to limit search results.</param>
        /// <returns>A generator encapsulating a series of <see cref="Task"/> objects capable of streaming a response from a PRTG Server.</returns>
        public IEnumerable<PrtgObject> StreamObjects(params SearchFilter[] filters) =>
            StreamObjects(new PrtgObjectParameters(filters));

            #endregion
            #region Parameters

        /// <summary>
        /// Retrieves uniquely identifiable objects from a PRTG Server using a custom set of parameters.
        /// </summary>
        /// <param name="parameters">A custom set of parameters used to retrieve PRTG objects.</param>
        /// <returns>A list of objects that match the specified parameters.</returns>
        public List<PrtgObject> GetObjects(PrtgObjectParameters parameters) =>
            ObjectEngine.GetObjects<PrtgObject>(parameters).OrderBy(o => o.Id).ToList();

        /// <summary>
        /// Asynchronously retrieves uniquely identifiable objects from a PRTG Server using a custom set of parameters.
        /// </summary>
        /// <param name="parameters">A custom set of parameters used to retrieve PRTG objects.</param>
        /// <returns>A list of objects that match the specified parameters.</returns>
        public async Task<List<PrtgObject>> GetObjectsAsync(PrtgObjectParameters parameters) =>
            (await ObjectEngine.GetObjectsAsync<PrtgObject>(parameters).ConfigureAwait(false)).OrderBy(o => o.Id).ToList();

        /// <summary>
        /// Streams uniquely identifiable objects from a PRTG Server using a custom set of parameters. If <paramref name="serial"/> is false, when this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <param name="parameters">A custom set of parameters used to retrieve PRTG objects.</param>
        /// <param name="serial">Specifies whether PrtgAPI should execute all requests one at a time rather than all at once.</param>
        /// <returns>If <paramref name="serial"/> is false, a generator encapsulating a series of <see cref="Task"/> objects capable of streaming a response from a PRTG Server. Otherwise, an enumeration that when iterated retrieves the specified objects.</returns>
        public IEnumerable<PrtgObject> StreamObjects(PrtgObjectParameters parameters, bool serial = false) =>
            ObjectEngine.StreamObjects<PrtgObject, PrtgObjectParameters>(parameters, serial);

            #endregion
        #endregion
        #region Sensors
            #region Single

        /// <summary>
        /// Retrieves a sensor with a specified ID from a PRTG Server. If the sensor does not exist or an ambiguous match is found, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="id">The ID of the sensor to retrieve.</param>
        /// <exception cref="InvalidOperationException">The specified sensor does not exist or multiple sensors were resolved with the specified ID.</exception> 
        /// <returns>The sensor with the specified ID.</returns>
        public Sensor GetSensor(int id) => GetSensors(Property.Id, id).SingleObject(id);

        /// <summary>
        /// Asynchronously retrieves a sensor with a specified ID from a PRTG Server. If the sensor does not exist or an ambiguous match is found, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="id">The ID of the sensor to retrieve.</param>
        /// <exception cref="InvalidOperationException">The specified sensor does not exist or multiple sensors were resolved with the specified ID.</exception> 
        /// <returns>The sensor with the specified ID.</returns>
        public async Task<Sensor> GetSensorAsync(int id) => (await GetSensorsAsync(Property.Id, id).ConfigureAwait(false)).SingleObject(id);

            #endregion
            #region Multiple

        /// <summary>
        /// Retrieves all sensors from a PRTG Server.
        /// </summary>
        /// <returns>A list of all sensors on a PRTG Server.</returns>
        public List<Sensor> GetSensors() => GetSensors(new SensorParameters());

        /// <summary>
        /// Asynchronously retrieves all sensors from a PRTG Server.
        /// </summary>
        /// <returns>A task that returns a list of all sensors on a PRTG Server.</returns>
        public async Task<List<Sensor>> GetSensorsAsync() => await GetSensorsAsync(new SensorParameters()).ConfigureAwait(false);

        /// <summary>
        /// Streams all sensors from a PRTG Server. If <paramref name="serial"/> is false, when this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <param name="serial">Specifies whether PrtgAPI should execute all requests one at a time rather than all at once.</param>
        /// <returns>If <paramref name="serial"/> is false, a generator encapsulating a series of <see cref="Task"/> objects capable of streaming a response from a PRTG Server. Otherwise, an enumeration that when iterated retrieves the specified objects.</returns>
        public IEnumerable<Sensor> StreamSensors(bool serial = false) => StreamSensors(new SensorParameters(), serial);

            #endregion
            #region Sensor Status
   
        /// <summary>
        /// Retrieves sensors from a PRTG Server of one or more statuses.
        /// </summary>
        /// <param name="statuses">A list of sensor statuses to filter for.</param>
        /// <returns>A list of sensors that match the specified search criteria.</returns>
        public List<Sensor> GetSensors(params Status[] statuses) => GetSensors(new SensorParameters { Status = statuses });

        /// <summary>
        /// Asynchronously retrieves sensors from a PRTG Server of one or more statuses.
        /// </summary>
        /// <param name="statuses">A list of sensor statuses to filter for.</param>
        /// <returns>A list of sensors that match the specified search criteria.</returns>
        public async Task<List<Sensor>> GetSensorsAsync(params Status[] statuses) => await GetSensorsAsync(new SensorParameters { Status = statuses }).ConfigureAwait(false);

        /// <summary>
        /// Streams sensors from a PRTG Server of one or more statuses. When this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <param name="statuses">A list of sensor statuses to filter for.</param>
        /// <returns>A generator encapsulating a series of <see cref="Task"/> objects capable of streaming a response from a PRTG Server.</returns>
        public IEnumerable<Sensor> StreamSensors(params Status[] statuses) => StreamSensors(new SensorParameters { Status = statuses });

            #endregion
            #region Filter (Property, Value)

        /// <summary>
        /// Retrieves sensors from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns>A list of sensors that match the specified search criteria.</returns>
        public List<Sensor> GetSensors(Property property, object value) => GetSensors(new SearchFilter(property, value));

        /// <summary>
        /// Asynchronously retrieves sensors from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns>A list of sensors that match the specified search criteria.</returns>
        public async Task<List<Sensor>> GetSensorsAsync(Property property, object value) => await GetSensorsAsync(new SearchFilter(property, value)).ConfigureAwait(false);

        /// <summary>
        /// Streams sensors from a PRTG Server based on the value of a certain property. When this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns>A generator encapsulating a series of <see cref="Task"/> objects capable of streaming a response from a PRTG Server.</returns>
        public IEnumerable<Sensor> StreamSensors(Property property, object value) => StreamSensors(new SearchFilter(property, value));

            #endregion
            #region Filter (Property, Operator, Value)

        /// <summary>
        /// Retrieves sensors from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="operator">Operator to compare value and property value with.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns>A list of sensors that match the specified search criteria.</returns>
        public List<Sensor> GetSensors(Property property, FilterOperator @operator, object value) => GetSensors(new SearchFilter(property, @operator, value));

        /// <summary>
        /// Asynchronously retrieves sensors from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="operator">Operator to compare value and property value with.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns>A list of sensors that match the specified search criteria.</returns>
        public async Task<List<Sensor>> GetSensorsAsync(Property property, FilterOperator @operator, object value) => await GetSensorsAsync(new SearchFilter(property, @operator, value)).ConfigureAwait(false);

        /// <summary>
        /// Streams sensors from a PRTG Server based on the value of a certain property. When this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="operator">Operator to compare value and property value with.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns>A generator encapsulating a series of <see cref="Task"/> objects capable of streaming a response from a PRTG Server.</returns>
        public IEnumerable<Sensor> StreamSensors(Property property, FilterOperator @operator, object value) => StreamSensors(new SearchFilter(property, @operator, value));

            #endregion
            #region Filter (Array)

        /// <summary>
        /// Retrieves sensors from a PRTG Server based on the values of multiple properties.
        /// </summary>
        /// <param name="filters">One or more filters used to limit search results.</param>
        /// <returns>A list of sensors that match the specified search criteria.</returns>
        public List<Sensor> GetSensors(params SearchFilter[] filters) => GetSensors(new SensorParameters { SearchFilter = filters });

        /// <summary>
        /// Asynchronously retrieves sensors from a PRTG Server based on the values of multiple properties.
        /// </summary>
        /// <param name="filters">One or more filters used to limit search results.</param>
        /// <returns>A list of sensors that match the specified search criteria.</returns>
        public async Task<List<Sensor>> GetSensorsAsync(params SearchFilter[] filters) => await GetSensorsAsync(new SensorParameters { SearchFilter = filters }).ConfigureAwait(false);

        /// <summary>
        /// Streams sensors from a PRTG Server based on the values of multiple properties. When this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <param name="filters">One or more filters used to limit search results.</param>
        /// <returns>A generator encapsulating a series of <see cref="Task"/> objects capable of streaming a response from a PRTG Server.</returns>
        public IEnumerable<Sensor> StreamSensors(params SearchFilter[] filters) => StreamSensors(new SensorParameters { SearchFilter = filters });
            #endregion
            #region Query

        /// <summary>
        /// Retrieves sensors from a PRTG Server based on one or more <see cref="Queryable"/> expressions.
        /// </summary>
        /// <returns>An <see cref="IQueryable{Sensor}"/> to be wrapped by <see cref="Queryable"/> LINQ Expressions.</returns>
        public IQueryable<Sensor> QuerySensors() => QuerySensors(null);

        /// <summary>
        /// Retrieves sensors from a PRTG Server based on one or more <see cref="Queryable"/> expressions, specifying whether to use strict parsing semantics.
        /// </summary>
        /// <param name="strict">Whether to use strict evaluation. If true, a <see cref="NotSupportedException"/> will be thrown
        /// if an expression is encountered that cannot be evaluated server side.<para/>If <paramref name="strict"/> is false,
        /// the maximal supported expression will be executed server side, with any remaining expressions executed client side.</param>
        /// <exception cref="NotSupportedException">An expression was encountered that cannot be evaluated server side.</exception>
        /// <returns>An <see cref="IQueryable{Device}"/> to be wrapped by <see cref="Queryable"/> LINQ Expressions.</returns>
        public IQueryable<Sensor> QuerySensors(bool strict) => QuerySensors(null, strict);

        /// <summary>
        /// Retrieves sensors from a PRTG Server based on one or more <see cref="Queryable"/> expressions,
        /// starting with an expression to filter sensors according to a specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate to initially filter sensors by.</param>
        /// <returns>An <see cref="IQueryable{Sensor}"/> representing the result of filtering the sensors.</returns>
        public IQueryable<Sensor> QuerySensors(Expression<Func<Sensor, bool>> predicate) => QuerySensors(predicate, false);

        /// <summary>
        /// Retrieves sensors from a PRTG Server based on one or more <see cref="Queryable"/> expressions,
        /// starting with an expression to filter sensors according to a specified predicate and specifying whether
        /// to use strict parsing semantics.
        /// </summary>
        /// <param name="predicate">The predicate to initially filter sensors by.</param>
        /// <param name="strict">Whether to use strict evaluation. If true, a <see cref="NotSupportedException"/> will be thrown
        /// if an expression is encountered that cannot be evaluated server side.<para/>If <paramref name="strict"/> is false,
        /// the maximal supported expression will be executed server side, with any remaining expressions executed client side.</param>
        /// <exception cref="NotSupportedException">An expression was encountered that cannot be evaluated server side.</exception>
        /// <returns>An <see cref="IQueryable{Sensor}"/> representing the result of filtering the sensors.</returns>
        public IQueryable<Sensor> QuerySensors(Expression<Func<Sensor, bool>> predicate, bool strict) =>
            ObjectEngine.QueryObjects(predicate, strict, () => new SensorParameters());

            #endregion
            #region Parameters

        /// <summary>
        /// Retrieves sensors from a PRTG Server using a custom set of parameters.
        /// </summary>
        /// <param name="parameters">A custom set of parameters used to retrieve PRTG Sensors.</param>
        /// <returns>A list of sensors that match the specified parameters.</returns>
        public List<Sensor> GetSensors(SensorParameters parameters) =>
            ObjectEngine.GetObjects<Sensor>(parameters);

        /// <summary>
        /// Asynchronously retrieves sensors from a PRTG Server using a custom set of parameters.
        /// </summary>
        /// <param name="parameters">A custom set of parameters used to retrieve PRTG Sensors.</param>
        /// <returns>A list of sensors that match the specified parameters.</returns>
        public async Task<List<Sensor>> GetSensorsAsync(SensorParameters parameters) =>
            await ObjectEngine.GetObjectsAsync<Sensor>(parameters).ConfigureAwait(false);

        /// <summary>
        /// Streams sensors from a PRTG Server using a custom set of parameters. If <paramref name="serial"/> is false, when this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <param name="parameters">A custom set of parameters used to retrieve PRTG Sensors.</param>
        /// <param name="serial">Specifies whether PrtgAPI should execute all requests one at a time rather than all at once.</param>
        /// <returns>If <paramref name="serial"/> is false, a generator encapsulating a series of <see cref="Task"/> objects capable of streaming a response from a PRTG Server. Otherwise, an enumeration that when iterated retrieves the specified objects.</returns>
        public IEnumerable<Sensor> StreamSensors(SensorParameters parameters, bool serial = false) =>
            ObjectEngine.StreamObjects<Sensor, SensorParameters>(parameters, serial);

            #endregion
            #region Types

        /// <summary>
        /// Retrieves descriptions of all sensor types that can be created under a specified object. Actual supported types may differ based on current PRTG settings.<para/>
        /// If the specified object does not support querying sensor types, this method returns null.
        /// </summary>
        /// <param name="objectId">The ID of the object to retrieve supported types of.</param>
        /// <returns>If the specified object supports querying sensor types, a list descriptions of sensor types supported by the specified object. Otherwise, null.</returns>
        public List<SensorTypeDescriptor> GetSensorTypes(int objectId = 1) =>
            ResponseParser.ParseSensorTypes(ObjectEngine.GetObject<SensorTypeDescriptorInternal>(new SensorTypeParameters(objectId), ResponseParser.ValidateHasContent).Types);

        /// <summary>
        /// Asynchronously retrieves descriptions of all sensor types that can be created under a specified object. Actual supported types may differ based on current PRTG settings.<para/>
        /// If the specified object does not support querying sensor types, this method returns null.
        /// </summary>
        /// <param name="objectId">The ID of the object to retrieve supported types of.</param>
        /// <returns>If the specified object supports querying sensor types, a list descriptions of sensor types supported by the specified object. Otherwise, null.</returns>
        public async Task<List<SensorTypeDescriptor>> GetSensorTypesAsync(int objectId = 1) =>
            ResponseParser.ParseSensorTypes((await ObjectEngine.GetObjectAsync<SensorTypeDescriptorInternal>(new SensorTypeParameters(objectId), ResponseParser.ValidateHasContentAsync).ConfigureAwait(false)).Types);

            #endregion
            #region Totals

        /// <summary>
        /// Retrieves the number of sensors of each sensor type in the system.
        /// </summary>
        /// <returns>The total number of sensors of each <see cref="Status"/> type.</returns>
        public SensorTotals GetSensorTotals() =>
            ObjectEngine.GetObject<SensorTotals>(new XmlFunctionParameters(XmlFunction.GetTreeNodeStats));

        /// <summary>
        /// Asynchronously retrieves the number of sensors of each sensor type in the system.
        /// </summary>
        /// <returns>The total number of sensors of each <see cref="Status"/> type.</returns>
        public async Task<SensorTotals> GetSensorTotalsAsync() =>
            await ObjectEngine.GetObjectAsync<SensorTotals>(new XmlFunctionParameters(XmlFunction.GetTreeNodeStats)).ConfigureAwait(false);

            #endregion
        #endregion
        #region Devices
            #region Single

        /// <summary>
        /// Retrieves a device with a specified ID from a PRTG Server. If the device does not exist or an ambiguous match is found, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="id">The ID of the device to retrieve.</param>
        /// <exception cref="InvalidOperationException">The specified device does not exist or multiple devices were resolved with the specified ID.</exception> 
        /// <returns>The device with the specified ID.</returns>
        public Device GetDevice(int id) => GetDevices(Property.Id, id).SingleObject(id);

        /// <summary>
        /// Asynchronously retrieves a device with a specified ID from a PRTG Server. If the device does not exist or an ambiguous match is found, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="id">The ID of the device to retrieve.</param>
        /// <exception cref="InvalidOperationException">The specified device does not exist or multiple devices were resolved with the specified ID.</exception> 
        /// <returns>The device with the specified ID.</returns>
        public async Task<Device> GetDeviceAsync(int id) => (await GetDevicesAsync(Property.Id, id).ConfigureAwait(false)).SingleObject(id);

            #endregion
            #region Multiple

        /// <summary>
        /// Retrieves all devices from a PRTG Server.
        /// </summary>
        /// <returns>A list of all devices on a PRTG Server.</returns>
        public List<Device> GetDevices() => GetDevices(new DeviceParameters());

        /// <summary>
        /// Asynchronously retrieves all devices from a PRTG Server.
        /// </summary>
        /// <returns>A list of all devices on a PRTG Server.</returns>
        public async Task<List<Device>> GetDevicesAsync() => await GetDevicesAsync(new DeviceParameters()).ConfigureAwait(false);

        /// <summary>
        /// Streams all devices from a PRTG Server. If <paramref name="serial"/> is false, when this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <param name="serial">Specifies whether PrtgAPI should execute all requests one at a time rather than all at once.</param>
        /// <returns>If <paramref name="serial"/> is false, a generator encapsulating a series of <see cref="Task"/> objects capable of streaming a response from a PRTG Server. Otherwise, an enumeration that when iterated retrieves the specified objects.</returns>
        public IEnumerable<Device> StreamDevices(bool serial = false) => StreamDevices(new DeviceParameters(), serial);

            #endregion
            #region Filter (Property, Value)

        /// <summary>
        /// Retrieves devices from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns>A list of devices that match the specified search criteria.</returns>
        public List<Device> GetDevices(Property property, object value) => GetDevices(new SearchFilter(property, value));

        /// <summary>
        /// Asynchronously retrieves devices from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns>A list of devices that match the specified search criteria.</returns>
        public async Task<List<Device>> GetDevicesAsync(Property property, object value) => await GetDevicesAsync(new SearchFilter(property, value)).ConfigureAwait(false);

        /// <summary>
        /// Streams devices from a PRTG Server based on the value of a certain property. When this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns>A generator encapsulating a series of <see cref="Task"/> objects capable of streaming a response from a PRTG Server.</returns>
        public IEnumerable<Device> StreamDevices(Property property, object value) => StreamDevices(new SearchFilter(property, value));

            #endregion
            #region Filter (Property, Operator, Value)

        /// <summary>
        /// Retrieves devices from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="operator">Operator to compare value and property value with.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns>A list of devices that match the specified search criteria.</returns>
        public List<Device> GetDevices(Property property, FilterOperator @operator, string value) => GetDevices(new SearchFilter(property, @operator, value));

        /// <summary>
        /// Asynchronously retrieves devices from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="operator">Operator to compare value and property value with.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns>A list of devices that match the specified search criteria.</returns>
        public async Task<List<Device>> GetDevicesAsync(Property property, FilterOperator @operator, string value) => await GetDevicesAsync(new SearchFilter(property, @operator, value)).ConfigureAwait(false);

        /// <summary>
        /// Streams devices from a PRTG Server based on the value of a certain property. When this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="operator">Operator to compare value and property value with.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns>A generator encapsulating a series of <see cref="Task"/> objects capable of streaming a response from a PRTG Server.</returns>
        public IEnumerable<Device> StreamDevices(Property property, FilterOperator @operator, string value) => StreamDevices(new SearchFilter(property, @operator, value));

            #endregion
            #region Filter (Array)

        /// <summary>
        /// Retrieves devices from a PRTG Server based on the values of multiple properties.
        /// </summary>
        /// <param name="filters">One or more filters used to limit search results.</param>
        /// <returns>A list of devices that match the specified search criteria.</returns>
        public List<Device> GetDevices(params SearchFilter[] filters) => GetDevices(new DeviceParameters { SearchFilter = filters });

        /// <summary>
        /// Asynchronously retrieves devices from a PRTG Server based on the values of multiple properties.
        /// </summary>
        /// <param name="filters">One or more filters used to limit search results.</param>
        /// <returns>A list of devices that match the specified search criteria.</returns>
        public async Task<List<Device>> GetDevicesAsync(params SearchFilter[] filters) => await GetDevicesAsync(new DeviceParameters { SearchFilter = filters }).ConfigureAwait(false);

        /// <summary>
        /// Streams devices from a PRTG Server based on the values of multiple properties. When this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <param name="filters">One or more filters used to limit search results.</param>
        /// <returns>A generator encapsulating a series of <see cref="Task"/> objects capable of streaming a response from a PRTG Server.</returns>
        public IEnumerable<Device> StreamDevices(params SearchFilter[] filters) => StreamDevices(new DeviceParameters { SearchFilter = filters });
            #endregion
            #region Query

        /// <summary>
        /// Retrieves devices from a PRTG Server based on one or more <see cref="Queryable"/> expressions.
        /// </summary>
        /// <returns>An <see cref="IQueryable{Device}"/> to be wrapped by <see cref="Queryable"/> LINQ Expressions.</returns>
        public IQueryable<Device> QueryDevices() => QueryDevices(null);

        /// <summary>
        /// Retrieves devices from a PRTG Server based on one or more <see cref="Queryable"/> expressions, specifying whether to use strict parsing semantics.
        /// </summary>
        /// <param name="strict">Whether to use strict evaluation. If true, a <see cref="NotSupportedException"/> will be thrown
        /// if an expression is encountered that cannot be evaluated server side.<para/>If <paramref name="strict"/> is false,
        /// the maximal supported expression will be executed server side, with any remaining expressions executed client side.</param>
        /// <exception cref="NotSupportedException">An expression was encountered that cannot be evaluated server side.</exception>
        /// <returns>An <see cref="IQueryable{Device}"/> to be wrapped by <see cref="Queryable"/> LINQ Expressions.</returns>
        public IQueryable<Device> QueryDevices(bool strict) => QueryDevices(null, strict);

        /// <summary>
        /// Retrieves devices from a PRTG Server based on one or more <see cref="Queryable"/> expressions,
        /// starting with an expression to filter devices according to a specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate to initially filter devices by.</param>
        /// <returns>An <see cref="IQueryable{Device}"/> representing the result of filtering the devices.</returns>
        public IQueryable<Device> QueryDevices(Expression<Func<Device, bool>> predicate) => QueryDevices(predicate, false);

        /// <summary>
        /// Retrieves devices from a PRTG Server based on one or more <see cref="Queryable"/> expressions,
        /// starting with an expression to filter devices according to a specified predicate and specifying whether
        /// to use strict parsing semantics.
        /// </summary>
        /// <param name="predicate">The predicate to initially filter devices by.</param>
        /// <param name="strict">Whether to use strict evaluation. If true, a <see cref="NotSupportedException"/> will be thrown
        /// if an expression is encountered that cannot be evaluated server side.<para/>If <paramref name="strict"/> is false,
        /// the maximal supported expression will be executed server side, with any remaining expressions executed client side.</param>
        /// <exception cref="NotSupportedException">An expression was encountered that cannot be evaluated server side.</exception>
        /// <returns>An <see cref="IQueryable{Device}"/> representing the result of filtering the devices.</returns>
        public IQueryable<Device> QueryDevices(Expression<Func<Device, bool>> predicate, bool strict) =>
            ObjectEngine.QueryObjects(predicate, strict, () => new DeviceParameters());

            #endregion
            #region Parameters

        /// <summary>
        /// Retrieves devices from a PRTG Server using a custom set of parameters.
        /// </summary>
        /// <param name="parameters">A custom set of parameters used to retrieve PRTG Devices.</param>
        /// <returns>A list of devices that match the specified parameters.</returns>
        public List<Device> GetDevices(DeviceParameters parameters) =>
            ObjectEngine.GetObjects<Device>(parameters);

        /// <summary>
        /// Asynchronously retrieves devices from a PRTG Server using a custom set of parameters.
        /// </summary>
        /// <param name="parameters">A custom set of parameters used to retrieve PRTG Devices.</param>
        /// <returns>A list of devices that match the specified parameters.</returns>
        public async Task<List<Device>> GetDevicesAsync(DeviceParameters parameters) =>
            await ObjectEngine.GetObjectsAsync<Device>(parameters).ConfigureAwait(false);

        /// <summary>
        /// Streams devices from a PRTG Server using a custom set of parameters. If <paramref name="serial"/> is false, when this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <param name="parameters">A custom set of parameters used to retrieve PRTG Devices.</param>
        /// <param name="serial">Specifies whether PrtgAPI should execute all requests one at a time rather than all at once.</param>
        /// <returns>If <paramref name="serial"/> is false, a generator encapsulating a series of <see cref="Task"/> objects capable of streaming a response from a PRTG Server. Otherwise, an enumeration that when iterated retrieves the specified objects.</returns>
        public IEnumerable<Device> StreamDevices(DeviceParameters parameters, bool serial = false) =>
            ObjectEngine.StreamObjects<Device, DeviceParameters>(parameters, serial);

            #endregion

        /// <summary>
        /// Retrieves all auto-discovery device templates supported by the specified object.
        /// </summary>
        /// <param name="deviceId">The ID of the device to retrieve supported device templates of. In practice all devices should support the same device templates.</param>
        /// <returns>A list of device templates supported by the specified object.</returns>
        public List<DeviceTemplate> GetDeviceTemplates(int deviceId = 40) =>
            ResponseParser.GetTemplates(GetObjectPropertiesRawInternal(deviceId, ObjectType.Device));

        /// <summary>
        /// Asynchronously retrieves all auto-discovery device templates supported by the specified object.
        /// </summary>
        /// <param name="deviceId">The ID of the device to retrieve supported device templates of. In practice all devices should support the same device templates.</param>
        /// <returns>A list of device templates supported by the specified object.</returns>
        public async Task<List<DeviceTemplate>> GetDeviceTemplatesAsync(int deviceId = 40) =>
            ResponseParser.GetTemplates(await GetObjectPropertiesRawInternalAsync(deviceId, ObjectType.Device).ConfigureAwait(false));

        #endregion
        #region Groups
            #region Single

        /// <summary>
        /// Retrieves a group with a specified ID from a PRTG Server. If the group does not exist or an ambiguous match is found, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="id">The ID of the group to retrieve.</param>
        /// <exception cref="InvalidOperationException">The specified group does not exist or multiple groups were resolved with the specified ID.</exception> 
        /// <returns>The group with the specified ID.</returns>
        public Group GetGroup(int id) => GetGroups(Property.Id, id).SingleObject(id);

        /// <summary>
        /// Asynchronously retrieves a group with a specified ID from a PRTG Server. If the group does not exist or an ambiguous match is found, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="id">The ID of the group to retrieve.</param>
        /// <exception cref="InvalidOperationException">The specified group does not exist or multiple groups were resolved with the specified ID.</exception> 
        /// <returns>The group with the specified ID.</returns>
        public async Task<Group> GetGroupAsync(int id) => (await GetGroupsAsync(Property.Id, id).ConfigureAwait(false)).SingleObject(id);

            #endregion
            #region Multiple

        /// <summary>
        /// Retrieves all groups from a PRTG Server.
        /// </summary>
        /// <returns>A list of all groups on a PRTG Server.</returns>
        public List<Group> GetGroups() => GetGroups(new GroupParameters());

        /// <summary>
        /// Asynchronously retrieves all groups from a PRTG Server.
        /// </summary>
        /// <returns>A list of all groups on a PRTG Server.</returns>
        public async Task<List<Group>> GetGroupsAsync() => await GetGroupsAsync(new GroupParameters()).ConfigureAwait(false);

        /// <summary>
        /// Streams all groups from a PRTG Server. If <paramref name="serial"/> is false, when this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <param name="serial">Specifies whether PrtgAPI should execute all requests one at a time rather than all at once.</param>
        /// <returns>If <paramref name="serial"/> is false, a generator encapsulating a series of <see cref="Task"/> objects capable of streaming a response from a PRTG Server. Otherwise, an enumeration that when iterated retrieves the specified objects.</returns>
        public IEnumerable<Group> StreamGroups(bool serial = false) => StreamGroups(new GroupParameters(), serial);

            #endregion
            #region Filter (Property, Value)

        /// <summary>
        /// Retrieves groups from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns>A list of groups that match the specified search criteria.</returns>
        public List<Group> GetGroups(Property property, object value) => GetGroups(new SearchFilter(property, value));

        /// <summary>
        /// Asynchronously retrieves groups from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns>A list of groups that match the specified search criteria.</returns>
        public async Task<List<Group>> GetGroupsAsync(Property property, object value) => await GetGroupsAsync(new SearchFilter(property, value)).ConfigureAwait(false);

        /// <summary>
        /// Streams groups from a PRTG Server based on the value of a certain property. When this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns>A generator encapsulating a series of <see cref="Task"/> objects capable of streaming a response from a PRTG Server.</returns>
        public IEnumerable<Group> StreamGroups(Property property, object value) => StreamGroups(new SearchFilter(property, value));

            #endregion
            #region Filter (Property, Operator, Group)

        /// <summary>
        /// Retrieves groups from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="operator">Operator to compare value and property value with.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns>A list of groups that match the specified search criteria.</returns>
        public List<Group> GetGroups(Property property, FilterOperator @operator, string value) => GetGroups(new SearchFilter(property, @operator, value));

        /// <summary>
        /// Asynchronously retrieves groups from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="operator">Operator to compare value and property value with.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns>A list of groups that match the specified search criteria.</returns>
        public async Task<List<Group>> GetGroupsAsync(Property property, FilterOperator @operator, string value) => await GetGroupsAsync(new SearchFilter(property, @operator, value)).ConfigureAwait(false);

        /// <summary>
        /// Streams groups from a PRTG Server based on the value of a certain property. When this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="operator">Operator to compare value and property value with.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns>A generator encapsulating a series of <see cref="Task"/> objects capable of streaming a response from a PRTG Server.</returns>
        public IEnumerable<Group> StreamGroups(Property property, FilterOperator @operator, string value) => StreamGroups(new SearchFilter(property, @operator, value));

            #endregion
            #region Filter (Array)

        /// <summary>
        /// Retrieves groups from a PRTG Server based on the values of multiple properties.
        /// </summary>
        /// <param name="filters">One or more filters used to limit search results.</param>
        /// <returns>A list of groups that match the specified search criteria.</returns>
        public List<Group> GetGroups(params SearchFilter[] filters) => GetGroups(new GroupParameters { SearchFilter = filters });

        /// <summary>
        /// Asynchronously retrieves groups from a PRTG Server based on the values of multiple properties.
        /// </summary>
        /// <param name="filters">One or more filters used to limit search results.</param>
        /// <returns>A list of groups that match the specified search criteria.</returns>
        public async Task<List<Group>> GetGroupsAsync(params SearchFilter[] filters) => await GetGroupsAsync(new GroupParameters { SearchFilter = filters }).ConfigureAwait(false);

        /// <summary>
        /// Streams groups from a PRTG Server based on the values of multiple properties. When this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <param name="filters">One or more filters used to limit search results.</param>
        /// <returns>A generator encapsulating a series of <see cref="Task"/> objects capable of streaming a response from a PRTG Server.</returns>
        public IEnumerable<Group> StreamGroups(params SearchFilter[] filters) => StreamGroups(new GroupParameters { SearchFilter = filters });

            #endregion
            #region Query

        /// <summary>
        /// Retrieves groups from a PRTG Server based on one or more <see cref="Queryable"/> expressions.
        /// </summary>
        /// <returns>An <see cref="IQueryable{Group}"/> to be wrapped by <see cref="Queryable"/> LINQ Expressions.</returns>
        public IQueryable<Group> QueryGroups() => QueryGroups(null);

        /// <summary>
        /// Retrieves groups from a PRTG Server based on one or more <see cref="Queryable"/> expressions, specifying whether to use strict parsing semantics.
        /// </summary>
        /// <param name="strict">Whether to use strict evaluation. If true, a <see cref="NotSupportedException"/> will be thrown
        /// if an expression is encountered that cannot be evaluated server side.<para/>If <paramref name="strict"/> is false,
        /// the maximal supported expression will be executed server side, with any remaining expressions executed client side.</param>
        /// <exception cref="NotSupportedException">An expression was encountered that cannot be evaluated server side.</exception>
        /// <returns>An <see cref="IQueryable{Group}"/> to be wrapped by <see cref="Queryable"/> LINQ Expressions.</returns>
        public IQueryable<Group> QueryGroups(bool strict) => QueryGroups(null, strict);

        /// <summary>
        /// Retrieves groups from a PRTG Server based on one or more <see cref="Queryable"/> expressions,
        /// starting with an expression to filter groups according to a specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate to initially filter groups by.</param>
        /// <returns>An <see cref="IQueryable{Group}"/> representing the result of filtering the groups.</returns>
        public IQueryable<Group> QueryGroups(Expression<Func<Group, bool>> predicate) =>
            QueryGroups(predicate, false);

        /// <summary>
        /// Retrieves groups from a PRTG Server based on one or more <see cref="Queryable"/> expressions,
        /// starting with an expression to filter groups according to a specified predicate and specifying whether
        /// to use strict parsing semantics.
        /// </summary>
        /// <param name="predicate">The predicate to initially filter groups by.</param>
        /// <param name="strict">Whether to use strict evaluation. If true, a <see cref="NotSupportedException"/> will be thrown
        /// if an expression is encountered that cannot be evaluated server side.<para/>If <paramref name="strict"/> is false,
        /// the maximal supported expression will be executed server side, with any remaining expressions executed client side.</param>
        /// <exception cref="NotSupportedException">An expression was encountered that cannot be evaluated server side.</exception>
        /// <returns>An <see cref="IQueryable{Group}"/> representing the result of filtering the groups.</returns>
        public IQueryable<Group> QueryGroups(Expression<Func<Group, bool>> predicate, bool strict) =>
            ObjectEngine.QueryObjects(predicate, strict, () => new GroupParameters());

            #endregion
            #region Parameters

        /// <summary>
        /// Retrieves groups from a PRTG Server using a custom set of parameters.
        /// </summary>
        /// <param name="parameters">A custom set of parameters used to retrieve PRTG Groups.</param>
        /// <returns>A list of groups that match the specified parameters.</returns>
        public List<Group> GetGroups(GroupParameters parameters) =>
            ObjectEngine.GetObjects<Group>(parameters);

        /// <summary>
        /// Asynchronously retrieves groups from a PRTG Server using a custom set of parameters.
        /// </summary>
        /// <returns>A list of groups that match the specified parameters.</returns>
        public async Task<List<Group>> GetGroupsAsync(GroupParameters parameters) =>
            await ObjectEngine.GetObjectsAsync<Group>(parameters).ConfigureAwait(false);

        /// <summary>
        /// Streams groups from a PRTG Server using a custom set of parameters. If <paramref name="serial"/> is false, when this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <returns>If <paramref name="serial"/> is false, a generator encapsulating a series of <see cref="Task"/> objects capable of streaming a response from a PRTG Server. Otherwise, an enumeration that when iterated retrieves the specified objects.</returns>
        public IEnumerable<Group> StreamGroups(GroupParameters parameters, bool serial = false) =>
            ObjectEngine.StreamObjects<Group, GroupParameters>(parameters, serial);

            #endregion
        #endregion
        #region Probes
            #region Single

        /// <summary>
        /// Retrieves a probe with a specified ID from a PRTG Server. If the probe does not exist or an ambiguous match is found, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="id">The ID of the probe to retrieve.</param>
        /// <exception cref="InvalidOperationException">The specified probe does not exist or multiple probes were resolved with the specified ID.</exception> 
        /// <returns>The probe with the specified ID.</returns>
        public Probe GetProbe(int id) => GetProbes(Property.Id, id).SingleObject(id);

        /// <summary>
        /// Asynchronously retrieves a probe with a specified ID from a PRTG Server. If the probe does not exist or an ambiguous match is found, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="id">The ID of the probe to retrieve.</param>
        /// <exception cref="InvalidOperationException">The specified probe does not exist or multiple probes were resolved with the specified ID.</exception> 
        /// <returns>The probe with the specified ID.</returns>
        public async Task<Probe> GetProbeAsync(int id) => (await GetProbesAsync(Property.Id, id).ConfigureAwait(false)).SingleObject(id);

            #endregion
            #region Multiple

        /// <summary>
        /// Retrieves all probes from a PRTG Server.
        /// </summary>
        /// <returns>A list of all probes on a PRTG Server.</returns>
        public List<Probe> GetProbes() => GetProbes(new ProbeParameters());

        /// <summary>
        /// Asynchronously retrieves all probes from a PRTG Server.
        /// </summary>
        /// <returns>A list of all probes on a PRTG Server.</returns>
        public async Task<List<Probe>> GetProbesAsync() => await GetProbesAsync(new ProbeParameters()).ConfigureAwait(false);

        /// <summary>
        /// Streams all probes from a PRTG Server. If <paramref name="serial"/> is false, when this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <param name="serial">Specifies whether PrtgAPI should execute all requests one at a time rather than all at once.</param>
        /// <returns>If <paramref name="serial"/> is false, a generator encapsulating a series of <see cref="Task"/> objects capable of streaming a response from a PRTG Server. Otherwise, an enumeration that when iterated retrieves the specified objects.</returns>
        public IEnumerable<Probe> StreamProbes(bool serial = false) => StreamProbes(new ProbeParameters(), serial);

            #endregion
            #region Filter (Property, Value)

        /// <summary>
        /// Retrieves probes from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns>A list of probes that match the specified search criteria.</returns>
        public List<Probe> GetProbes(Property property, object value) => GetProbes(new SearchFilter(property, value));

        /// <summary>
        /// Asynchronously retrieves probes from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns>A list of probes that match the specified search criteria.</returns>
        public async Task<List<Probe>> GetProbesAsync(Property property, object value) => await GetProbesAsync(new SearchFilter(property, value)).ConfigureAwait(false);

        /// <summary>
        /// Streams probes from a PRTG Server based on the value of a certain property. When this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns>A generator encapsulating a series of <see cref="Task"/> objects capable of streaming a response from a PRTG Server.</returns>
        public IEnumerable<Probe> StreamProbes(Property property, object value) => StreamProbes(new SearchFilter(property, value));

            #endregion
            #region Filter (Property, Operator, Value)

        /// <summary>
        /// Retrieves probes from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="operator">Operator to compare value and property value with.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns>A list of probes that match the specified search criteria.</returns>
        public List<Probe> GetProbes(Property property, FilterOperator @operator, string value) => GetProbes(new SearchFilter(property, @operator, value));

        /// <summary>
        /// Asynchronously retrieves probes from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="operator">Operator to compare value and property value with.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns>A list of probes that match the specified search criteria.</returns>
        public async Task<List<Probe>> GetProbesAsync(Property property, FilterOperator @operator, string value) => await GetProbesAsync(new SearchFilter(property, @operator, value)).ConfigureAwait(false);

        /// <summary>
        /// Streams probes from a PRTG Server based on the value of a certain property. When this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="operator">Operator to compare value and property value with.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns>A generator encapsulating a series of <see cref="Task"/> objects capable of streaming a response from a PRTG Server.</returns>
        public IEnumerable<Probe> StreamProbes(Property property, FilterOperator @operator, string value) => StreamProbes(new SearchFilter(property, @operator, value));

            #endregion
            #region Filter (Array)

        /// <summary>
        /// Retrieves probes from a PRTG Server based on the values of multiple properties.
        /// </summary>
        /// <param name="filters">One or more filters used to limit search results.</param>
        /// <returns>A list of probes that match the specified search criteria.</returns>
        public List<Probe> GetProbes(params SearchFilter[] filters) => GetProbes(new ProbeParameters { SearchFilter = filters });

        /// <summary>
        /// Asynchronously retrieves probes from a PRTG Server based on the values of multiple properties.
        /// </summary>
        /// <param name="filters">One or more filters used to limit search results.</param>
        /// <returns>A list of probes that match the specified search criteria.</returns>
        public async Task<List<Probe>> GetProbesAsync(params SearchFilter[] filters) => await GetProbesAsync(new ProbeParameters { SearchFilter = filters }).ConfigureAwait(false);

        /// <summary>
        /// Streams probes from a PRTG Server based on the values of multiple properties. When this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <param name="filters">One or more filters used to limit search results.</param>
        /// <returns>A generator encapsulating a series of <see cref="Task"/> objects capable of streaming a response from a PRTG Server.</returns>
        public IEnumerable<Probe> StreamProbes(params SearchFilter[] filters) => StreamProbes(new ProbeParameters { SearchFilter = filters });

            #endregion
            #region Query

        /// <summary>
        /// Retrieves probes from a PRTG Server based on one or more <see cref="Queryable"/> expressions.
        /// </summary>
        /// <returns>An <see cref="IQueryable{Probe}"/> to be wrapped by <see cref="Queryable"/> LINQ Expressions.</returns>
        public IQueryable<Probe> QueryProbes() => QueryProbes(null);

        /// <summary>
        /// Retrieves probes from a PRTG Server based on one or more <see cref="Queryable"/> expressions, specifying whether to use strict parsing semantics.
        /// </summary>
        /// <param name="strict">Whether to use strict evaluation. If true, a <see cref="NotSupportedException"/> will be thrown
        /// if an expression is encountered that cannot be evaluated server side.<para/>If <paramref name="strict"/> is false,
        /// the maximal supported expression will be executed server side, with any remaining expressions executed client side.</param>
        /// <exception cref="NotSupportedException">An expression was encountered that cannot be evaluated server side.</exception>
        /// <returns>An <see cref="IQueryable{Probe}"/> to be wrapped by <see cref="Queryable"/> LINQ Expressions.</returns>
        public IQueryable<Probe> QueryProbes(bool strict) => QueryProbes(null, strict);

        /// <summary>
        /// Retrieves probes from a PRTG Server based on one or more <see cref="Queryable"/> expressions,
        /// starting with an expression to filter probes according to a specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate to initially filter probes by.</param>
        /// <returns>An <see cref="IQueryable{Probe}"/> representing the result of filtering the probes.</returns>
        public IQueryable<Probe> QueryProbes(Expression<Func<Probe, bool>> predicate) => QueryProbes(predicate, false);

        /// <summary>
        /// Retrieves probes from a PRTG Server based on one or more <see cref="Queryable"/> expressions,
        /// starting with an expression to filter probes according to a specified predicate and specifying whether
        /// to use strict parsing semantics.
        /// </summary>
        /// <param name="predicate">The predicate to initially filter probes by.</param>
        /// <param name="strict">Whether to use strict evaluation. If true, a <see cref="NotSupportedException"/> will be thrown
        /// if an expression is encountered that cannot be evaluated server side.<para/>If <paramref name="strict"/> is false,
        /// the maximal supported expression will be executed server side, with any remaining expressions executed client side.</param>
        /// <exception cref="NotSupportedException">An expression was encountered that cannot be evaluated server side.</exception>
        /// <returns>An <see cref="IQueryable{Probe}"/> representing the result of filtering the probes.</returns>
        public IQueryable<Probe> QueryProbes(Expression<Func<Probe, bool>> predicate, bool strict) =>
            ObjectEngine.QueryObjects(predicate, strict, () => new ProbeParameters());

            #endregion
            #region Parameters

        /// <summary>
        /// Retrieves probes from a PRTG Server using a custom set of parameters.
        /// </summary>
        /// <param name="parameters">A custom set of parameters used to retrieve PRTG Probes.</param>
        /// <returns>A list of probes that match the specified parameters.</returns>
        public List<Probe> GetProbes(ProbeParameters parameters) =>
            ObjectEngine.GetObjects<Probe>(parameters);

        /// <summary>
        /// Asynchronously retrieves probes from a PRTG Server using a custom set of parameters.
        /// </summary>
        /// <param name="parameters">A custom set of parameters used to retrieve PRTG Probes.</param>
        /// <returns>A list of probes that match the specified parameters.</returns>
        public async Task<List<Probe>> GetProbesAsync(ProbeParameters parameters) =>
            await ObjectEngine.GetObjectsAsync<Probe>(parameters).ConfigureAwait(false);

        /// <summary>
        /// Streams probes from a PRTG Server using a custom set of parameters. If <paramref name="serial"/> is false, when this method's response is enumerated multiple parallel requests will be executed against the PRTG Server and yielded in the order they return.
        /// </summary>
        /// <param name="parameters">A custom set of parameters used to retrieve PRTG Probes.</param>
        /// <param name="serial">Specifies whether PrtgAPI should execute all requests one at a time rather than all at once.</param>
        /// <returns>If <paramref name="serial"/> is false, a generator encapsulating a series of <see cref="Task"/> objects capable of streaming a response from a PRTG Server. Otherwise, an enumeration that when iterated retrieves the specified objects.</returns>
        public IEnumerable<Probe> StreamProbes(ProbeParameters parameters, bool serial = false) =>
            ObjectEngine.StreamObjects<Probe, ProbeParameters>(parameters, serial);

            #endregion
        #endregion
        #region Channel
            #region Single

        /// <summary>
        /// Retrieves a channel with a specified ID from a PRTG Server. If the channel does not exist or an ambiguous match is found, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="sensorId">The ID of the sensor containing the channel.</param>
        /// <param name="id">The ID of the channel to retrieve.</param>
        /// <exception cref="InvalidOperationException">The specified channel does not exist or multiple channels were resolved with the specified ID.</exception> 
        /// <returns>The channel with the specified ID.</returns>
        public Channel GetChannel(int sensorId, int id) => GetChannelsInternal(sensorId, null, i => i == id).SingleObject(id);

        /// <summary>
        /// Asynchronously retrieves a channel with a specified ID from a PRTG Server. If the channel does not exist or an ambiguous match is found, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="sensorId">The ID of the sensor containing the channel.</param>
        /// <param name="id">The ID of the channel to retrieve.</param>
        /// <exception cref="InvalidOperationException">The specified object does not exist or multiple objects were resolved with the specified ID.</exception> 
        /// <returns>The channel with the specified ID.</returns>
        public async Task<Channel> GetChannelAsync(int sensorId, int id) => (await GetChannelsInternalAsync(sensorId, null, i => i == id).ConfigureAwait(false)).SingleObject(id);

        /// <summary>
        /// Retrieves a channel with a specified name from a PRTG Server. If the channel does not exist or an ambiguous match is found, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="sensorId">The ID of the sensor containing the channel.</param>
        /// <param name="name">The name of the channel to retrieve.</param>
        /// <exception cref="InvalidOperationException">The specified channel does not exist or multiple channels were resolved with the specified name.</exception> 
        /// <returns>The channel with the specified name.</returns>
        public Channel GetChannel(int sensorId, string name) => GetChannelsInternal(sensorId, n => n == name).SingleObject(name, "Name");

        /// <summary>
        /// Asynchronously retrieves a channel with a specified name from a PRTG Server. If the channel does not exist or an ambiguous match is found, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="sensorId">The ID of the sensor containing the channel.</param>
        /// <param name="name">The name of the channel to retrieve.</param>
        /// <exception cref="InvalidOperationException">The specified channel does not exist or multiple channels were resolved with the specified name.</exception> 
        /// <returns>The channel with the specified name.</returns>
        public async Task<Channel> GetChannelAsync(int sensorId, string name) => (await GetChannelsInternalAsync(sensorId, n => n == name).ConfigureAwait(false)).SingleObject(name, "Name");

            #endregion

        /// <summary>
        /// Retrieves all channels of a sensor.
        /// </summary>
        /// <param name="sensorId">The ID of the sensor to retrieve channels for.</param>
        /// <returns></returns>
        public List<Channel> GetChannels(int sensorId) => GetChannelsInternal(sensorId);

        /// <summary>
        /// Retrieves all channels of a sensor that match the specified name.
        /// </summary>
        /// <param name="sensorId">The ID of the sensor to retrieve channels for.</param>
        /// <param name="channelName">The name of the channel to retrieve.</param>
        /// <returns></returns>
        public List<Channel> GetChannels(int sensorId, string channelName) => GetChannelsInternal(sensorId, name => name == channelName);

        /// <summary>
        /// Asynchronously retrieves all channels of a sensor.
        /// </summary>
        /// <param name="sensorId">The ID of the sensor to retrieve channels for.</param>
        /// <returns></returns>
        public async Task<List<Channel>> GetChannelsAsync(int sensorId) => await GetChannelsInternalAsync(sensorId).ConfigureAwait(false);

        /// <summary>
        /// Asynchronously retrieves all channels of a sensor that match the specified name.
        /// </summary>
        /// <param name="sensorId">The ID of the sensor to retrieve channels for.</param>
        /// <param name="channelName">The name of the channel to retrieve.</param>
        /// <returns></returns>
        public async Task<List<Channel>> GetChannelsAsync(int sensorId, string channelName) => await GetChannelsInternalAsync(sensorId, name => name == channelName).ConfigureAwait(false);

        private XElement GetChannelProperties(int sensorId, int channelId)
        {
            var parameters = new ChannelPropertiesParameters(sensorId, channelId);

            return RequestEngine.ExecuteRequest(parameters, r => ChannelSettings.GetChannelXml(r, channelId));
        }

        private async Task<XElement> GetChannelPropertiesAsync(int sensorId, int channelId)
        {
            var parameters = new ChannelPropertiesParameters(sensorId, channelId);

            return await RequestEngine.ExecuteRequestAsync(parameters, r => ChannelSettings.GetChannelXml(r, channelId)).ConfigureAwait(false);
        }

        #endregion
        #region Logs
            #region DateTime

        /// <summary>
        /// Retrieve logs between two time periods from a PRTG Server. Logs are ordered from newest to oldest.
        /// </summary>
        /// <param name="objectId">ID of the object to retrieve logs from. If this value is null or 0, logs will be retrieved from the root group.</param>
        /// <param name="startDate">Start date to retrieve logs from. If this value is null, logs will be retrieved from the current date and time.</param>
        /// <param name="endDate">End date to retrieve logs to. If this value is null, logs will be retrieved until the beginning of all logs.</param>
        /// <param name="count">Number of logs to retrieve. Depending on the number of logs stored in the system, specifying a high number may cause the request to timeout.</param>
        /// <param name="status">Log event types to retrieve records for. If no types are specified, all record types will be retrieved.</param>
        /// <returns>All logs that meet the specified criteria.</returns>
        public List<Log> GetLogs(int? objectId, DateTime? startDate = null, DateTime? endDate = null, int count = 500, params LogStatus[] status) =>
            GetObjects<Log>(new LogParameters(objectId, startDate, endDate, count, status));

        /// <summary>
        /// Asynchronously retrieve logs between two time periods from a PRTG Server. Logs are ordered from newest to oldest.
        /// </summary>
        /// <param name="objectId">ID of the object to retrieve logs from. If this value is null or 0, logs will be retrieved from the root group.</param>
        /// <param name="startDate">Start date to retrieve logs from. If this value is null, logs will be retrieved from the current date and time.</param>
        /// <param name="endDate">End date to retrieve logs to. If this value is null, logs will be retrieved until the beginning of all logs.</param>
        /// <param name="count">Number of logs to retrieve. Depending on the number of logs stored in the system, specifying a high number may cause the request to timeout.</param>
        /// <param name="status">Log event types to retrieve records for. If no types are specified, all record types will be retrieved.</param>
        /// <returns>All logs that meet the specified criteria.</returns>
        public async Task<List<Log>> GetLogsAsync(int? objectId, DateTime? startDate = null, DateTime? endDate = null, int count = 500, params LogStatus[] status) =>
            await GetObjectsAsync<Log>(new LogParameters(objectId, startDate, endDate, count, status)).ConfigureAwait(false);

        /// <summary>
        /// Stream logs between two time periods from a PRTG Server. Logs are ordered from newest to oldest.
        /// </summary>
        /// <param name="objectId">ID of the object to retrieve logs from. If this value is null or 0, logs will be retrieved from the root group.</param>
        /// <param name="startDate">Start date to retrieve logs from. If this value is null, logs will be retrieved from the current date and time.</param>
        /// <param name="endDate">End date to retrieve logs to. If this value is null, logs will be retrieved until the beginning of all logs.</param>
        /// <param name="serial">Specifies whether PrtgAPI should execute all requests one at a time rather than all at once.</param>
        /// <param name="status">Log event types to retrieve records for. If no types are specified, all record types will be retrieved.</param>
        /// <returns>All logs that meet the specified criteria.</returns>
        public IEnumerable<Log> StreamLogs(int? objectId, DateTime? startDate = null, DateTime? endDate = null, bool serial = false, params LogStatus[] status) =>
            StreamObjects(new LogParameters(objectId, startDate, endDate, status: status), serial);

            #endregion
            #region RecordAge
        
        /// <summary>
        /// Retrieve logs from a standard time period from a PRTG Server. Logs are ordered from newest to oldest.
        /// </summary>
        /// <param name="objectId">ID of the object to retrieve logs from. If this value is null or 0, logs will be retrieved from the root group.</param>
        /// <param name="timeSpan">Time period to retrieve logs from. Logs will be retrieved from the beginning of this period until the current date and time, ordered newest to oldest.</param>
        /// <param name="count">Number of logs to retrieve. Depending on the number of logs stored in the system, specifying a high number may cause the request to timeout.</param>
        /// <param name="status">Log event types to retrieve records for. If no types are specified, all record types will be retrieved.</param>
        /// <returns>All logs that meet the specified criteria.</returns>
        public List<Log> GetLogs(int? objectId = null, RecordAge timeSpan = RecordAge.LastWeek, int count = 500, params LogStatus[] status) =>
            GetObjects<Log>(new LogParameters(objectId, timeSpan, count, status));

        /// <summary>
        /// Asynchronously retrieve logs from a standard time period from a PRTG Server. Logs are ordered from newest to oldest.
        /// </summary>
        /// <param name="objectId">ID of the object to retrieve logs from. If this value is null or 0, logs will be retrieved from the root group.</param>
        /// <param name="timeSpan">Time period to retrieve logs from. Logs will be retrieved from the beginning of this period until the current date and time, ordered newest to oldest.</param>
        /// <param name="count">Number of logs to retrieve. Depending on the number of logs stored in the system, specifying a high number may cause the request to timeout.</param>
        /// <param name="status">Log event types to retrieve records for. If no types are specified, all record types will be retrieved.</param>
        /// <returns>All logs that meet the specified criteria.</returns>
        public async Task<List<Log>> GetLogsAsync(int? objectId = null, RecordAge timeSpan = RecordAge.LastWeek, int count = 500, params LogStatus[] status) =>
            await GetObjectsAsync<Log>(new LogParameters(objectId, timeSpan, count, status)).ConfigureAwait(false);

        /// <summary>
        /// Stream logs from a standard time period from a PRTG Server. Logs are ordered from newest to oldest.
        /// </summary>
        /// <param name="objectId">ID of the object to retrieve logs from. If this value is null or 0, logs will be retrieved from the root group.</param>
        /// <param name="timeSpan">Time period to retrieve logs from. Logs will be retrieved from the beginning of this period until the current date and time, ordered newest to oldest.</param>
        /// <param name="serial">Specifies whether PrtgAPI should execute all requests one at a time rather than all at once.</param>
        /// <param name="status">Log event types to retrieve records for. If no types are specified, all record types will be retrieved.</param>
        /// <returns>All logs that meet the specified criteria.</returns>
        public IEnumerable<Log> StreamLogs(int? objectId = null, RecordAge timeSpan = RecordAge.LastWeek, bool serial = false, params LogStatus[] status) =>
            StreamObjects(new LogParameters(objectId, timeSpan, status: status), serial);
            #endregion
            #region Query

        /// <summary>
        /// Retrieves logs from a PRTG Server based on one or more <see cref="Queryable"/> expressions.<para/>
        /// If a lower date range is not specified, logs will be retrieved between the upper range and the beginning of all logs.
        /// If an upper range is not specified, logs will be retrieved from the current date and time. If an Id is not specified,
        /// logs will be retrieved for all objects.
        /// </summary>
        /// <returns>An <see cref="IQueryable{Log}"/> to be wrapped by <see cref="Queryable"/> LINQ Expressions.</returns>
        public IQueryable<Log> QueryLogs() => QueryLogs(null);

        /// <summary>
        /// Retrieves logs from a PRTG Server based on one or more <see cref="Queryable"/> expressions using strict parsing semantics.<para/>
        /// If a lower date range is not specified, logs will be retrieved between the upper range and the beginning of all logs.
        /// If an upper range is not specified, logs will be retrieved from the current date and time. If an Id is not specified,
        /// logs will be retrieved for all objects.<para/>
        /// </summary>
        /// <param name="strict">Whether to use strict evaluation. If true, a <see cref="NotSupportedException"/> will be thrown
        /// if an expression is encountered that cannot be evaluated server side.<para/>If <paramref name="strict"/> is false,
        /// the maximal supported expression will be executed server side, with any remaining expressions executed client side.</param>
        /// <exception cref="NotSupportedException">An expression was specified that cannot be evaluated server side.</exception>
        /// <returns>An <see cref="IQueryable{Log}"/> to be wrapped by <see cref="Queryable"/> LINQ Expressions.</returns>
        public IQueryable<Log> QueryLogs(bool strict) => QueryLogs(null, strict);

        /// <summary>
        /// Retrieves logs from a PRTG Server based on one or more <see cref="Queryable"/> expressions,
        /// starting with an expression to filter logs according to a specified predicate.<para/>
        /// If a lower date range is not specified in any predicate, logs will be retrieved between the upper range and the beginning of all logs.
        /// If an upper range is not specified, logs will be retrieved from the current date and time. If an Id is not specified,
        /// logs will be retrieved for all objects.
        /// </summary>
        /// <param name="predicate">The predicate to initially filter logs by.</param>
        /// <returns>An <see cref="IQueryable{Probe}"/> representing the result of filtering the logs.</returns>
        public IQueryable<Log> QueryLogs(Expression<Func<Log, bool>> predicate) => QueryLogs(predicate, false);

        /// <summary>
        /// Retrieves logs from a PRTG Server based on one or more <see cref="Queryable"/> expressions,
        /// starting with an expression to filter logs according to a specified predicate and specifying
        /// whether to use strict parsing semantics.<para/>
        /// If a lower date range is not specified in any predicate, logs will be retrieved between the upper range and the beginning of all logs.
        /// If an upper range is not specified, logs will be retrieved from the current date and time. If an Id is not specified,
        /// logs will be retrieved for all objects.
        /// </summary>
        /// <param name="predicate">The predicate to initially filter logs by.</param>
        /// <param name="strict">Whether to use strict evaluation. If true, a <see cref="NotSupportedException"/> will be thrown
        /// if an expression is encountered that cannot be evaluated server side.<para/>If <paramref name="strict"/> is false,
        /// the maximal supported expression will be executed server side, with any remaining expressions executed client side.</param>
        /// <exception cref="NotSupportedException">An expression was specified that cannot be evaluated server side.</exception>
        /// <returns>An <see cref="IQueryable{Probe}"/> representing the result of filtering the logs.</returns>
        public IQueryable<Log> QueryLogs(Expression<Func<Log, bool>> predicate, bool strict) =>
            ObjectEngine.QueryObjects(predicate, strict, () => new LogParameters(null), new QueryLogHelper(strict));

            #endregion
        #endregion
        #region Notification Actions

        /// <summary>
        /// Retrieves a notification action with a specified ID from a PRTG Server. If the notification action does not exist or an ambiguous match is found, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="id">The ID of the notification action to retrieve.</param>
        /// <exception cref="InvalidOperationException">The specified notification action does not exist or multiple notification actions were resolved with the specified ID.</exception> 
        /// <returns>The notification action with the specified ID.</returns>
        public NotificationAction GetNotificationAction(int id) => GetNotificationActions(Property.Id, id).SingleObject(id);

        /// <summary>
        /// Asynchronously retrieves a notification action with a specified ID from a PRTG Server. If the notification action does not exist or an ambiguous match is found, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="id">The ID of the notification action to retrieve.</param>
        /// <exception cref="InvalidOperationException">The specified notification action does not exist or multiple notification actions were resolved with the specified ID.</exception> 
        /// <returns>The notification action with the specified ID.</returns>
        public async Task<NotificationAction> GetNotificationActionAsync(int id) => (await GetNotificationActionsAsync(Property.Id, id).ConfigureAwait(false)).SingleObject(id);

        /// <summary>
        /// Retrieves a notification action with a specified name from a PRTG Server. If the notification action does not exist or an ambiguous match is found, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="name">The name of the notification action to retrieve.</param>
        /// <exception cref="InvalidOperationException">The specified notification action does not exist or multiple notification actions were resolved with the specified name.</exception> 
        /// <returns>The notification action with the specified name.</returns>
        public NotificationAction GetNotificationAction(string name) => GetNotificationActions(Property.Name, name).SingleObject(name, "Name");

        /// <summary>
        /// Asynchronously retrieves a notification action with a specified name from a PRTG Server. If the notification action does not exist or an ambiguous match is found, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="name">The name of the notification action to retrieve.</param>
        /// <exception cref="InvalidOperationException">The specified notification action does not exist or multiple notification actions were resolved with the specified name.</exception> 
        /// <returns>The notification action with the specified name.</returns>
        public async Task<NotificationAction> GetNotificationActionAsync(string name) => (await GetNotificationActionsAsync(Property.Name, "name").ConfigureAwait(false)).SingleObject(name, "Name");

        /// <summary>
        /// Retrieves notification actions from a PRTG Server.
        /// </summary>
        /// <returns>A list of all notification actions present on a PRTG Server.</returns>
        public List<NotificationAction> GetNotificationActions() =>
            GetNotificationActionsInternal(new NotificationActionParameters());

        /// <summary>
        /// Retrieves notification actions from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns>All notification actions whose property matched the specified value.</returns>
        public List<NotificationAction> GetNotificationActions(Property property, object value) =>
            GetNotificationActions(new SearchFilter(property, value));

        /// <summary>
        /// Retrieves all notification actions on a PRTG Server, filtering for objects by one or more conditions.
        /// </summary>
        /// <param name="filters">One or more filters used to limit search results.</param>
        /// <returns>All objects that match the specified conditions.</returns>
        public List<NotificationAction> GetNotificationActions(params SearchFilter[] filters) =>
            GetNotificationActionsInternal(new NotificationActionParameters { SearchFilter = filters });

        /// <summary>
        /// Asynchronously retrieves notification actions from a PRTG Server.
        /// </summary>
        /// <returns>A list of all notification actions present on a PRTG Server.</returns>
        public async Task<List<NotificationAction>> GetNotificationActionsAsync() =>
            await GetNotificationActionsInternalAsync(new NotificationActionParameters()).ConfigureAwait(false);

        /// <summary>
        /// Asynchronously retrieves notification actions from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns>All notification actions whose property matched the specified value.</returns>
        public async Task<List<NotificationAction>> GetNotificationActionsAsync(Property property, object value) =>
            await GetNotificationActionsAsync(new SearchFilter(property, value)).ConfigureAwait(false);

        /// <summary>
        /// Asynchronously retrieves all notification actions on a PRTG Server, filtering for objects by one or more conditions.
        /// </summary>
        /// <param name="filters">One or more filters used to limit search results.</param>
        /// <returns>All objects that match the specified conditions.</returns>
        public async Task<List<NotificationAction>> GetNotificationActionsAsync(params SearchFilter[] filters) =>
            await GetNotificationActionsInternalAsync(new NotificationActionParameters(filters)).ConfigureAwait(false);

        private XElement GetNotificationActionProperties(int id)
        {
            var xml = RequestEngine.ExecuteRequest(new GetObjectPropertyParameters(id, ObjectType.Notification), ObjectSettings.GetXml);

            xml = ResponseParser.GroupNotificationActionProperties(xml);

            return xml;
        }

        private async Task<XElement> GetNotificationActionPropertiesAsync(int id)
        {
            var xml = await RequestEngine.ExecuteRequestAsync(new GetObjectPropertyParameters(id, ObjectType.Notification), ObjectSettings.GetXml).ConfigureAwait(false);

            xml = ResponseParser.GroupNotificationActionProperties(xml);

            return xml;
        }

        private void UpdateActionSchedules(List<IGrouping<int, NotificationAction>> actions)
        {
            if (actions.Count > 0)
            {
                var schedules = new Lazy<List<Schedule>>(() => GetSchedules(Property.Id, actions.Select(a => a.Key)));

                foreach (var group in actions)
                {
                    foreach (var action in group)
                        action.schedule = new Lazy<Schedule>(() => schedules.Value.First(s => s.Id == group.Key));
                }
            }
        }

        private async Task UpdateActionSchedulesAsync(List<IGrouping<int, NotificationAction>> actions)
        {
            if (actions.Count > 0)
            {
                var schedules = await GetSchedulesAsync(Property.Id, actions.Select(a => a.Key)).ConfigureAwait(false);

                foreach (var group in actions)
                {
                    foreach (var action in group)
                        action.schedule = new Lazy<Schedule>(() => schedules.First(s => s.Id == group.Key));
                }
            }
        }

        #endregion
        #region Notification Triggers

        /// <summary>
        /// Retrieves all notification triggers of a PRTG Object.
        /// </summary>
        /// <param name="objectId">The object to retrieve triggers for.</param>
        /// <returns>A list of notification triggers that apply to the specified object.</returns>
        public List<NotificationTrigger> GetNotificationTriggers(int objectId)
        {
            var xmlResponse = RequestEngine.ExecuteRequest(new NotificationTriggerParameters(objectId));

            var parsed = ResponseParser.ParseNotificationTriggerResponse(objectId, xmlResponse);

            UpdateTriggerChannels(parsed);
            UpdateTriggerActions(parsed);

            return parsed;
        }

        /// <summary>
        /// Asynchronously retrieves all notification triggers of a PRTG Object.
        /// </summary>
        /// <param name="objectId">The object to retrieve triggers for.</param>
        /// <returns>A list of notification triggers that apply to the specified object.</returns>
        public async Task<List<NotificationTrigger>> GetNotificationTriggersAsync(int objectId)
        {
            var xmlResponse = await RequestEngine.ExecuteRequestAsync(new NotificationTriggerParameters(objectId)).ConfigureAwait(false);

            var parsed = ResponseParser.ParseNotificationTriggerResponse(objectId, xmlResponse);

            await UpdateTriggerChannelsAsync(parsed).ConfigureAwait(false);
            await UpdateTriggerActionsAsync(parsed).ConfigureAwait(false);

            return parsed;
        }

        private void UpdateTriggerActions(List<NotificationTrigger> triggers)
        {
            //Group all actions from all triggers together based on their object ID
            var actions = ResponseParser.GroupTriggerActions(triggers);

            //Retrieve the XML required to construct "proper" notification actions for all unique actions
            //specified in the triggers
            var actionParameters = new NotificationActionParameters(actions.Select(a => a.Key).ToArray());
            var normalActions = new Lazy<XDocument>(() => ObjectEngine.GetObjectsXml(actionParameters));

            foreach (var group in actions)
            {
                //As soon as a notification with a specified ID is accessed on any one of the triggers, retrieve
                //the "supported" properties of ALL of the notification actions, and then retrieve the "unsupported"
                //properties of JUST the notification action object ID that was accessed.
                var lazyAction = new Lazy<XDocument>(() => RequestParser.ExtractActionXml(normalActions.Value, GetNotificationActionProperties(group.Key), @group.Key));

                Logger.Log("Setting lazy action to retrieve notification actions");

                foreach (var action in group)
                {
                    action.LazyXml = lazyAction;

                    Logger.Log("Setting lazy action to retrieve notification schedule");

                    action.schedule = new Lazy<Schedule>(
                        () =>
                        {
                            if (action.lazyScheduleStr != null && PrtgObject.GetId(action.lazyScheduleStr) != -1)
                            {
                                Logger.Log($"Resolving schedule {action.lazyScheduleStr} to schedule");

                                return GetSchedule(new Schedule(action.lazyScheduleStr).Id);
                            }

                            return new Schedule(action.lazyScheduleStr);
                        });
                }
            }
        }

        private async Task UpdateTriggerActionsAsync(List<NotificationTrigger> triggers)
        {
            var actions = ResponseParser.GroupTriggerActions(triggers);

            var parameters = new NotificationActionParameters(actions.Select(a => a.Key).ToArray());

            var tasks = actions.Select(g => GetNotificationActionPropertiesAsync(g.Key));
            var normal = await ObjectEngine.GetObjectsXmlAsync(parameters).ConfigureAwait(false);

            //All the properties of all desired notifications
            var results = await Task.WhenAll(tasks).ConfigureAwait(false);

            //For each different notification action
            for (int i = 0; i < actions.Count; i++)
            {
                var xDoc = RequestParser.ExtractActionXml(normal, results[i], actions[i].Key);

                //Foreach notification action with the same ID
                foreach (var action in actions[i])
                {
                    action.LazyXml = new Lazy<XDocument>(() => xDoc);
                }
            }

            var list = ResponseParser.GroupActionSchedules(actions.SelectMany(g => g).ToList()).ToList();

            List<Schedule> schedules = new List<Schedule>();

            if(list.Count > 0)
                schedules = await GetSchedulesAsync(Property.Id, list.Select(l => l.Key).ToArray()).ConfigureAwait(false);

            foreach (var group in actions)
            {
                foreach (var action in group)
                {
                    if (action.lazyScheduleStr != null)
                    {
                        var id = PrtgObject.GetId(action.lazyScheduleStr);

                        if (id != -1)
                            action.schedule = new Lazy<Schedule>(() => schedules.First(s => s.Id == id));
                        else
                            action.schedule = new Lazy<Schedule>(() => new Schedule(action.lazyScheduleStr));
                    }
                    else
                        action.schedule = new Lazy<Schedule>(() => new Schedule(action.lazyScheduleStr));
                }
            }
        }

        /// <summary>
        /// Retrieves all notification trigger types supported by a PRTG Object.
        /// </summary>
        /// <param name="objectId">The object to retrieve supported trigger types for.</param>
        /// <returns>The trigger types supported by the object.</returns>
        public List<TriggerType> GetNotificationTriggerTypes(int objectId) =>
            GetNotificationTriggerData(objectId).SupportedTypes.ToList();

        /// <summary>
        /// Asynchronously retrieves all notification trigger types supported by a PRTG Object.
        /// </summary>
        /// <param name="objectId">The object to retrieve supported trigger types for.</param>
        /// <returns>The trigger types supported by the object.</returns>
        public async Task<List<TriggerType>> GetNotificationTriggerTypesAsync(int objectId) =>
            (await GetNotificationTriggerDataAsync(objectId).ConfigureAwait(false)).SupportedTypes.ToList();

        private NotificationTriggerData GetNotificationTriggerData(int objectId) =>
            ObjectEngine.GetObject<NotificationTriggerData>(
                new NotificationTriggerDataParameters(objectId),
                ParseNotificationTriggerTypes
            );

        private async Task<NotificationTriggerData> GetNotificationTriggerDataAsync(int objectId) =>
            await ObjectEngine.GetObjectAsync<NotificationTriggerData>(
                new NotificationTriggerDataParameters(objectId),
                ParseNotificationTriggerTypesAsync
            ).ConfigureAwait(false);

        #endregion
        #region Schedules

        /// <summary>
        /// Retrieves a monitoring schedule with a specified ID from a PRTG Server. If the schedule does not exist or an ambiguous match is found, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="id">The ID of the schedule to retrieve.</param>
        /// <exception cref="InvalidOperationException">The specified schedule does not exist or multiple schedules were resolved with the specified ID.</exception> 
        /// <returns>The schedule with the specified ID.</returns>
        public Schedule GetSchedule(int id) => GetSchedules(Property.Id, id).SingleObject(id);

        /// <summary>
        /// Asynchronously retrieves a monitoring schedule with a specified ID from a PRTG Server. If the schedule does not exist or an ambiguous match is found, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="id">The ID of the schedule to retrieve.</param>
        /// <exception cref="InvalidOperationException">The specified schedule does not exist or multiple schedulea were resolved with the specified ID.</exception> 
        /// <returns>The schedule with the specified ID.</returns>
        public async Task<Schedule> GetScheduleAsync(int id) => (await GetSchedulesAsync(Property.Id, id).ConfigureAwait(false)).SingleObject(id);

        /// <summary>
        /// Retrieves a monitoring schedule with a specified name from a PRTG Server. If the schedule does not exist or an ambiguous match is found, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="name">The name of the schedule to retrieve.</param>
        /// <exception cref="InvalidOperationException">The specified schedule does not exist or multiple schedules were resolved with the specified name.</exception> 
        /// <returns>The schedule with the specified name.</returns>
        public Schedule GetSchedule(string name) => GetSchedules(Property.Name, name).SingleObject(name, "Name");

        /// <summary>
        /// Asynchronously retrieves a monitoring schedule with a specified name from a PRTG Server. If the schedule does not exist or an ambiguous match is found, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="name">The name of the schedule to retrieve.</param>
        /// <exception cref="InvalidOperationException">The specified schedule does not exist or multiple schedules were resolved with the specified name.</exception> 
        /// <returns>The schedule with the specified name.</returns>
        public async Task<Schedule> GetScheduleAsync(string name) => (await GetSchedulesAsync(Property.Name, name).ConfigureAwait(false)).SingleObject(name, "Name");

        /// <summary>
        /// Retrieves all monitoring schedules from a PRTG Server.
        /// </summary>
        /// <returns>A list of monitoring schedules supported by a PRTG Server.</returns>
        public List<Schedule> GetSchedules() => GetSchedulesInternal(new ScheduleParameters());

        /// <summary>
        /// Retrieves all monitoring schedules from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns>All monitoring schedules whose value matched the specified property.</returns>
        public List<Schedule> GetSchedules(Property property, object value) =>
            GetSchedules(new SearchFilter(property, value));

        /// <summary>
        /// Retrieves all monitoring schedules from a PRTG Server, filtering for objects by one or more conditions.
        /// </summary>
        /// <param name="filters">One or more filters used to limit search results.</param>
        /// <returns>A list of schedules that match the specified search criteria.</returns>
        public List<Schedule> GetSchedules(params SearchFilter[] filters) =>
            GetSchedulesInternal(new ScheduleParameters(filters));

        /// <summary>
        /// Asynchronously retrieves all monitoring schedules from a PRTG Server.
        /// </summary>
        /// <returns>A list of monitoring schedules supported by a PRTG Server.</returns>
        public async Task<List<Schedule>> GetSchedulesAsync() =>
            await GetSchedulesInternalAsync(new ScheduleParameters()).ConfigureAwait(false);

        /// <summary>
        /// Asynchronously retrieves all monitoring schedules from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns>All monitoring schedules whose value matched the specified property.</returns>
        public async Task<List<Schedule>> GetSchedulesAsync(Property property, object value) =>
            await GetSchedulesAsync(new SearchFilter(property, value)).ConfigureAwait(false);

        /// <summary>
        /// Asynchronously retrieves all monitoring schedules from a PRTG Server, filtering for objects by one or more conditions.
        /// </summary>
        /// <param name="filters">One or more filters used to limit search results.</param>
        /// <returns>A list of schedules that match the specified search criteria.</returns>
        public async Task<List<Schedule>> GetSchedulesAsync(params SearchFilter[] filters) =>
            await GetSchedulesInternalAsync(new ScheduleParameters(filters)).ConfigureAwait(false);

        #endregion
    #endregion
    #region Object Manipulation
        #region Add Objects

        /// <summary>
        /// Adds a new sensor to a PRTG device. Based on the specified sensor parameters, multiple new sensors may be created.
        /// </summary>
        /// <param name="deviceId">The ID of the device the sensor will apply to.</param>
        /// <param name="parameters">A set of parameters describing the properties of the sensor to create.
        /// Depending on the type of sensor parameters specified, this may result in the creation of several new sensors.</param>
        /// <param name="resolve">Whether to resolve the new sensors to their resultant <see cref="Sensor"/> objects.
        /// If this value is false, this method will return null.</param>
        /// <returns>If <paramref name="resolve"/> is true, all new sensors that were created from the sensor <paramref name="parameters"/>. Otherwise, null.</returns>
        public List<Sensor> AddSensor(int deviceId, NewSensorParameters parameters, bool resolve = true) =>
            AddObject(deviceId, parameters, GetSensors, resolve, allowMultiple: true);

        /// <summary>
        /// Asynchronously adds a new sensor to a PRTG device. Based on the specified sensor parameters, multiple new sensors may be created.
        /// </summary>
        /// <param name="deviceId">The ID of the device the sensor will apply to.</param>
        /// <param name="parameters">A set of parameters describing the properties of the sensor to create.
        /// Depending on the type of sensor parameters specified, this may result in the creation of several new sensors.</param>
        /// <param name="resolve">Whether to resolve the new sensors to their resultant <see cref="Sensor"/> objects.
        /// If this value is false, this method will return null.</param>
        /// <returns>If <paramref name="resolve"/> is true, all new sensors that were created from the sensor <paramref name="parameters"/>. Otherwise, null.</returns>
        public async Task<List<Sensor>> AddSensorAsync(int deviceId, NewSensorParameters parameters, bool resolve = true) =>
            await AddObjectAsync(deviceId, parameters, GetSensorsAsync, resolve, allowMultiple: true).ConfigureAwait(false);

        /// <summary>
        /// Adds a new device to a PRTG group or probe.
        /// </summary>
        /// <param name="parentId">The ID of the group or probe the device will apply to.</param>
        /// <param name="name">The name to use for the new device.</param>
        /// <param name="host">The hostname or IP Address PRTG should use to communicate with the device. If this value is null, the <paramref name="name"/> will be used.</param>
        /// <param name="discoveryMode">Whether an auto-discovery should be automatically performed after device creation.</param>
        /// <param name="resolve">Whether to resolve the new device to its resultant <see cref="Device"/> object.
        /// If this value is false, this method will return null.</param>
        /// <returns>If <paramref name="resolve"/> is true, the device that was created from this method's device parameters. Otherwise, null.</returns>
        public Device AddDevice(int parentId, string name, string host = null, AutoDiscoveryMode discoveryMode = AutoDiscoveryMode.Manual, bool resolve = true) =>
                AddDevice(parentId, new NewDeviceParameters(name, host) { AutoDiscoveryMode = discoveryMode }, resolve);

        /// <summary>
        /// Adds a new device to a PRTG group or probe with a complex set of parameters.
        /// </summary>
        /// <param name="parentId">The ID of the group or probe the device will apply to.</param>
        /// <param name="parameters">A set of parameters describing the properties of the device to create.</param>
        /// <param name="resolve">Whether to resolve the new device to its resultant <see cref="Device"/> object.
        /// If this value is false, this method will return null.</param>
        /// <returns>If <paramref name="resolve"/> is true, the device that was created from this method's device <paramref name="parameters"/>. Otherwise, null.</returns>
        public Device AddDevice(int parentId, NewDeviceParameters parameters, bool resolve = true) =>
            AddObject(parentId, parameters, GetDevices, resolve)?.Single();

        /// <summary>
        /// Asynchronously adds a new device to a PRTG group or probe.
        /// </summary>
        /// <param name="parentId">The ID of the group or probe the device will apply to.</param>
        /// <param name="name">The name to use for the new device.</param>
        /// <param name="host">The hostname or IP Address PRTG should use to communicate with the device. If this value is null, the <paramref name="name"/> will be used.</param>
        /// <param name="discoveryMode">Whether an auto-discovery should be automatically performed after device creation.</param>
        /// <param name="resolve">Whether to resolve the new device to its resultant <see cref="Device"/> object.
        /// If this value is false, this method will return null.</param>
        /// <returns>If <paramref name="resolve"/> is true, the device that was created from this method's device parameters. Otherwise, null.</returns>
        public async Task<Device> AddDeviceAsync(int parentId, string name, string host = null, AutoDiscoveryMode discoveryMode = AutoDiscoveryMode.Manual, bool resolve = true) =>
                await AddDeviceAsync(parentId, new NewDeviceParameters(name, host) { AutoDiscoveryMode = discoveryMode }, resolve).ConfigureAwait(false);

        /// <summary>
        /// Asynchronously adds a new device to a PRTG group or probe with a complex set of parameters.
        /// </summary>
        /// <param name="parentId">The ID of the group or device the device will apply to.</param>
        /// <param name="parameters">A set of parameters describing the properties of the device to create.</param>
        /// <param name="resolve">Whether to resolve the new device to its resultant <see cref="Device"/> object.
        /// If this value is false, this method will return null.</param>
        /// <returns>If <paramref name="resolve"/> is true, the device that was created from this method's device <paramref name="parameters"/>. Otherwise, null.</returns>
        public async Task<Device> AddDeviceAsync(int parentId, NewDeviceParameters parameters, bool resolve = true) =>
            (await AddObjectAsync(parentId, parameters, GetDevicesAsync, resolve).ConfigureAwait(false))?.Single();

        /// <summary>
        /// Adds a new group to a PRTG group or probe.
        /// </summary>
        /// <param name="parentId">The ID of the group or probe the group will apply to.</param>
        /// <param name="name">The name to use for the new group.</param>
        /// <param name="resolve">Whether to resolve the new group to its resultant <see cref="Group"/> object.
        /// If this value is false, this method will return null.</param>
        /// <returns>If <paramref name="resolve"/> is true, the group that was created from this method's group parameters. Otherwise, null.</returns>
        public Group AddGroup(int parentId, string name, bool resolve = true) =>
            AddGroup(parentId, new NewGroupParameters(name), resolve);

        /// <summary>
        /// Adds a new group to a PRTG group or probe with a complex set of parameters.
        /// </summary>
        /// <param name="parentId">The ID of the group or probe the group will apply to.</param>
        /// <param name="parameters">A set of parameters describing the properties of the group to create.</param>
        /// <param name="resolve">Whether to resolve the new group to its resultant <see cref="Group"/> object.
        /// If this value is false, this method will return null.</param>
        /// <returns>If <paramref name="resolve"/> is true, the group that was created from this method's group <paramref name="parameters"/>. Otherwise, null.</returns>
        public Group AddGroup(int parentId, NewGroupParameters parameters, bool resolve = true) =>
            AddObject(parentId, parameters, GetGroups, resolve)?.Single();

        /// <summary>
        /// Asynchronously adds a new group to a PRTG group or probe.
        /// </summary>
        /// <param name="parentId">The ID of the group or probe the group will apply to.</param>
        /// <param name="name">The name to use for the new group.</param>
        /// <param name="resolve">Whether to resolve the new group to its resultant <see cref="Group"/> object.
        /// If this value is false, this method will return null.</param>
        /// <returns>If <paramref name="resolve"/> is true, the group that was created from this method's group parameters. Otherwise, null.</returns>
        public async Task<Group> AddGroupAsync(int parentId, string name, bool resolve = true) =>
            await AddGroupAsync(parentId, new NewGroupParameters(name), resolve).ConfigureAwait(false);

        /// <summary>
        /// Asynchronously adds a new group to a PRTG group or probe with a complex set of parameters.
        /// </summary>
        /// <param name="parentId">The ID of the group or probe the group will apply to.</param>
        /// <param name="parameters">A set of parameters describing the properties of the group to create.</param>
        /// <param name="resolve">Whether to resolve the new group to its resultant <see cref="Group"/> object.
        /// If this value is false, this method will return null.</param>
        /// <returns>If <paramref name="resolve"/> is true, the group that was created from this method's group <paramref name="parameters"/>. Otherwise, null.</returns>
        public async Task<Group> AddGroupAsync(int parentId, NewGroupParameters parameters, bool resolve = true) =>
            (await AddObjectAsync(parentId, parameters, GetGroupsAsync, resolve).ConfigureAwait(false))?.Single();

        /// <summary>
        /// Creates a set of dynamic sensor parameters for creating a new sensor of a specified type.
        /// </summary>
        /// <param name="deviceId">The ID of a device that supports the specified sensor type.</param>
        /// <param name="sensorType">The type of sensor to create sensor paramters for.<para/>
        /// Note: sensor parameters cannot be created for types that require additional information
        /// to be added before interrogating the target device.</param>
        /// <param name="progressCallback">A callback function used to monitor the progress of the request. If this function returns false, the request is aborted and this method returns null.</param>
        /// <returns>A dynamic set of sensor parameters that store the the parameters required to create a sensor of a specified type.</returns>
        public DynamicSensorParameters GetDynamicSensorParameters(int deviceId, string sensorType, Func<int, bool> progressCallback = null) =>
            new DynamicSensorParameters(GetSensorTargetsResponse(deviceId, sensorType, progressCallback), sensorType);

        /// <summary>
        /// Asynchronously creates a set of dynamic sensor parameters for creating a new sensor of a specified type.
        /// </summary>
        /// <param name="deviceId">The ID of a device that supports the specified sensor type.</param>
        /// <param name="sensorType">The type of sensor to create sensor paramters for.<para/>
        /// Note: sensor parameters cannot be created for types that require additional information
        /// to be added before interrogating the target device.</param>
        /// <param name="progressCallback">A callback function used to monitor the progress of the request. If this function returns false, the request is aborted and this method returns null.</param>
        /// <returns>A dynamic set of sensor parameters that store the the parameters required to create a sensor of a specified type.</returns>
        public async Task<DynamicSensorParameters> GetDynamicSensorParametersAsync(int deviceId, string sensorType, Func<int, bool> progressCallback = null) =>
            new DynamicSensorParameters(await GetSensorTargetsResponseAsync(deviceId, sensorType, progressCallback).ConfigureAwait(false), sensorType);

        #endregion
        #region Sensor State
            #region Acknowledge

        /// <summary>
        /// Marks a <see cref="Status.Down"/> sensor as <see cref="Status.DownAcknowledged"/>. If an acknowledged sensor returns to <see cref="Status.Up"/>, it will not be acknowledged when it goes down again.
        /// </summary>
        /// <param name="objectId">ID of the sensor to acknowledge.</param>
        /// <param name="duration">Duration (in minutes) to acknowledge the sensor for. If null, sensor will be acknowledged indefinitely.</param>
        /// <param name="message">Message to display on the acknowledged sensor.</param>
        public void AcknowledgeSensor(int objectId, int? duration = null, string message = null) =>
            AcknowledgeSensor(new[] {objectId}, duration, message);

        /// <summary>
        /// Marks one or more <see cref="Status.Down"/> sensors as <see cref="Status.DownAcknowledged"/>. If an acknowledged sensor returns to <see cref="Status.Up"/>, it will not be acknowledged when it goes down again.
        /// </summary>
        /// <param name="objectIds">IDs of the sensors to acknowledge.</param>
        /// <param name="duration">Duration (in minutes) to acknowledge the sensors for. If null, sensors will be acknowledged indefinitely.</param>
        /// <param name="message">Message to display on the acknowledged sensors.</param>
        public void AcknowledgeSensor(int[] objectIds, int? duration = null, string message = null) =>
            RequestEngine.ExecuteRequest(new AcknowledgeSensorParameters(objectIds, duration, message));

        /// <summary>
        /// Asynchronously marks a <see cref="Status.Down"/> sensor as <see cref="Status.DownAcknowledged"/>. If an acknowledged sensor returns to <see cref="Status.Up"/>, it will not be acknowledged when it goes down again.
        /// </summary>
        /// <param name="objectId">ID of the sensor to acknowledge.</param>
        /// <param name="duration">Duration (in minutes) to acknowledge the sensor for. If null, sensor will be paused indefinitely.</param>
        /// <param name="message">Message to display on the acknowledged sensor.</param>
        public async Task AcknowledgeSensorAsync(int objectId, int? duration = null, string message = null) =>
            await AcknowledgeSensorAsync(new[] {objectId}, duration, message).ConfigureAwait(false);

        /// <summary>
        /// Asynchronously marks one or more <see cref="Status.Down"/> sensors as <see cref="Status.DownAcknowledged"/>. If an acknowledged sensor returns to <see cref="Status.Up"/>, it will not be acknowledged when it goes down again.
        /// </summary>
        /// <param name="objectIds">IDs of the sensors to acknowledge.</param>
        /// <param name="duration">Duration (in minutes) to acknowledge the sensors for. If null, sensors will be acknowledged indefinitely.</param>
        /// <param name="message">Message to display on the acknowledged sensors.</param>
        public async Task AcknowledgeSensorAsync(int[] objectIds, int? duration = null, string message = null) =>
            await RequestEngine.ExecuteRequestAsync(new AcknowledgeSensorParameters(objectIds, duration, message)).ConfigureAwait(false);

            #endregion
            #region Pause

        /// <summary>
        /// Pauses monitoring on a PRTG Object and all child objects.
        /// </summary>
        /// <param name="objectId">ID of the object to pause.</param>
        /// <param name="durationMinutes">Duration (in minutes) to pause the object for. If null, object will be paused indefinitely.</param>
        /// <param name="pauseMessage">Message to display on the paused object.</param>
        public void PauseObject(int objectId, int? durationMinutes = null, string pauseMessage = null) =>
            PauseObject(new[] {objectId}, durationMinutes, pauseMessage);

        /// <summary>
        /// Pauses monitoring on one or more PRTG Objects and all child objects.
        /// </summary>
        /// <param name="objectIds">IDs of the objects to pause.</param>
        /// <param name="durationMinutes">Duration (in minutes) to pause the object for. If null, object will be paused indefinitely.</param>
        /// <param name="pauseMessage">Message to display on the paused objects.</param>
        public void PauseObject(int[] objectIds, int? durationMinutes = null, string pauseMessage = null) =>
            RequestEngine.ExecuteRequest(new PauseParameters(objectIds, durationMinutes, pauseMessage));

        /// <summary>
        /// Asynchronously pauses monitoring on a PRTG Object and all child objects.
        /// </summary>
        /// <param name="objectId">ID of the object to pause.</param>
        /// <param name="durationMinutes">Duration (in minutes) to pause the object for. If null, object will be paused indefinitely.</param>
        /// <param name="pauseMessage">Message to display on the paused object.</param>
        public async Task PauseObjectAsync(int objectId, int? durationMinutes = null, string pauseMessage = null) =>
            await PauseObjectAsync(new[] {objectId}, durationMinutes, pauseMessage).ConfigureAwait(false);

        /// <summary>
        /// Asynchronously pauses monitoring on one or more PRTG Objects and all child objects.
        /// </summary>
        /// <param name="objectIds">IDs of the objects to pause.</param>
        /// <param name="durationMinutes">Duration (in minutes) to pause the object for. If null, object will be paused indefinitely.</param>
        /// <param name="pauseMessage">Message to display on the paused objects.</param>
        public async Task PauseObjectAsync(int[] objectIds, int? durationMinutes = null, string pauseMessage = null) =>
            await RequestEngine.ExecuteRequestAsync(new PauseParameters(objectIds, durationMinutes, pauseMessage)).ConfigureAwait(false);

            #endregion
            #region Resume

        /// <summary>
        /// Resumes monitoring on one or more PRTG Objects (including sensors, devices, groups and probes) from a Paused or Simulated Error state.
        /// </summary>
        /// <param name="objectId">IDs of the objects to resume.</param>
        public void ResumeObject(params int[] objectId) =>
            RequestEngine.ExecuteRequest(new PauseParameters(objectId, PauseAction.Resume));

        /// <summary>
        /// Asynchronously resumes monitoring on one or more PRTG Objects (including sensors, devices, groups and probes) from a Paused or Simulated Error state.
        /// </summary>
        /// <param name="objectId">ID of the object to resume.</param>
        public async Task ResumeObjectAsync(params int[] objectId) =>
            await RequestEngine.ExecuteRequestAsync(new PauseParameters(objectId, PauseAction.Resume)).ConfigureAwait(false);

            #endregion
            #region Resume

        /// <summary>
        /// Simulates a <see cref="Status.Down"/> state for one or more sensors.
        /// </summary>
        /// <param name="sensorIds">IDs of the sensors to simulate an error for.</param>
        public void SimulateError(params int[] sensorIds) => RequestEngine.ExecuteRequest(new SimulateErrorParameters(sensorIds));

        /// <summary>
        /// Asynchronously simulates a <see cref="Status.Down"/> state for one or more sensors.
        /// </summary>
        /// <param name="sensorIds">IDs of the sensors to simulate an error for.</param>
        public async Task SimulateErrorAsync(params int[] sensorIds) => await RequestEngine.ExecuteRequestAsync(new SimulateErrorParameters(sensorIds)).ConfigureAwait(false);

            #endregion
        #endregion
        #region Notifications

        /// <summary>
        /// Adds a notification trigger to an object specified by a set of trigger parameters.
        /// </summary>
        /// <param name="parameters">A set of parameters describing the type of notification trigger to create and the object to apply it to.</param>
        /// <param name="resolve">Whether to resolve the new trigger to its resultant <see cref="NotificationTrigger"/> object.
        /// If this value is false, this method will return null.</param>
        /// <returns>If <paramref name="resolve"/> is true, the trigger that was created from this method's trigger parameters. Otherwise, null.</returns>
        public NotificationTrigger AddNotificationTrigger(TriggerParameters parameters, bool resolve = true) =>
            AddNotificationTriggerInternal(parameters, resolve)?.Single();

        /// <summary>
        /// Asynchronously adds a notification trigger to an object specified by a set of trigger parameters.
        /// </summary>
        /// <param name="parameters">A set of parameters describing the type of notification trigger to create and the object to apply it to.</param>
        /// <param name="resolve">Whether to resolve the new trigger to its resultant <see cref="NotificationTrigger"/> object.
        /// If this value is false, this method will return null.</param>
        /// <returns>If <paramref name="resolve"/> is true, the trigger that was created from this method's trigger parameters. Otherwise, null.</returns>
        public async Task<NotificationTrigger> AddNotificationTriggerAsync(TriggerParameters parameters, bool resolve = true) =>
            (await AddNotificationTriggerInternalAsync(parameters, resolve).ConfigureAwait(false))?.Single();

        /// <summary>
        /// Adds or edits a notification trigger on an object specified by a set of trigger parameters.
        /// </summary>
        /// <param name="parameters">A set of parameters describing the type of notification trigger and how to manipulate it.</param>
        public void SetNotificationTrigger(TriggerParameters parameters)
        {
            ValidateTriggerParameters(parameters);

            RequestEngine.ExecuteRequest(parameters);
        }

        /// <summary>
        /// Asynchronously adds or edits a notification trigger on an object specified by a set of trigger parameters.
        /// </summary>
        /// <param name="parameters">A set of parameters describing the type of notification trigger and how to manipulate it.</param>
        public async Task SetNotificationTriggerAsync(TriggerParameters parameters)
        {
            await ValidateTriggerParametersAsync(parameters).ConfigureAwait(false);

            await RequestEngine.ExecuteRequestAsync(parameters).ConfigureAwait(false);
        }

        /// <summary>
        /// Removes a notification trigger from an object. Triggers can only be removed from their parent objects, and cannot be removed from objects that have inherited them.
        /// </summary>
        /// <param name="trigger">The notification trigger to remove.</param>
        /// <exception cref="InvalidOperationException">The <paramref name="trigger"/> was inherited from another object.</exception>
        public void RemoveNotificationTrigger(NotificationTrigger trigger) =>
            RequestEngine.ExecuteRequest(new RemoveTriggerParameters(trigger));

        /// <summary>
        /// Asynchronously removes a notification trigger from an object. Triggers can only be removed from their parent objects, and cannot be removed from objects that have inherited them.
        /// </summary>
        /// <param name="trigger">The notification trigger to remove.</param>
        /// <exception cref="InvalidOperationException">The <paramref name="trigger"/> was inherited from another object.</exception>
        public async Task RemoveNotificationTriggerAsync(NotificationTrigger trigger) =>
            await RequestEngine.ExecuteRequestAsync(new RemoveTriggerParameters(trigger)).ConfigureAwait(false);

        #endregion
        #region Clone Object

        /// <summary>
        /// Clones a sensor or group to another device or group.
        /// </summary>
        /// <param name="sourceObjectId">The ID of a sensor or group to clone.</param>
        /// <param name="cloneName">The name that should be given to the cloned object.</param>
        /// <param name="targetLocationObjectId">If this is a sensor, the ID of the device to clone to. If this is a group, the ID of the group to clone to.</param>
        /// <returns>The ID of the object that was created</returns>
        public int CloneObject(int sourceObjectId, string cloneName, int targetLocationObjectId) =>
            CloneObject(new CloneParameters(sourceObjectId, cloneName, targetLocationObjectId));

        /// <summary>
        /// Clones a device to another group or probe.
        /// </summary>
        /// <param name="deviceId">The ID of the device to clone.</param>
        /// <param name="cloneName">The name that should be given to the cloned device.</param>
        /// <param name="host">The hostname or IP Address that should be assigned to the new device.</param>
        /// <param name="targetLocationObjectId">The group or probe the device should be cloned to.</param>
        public int CloneObject(int deviceId, string cloneName, string host, int targetLocationObjectId) =>
            CloneObject(new CloneParameters(deviceId, cloneName, targetLocationObjectId, host));

        private int CloneObject(CloneParameters parameters) =>
            ResponseParser.Amend(RequestEngine.ExecuteRequest(parameters, ResponseParser.CloneRequestParser), ResponseParser.CloneResponseParser);

        /// <summary>
        /// Asynchronously clones a sensor or group to another device or group.
        /// </summary>
        /// <param name="sourceObjectId">The ID of a sensor or group to clone.</param>
        /// <param name="cloneName">The name that should be given to the cloned object.</param>
        /// <param name="targetLocationObjectId">If this is a sensor, the ID of the device to clone to. If this is a group, the ID of the group to clone to.</param>
        /// <returns>The ID of the object that was created</returns>
        public async Task<int> CloneObjectAsync(int sourceObjectId, string cloneName, int targetLocationObjectId) =>
            await CloneObjectAsync(new CloneParameters(sourceObjectId, cloneName, targetLocationObjectId)).ConfigureAwait(false);

        /// <summary>
        /// Asynchronously clones a device to another group or probe.
        /// </summary>
        /// <param name="deviceId">The ID of the device to clone.</param>
        /// <param name="cloneName">The name that should be given to the cloned device.</param>
        /// <param name="host">The hostname or IP Address that should be assigned to the new device.</param>
        /// <param name="targetLocationObjectId">The group or probe the device should be cloned to.</param>
        public async Task<int> CloneObjectAsync(int deviceId, string cloneName, string host, int targetLocationObjectId) =>
            await CloneObjectAsync(new CloneParameters(deviceId, cloneName, targetLocationObjectId, host)).ConfigureAwait(false);

        private async Task<int> CloneObjectAsync(CloneParameters parameters) =>
            ResponseParser.Amend(
                await RequestEngine.ExecuteRequestAsync(
                    parameters,
                    async r => await Task.FromResult(ResponseParser.CloneRequestParser(r)).ConfigureAwait(false)
                ).ConfigureAwait(false), ResponseParser.CloneResponseParser
            );

        #endregion
        #region Get Object Properties
            #region Get Typed Properties

        /// <summary>
        /// Retrieves properties and settings of a PRTG Sensor.
        /// </summary>
        /// <param name="sensorId">ID of the sensor to retrieve settings for.</param>
        /// <returns>All settings of the specified sensor.</returns>
        public SensorSettings GetSensorProperties(int sensorId) =>
            GetObjectProperties<SensorSettings>(sensorId, ObjectType.Sensor);

        /// <summary>
        /// Asynchronously retrieves properties and settings of a PRTG Sensor.
        /// </summary>
        /// <param name="sensorId">ID of the sensor to retrieve settings for.</param>
        /// <returns>All settings of the specified sensor.</returns>
        public async Task<SensorSettings> GetSensorPropertiesAsync(int sensorId) =>
            await GetObjectPropertiesAsync<SensorSettings>(sensorId, ObjectType.Sensor).ConfigureAwait(false);

        /// <summary>
        /// Retrieves properties and settings of a PRTG Device.
        /// </summary>
        /// <param name="deviceId">ID of the device to retrieve settings for.</param>
        /// <returns>All settings of the specified device.</returns>
        public DeviceSettings GetDeviceProperties(int deviceId) =>
            GetObjectProperties<DeviceSettings>(deviceId, ObjectType.Device);

        /// <summary>
        /// Asynchronously retrieves properties and settings of a PRTG Device.
        /// </summary>
        /// <param name="deviceId">ID of the device to retrieve settings for.</param>
        /// <returns>All settings of the specified device.</returns>
        public async Task<DeviceSettings> GetDevicePropertiesAsync(int deviceId) =>
            await GetObjectPropertiesAsync<DeviceSettings>(deviceId, ObjectType.Device).ConfigureAwait(false);

        /// <summary>
        /// Retrieves properties and settings of a PRTG Group.
        /// </summary>
        /// <param name="groupId">ID of the group to retrieve settings for.</param>
        /// <returns>All settings of the specified group.</returns>
        public GroupSettings GetGroupProperties(int groupId) =>
            GetObjectProperties<GroupSettings>(groupId, ObjectType.Group);

        /// <summary>
        /// Asynchronously retrieves properties and settings of a PRTG Group.
        /// </summary>
        /// <param name="groupId">ID of the group to retrieve settings for.</param>
        /// <returns>All settings of the specified group.</returns>
        public async Task<GroupSettings> GetGroupPropertiesAsync(int groupId) =>
            await GetObjectPropertiesAsync<GroupSettings>(groupId, ObjectType.Group).ConfigureAwait(false);

        /// <summary>
        /// Retrieves properties and settings of a PRTG Probe.
        /// </summary>
        /// <param name="probeId">ID of the probe to retrieve settings for.</param>
        /// <returns>All settings of the specified probe.</returns>
        public ProbeSettings GetProbeProperties(int probeId) =>
            GetObjectProperties<ProbeSettings>(probeId, ObjectType.Probe);

        /// <summary>
        /// Asynchronously retrieves properties and settings of a PRTG Probe.
        /// </summary>
        /// <param name="probeId">ID of the probe to retrieve settings for.</param>
        /// <returns>All settings of the specified probe.</returns>
        public async Task<ProbeSettings> GetProbePropertiesAsync(int probeId) =>
            await GetObjectPropertiesAsync<ProbeSettings>(probeId, ObjectType.Probe).ConfigureAwait(false);

        private T GetObjectProperties<T>(int objectId, ObjectType objectType)
        {
            var response = GetObjectPropertiesRawInternal(objectId, objectType);

            var data = ResponseParser.GetObjectProperties<T>(response);

            if (data is TableSettings)
            {
                var table = (TableSettings) (object) data;

                if (table.scheduleStr == null || PrtgObject.GetId(table.scheduleStr) == -1)
                    table.schedule = new LazyValue<Schedule>(table.scheduleStr, () => new Schedule(table.scheduleStr));
                else
                {
                    table.schedule = new LazyValue<Schedule>(
                        table.scheduleStr,
                        () => GetSchedule(PrtgObject.GetId(table.scheduleStr))
                    );
                }
            }

            return data;
        }

        private async Task<T> GetObjectPropertiesAsync<T>(int objectId, ObjectType objectType)
        {
            var response = await GetObjectPropertiesRawInternalAsync(objectId, objectType).ConfigureAwait(false);

            var data = ResponseParser.GetObjectProperties<T>(response);

            if (data is TableSettings)
            {
                var table = (TableSettings)(object)data;

                if (table.scheduleStr == null || PrtgObject.GetId(table.scheduleStr) == -1)
                    table.schedule = new LazyValue<Schedule>(table.scheduleStr, () => new Schedule(table.scheduleStr));
                else
                {
                    var schedule = await GetScheduleAsync(PrtgObject.GetId(table.scheduleStr)).ConfigureAwait(false);

                    table.schedule = new LazyValue<Schedule>(table.scheduleStr, () => schedule);
                }
            }

            return data;
        }

            #endregion
            #region Get Multiple Raw Properties

        /// <summary>
        /// Retrieves all raw properties and settings of a PRTG Object. Note: objects may have additional properties
        /// that cannot be retrieved via this method.
        /// </summary>
        /// <param name="objectId">The ID of the object to retrieve settings and properties for.</param>
        /// <param name="objectType">The type of object to retrieve settings and properties for.</param>
        /// <returns>A dictionary mapping all discoverable properties to raw values.</returns>
        public Dictionary<string, string> GetObjectPropertiesRaw(int objectId, ObjectType objectType) =>
            GetObjectPropertiesRawDictionary(objectId, objectType);

        /// <summary>
        /// Retrieves all raw properties and settings of an unsupported object type. Note: objects may have additional properties
        /// that cannot be retrieved via this method.
        /// </summary>
        /// <param name="objectId">The ID of the object to retrieve settings and properties for.</param>
        /// <param name="objectType">The type of object to retrieve settings and properties for.
        /// If this value is null, PRTG will attempt to guess the object type based on the specified <paramref name="objectId"/>.</param>
        /// <returns>A dictionary mapping all discoverable properties to raw values.</returns>
        public Dictionary<string, string> GetObjectPropertiesRaw(int objectId, string objectType = null) =>
            GetObjectPropertiesRawDictionary(objectId, objectType);

        /// <summary>
        /// Asynchronously retrieves all raw properties and settings of a PRTG Object. Note: objects may have additional properties
        /// that cannot be retrieved via this method.
        /// </summary>
        /// <param name="objectId">The ID of the object to retrieve settings and properties for.</param>
        /// <param name="objectType">The type of object to retrieve settings and properties for.</param>
        /// <returns>A dictionary mapping all discoverable properties to raw values.</returns>
        public async Task<Dictionary<string, string>> GetObjectPropertiesRawAsync(int objectId, ObjectType objectType) =>
            await GetObjectPropertiesRawDictionaryAsync(objectId, objectType).ConfigureAwait(false);

        /// <summary>
        /// Asynchronously retrieves all raw properties and settings of an unsupported object type. Note: objects may have additional properties
        /// that cannot be retrieved via this method.
        /// </summary>
        /// <param name="objectId">The ID of the object to retrieve settings and properties for.</param>
        /// <param name="objectType">The type of object to retrieve settings and properties for.
        /// If this value is null, PRTG will attempt to guess the object type based on the specified <paramref name="objectId"/>.</param>
        /// <returns>A dictionary mapping all discoverable properties to raw values.</returns>
        public async Task<Dictionary<string, string>> GetObjectPropertiesRawAsync(int objectId, string objectType = null) =>
            await GetObjectPropertiesRawDictionaryAsync(objectId, objectType).ConfigureAwait(false);

        private Dictionary<string, string> GetObjectPropertiesRawDictionary(int objectId, object objectType) =>
            ObjectSettings.GetDictionary(GetObjectPropertiesRawInternal(objectId, objectType));

        private async Task<Dictionary<string, string>> GetObjectPropertiesRawDictionaryAsync(int objectId, object objectType) =>
            ObjectSettings.GetDictionary(await GetObjectPropertiesRawInternalAsync(objectId, objectType).ConfigureAwait(false));

        private string GetObjectPropertiesRawInternal(int objectId, object objectType) =>
            RequestEngine.ExecuteRequest(new GetObjectPropertyParameters(objectId, objectType));

        private async Task<string> GetObjectPropertiesRawInternalAsync(int objectId, object objectType) =>
            await RequestEngine.ExecuteRequestAsync(new GetObjectPropertyParameters(objectId, objectType)).ConfigureAwait(false);
            
        /// <summary>
        /// Retrieves a type safe property from a PRTG Server.
        /// </summary>
        /// <param name="objectId">The ID of the object to retrieve the property from.</param>
        /// <param name="property">The well known property to retrieve.</param>
        /// <returns>A type safe representation of the specified object.</returns>
        public object GetObjectProperty(int objectId, ObjectProperty property)
        {
            var rawName = BaseSetObjectPropertyParameters<ObjectProperty>.GetParameterName(property);

            var rawValue = GetObjectPropertyRaw(objectId, rawName);

            return XmlSerializer.DeserializeRawPropertyValue(property, rawName, rawValue);
        }

        /// <summary>
        /// Asynchronously retrieves a type safe property from a PRTG Server.
        /// </summary>
        /// <param name="objectId">The ID of the object to retrieve the property from.</param>
        /// <param name="property">The well known property to retrieve.</param>
        /// <returns>A type safe representation of the specified property.</returns>
        public async Task<object> GetObjectPropertyAsync(int objectId, ObjectProperty property)
        {
            var rawName = BaseSetObjectPropertyParameters<ObjectProperty>.GetParameterName(property);

            var rawValue = await GetObjectPropertyRawAsync(objectId, rawName).ConfigureAwait(false);

            return XmlSerializer.DeserializeRawPropertyValue(property, rawName, rawValue);
        }

        /// <summary>
        /// Retrieves a type safe property from a PRTG Server, cast to its actual type. If the object is not of the type specified,
        /// an <see cref="InvalidCastException"/> will be thrown.
        /// </summary>
        /// <typeparam name="T">The type to cast the object to. If the object</typeparam>
        /// <param name="objectId">The ID of the object to retrieve the property from.</param>
        /// <param name="property">The well known property to retrieve.</param>
        /// <exception cref="InvalidCastException"/>
        /// <returns>A type safe representation of the specified property, cast to its actual type.</returns>
        public T GetObjectProperty<T>(int objectId, ObjectProperty property) =>
            ResponseParser.GetTypedProperty<T>(GetObjectProperty(objectId, property));

        /// <summary>
        /// Asynchronously retrieves a type safe property from a PRTG Server, cast to its actual type. If the object is not of the type specified,
        /// an <see cref="InvalidCastException"/> will be thrown.
        /// </summary>
        /// <typeparam name="T">The type to cast the object to. If the object</typeparam>
        /// <param name="objectId">The ID of the object to retrieve the property from.</param>
        /// <param name="property">The well known property to retrieve.</param>
        /// <exception cref="InvalidCastException"/>
        /// <returns>A type safe representation of the specified property, cast to its actual type.</returns>
        public async Task<T> GetObjectPropertyAsync<T>(int objectId, ObjectProperty property) =>
            ResponseParser.GetTypedProperty<T>(await GetObjectPropertyAsync(objectId, property).ConfigureAwait(false));

            #endregion
            #region Get Single Raw Property

        /// <summary>
        /// Retrieves unsupported properties and settings of a PRTG Object.
        /// </summary>
        /// <param name="objectId">The ID of the object whose property should be retrieved.</param>
        /// <param name="property">The property of the object to retrieve. This can be typically discovered by inspecting the "name" attribute of the properties' &lt;input/&gt; tag on the Settings page of PRTG.<para/>
        /// If the properties name ends in an underscore, this must be included.</param>
        /// <returns>The raw value of the object's property.</returns>
        public string GetObjectPropertyRaw(int objectId, string property)
        {
            var parameters = new GetObjectPropertyRawParameters(objectId, property);

            var response = requestEngine.ExecuteRequest(RequestParser.GetGetObjectPropertyFunction(property), parameters);

            return ResponseParser.ValidateRawObjectProperty(response, parameters);
        }

        /// <summary>
        /// Asynchronously retrieves unsupported properties and settings of a PRTG Object.
        /// </summary>
        /// <param name="objectId">The ID of the object whose property should be retrieved.</param>
        /// <param name="property">The property of the object to retrieve. This can be typically discovered by inspecting the "name" attribute of the properties' &lt;input/&gt; tag on the Settings page of PRTG.<para/>
        /// If the properties name ends in an underscore, this must be included.</param>
        /// <returns>The raw value of the object's property.</returns>
        public async Task<string> GetObjectPropertyRawAsync(int objectId, string property)
        {
            var parameters = new GetObjectPropertyRawParameters(objectId, property);

            var response = await requestEngine.ExecuteRequestAsync(RequestParser.GetGetObjectPropertyFunction(property), parameters).ConfigureAwait(false);

            return ResponseParser.ValidateRawObjectProperty(response, parameters);
        }

            #endregion
        #endregion
        #region Set Object Properties
            #region Normal

        /// <summary>
        /// Modifies properties and settings of a PRTG Object.<para/>
        /// Each <see cref="ObjectProperty"/> corresponds with a Property of a type derived from <see cref="ObjectSettings"/>.<para/>
        /// If PrtgAPI cannot convert the specified value to the type required by the property, PrtgAPI will throw an exception indicating the type that was expected.
        /// </summary>
        /// <param name="objectId">The ID of the object whose properties should be modified.</param>
        /// <param name="property">The property of the object to modify.</param>
        /// <param name="value">The value to set the object's property to.</param>
        public void SetObjectProperty(int objectId, ObjectProperty property, object value) =>
            SetObjectProperty(new[] {objectId}, property, value);  

        /// <summary>
        /// Modifies properties and settings of one or more PRTG Objects.<para/>
        /// Each <see cref="ObjectProperty"/> corresponds with a Property of a type derived from <see cref="ObjectSettings"/>.<para/>
        /// If PrtgAPI cannot convert the specified value to the type required by the property, PrtgAPI will throw an exception indicating the type that was expected.
        /// </summary>
        /// <param name="objectIds">The IDs of the objects whose properties should be modified.</param>
        /// <param name="property">The property of each object to modify.</param>
        /// <param name="value">The value to set each object's property to.</param>
        public void SetObjectProperty(int[] objectIds, ObjectProperty property, object value) =>
            SetObjectProperty(objectIds, new PropertyParameter(property, value));

        /// <summary>
        /// Asynchronously modifies properties and settings of a PRTG Object.<para/>
        /// Each <see cref="ObjectProperty"/> corresponds with a Property of a type derived from <see cref="ObjectSettings"/>.<para/>
        /// If PrtgAPI cannot convert the specified value to the type required by the property, PrtgAPI will throw an exception indicating the type that was expected.
        /// </summary>
        /// <param name="objectId">The ID of the object whose properties should be modified.</param>
        /// <param name="property">The property of the object to modify.</param>
        /// <param name="value">The value to set the object's property to.</param>
        public async Task SetObjectPropertyAsync(int objectId, ObjectProperty property, object value) =>
            await SetObjectPropertyAsync(new[] {objectId}, property, value).ConfigureAwait(false);

        /// <summary>
        /// Asynchronously modifies properties and settings of one or more PRTG Objects.<para/>
        /// Each <see cref="ObjectProperty"/> corresponds with a Property of a type derived from <see cref="ObjectSettings"/>.<para/>
        /// If PrtgAPI cannot convert the specified value to the type required by the property, PrtgAPI will throw an exception indicating the type that was expected.
        /// </summary>
        /// <param name="objectIds">The IDs of the objects whose properties should be modified.</param>
        /// <param name="property">The property of each object to modify.</param>
        /// <param name="value">The value to set each object's property to.</param>
        public async Task SetObjectPropertyAsync(int[] objectIds, ObjectProperty property, object value) =>
            await SetObjectPropertyAsync(objectIds, new PropertyParameter(property, value)).ConfigureAwait(false);

            #endregion Normal
            #region Normal (Multiple

        /// <summary>
        /// Modifies multiple properties of a PRTG Object.<para/>
        /// Each <see cref="ObjectProperty"/> corresponds with a Property of a type derived from <see cref="ObjectSettings"/>.<para/>
        /// If PrtgAPI cannot convert the specified value to the type required by the property, PrtgAPI will throw an exception indicating the type that was expected.
        /// </summary>
        /// <param name="objectId">The ID of the object whose properties should be modified.</param>
        /// <param name="parameters">A set of parameters describing the properties and their values to process.</param>
        public void SetObjectProperty(int objectId, params PropertyParameter[] parameters) =>
            SetObjectProperty(new[] {objectId}, parameters);

        /// <summary>
        /// Modifies multiple properties of one or more PRTG Objects.<para/>
        /// Each <see cref="ObjectProperty"/> corresponds with a Property of a type derived from <see cref="ObjectSettings"/>.<para/>
        /// If PrtgAPI cannot convert the specified value to the type required by the property, PrtgAPI will throw an exception indicating the type that was expected.
        /// </summary>
        /// <param name="objectIds">The IDs of the objects whose properties should be modified.</param>
        /// <param name="parameters">A set of parameters describing the properties and their values to process.</param>
        public void SetObjectProperty(int[] objectIds, params PropertyParameter[] parameters) =>
            SetObjectProperty(CreateSetObjectPropertyParameters(objectIds, parameters), objectIds.Length);

        /// <summary>
        /// Asynchronously modifies multiple properties of a PRTG Object.<para/>
        /// Each <see cref="ObjectProperty"/> corresponds with a Property of a type derived from <see cref="ObjectSettings"/>.<para/>
        /// If PrtgAPI cannot convert the specified value to the type required by the property, PrtgAPI will throw an exception indicating the type that was expected.
        /// </summary>
        /// <param name="objectId">The ID of the object whose properties should be modified.</param>
        /// <param name="parameters">A set of parameters describing the properties and their values to process.</param>
        public async Task SetObjectPropertyAsync(int objectId, params PropertyParameter[] parameters) =>
            await SetObjectPropertyAsync(new[] { objectId }, parameters).ConfigureAwait(false);

        /// <summary>
        /// Asynchronously modifies multiple properties of one or more PRTG Objects.<para/>
        /// Each <see cref="ObjectProperty"/> corresponds with a Property of a type derived from <see cref="ObjectSettings"/>.<para/>
        /// If PrtgAPI cannot convert the specified value to the type required by the property, PrtgAPI will throw an exception indicating the type that was expected.
        /// </summary>
        /// <param name="objectIds">The IDs of the objects whose properties should be modified.</param>
        /// <param name="parameters">A set of parameters describing the properties and their values to process.</param>
        public async Task SetObjectPropertyAsync(int[] objectIds, params PropertyParameter[] parameters) =>
            await SetObjectPropertyAsync(await CreateSetObjectPropertyParametersAsync(objectIds, parameters).ConfigureAwait(false), objectIds.Length).ConfigureAwait(false);

            #endregion
            #region Channel

        /// <summary>
        /// Modifies channel properties for a PRTG Sensor.
        /// </summary>
        /// <param name="sensorId">The ID of the sensor whose channels should be modified.</param>
        /// <param name="channelId">The ID of the channel to modify.</param>
        /// <param name="property">The property of the channel to modify</param>
        /// <param name="value">The value to set the channel's property to.</param>
        public void SetObjectProperty(int sensorId, int channelId, ChannelProperty property, object value) =>
            SetObjectProperty(new[] {sensorId}, channelId, property, value);

        /// <summary>
        /// Modifies channel properties for one or more PRTG Sensors.
        /// </summary>
        /// <param name="sensorIds">The IDs of the sensors whose channels should be modified.</param>
        /// <param name="channelId">The ID of the channel of each sensor to modify.</param>
        /// <param name="property">The property of each channel to modify</param>
        /// <param name="value">The value to set each channel's property to.</param>
        public void SetObjectProperty(int[] sensorIds, int channelId, ChannelProperty property, object value) =>
            SetObjectProperty(sensorIds, channelId, new ChannelParameter(property, value));
        
        /// <summary>
        /// Asynchronously modifies channel properties for a PRTG Sensor.
        /// </summary>
        /// <param name="sensorId">The ID of the sensor whose channels should be modified.</param>
        /// <param name="channelId">The ID of the channel to modify.</param>
        /// <param name="property">The property of the channel to modify</param>
        /// <param name="value">The value to set the channel's property to.</param>
        public async Task SetObjectPropertyAsync(int sensorId, int channelId, ChannelProperty property, object value) =>
            await SetObjectPropertyAsync(new[] {sensorId}, channelId, property, value).ConfigureAwait(false);

        /// <summary>
        /// Asynchronously modifies channel properties for one or more PRTG Sensors.
        /// </summary>
        /// <param name="sensorIds">The IDs of the sensors whose channels should be modified.</param>
        /// <param name="channelId">The ID of the channel of each sensor to modify.</param>
        /// <param name="property">The property of each channel to modify</param>
        /// <param name="value">The value to set each channel's property to.</param>
        public async Task SetObjectPropertyAsync(int[] sensorIds, int channelId, ChannelProperty property, object value) =>
            await SetObjectPropertyAsync(sensorIds, channelId, new ChannelParameter(property, value)).ConfigureAwait(false);

            #endregion Channel
            #region Channel (Multiple)

        /// <summary>
        /// Modifies multiple channel properties for a PRTG Sensor.
        /// </summary>
        /// <param name="sensorId">The ID of the sensor whose channels should be modified.</param>
        /// <param name="channelId">The ID of the channel to modify.</param>
        /// <param name="parameters">A set of parameters describing the properties and their values to process.</param>
        public void SetObjectProperty(int sensorId, int channelId, params ChannelParameter[] parameters) =>
            SetObjectProperty(new[] { sensorId }, channelId, parameters);

        /// <summary>
        /// Modifies multiple channel properties for one or more PRTG Sensors.
        /// </summary>
        /// <param name="sensorIds">The IDs of the sensors whose channels should be modified.</param>
        /// <param name="channelId">The ID of the channel of each sensor to modify.</param>
        /// <param name="parameters">A set of parameters describing the properties and their values to process.</param>
        public void SetObjectProperty(int[] sensorIds, int channelId, params ChannelParameter[] parameters) =>
            GetVersionClient<ChannelParameter, ChannelProperty>(parameters.ToList()).SetChannelProperty(sensorIds, channelId, null, parameters);

        /// <summary>
        /// Asynchronously modifies multiple channel properties for a PRTG Sensor.
        /// </summary>
        /// <param name="sensorId">The ID of the sensor whose channels should be modified.</param>
        /// <param name="channelId">The ID of the channel to modify.</param>
        /// <param name="parameters">A set of parameters describing the properties and their values to process.</param>
        public async Task SetObjectPropertyAsync(int sensorId, int channelId, params ChannelParameter[] parameters) =>
            await SetObjectPropertyAsync(new[] { sensorId }, channelId, parameters).ConfigureAwait(false);

        /// <summary>
        /// Asynchronously modifies multiple channel properties for one or more PRTG Sensors.
        /// </summary>
        /// <param name="sensorIds">The IDs of the sensors whose channels should be modified.</param>
        /// <param name="channelId">The ID of the channel of each sensor to modify.</param>
        /// <param name="parameters">A set of parameters describing the properties and their values to process.</param>
        public async Task SetObjectPropertyAsync(int[] sensorIds, int channelId, params ChannelParameter[] parameters) =>
            await GetVersionClient<ChannelParameter, ChannelProperty>(parameters.ToList()).SetChannelPropertyAsync(sensorIds, channelId, null, parameters).ConfigureAwait(false);

            #endregion
            #region Custom

        /// <summary>
        /// Modifies unsupported properties and settings of an object.
        /// </summary>
        /// <param name="objectId">The ID of the object whose properties should be modified.</param>
        /// <param name="property">The property of the object to modify. This can be typically discovered by inspecting the "name" attribute of the properties' &lt;input/&gt; tag on the Settings page of PRTG.<para/>
        /// If the properties name ends in an underscore, this must be included.</param>
        /// <param name="value">The value to set the object's property to. For radio buttons and dropdown lists, this is the integer found in the 'value' attribute.</param>
        public void SetObjectPropertyRaw(int objectId, string property, string value) =>
            SetObjectPropertyRaw(new[] {objectId}, property, value);

        /// <summary>
        /// Modifies unsupported properties and settings of one or more PRTG Objects.
        /// </summary>
        /// <param name="objectIds">The IDs of the objects whose properties should be modified.</param>
        /// <param name="property">The property of each object to modify. This can be typically discovered by inspecting the "name" attribute of the properties' &lt;input/&gt; tag on the Settings page of PRTG.<para/>
        /// If the properties name ends in an underscore, this must be included.</param>
        /// <param name="value">The value to set each object's property to. For radio buttons and dropdown lists, this is the integer found in the 'value' attribute.</param>
        public void SetObjectPropertyRaw(int[] objectIds, string property, string value) =>
            SetObjectPropertyRaw(objectIds, new CustomParameter(property, value));

        /// <summary>
        /// Asynchronously modifies unsupported properties and settings of a PRTG Object.
        /// </summary>
        /// <param name="objectId">The ID of the object whose properties should be modified.</param>
        /// <param name="property">The property of the object to modify. This can be typically discovered by inspecting the "name" attribute of the properties' &lt;input/&gt; tag on the Settings page of PRTG.<para/>
        /// If the properties name ends in an underscore, this must be included.</param>
        /// <param name="value">The value to set the object's property to. For radio buttons and dropdown lists, this is the integer found in the 'value' attribute.</param>
        public async Task SetObjectPropertyRawAsync(int objectId, string property, string value) =>
            await SetObjectPropertyRawAsync(new[] {objectId}, property, value).ConfigureAwait(false);

        /// <summary>
        /// Asynchronously modifies unsupported properties and settings of one or more PRTG Objects.
        /// </summary>
        /// <param name="objectIds">The IDs of the objects whose properties should be modified.</param>
        /// <param name="property">The property of each object to modify. This can be typically discovered by inspecting the "name" attribute of the properties' &lt;input/&gt; tag on the Settings page of PRTG.<para/>
        /// If the properties name ends in an underscore, this must be included.</param>
        /// <param name="value">The value to set each object's property to. For radio buttons and dropdown lists, this is the integer found in the 'value' attribute.</param>
        public async Task SetObjectPropertyRawAsync(int[] objectIds, string property, string value) =>
            await SetObjectPropertyRawAsync(objectIds, new CustomParameter(property, value)).ConfigureAwait(false);

            #endregion
            #region Custom (Multiple)

        /// <summary>
        /// Modifies multiple unsupported properties of a PRTG Object.
        /// </summary>
        /// <param name="objectId">The ID of the object whose properties should be modified.</param>
        /// <param name="parameters">A set of parameters describing the properties and their values to process.</param>
        public void SetObjectPropertyRaw(int objectId, params CustomParameter[] parameters) =>
            SetObjectPropertyRaw(new[] { objectId }, parameters);

        /// <summary>
        /// Modifies multiple unsupported properties of one or more PRTG Objects.
        /// </summary>
        /// <param name="objectIds">The IDs of the objects whose properties should be modified.</param>
        /// <param name="parameters">A set of parameters describing the properties and their values to process.</param>
        public void SetObjectPropertyRaw(int[] objectIds, params CustomParameter[] parameters) =>
            SetObjectProperty(new SetObjectPropertyParameters(objectIds, parameters), objectIds.Length);

        /// <summary>
        /// Asynchronously modifies multiple unsupported properties of a PRTG Object.
        /// </summary>
        /// <param name="objectId">The ID of the object whose properties should be modified.</param>
        /// <param name="parameters">A set of parameters describing the properties and their values to process.</param>
        public async Task SetObjectPropertyRawAsync(int objectId, params CustomParameter[] parameters) =>
            await SetObjectPropertyRawAsync(new[] { objectId }, parameters).ConfigureAwait(false);

        /// <summary>
        /// Asynchronously modifies multiple unsupported properties of one or more PRTG Objects.
        /// </summary>
        /// <param name="objectIds">The IDs of the objects whose properties should be modified.</param>
        /// <param name="parameters">A set of parameters describing the properties and their values to process.</param>
        public async Task SetObjectPropertyRawAsync(int[] objectIds, params CustomParameter[] parameters) =>
            await SetObjectPropertyAsync(new SetObjectPropertyParameters(objectIds, parameters), objectIds.Length).ConfigureAwait(false);

            #endregion

        internal void SetObjectProperty<T>(BaseSetObjectPropertyParameters<T> parameters, int numObjectIds) =>
            RequestEngine.ExecuteRequest(parameters, m => ResponseParser.ParseSetObjectPropertyUrl(numObjectIds, m));

        internal async Task SetObjectPropertyAsync<T>(BaseSetObjectPropertyParameters<T> parameters, int numObjectIds) =>
            await RequestEngine.ExecuteRequestAsync(parameters, m => Task.FromResult(ResponseParser.ParseSetObjectPropertyUrl(numObjectIds, m))).ConfigureAwait(false);

        private SetObjectPropertyParameters CreateSetObjectPropertyParameters(int[] objectIds, params PropertyParameter[] @params)
        {
            foreach (var prop in @params)
            {
                var attrib = prop.Property.GetEnumAttribute<TypeAttribute>();

                if (attrib != null)
                {
                    try
                    {
                        var method = attrib.Class.GetMethod("Resolve", BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Static);

                        if (method != null)
                        {
                            prop.Value = method.Invoke(null, new[] { this, prop.Value });
                        }
                    }
                    catch (TargetInvocationException ex)
                    {
                        throw ex.InnerException;
                    }
                }
            }
            
            var parameters = new SetObjectPropertyParameters(objectIds, @params);

            return parameters;
        }

        private async Task<SetObjectPropertyParameters> CreateSetObjectPropertyParametersAsync(int[] objectIds, params PropertyParameter[] @params)
        {
            foreach (var prop in @params)
            {
                var attrib = prop.Property.GetEnumAttribute<TypeAttribute>();

                if (attrib != null)
                {
                    try
                    {
                        var method = attrib.Class.GetMethod("ResolveAsync", BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Static);

                        if (method != null)
                        {
                            var task = ((Task)method.Invoke(null, new[] { this, prop.Value }));

                            await task.ConfigureAwait(false);

                            prop.Value = task.GetType().GetProperty("Result").GetValue(task);
                        }
                    }
                    catch (TargetInvocationException ex)
                    {
                        throw ex.InnerException;
                    }
                }
            }

            var parameters = new SetObjectPropertyParameters(objectIds, @params);

            return parameters;
        }

        #endregion
        #region System Administration

        /// <summary>
        /// Requests PRTG generate a backup of the PRTG Configuration Database.<para/>
        /// When executed, this method will request PRTG store a backup of its configuration database under
        /// the Configuration Auto-Backups folder after first writing the current running configuration to disk.<para/>
        /// Depending on the size of your database, this may take several seconds to complete. Note that PRTG always creates
        /// its backup asynchronously; as such when this method returns the backup may not have fully completed.<para/>
        /// By default, configuration backups are stored under C:\ProgramData\Paessler\PRTG Network Monitor\Configuration Auto-Backups.
        /// </summary>
        public void BackupConfigDatabase() =>
            RequestEngine.ExecuteRequest(new CommandFunctionParameters(CommandFunction.SaveNow));

        /// <summary>
        /// Asynchronously requests PRTG generate a backup of the PRTG Configuration Database.<para/>
        /// When executed, this method will request PRTG store a backup of its configuration database under
        /// the Configuration Auto-Backups folder after first writing the current running configuration to disk.<para/>
        /// Depending on the size of your database, this may take several seconds to complete. Note that PRTG always creates
        /// its backup asynchronously; as such when this method returns the backup may not have fully completed.<para/>
        /// By default, configuration backups are stored under C:\ProgramData\Paessler\PRTG Network Monitor\Configuration Auto-Backups.
        /// </summary>
        public async Task BackupConfigDatabaseAsync() =>
            await RequestEngine.ExecuteRequestAsync(new CommandFunctionParameters(CommandFunction.SaveNow)).ConfigureAwait(false);

        /// <summary>
        /// Clears cached data used by PRTG, including map, graph and authentication caches. Note: clearing certain cache types may result in a restart of the PRTG Core Server.
        /// See each cache type for further details.
        /// </summary>
        /// <param name="cacheType">The type of cache to clear. Note: clearing certain cache types may result in a restart of the PRTG Core Server.
        /// See each cache type for further details.</param>
        public void ClearSystemCache(SystemCacheType cacheType) =>
            RequestEngine.ExecuteRequest(new ClearSystemCacheParameters(cacheType));

        /// <summary>
        /// Asynchronously clears cached data used by PRTG, including map, graph and authentication caches. Note: clearing certain cache types may result in a restart of the PRTG Core Server.
        /// See each cache type for further details.
        /// </summary>
        /// <param name="cacheType">The type of cache to clear. Note: clearing certain cache types may result in a restart of the PRTG Core Server.
        /// See each cache type for further details.</param>
        public async Task ClearSystemCacheAsync(SystemCacheType cacheType) =>
            await RequestEngine.ExecuteRequestAsync(new ClearSystemCacheParameters(cacheType)).ConfigureAwait(false);
        
        /// <summary>
        /// Reloads config files including sensor lookups, device icons and report templates used by PRTG.
        /// </summary>
        /// <param name="fileType">The type of files to reload.</param>
        public void LoadConfigFiles(ConfigFileType fileType) =>
            RequestEngine.ExecuteRequest(new LoadConfigFilesParameters(fileType));

        /// <summary>
        /// Asymchronously reloads config files including sensor lookups, device icons and report templates used by PRTG.
        /// </summary>
        /// <param name="fileType">The type of files to reload.</param>
        public async Task LoadConfigFilesAsync(ConfigFileType fileType) =>
            await RequestEngine.ExecuteRequestAsync(new LoadConfigFilesParameters(fileType)).ConfigureAwait(false);

        /// <summary>
        /// Restarts the PRTG Probe Service of a specified PRTG Probe. If no probe ID is specified, the PRTG Probe Service will be restarted on all PRTG Probes.<para/>
        /// By default, PrtgAPI will wait 5 seconds between each probing attempt to confirm whether all probes have successfully restarted.<para/>
        /// If a progress callback is specified, it is up to the programmer to specify the wait duration between each request. If at any time
        /// the progress callback returns false, PrtgAPI will stop waiting for all probes to restart.
        /// </summary>
        /// <param name="probeId">The ID of the probe to restart. If this value is null, the PRTG Probe Service of all probes will be restarted.</param>
        /// <param name="waitForRestart">Whether to wait for the Probe Service on all probes to restart before completing this method.</param>
        /// <param name="progressCallback">A callback method to execute upon each request against PRTG to check whether all probes have restarted.</param>
        public void RestartProbe(int? probeId, bool waitForRestart = false, Func<List<RestartProbeProgress>, bool> progressCallback = null)
        {
            var restartTime = DateTime.Now;

            RequestEngine.ExecuteRequest(new RestartProbeParameters(probeId));

            if (waitForRestart)
            {
                var probe = probeId == null ? GetProbes() : GetProbes(Property.Id, probeId);
                WaitForProbeRestart(restartTime, probe, progressCallback);
            }
        }

        /// <summary>
        /// Asynchronously restarts the PRTG Probe Service of a specified PRTG Probe. If no probe ID is specified, the PRTG Probe Service will be restarted on all PRTG Probes.<para/>
        /// By default, PrtgAPI will wait 5 seconds between each probing attempt to confirm whether all probes have successfully restarted.<para/>
        /// If a progress callback is specified, it is up to the programmer to specify the wait duration between each request. If at any time
        /// the progress callback returns false, PrtgAPI will stop waiting for all probes to restart.
        /// </summary>
        /// <param name="probeId">The ID of the probe to restart. If this value is null, the PRTG Probe Service of all probes will be restarted.</param>
        /// <param name="waitForRestart">Whether to wait for the Probe Service on all probes to restart before completing this method.</param>
        /// <param name="progressCallback">A callback method to execute upon each request against PRTG to check whether all probes have restarted.</param>
        public async Task RestartProbeAsync(int? probeId, bool waitForRestart = false, Func<List<RestartProbeProgress>, bool> progressCallback = null)
        {
            var restartTime = DateTime.Now;

            await RequestEngine.ExecuteRequestAsync(new RestartProbeParameters(probeId)).ConfigureAwait(false);

            if (waitForRestart)
            {
                var probe = probeId == null ? await GetProbesAsync().ConfigureAwait(false) : await GetProbesAsync(Property.Id, probeId).ConfigureAwait(false);
                await WaitForProbeRestartAsync(restartTime, probe, progressCallback).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Restarts the PRTG Core Service. This will cause PRTG to disconnect all users and become completely unavailable while the service restarts.<para/>
        /// If PRTG is part of a cluster, only the server specified by the current <see cref="PrtgClient"/> will be restarted.<para/>
        /// By default, PrtgAPI will wait 5 seconds between each probing attempt to confirm whether PRTG has successfully restarted.<para/>
        /// If a progress callback is specified, it is up to the programmer to specify the wait duration between each request. If at any time
        /// the progress callback returns false, PrtgAPI will stop waiting for the core to restart.
        /// </summary>
        /// <param name="waitForRestart">Whether wait for the Core Service to restart before completing this method.</param>
        /// <param name="progressCallback">A callback method to execute upon each request against PRTG to check whether PRTG has restarted.</param>
        public void RestartCore(bool waitForRestart = false, Func<RestartCoreStage, bool> progressCallback = null)
        {
            DateTime restartTime = DateTime.Now;

            RequestEngine.ExecuteRequest(new CommandFunctionParameters(CommandFunction.RestartServer));

            if (waitForRestart)
                WaitForCoreRestart(restartTime, progressCallback);
        }

        /// <summary>
        /// Asynchronously restarts the PRTG Core Service. This will cause PRTG to disconnect all users and become completely unavailable while the service restarts.<para/>
        /// If PRTG is part of a cluster, only the server specified by the current <see cref="PrtgClient"/> will be restarted.<para/>
        /// By default, PrtgAPI will wait 5 seconds between each probing attempt to confirm whether PRTG has successfully restarted.<para/>
        /// If a progress callback is specified, it is up to the programmer to specify the wait duration between each request. If at any time
        /// the progress callback returns false, PrtgAPI will stop waiting for the core to restart.
        /// </summary>
        public async Task RestartCoreAsync(bool waitForRestart = false, Func<RestartCoreStage, bool> progressCallback = null)
        {
            DateTime restartTime = DateTime.Now;

            await RequestEngine.ExecuteRequestAsync(new CommandFunctionParameters(CommandFunction.RestartServer)).ConfigureAwait(false);

            if (waitForRestart)
                await WaitForCoreRestartAsync(restartTime, progressCallback).ConfigureAwait(false);
        }

        #endregion
        #region Miscellaneous

        /// <summary>
        /// Requests an object or any children of an one or more objects refresh themselves immediately.
        /// </summary>
        /// <param name="objectIds">The IDs of the Sensors and/or the IDs of the Probes, Groups or Devices whose child sensors should be refreshed.</param>
        public void RefreshObject(params int[] objectIds) => RequestEngine.ExecuteRequest(new RefreshObjectParameters(objectIds));

        /// <summary>
        /// Asynchronously requests an object or any children of one or more objects refresh themselves immediately.
        /// </summary>
        /// <param name="objectIds">The IDs of the Sensors and/or the IDs of the Probes, Groups or Devices whose child sensors should be refreshed.</param>
        public async Task RefreshObjectAsync(params int[] objectIds) => await RequestEngine.ExecuteRequestAsync(new RefreshObjectParameters(objectIds)).ConfigureAwait(false);

        /// <summary>
        /// Automatically creates sensors under an object based on the object's (or it's children's) device type.
        /// </summary>
        /// <param name="objectId">The object to run Auto-Discovery for (such as a device or group).</param>
        /// <param name="templates">An optional list of device templates to use for performing the auto-discovery.</param>
        public void AutoDiscover(int objectId, params DeviceTemplate[] templates) =>
            RequestEngine.ExecuteRequest(new AutoDiscoverParameters(objectId, templates));

        /// <summary>
        /// Asynchronously automatically creates sensors under an object based on the object's (or it's children's) device type.
        /// </summary>
        /// <param name="objectId">The object to run Auto-Discovery for (such as a device or group).</param>
        /// <param name="templates">An optional list of device templates to use for performing the auto-discovery.</param>
        public async Task AutoDiscoverAsync(int objectId, params DeviceTemplate[] templates) =>
            await RequestEngine.ExecuteRequestAsync(new AutoDiscoverParameters(objectId, templates)).ConfigureAwait(false);

        /// <summary>
        /// Moves the position of an object up or down under its parent within the PRTG User Interface.
        /// </summary>
        /// <param name="objectId">The object to reposition.</param>
        /// <param name="position">The direction to move in.</param>
        public void SetPosition(int objectId, Position position) => RequestEngine.ExecuteRequest(new SetPositionParameters(objectId, position));

        /// <summary>
        /// Sets the absolute position of an object under its parent within the PRTG User Interface
        /// </summary>
        /// <param name="obj">The object to reposition.</param>
        /// <param name="position">The position to move the object to. If this value is higher than the total number of objects under the parent node, the object will be moved to the last possible position.</param>
        public void SetPosition(SensorOrDeviceOrGroupOrProbe obj, int position) => RequestEngine.ExecuteRequest(new SetPositionParameters(obj, position));

        /// <summary>
        /// Asynchronously moves the position of an object up or down under its parent within the PRTG User Interface.
        /// </summary>
        /// <param name="objectId">The object to reposition.</param>
        /// <param name="position">The direction to move in.</param>
        public async Task SetPositionAsync(int objectId, Position position) => await RequestEngine.ExecuteRequestAsync(new SetPositionParameters(objectId, position)).ConfigureAwait(false);

        /// <summary>
        /// Asynchronously sets the absolute position of an object under its parent within the PRTG User Interface
        /// </summary>
        /// <param name="obj">The object to reposition.</param>
        /// <param name="position">The position to move the object to. If this value is higher than the total number of objects under the parent node, the object will be moved to the last possible position.</param>
        public async Task SetPositionAsync(SensorOrDeviceOrGroupOrProbe obj, int position) => await RequestEngine.ExecuteRequestAsync(new SetPositionParameters(obj, position)).ConfigureAwait(false);

        /// <summary>
        /// Moves a device or group (excluding the root group) to another group or probe within PRTG.
        /// </summary>
        /// <param name="objectId">The ID of a device or group to move.</param>
        /// <param name="destinationId">The group or probe to move the object to.</param>
        public void MoveObject(int objectId, int destinationId) => RequestEngine.ExecuteRequest(new MoveObjectParameters(objectId, destinationId));

        /// <summary>
        /// Asynchronously moves a device or group (excluding the root group) to another group or probe within PRTG.
        /// </summary>
        /// <param name="objectId">The ID of a device or group to move.</param>
        /// <param name="destinationId">The group or probe to move the object to.</param>
        public async Task MoveObjectAsync(int objectId, int destinationId) => await RequestEngine.ExecuteRequestAsync(new MoveObjectParameters(objectId, destinationId)).ConfigureAwait(false);

        /// <summary>
        /// Sorts the children of a device, group or probe alphabetically.
        /// </summary>
        /// <param name="objectId">The object to sort.</param>
        public void SortAlphabetically(int objectId) => RequestEngine.ExecuteRequest(new SortAlphabeticallyParameters(objectId));

        /// <summary>
        /// Asynchronously sorts the children of a device, group or probe alphabetically.
        /// </summary>
        /// <param name="objectId">The object to sort.</param>
        public async Task SortAlphabeticallyAsync(int objectId) => await RequestEngine.ExecuteRequestAsync(new SortAlphabeticallyParameters(objectId)).ConfigureAwait(false);

        /// <summary>
        /// Permanently removes one or more objects such as a Sensor, Device, Group or Probe from PRTG. This cannot be undone.
        /// </summary>
        /// <param name="objectIds">IDs of the objects to remove.</param>
        public void RemoveObject(params int[] objectIds) => RequestEngine.ExecuteRequest(new DeleteParameters(objectIds));

        /// <summary>
        /// Asynchronously permanently removes one or more objects such as a Sensor, Device, Group or Probe from PRTG. This cannot be undone.
        /// </summary>
        /// <param name="objectIds">IDs of the objects to remove.</param>
        public async Task RemoveObjectAsync(params int[] objectIds) => await RequestEngine.ExecuteRequestAsync(new DeleteParameters(objectIds)).ConfigureAwait(false);

        /// <summary>
        /// Renames a Sensor, Device, Group or Probe within PRTG.
        /// </summary>
        /// <param name="objectId">ID of the object to rename.</param>
        /// <param name="name">New name to give the object.</param>
        public void RenameObject(int objectId, string name) => RenameObject(new[] {objectId}, name);

        /// <summary>
        /// Renames one or more Sensors, Devices, Groups or Probe within PRTG.
        /// </summary>
        /// <param name="objectIds">IDs of the objects to rename.</param>
        /// <param name="name">New name to give the objects.</param>
        public void RenameObject(int[] objectIds, string name) => RequestEngine.ExecuteRequest(new RenameParameters(objectIds, name));

        /// <summary>
        /// Asynchronously renames a Sensor, Device, Group or Probe within PRTG.
        /// </summary>
        /// <param name="objectId">ID of the object to rename.</param>
        /// <param name="name">New name to give the object.</param>
        public async Task RenameObjectAsync(int objectId, string name) => await RenameObjectAsync(new[] {objectId}, name).ConfigureAwait(false);

        /// <summary>
        /// Asynchronously renames one or more Sensors, Devices, Groups or Probes within PRTG.
        /// </summary>
        /// <param name="objectIds">IDs of the objects to rename.</param>
        /// <param name="name">New name to give the objects.</param>
        public async Task RenameObjectAsync(int[] objectIds, string name) => await RequestEngine.ExecuteRequestAsync(new RenameParameters(objectIds, name)).ConfigureAwait(false);

        #endregion
    #endregion
#endregion

#region Unsorted

        /// <summary>
        /// Calculates the total number of objects of a given type present on a PRTG Server.
        /// </summary>
        /// <param name="content">The type of object to total.</param>
        /// <returns>The total number of objects of the given type.</returns>
        public int GetTotalObjects(Content content) =>
            ObjectEngine.GetObjectsRaw<object>(new TotalObjectParameters(content)).TotalCount;

        /// <summary>
        /// Asynchronously calculates the total number of objects of a given type present on a PRTG Server.
        /// </summary>
        /// <param name="content">The type of object to total.</param>
        /// <returns>The total number of objects of the given type.</returns>
        public async Task<int> GetTotalObjectsAsync(Content content) =>
            (await ObjectEngine.GetObjectsRawAsync<object>(new TotalObjectParameters(content)).ConfigureAwait(false)).TotalCount;

        /// <summary>
        /// Calculates the total number of objects of a given type present on a PRTG Server that match one or more search criteria.
        /// </summary>
        /// <param name="content">The type of object to total.</param>
        /// <param name="filters">One or more filters used to limit search results.</param>
        /// <returns>The total number of objects of the given type.</returns>
        public int GetTotalObjects(Content content, params SearchFilter[] filters) =>
            ObjectEngine.GetObjectsRaw<object>(new TotalObjectParameters(content, filters)).TotalCount;

        /// <summary>
        /// Asynchronously calculates the total number of objects of a given type present on a PRTG Server that match one or more search criteria.
        /// </summary>
        /// <param name="content">The type of object to total.</param>
        /// <param name="filters">One or more filters used to limit search results.</param>
        /// <returns>The total number of objects of the given type.</returns>
        public async Task<int> GetTotalObjectsAsync(Content content, params SearchFilter[] filters) =>
            (await ObjectEngine.GetObjectsRawAsync<object>(new TotalObjectParameters(content, filters)).ConfigureAwait(false)).TotalCount;

        /// <summary>
        /// Retrieves the setting/state modification history of a PRTG Object.
        /// </summary>
        /// <param name="objectId">The ID of the object to retrieve historical records for.</param>
        /// <returns>A list of all setting/state modifications to the specified object.</returns>
        public List<ModificationEvent> GetModificationHistory(int objectId) =>
            ResponseParser.Amend(ObjectEngine.GetObjects<ModificationEvent>(new ModificationHistoryParameters(objectId)), e => e.ObjectId = objectId);

        /// <summary>
        /// Asynchronously retrieves the setting/state modification history of a PRTG Object.
        /// </summary>
        /// <param name="objectId">The ID of the object to retrieve historical records for.</param>
        /// <returns>A list of all setting/state modifications to the specified object.</returns>
        public async Task<List<ModificationEvent>> GetModificationHistoryAsync(int objectId) =>
            ResponseParser.Amend(await ObjectEngine.GetObjectsAsync<ModificationEvent>(new ModificationHistoryParameters(objectId)).ConfigureAwait(false), e => e.ObjectId = objectId);

        /// <summary>
        /// Retrieves the historical values of a sensor's channels from within a specified time period.
        /// </summary>
        /// <param name="sensorId">The ID of the sensor to retrieve historical data for.</param>
        /// <param name="average">The time span (in seconds) to average results up to. For example, a value of 300 shows the average of results every 5 minutes. If a value of 0
        /// is used, PRTG will use the true interval of the sensor.<para/>
        /// Note: due to limitations of the PRTG API, value lookup labels can only be retrieved when the average is 0, while downtime information
        /// can only be retrieved when the average is not 0.</param>
        /// <param name="startDate">The start date and time to retrieve data from. If this value is null, records will be retrieved from the current date and time.</param>
        /// <param name="endDate">The end date and time to retrieve data to. If this value is null, records will be retrieved from one hour prior to <paramref name="startDate"/>.</param>
        /// <param name="count">Limit results to the specified number of items within the specified time period.</param>
        /// <returns>Historical data for the specified sensor within the desired date range.</returns>
        public List<SensorHistoryData> GetSensorHistory(int sensorId, int average = 300, DateTime? startDate = null, DateTime? endDate = null, int? count = null)
        {
            var parameters = new SensorHistoryParameters(sensorId, average, startDate, endDate, count);

            return GetSensorHistoryInternal(parameters);
        }

        internal List<SensorHistoryData> GetSensorHistoryInternal(SensorHistoryParameters parameters)
        {
            var items = GetObjects<SensorHistoryData>(parameters, XmlFunction.HistoricData, ResponseParser.ValidateSensorHistoryResponse);

            return ResponseParser.ParseSensorHistoryResponse(items, parameters.SensorId);
        }

        /// <summary>
        /// Asynchronously retrieves the historical values of a sensor's channels from within a specified time period.
        /// </summary>
        /// <param name="sensorId">The ID of the sensor to retrieve historical data for.</param>
        /// <param name="average">The time span (in seconds) to average results up to. For example, a value of 300 shows the average of results every 5 minutes. If a value of 0
        /// is used, PRTG will use the true interval of the sensor.<para/>
        /// Note: due to limitations of the PRTG API, value lookup labels can only be retrieved when the average is 0, while downtime information
        /// can only be retrieved when the average is not 0.</param>
        /// <param name="startDate">The start date and time to retrieve data from. If this value is null, records will be retrieved from the current date and time.</param>
        /// <param name="endDate">The end date and time to retrieve data to. If this value is null, records will be retrieved from one hour prior to <paramref name="startDate"/>.</param>
        /// <param name="count">Limit results to the specified number of items within the specified time period.</param>
        /// <returns>Historical data for the specified sensor within the desired date range.</returns>
        public async Task<List<SensorHistoryData>> GetSensorHistoryAsync(int sensorId, int average = 300, DateTime? startDate = null, DateTime? endDate = null, int? count = null)
        {
            var parameters = new SensorHistoryParameters(sensorId, average, startDate, endDate, count);

            return await GetSensorHistoryAsyncInternal(parameters).ConfigureAwait(false);
        }

        internal async Task<List<SensorHistoryData>> GetSensorHistoryAsyncInternal(SensorHistoryParameters parameters)
        {
            var items = await ObjectEngine.GetObjectsAsync<SensorHistoryData>(parameters, ResponseParser.ValidateSensorHistoryResponse).ConfigureAwait(false);

            return ResponseParser.ParseSensorHistoryResponse(items, parameters.SensorId);
        }

        /// <summary>
        /// Streams historical values of a sensors channels from within a specified time period. When this method's response is enumerated,
        /// requests will be sent to PRTG as required in order to retrieve additional items.
        /// </summary>
        /// <param name="sensorId">The ID of the sensor to retrieve historical data for.</param>
        /// <param name="average">The time span (in seconds) to average results up to. For example, a value of 300 shows the average of results every 5 minutes. If a value of 0
        /// is used, PRTG will use the true interval of the sensor.<para/>
        /// Note: due to limitations of the PRTG API, value lookup labels can only be retrieved when the average is 0, while downtime information
        /// can only be retrieved when the average is not 0.</param>
        /// <param name="startDate">The start date and time to retrieve data from. If this value is null, records will be retrieved from the current date and time.</param>
        /// <param name="endDate">The end date and time to retrieve data to. If this value is null, records will be retrieved from one hour prior to <paramref name="startDate"/>.</param>
        /// <returns>A generator encapsulating a series of requests capable of streaming a response from a PRTG Server.</returns>
        public IEnumerable<SensorHistoryData> StreamSensorHistory(int sensorId, int average = 300, DateTime? startDate = null, DateTime? endDate = null)
        {
            var parameters = new SensorHistoryParameters(sensorId, average, startDate, endDate, null);

            return StreamSensorHistoryInternal(parameters, true);
        }

        private IEnumerable<SensorHistoryData> StreamSensorHistoryInternal(SensorHistoryParameters parameters, bool serial)
        {
            return ObjectEngine.StreamObjects(
                parameters,
                serial,
                () => GetSensorHistoryTotals(parameters),
                GetSensorHistoryAsyncInternal,
                GetSensorHistoryInternal
            );
        }

        internal int GetSensorHistoryTotals(SensorHistoryParameters parameters)
        {
            parameters.Count = 0;

            var data = ObjectEngine.GetObjectsRaw<SensorHistoryData>(parameters, ResponseParser.ValidateSensorHistoryResponse);

            parameters.GetParameters().Remove(Parameter.Count);

            return Convert.ToInt32(data.TotalCount);
        }

        //todo: check all arguments we can in this file and make sure we validate input. when theres a chain of methods, validate on the inner most one except if we pass a parameter object, in which case validate both

        /// <summary>
        /// Retrieves configuration, status and version details from a PRTG Server.
        /// </summary>
        /// <returns>Status details of a PRTG Server.</returns>
        public ServerStatus GetStatus() =>
            ObjectEngine.GetObject<ServerStatus>(new ServerStatusParameters());

        /// <summary>
        /// Asynchronously retrieves configuration, status and version details from a PRTG Server.
        /// </summary>
        /// <returns>Status details of a PRTG Server.</returns>
        public async Task<ServerStatus> GetStatusAsync() =>
            await ObjectEngine.GetObjectAsync<ServerStatus>(new ServerStatusParameters()).ConfigureAwait(false);

        /// <summary>
        /// Resolves an address to its latitudinal and longitudinal coordinates. May spuriously return no results.
        /// </summary>
        /// <param name="address">The address to resolve.</param>
        /// <returns></returns>
        internal List<Location> ResolveAddress(string address) =>
            ObjectEngine.GetObject<GeoResult>(new ResolveAddressParameters(address), ResponseParser.ResolveParser).Results.ToList();

        /// <summary>
        /// Asynchronously resolves an address to its latitudinal and longitudinal coordinates. May spuriously return no results.
        /// </summary>
        /// <param name="address">The address to resolve.</param>
        /// <returns></returns>
        internal async Task<List<Location>> ResolveAddressAsync(string address) =>
            (await ObjectEngine.GetObjectAsync<GeoResult>(new ResolveAddressParameters(address), m => Task.FromResult(ResponseParser.ResolveParser(m))).ConfigureAwait(false)).Results.ToList();

        internal void FoldObject(int objectId, bool fold) =>
            RequestEngine.ExecuteRequest(new FoldParameters(objectId, fold));

        #endregion
#if DEBUG
#pragma warning disable 1591
        [ExcludeFromCodeCoverage]
        internal bool UnitTest()
#pragma warning restore 1591
        {
            return Server == "prtg.example.com";
        }
#endif
    }
}
