using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using PrtgAPI.Attributes;
using PrtgAPI.Linq;
using PrtgAPI.Parameters;
using PrtgAPI.Request;
using PrtgAPI.Request.Serialization;
using PrtgAPI.Utilities;

/* Static code used by the PrtgClient class
 *
 * See also:
 * - Request\PrtgClient.Generated
 * - Request\PrtgClient.Methods.Generated
 */

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
        /// Specifies the types of events that should be logged by <see cref="LogVerbose"/>.
        /// </summary>
        public LogLevel LogLevel { get; set; } = LogLevel.Trace | LogLevel.Request;

        /// <summary>
        /// Gets the version of PRTG Network Monitor this client is connected to.
        /// </summary>
        public Version Version => version ?? (version = GetStatus().Version);

        internal Version version;

        /// <summary>
        /// The default <see cref="CancellationToken"/> token to use in requests when a token is not otherwise specified.
        /// </summary>
        internal CancellationToken DefaultCancellationToken
        {
            get { return RequestEngine.DefaultCancellationToken; }
            set { RequestEngine.DefaultCancellationToken = value; }
        }

        internal void Log(string message, LogLevel logLevel)
        {
            if ((LogLevel & logLevel) == logLevel)
                HandleEvent(logVerbose, new LogVerboseEventArgs(message, logLevel));
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
        /// <param name="ignoreSSL">Whether to ignore all SSL errors retuned by <paramref name="server"/>. In .NET Framework affects all requests to your server for the life of your program.</param>
        public PrtgClient(string server, string username, string password, AuthMode authMode = AuthMode.Password, bool ignoreSSL = false)
            : this(server, username, password, authMode, new PrtgWebClient(ignoreSSL, server))
        {
        }

        internal PrtgClient(string server, string username, string password, AuthMode authMode, IWebClient client,
            IXmlSerializer xmlSerializer = null)
        {
            if (xmlSerializer == null)
                xmlSerializer = new XmlExpressionSerializer();

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

            ObjectEngine = new ObjectEngine(this, RequestEngine, xmlSerializer);
        }

#region Requests

        internal VersionClient GetVersionClient(object[] obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var enums = obj.Where(o => o is Enum).ToList();

            if (enums.Count == 0)
                throw new NotImplementedException($"Don't know how to get {nameof(VersionAttribute)} for '{string.Join(",", obj)}'.");

            var result = obj.OfType<Enum>().Select(o => o.GetEnumAttribute<VersionAttribute>()).Where(a => a != null).OrderBy(a => a.Version).ToList();

            var attr = result.FirstOrDefault();
            var ver = attr?.Version ?? RequestVersion.v14_4;

            if (attr != null && attr.IsActive(Version))
                return GetVersionClient(ver);
            else
                return new VersionClient(ver, this);
        }

        internal VersionClient GetVersionClient()
        {
            RequestVersion max = RequestVersion.v14_4;

            foreach (var pair in VersionMap.Map)
            {
                if (Version >= pair.Value)
                    max = pair.Key;
            }

            return GetVersionClient(max);
        }

        internal VersionClient GetVersionClient(RequestVersion version)
        {
            switch (version)
            {
                case RequestVersion.v18_1:
                    return new VersionClient18_1(this);

                case RequestVersion.v17_4:
                    return new VersionClient17_4(this);

                default:
                    return new VersionClient(version, this);
            }
        }

        [ExcludeFromCodeCoverage]
        internal VersionClient GetVersionClient<T1, T2>(List<T1> parameters) where T1 : PropertyParameter<T2>
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            if (parameters.Any(p => p == null))
                throw new ArgumentException("Cannot process a null parameter.", nameof(parameters));

            if (parameters.Count == 0)
                throw new ArgumentException("At least one parameter must be specified.", nameof(parameters));

            return GetVersionClient(parameters.Select(p => p.Property).Cast<object>().ToArray());
        }

    #region Object Data

        private string GetPassHash(string password)
        {
            var response = RequestEngine.ExecuteRequest(new PassHashParameters(password), m => m.Content.ReadAsStringAsync().Result).StringValue;

            if (!Regex.Match(response, "^[0-9]+$").Success)
                throw new PrtgRequestException($"Could not retrieve PassHash from PRTG Server. PRTG responded '{response}'.");

            return response;
        }

        private T AssertHasValue<T>(T value, string paramName)
        {
            if (value == null)
                throw new ArgumentNullException(paramName, $"{paramName.ToSentenceCase()} cannot be null.");

            if (typeof(T) == typeof(string))
            {
                if (string.IsNullOrWhiteSpace((string)(object)value))
                    throw new ArgumentException($"{paramName.ToSentenceCase()} cannot be empty or whitespace.", paramName);
            }
            else
            {
                var collection = value as ICollection;

                if (collection != null)
                {
                    if (collection.Count == 0)
                        throw new ArgumentException($"At least one {paramName.FromPlural()} must be specified.", paramName);

                    foreach (var v in collection)
                    {
                        if (v == null)
                            throw new ArgumentException($"Cannot process a null {paramName.FromPlural()}.", nameof(paramName));
                    }
                }
            }

            return value;
        }

        internal List<Sensor> GetSensors(Property property, object value, CancellationToken token) =>
            GetSensors(new SensorParameters(new SearchFilter(property, value)), token);

        internal List<Probe> GetProbes(Property property, object value, CancellationToken token) =>
            GetProbes(new ProbeParameters(new SearchFilter(property, value)), token);

        internal Schedule GetSchedule(int id, CancellationToken token) =>
            GetSchedules(Property.Id, id, token).SingleObject(id);

        internal List<Schedule> GetSchedules(Property property, object value, CancellationToken token) =>
            GetSchedulesInternal(new ScheduleParameters(new SearchFilter(property, value)), token);

        #region System Information

        private SystemInfo GetSystemInfoInternal(Either<Device, int> deviceOrId)
        {
            var deviceId = deviceOrId.GetId();

            var system = GetSystemInfo<DeviceSystemInfo>(deviceId);
            var hardware = GetSystemInfo<DeviceHardwareInfo>(deviceId);
            var software = GetSystemInfo<DeviceSoftwareInfo>(deviceId);
            var processes = GetSystemInfo<DeviceProcessInfo>(deviceId);
            var services = GetSystemInfo<DeviceServiceInfo>(deviceId);
            var users = GetSystemInfo<DeviceUserInfo>(deviceId);

            return new SystemInfo(deviceId, system, hardware, software, processes, services, users);
        }

        private async Task<SystemInfo> GetSystemInfoInternalAsync(Either<Device, int> deviceOrId, CancellationToken token)
        {
            var deviceId = deviceOrId.GetId();

            var system = GetSystemInfoAsync<DeviceSystemInfo>(deviceId, token);
            var hardware = GetSystemInfoAsync<DeviceHardwareInfo>(deviceId, token);
            var software = GetSystemInfoAsync<DeviceSoftwareInfo>(deviceId, token);
            var processes = GetSystemInfoAsync<DeviceProcessInfo>(deviceId, token);
            var services = GetSystemInfoAsync<DeviceServiceInfo>(deviceId, token);
            var users = GetSystemInfoAsync<DeviceUserInfo>(deviceId, token);

            await Task.WhenAll(system, hardware, software, processes, services, users).ConfigureAwait(false);

            return new SystemInfo(deviceId,
                await system.ConfigureAwait(false),    await hardware.ConfigureAwait(false), await software.ConfigureAwait(false),
                await processes.ConfigureAwait(false), await services.ConfigureAwait(false), await users.ConfigureAwait(false)
            );
        }

        private void RefreshSystemInfoInternal(Either<Device, int> deviceOrId, SystemInfoType[] types, CancellationToken token)
        {
            if (types == null || types.Length == 0)
                types = typeof(SystemInfoType).GetEnumValues().Cast<SystemInfoType>().ToArray();

            foreach (var type in types)
                RequestEngine.ExecuteRequest(new RefreshSystemInfoParameters(deviceOrId, type), token: token);
        }

        private async Task RefreshSystemInfoInternalAsync(Either<Device, int> deviceOrId, SystemInfoType[] types, CancellationToken token)
        {
            if (types == null || types.Length == 0)
                types = typeof(SystemInfoType).GetEnumValues().Cast<SystemInfoType>().ToArray();

            var tasks = types.Select(t => RequestEngine.ExecuteRequestAsync(new RefreshSystemInfoParameters(deviceOrId, t), token: token)).ToArray();

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        #endregion
        #region Channel

        private XElement GetChannelProperties(Either<Sensor, int> sensorOrId, int channelId, CancellationToken token)
        {
            var parameters = new ChannelPropertiesParameters(sensorOrId, channelId);

            return RequestEngine.ExecuteRequest(parameters, r => ChannelSettings.GetChannelXml(r, channelId), token);
        }

        private async Task<XElement> GetChannelPropertiesAsync(Either<Sensor, int> sensorOrId, int channelId, CancellationToken token)
        {
            var parameters = new ChannelPropertiesParameters(sensorOrId, channelId);

            return await RequestEngine.ExecuteRequestAsync(parameters, r => ChannelSettings.GetChannelXml(r, channelId), token).ConfigureAwait(false);
        }

        #endregion
        #region Notification Actions
        
        private XElement GetNotificationActionProperties(Either<IPrtgObject, int> objectOrId, CancellationToken token)
        {
            var xml = RequestEngine.ExecuteRequest(new GetObjectPropertyParameters(objectOrId, ObjectType.Notification), HtmlParser.Default.GetXml, token);

            xml = ResponseParser.GroupNotificationActionProperties(xml);

            return xml;
        }

        private async Task<XElement> GetNotificationActionPropertiesAsync(Either<IPrtgObject, int> objectOrId, CancellationToken token)
        {
            var xml = await RequestEngine.ExecuteRequestAsync(new GetObjectPropertyParameters(objectOrId, ObjectType.Notification), HtmlParser.Default.GetXml, token).ConfigureAwait(false);

            xml = ResponseParser.GroupNotificationActionProperties(xml);

            return xml;
        }

        private void UpdateActionSchedules(List<IGrouping<int?, NotificationAction>> actions, CancellationToken token)
        {
            if (actions.Count > 0)
            {
                var schedules = new Lazy<List<Schedule>>(() => GetSchedules(Property.Id, actions.Select(a => a.Key), token), LazyThreadSafetyMode.PublicationOnly);

                foreach (var group in actions)
                {
                    //Key is null when we're a read only user
                    if (group.Key != null)
                    {
                        foreach (var action in group)
                            action.schedule = new Lazy<Schedule>(() => schedules.Value.First(s => s.Id == group.Key), LazyThreadSafetyMode.PublicationOnly);
                    }
                }
            }
        }

        private async Task UpdateActionSchedulesAsync(List<IGrouping<int?, NotificationAction>> actions, CancellationToken token)
        {
            if (actions.Count > 0 && actions.Any(a => a.Key != null))
            {
                var schedules = await GetSchedulesAsync(Property.Id, actions.Select(a => a.Key), token).ConfigureAwait(false);

                foreach (var group in actions)
                {
                    foreach (var action in group)
                        action.schedule = new Lazy<Schedule>(() => schedules.First(s => s.Id == group.Key));
                }
            }
        }

        #endregion
        #region Notification Triggers

        private bool? isEnglish;

        private bool IsEnglish
        {
            get
            {
                if (isEnglish == null)
                {
                    string language;

                    if (GetObjectPropertiesRaw(WellKnownId.WebServerOptions).TryGetValue("languagefile", out language))
                    {
                        isEnglish = language == "english.lng";
                    }
                    else
                        isEnglish = false;
                }

                return isEnglish.Value;
            }
        }

        private List<NotificationTrigger> GetNotificationTriggersInternal(Either<IPrtgObject, int> objectOrId, CancellationToken token)
        {
            var xmlResponse = ObjectEngine.GetObjectsXml(new NotificationTriggerParameters(objectOrId), token: token);

            var typed = ResponseParser.ParseNotificationTriggerResponse(objectOrId, xmlResponse);

            if (!IsEnglish)
            {
                var helper = new NotificationTriggerTranslator(typed, objectOrId.GetId());

                NotificationTriggerDataTrigger[] raw = null;

                int parentId;

                while (helper.TranslateTriggers(raw, out parentId))
                    raw = GetNotificationTriggerData(parentId, token).Triggers;                
            }

            UpdateTriggerChannels(objectOrId, typed, token);
            UpdateTriggerActions(typed, token);

            return typed;
        }

        private async Task<List<NotificationTrigger>> GetNotificationTriggersInternalAsync(Either<IPrtgObject, int> objectOrId, CancellationToken token)
        {
            var xmlResponse = await ObjectEngine.GetObjectsXmlAsync(new NotificationTriggerParameters(objectOrId), token: token).ConfigureAwait(false);

            var typed = ResponseParser.ParseNotificationTriggerResponse(objectOrId, xmlResponse);

            if (!IsEnglish)
            {
                var helper = new NotificationTriggerTranslator(typed, objectOrId.GetId());

                NotificationTriggerDataTrigger[] raw = null;

                int parentId;

                while (helper.TranslateTriggers(raw, out parentId))
                    raw = (await GetNotificationTriggerDataAsync(parentId, token).ConfigureAwait(false)).Triggers;
            }

            var updateTriggerChannels = UpdateTriggerChannelsAsync(objectOrId, typed, token);
            var updateTriggerActions = UpdateTriggerActionsAsync(typed, token);

            await Task.WhenAll(updateTriggerChannels, updateTriggerActions).ConfigureAwait(false);

            return typed;
        }

        private void UpdateTriggerActions(List<NotificationTrigger> triggers, CancellationToken token)
        {
            //Group all actions from all triggers together based on their object ID
            var actions = ResponseParser.GroupTriggerActions(triggers);

            //Retrieve the XML required to construct "proper" notification actions for all unique actions
            //specified in the triggers
            var actionParameters = new NotificationActionParameters(actions.Select(a => a.Key).ToArray());
            var normalActions = new Lazy<XDocument>(() => ObjectEngine.GetObjectsXml(actionParameters, token: token), LazyThreadSafetyMode.PublicationOnly);

            foreach (var group in actions)
            {
                //As soon as a notification with a specified ID is accessed on any one of the triggers, retrieve
                //the "supported" properties of ALL of the notification actions, and then retrieve the "unsupported"
                //properties of JUST the notification action object ID that was accessed.
                var lazyAction = new Lazy<XDocument>(
                    () => RequestParser.ExtractActionXml(normalActions.Value, GetNotificationActionProperties(group.Key, token), @group.Key),
                    LazyThreadSafetyMode.PublicationOnly
                );

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

                                return GetSchedule(new Schedule(action.lazyScheduleStr).Id, token);
                            }

                            return action.lazyScheduleStr == null ? null : new Schedule(action.lazyScheduleStr);
                        }, LazyThreadSafetyMode.PublicationOnly);
                }
            }
        }

        private async Task UpdateTriggerActionsAsync(List<NotificationTrigger> triggers, CancellationToken token)
        {
            var actions = ResponseParser.GroupTriggerActions(triggers);

            var parameters = new NotificationActionParameters(actions.Select(a => a.Key).ToArray());

            var tasks = actions.Select(g => GetNotificationActionPropertiesAsync(g.Key, token));

            var normal = await ObjectEngine.GetObjectsXmlAsync(parameters, token: token).ConfigureAwait(false);

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

            if (list.Count > 0)
                schedules = await GetSchedulesAsync(Property.Id, list.Select(l => l.Key).ToArray(), token).ConfigureAwait(false);

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
                }
            }
        }
        
        private NotificationTriggerData GetNotificationTriggerData(Either<IPrtgObject, int> objectOrId, CancellationToken token) =>
            ObjectEngine.GetObject<NotificationTriggerData>(
                new NotificationTriggerDataParameters(objectOrId),
                ParseNotificationTriggerTypes,
                token
            );

        private async Task<NotificationTriggerData> GetNotificationTriggerDataAsync(Either<IPrtgObject, int> objectOrId, CancellationToken token) =>
            await ObjectEngine.GetObjectAsync<NotificationTriggerData>(
                new NotificationTriggerDataParameters(objectOrId),
                ParseNotificationTriggerTypesAsync,
                token
            ).ConfigureAwait(false);

        private bool IsSensor(Either<IPrtgObject, int> objectOrId)
        {
            if (objectOrId.IsLeft)
                return IsSensorObject(objectOrId.Left);
            else
                return GetObjects(Property.Id, objectOrId.Right).FirstOrDefault()?.Type.Value == ObjectType.Sensor;
        }

        private async Task<bool> IsSensorAsync(Either<IPrtgObject, int> objectOrId)
        {
            if (objectOrId.IsLeft)
                return IsSensorObject(objectOrId.Left);
            else
                return (await GetObjectsAsync(Property.Id, objectOrId.Right).ConfigureAwait(false)).FirstOrDefault()?.Type.Value == ObjectType.Sensor;
        }

        private bool IsSensorObject(IPrtgObject obj)
        {
            var prtgObject = obj as PrtgObject;

            if (prtgObject?.Type.Value == ObjectType.Sensor)
                return true;
            else
                return false;
        }

        #endregion
        #region Sensor History

        internal Tuple<List<SensorHistoryRecord>, int> GetSensorHistoryInternal(SensorHistoryParameters parameters)
        {
            var raw = ObjectEngine.GetObjectsRaw<SensorHistoryRecord>(parameters, responseParser: m => ResponseParser.GetSensorHistoryResponse(m, LogLevel, RequestEngine.IsDirty));

            var data = ResponseParser.ParseSensorHistoryResponse(raw.Items, parameters.SensorId);

            return Tuple.Create(data, raw.TotalCount);
        }

        internal async Task<List<SensorHistoryRecord>> GetSensorHistoryInternalAsync(SensorHistoryParameters parameters, CancellationToken token)
        {
            var items = await ObjectEngine.GetObjectsAsync<SensorHistoryRecord>(parameters, responseParser: m => ResponseParser.GetSensorHistoryResponseAsync(m, LogLevel, RequestEngine.IsDirty), token: token).ConfigureAwait(false);

            return ResponseParser.ParseSensorHistoryResponse(items, parameters.SensorId);
        }

        private IEnumerable<SensorHistoryRecord> StreamSensorHistoryInternal(SensorHistoryParameters parameters, bool serial)
        {
            return ObjectEngine.StreamObjects(
                parameters,
                serial,
                () => GetSensorHistoryTotals(parameters),
                p => GetSensorHistoryInternalAsync(p, CancellationToken.None),
                GetSensorHistoryInternal
            );
        }

        internal int GetSensorHistoryTotals(SensorHistoryParameters parameters)
        {
            parameters.Count = 0;

            var data = ObjectEngine.GetObjectsRaw<SensorHistoryRecord>(parameters, responseParser: m => ResponseParser.GetSensorHistoryResponse(m, LogLevel, RequestEngine.IsDirty));

            parameters.GetParameters().Remove(Parameter.Count);

            return Convert.ToInt32(data.TotalCount);
        }

        #endregion
    #endregion
    #region Object Manipulation
        #region Notifications

        private void SetNotificationTriggerInternal(TriggerParameters parameters, CancellationToken token)
        {
            ValidateTriggerParameters(parameters, token);

            RequestEngine.ExecuteRequest(parameters, token: token);
        }

        private async Task SetNotificationTriggerInternalAsync(TriggerParameters parameters, CancellationToken token)
        {
            await ValidateTriggerParametersAsync(parameters, token).ConfigureAwait(false);

            await RequestEngine.ExecuteRequestAsync(parameters, token: token).ConfigureAwait(false);
        }

        #endregion
        #region Add Object

        internal BeginAddSensorQueryParameters ValidateAddSensorQueryParameters(BeginAddSensorQueryParameters parameters)
        {
            var types = GetSensorTypes(parameters.ObjectId);
            
            RequestParser.ValidateAddSensorQueryTarget(types, parameters);

            return parameters;
        }

        internal async Task<BeginAddSensorQueryParameters> ValidateAddSensorQueryParametersAsync(BeginAddSensorQueryParameters parameters)
        {
            var types = await GetSensorTypesAsync(parameters.ObjectId).ConfigureAwait(false);

            RequestParser.ValidateAddSensorQueryTarget(types, parameters);

            return parameters;
        }

        #endregion
        #region Clone Object

        private int CloneObject(CloneParameters parameters, CancellationToken token) =>
            ResponseParser.Amend(RequestEngine.ExecuteRequest(parameters, ResponseParser.CloneRequestParser, token), ResponseParser.CloneResponseParser);

        private async Task<int> CloneObjectAsync(CloneParameters parameters, CancellationToken token) =>
            ResponseParser.Amend(
                await RequestEngine.ExecuteRequestAsync(
                    parameters,
                    async r => await Task.FromResult(ResponseParser.CloneRequestParser(r)).ConfigureAwait(false),
                    token
                ).ConfigureAwait(false), ResponseParser.CloneResponseParser
            );

        #endregion
        #region Get Object Properties
            #region Get Typed Properties

        private T GetObjectProperties<T>(Either<IPrtgObject, int> objectOrId, ObjectType objectType, ObjectProperty mandatoryProperty)
        {
            var response = GetObjectPropertiesRawInternal(new Either<IPrtgObject, int>(objectOrId.GetId()), objectType);

            var data = ResponseParser.GetObjectProperties<T>(response, ObjectEngine.XmlEngine, mandatoryProperty);

            if (data is TableSettings)
            {
                var table = (TableSettings) (object) data;

                if (table.scheduleStr == null || PrtgObject.GetId(table.scheduleStr) == -1)
                    table.schedule = new LazyValue<Schedule>(table.scheduleStr, () => new Schedule(table.scheduleStr));
                else
                {
                    table.schedule = new LazyValue<Schedule>(
                        table.scheduleStr,
                        () => GetSchedule(PrtgObject.GetId(table.scheduleStr)),
                        LazyThreadSafetyMode.PublicationOnly
                    );
                }
            }

            ValidateTypedProperties(objectOrId, objectType, data);

            return data;
        }

        private async Task<T> GetObjectPropertiesAsync<T>(Either<IPrtgObject, int> objectOrId, ObjectType objectType, ObjectProperty mandatoryProperty, CancellationToken token)
        {
            var response = await GetObjectPropertiesRawInternalAsync(new Either<IPrtgObject, int>(objectOrId.GetId()), objectType, token).ConfigureAwait(false);

            var data = ResponseParser.GetObjectProperties<T>(response, ObjectEngine.XmlEngine, mandatoryProperty);

            if (data is TableSettings)
            {
                var table = (TableSettings)(object)data;

                if (table.scheduleStr == null || PrtgObject.GetId(table.scheduleStr) == -1)
                    table.schedule = new LazyValue<Schedule>(table.scheduleStr, () => new Schedule(table.scheduleStr));
                else
                {
                    var schedule = await GetScheduleAsync(PrtgObject.GetId(table.scheduleStr), token).ConfigureAwait(false);

                    table.schedule = new LazyValue<Schedule>(table.scheduleStr, () => schedule);
                }
            }

            ValidateTypedProperties(objectOrId, objectType, data);

            return data;
        }

        private void ValidateTypedProperties(Either<IPrtgObject, int> objectOrId, ObjectType type, object data)
        {
            if (data == null)
                throw new InvalidOperationException($"Cannot retrieve properties for read-only {type.ToString().ToLower()} with ID {objectOrId.GetId()}.");
        }

            #endregion
            #region Get Multiple Raw Properties

        private Dictionary<string, string> GetObjectPropertiesRawDictionary(Either<IPrtgObject, int> objectOrId, object objectType) =>
            HtmlParser.Default.GetDictionary(GetObjectPropertiesRawInternal(objectOrId, objectType));

        private async Task<Dictionary<string, string>> GetObjectPropertiesRawDictionaryAsync(Either<IPrtgObject, int> objectOrId, object objectType, CancellationToken token) =>
            HtmlParser.Default.GetDictionary(await GetObjectPropertiesRawInternalAsync(objectOrId, objectType, token).ConfigureAwait(false));

        private PrtgResponse GetObjectPropertiesRawInternal(Either<IPrtgObject, int> objectOrId, object objectType, CancellationToken token = default(CancellationToken)) =>
            RequestEngine.ExecuteRequest(new GetObjectPropertyParameters(objectOrId, objectType), token: token);

        private async Task<PrtgResponse> GetObjectPropertiesRawInternalAsync(Either<IPrtgObject, int> objectOrId, object objectType, CancellationToken token) =>
            (await RequestEngine.ExecuteRequestAsync(new GetObjectPropertyParameters(objectOrId, objectType), token: token).ConfigureAwait(false));

            #endregion
            #region Get Single Raw Property

        [ExcludeFromCodeCoverage]
        private object GetObjectProperty(int objectId, ObjectPropertyInternal property) =>
            GetObjectPropertyInternal(objectId, property);

        [ExcludeFromCodeCoverage]
        private async Task<object> GetObjectPropertyAsync(int objectId, ObjectPropertyInternal property, CancellationToken token) =>
            await GetObjectPropertyInternalAsync(objectId, property, token).ConfigureAwait(false);

        private string GetObjectPropertyRawInternal(GetObjectPropertyRawParameters parameters, string property)
        {
            var response = ObjectEngine.GetObjectsXml(
                parameters,
                responseParser: m => ResponseParser.ParseGetObjectPropertyResponse(
                    m.Content.ReadAsStringAsync().Result,
                    property
                )
            );

            return ResponseParser.ValidateRawObjectProperty(response, parameters);
        }

        private async Task<string> GetObjectPropertyRawInternalAsync(GetObjectPropertyRawParameters parameters, string property, CancellationToken token)
        {
            var response = await ObjectEngine.GetObjectsXmlAsync(
                parameters,
                responseParser: async m => ResponseParser.ParseGetObjectPropertyResponse(
                    await m.Content.ReadAsStringAsync().ConfigureAwait(false),
                    property
                ),
                token: token
            ).ConfigureAwait(false);

            return ResponseParser.ValidateRawObjectProperty(response, parameters);
        }

            #endregion
        #endregion
        #region Set Object Properties

        internal void SetObjectProperty<T>(BaseSetObjectPropertyParameters<T> parameters, int numObjectIds, CancellationToken token) =>
            RequestEngine.ExecuteRequest(parameters, m => ResponseParser.ParseSetObjectPropertyUrl(numObjectIds, m), token);

        internal async Task SetObjectPropertyAsync<T>(BaseSetObjectPropertyParameters<T> parameters, int numObjectIds, CancellationToken token) =>
            await RequestEngine.ExecuteRequestAsync(parameters, m => Task.FromResult<PrtgResponse>(ResponseParser.ParseSetObjectPropertyUrl(numObjectIds, m)), token).ConfigureAwait(false);

        #endregion
        #region System Administration

        private void RestartProbeInternal(int[] probeIds, bool waitForRestart, Func<ProbeRestartProgress[], bool> progressCallback, CancellationToken token)
        {
            var restartTime = waitForRestart ? (DateTime?) GetStatus().DateTime : null;

            if (probeIds != null && probeIds.Length > 1)
            {
                foreach (var probeId in probeIds)
                    RequestEngine.ExecuteRequest(new RestartProbeParameters(probeId), token: token);
            }
            else
                RequestEngine.ExecuteRequest(new RestartProbeParameters(probeIds?.Cast<int?>().FirstOrDefault()), token: token);

            if (waitForRestart)
            {
                var probe = probeIds == null || probeIds.Length == 0 ? GetProbes(new ProbeParameters(), token) : GetProbes(Property.Id, probeIds, token);
                WaitForProbeRestart(restartTime.Value, probe, progressCallback, token);
            }
        }

        private async Task RestartProbeInternalAsync(int[] probeIds, bool waitForRestart, Func<ProbeRestartProgress[], bool> progressCallback, CancellationToken token)
        {
            var restartTime = waitForRestart ? (DateTime?)(await GetStatusAsync(token).ConfigureAwait(false)).DateTime : null;

            if (probeIds != null && probeIds.Length > 1)
            {
                var tasks = probeIds.Select(probeId => RequestEngine.ExecuteRequestAsync(new RestartProbeParameters(probeId), token: token));

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            else
                RequestEngine.ExecuteRequest(new RestartProbeParameters(probeIds?.Cast<int?>().FirstOrDefault()), token: token);

            if (waitForRestart)
            {
                var probe = probeIds == null || probeIds.Length == 0 ? await GetProbesAsync(token).ConfigureAwait(false) : await GetProbesAsync(Property.Id, probeIds, token).ConfigureAwait(false);
                await WaitForProbeRestartAsync(restartTime.Value, probe, progressCallback, token).ConfigureAwait(false);
            }
        }

        private void RestartCoreInternal(bool waitForRestart, Func<RestartCoreStage, bool> progressCallback, CancellationToken token)
        {
            var restartTime = waitForRestart ? (DateTime?)GetStatus().DateTime : null;

            RequestEngine.ExecuteRequest(new CommandFunctionParameters(CommandFunction.RestartServer), token: token);

            if (waitForRestart)
                WaitForCoreRestart(restartTime.Value, progressCallback, token);
        }

        private async Task RestartCoreInternalAsync(bool waitForRestart, Func<RestartCoreStage, bool> progressCallback, CancellationToken token)
        {
            var restartTime = waitForRestart ? (DateTime?)(await GetStatusAsync(token).ConfigureAwait(false)).DateTime : null;

            await RequestEngine.ExecuteRequestAsync(new CommandFunctionParameters(CommandFunction.RestartServer), token: token).ConfigureAwait(false);

            if (waitForRestart)
                await WaitForCoreRestartAsync(restartTime.Value, progressCallback, token).ConfigureAwait(false);
        }

        internal void ApproveProbeInternal(Either<Probe, int> probeOrId, ProbeApproval action) =>
            RequestEngine.ExecuteRequest(new ApproveProbeParameters(probeOrId, action));

        internal async Task ApproveProbeInternalAsync(Either<Probe, int> probeOrId, ProbeApproval action, CancellationToken token) =>
            await RequestEngine.ExecuteRequestAsync(new ApproveProbeParameters(probeOrId, action), token: token).ConfigureAwait(false);

        #endregion
    #endregion
#endregion

#region Internal
    #region Address

        //todo: check all arguments we can in this file and make sure we validate input. when theres a chain of methods, validate on the inner most one except if we pass a parameter object, in which case validate both

        /// <summary>
        /// Resolves an address to its latitudinal and longitudinal coordinates. May spuriously return no results.
        /// </summary>
        /// <param name="address">The address to resolve.</param>
        /// <param name="token">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns></returns>
        internal Location ResolveAddress(string address, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(address))
                return new Location();

            Location longLat;

            if (RequestParser.IsLongLat(address, out longLat))
                return longLat;

            var label = RequestParser.GetLocationLabel(ref address);

            List<Location> result = new List<Location>();
            var client = GetVersionClient();

            for (int i = 0; i < 10; i++)
            {
                result = client.ResolveAddressInternal(address, token, i == 9);

                if (result.Any())
                    break;

#if !DEBUG
                token.WaitHandle.WaitOne(1000);
#endif
            }

            if (result.Count == 0)
                throw new PrtgRequestException($"Could not resolve '{address}' to an actual address.");

            var location = result.First();

            if (!string.IsNullOrWhiteSpace(label))
                location.Label = label;

            return location;
        }

        /// <summary>
        /// Asynchronously resolves an address to its latitudinal and longitudinal coordinates. May spuriously return no results.
        /// </summary>
        /// <param name="address">The address to resolve.</param>
        /// <param name="token">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns></returns>
        internal async Task<Location> ResolveAddressAsync(string address, CancellationToken token)
        {
            if (address == null)
                return new Location();

            Location longLat;

            if (RequestParser.IsLongLat(address, out longLat))
                return longLat;

            var label = RequestParser.GetLocationLabel(ref address);

            List<Location> result = new List<Location>();
            var client = GetVersionClient();

            for (int i = 0; i < 10; i++)
            {
                result = await client.ResolveAddressInternalAsync(address, token, i == 9).ConfigureAwait(false);

                if (result.Any())
                    break;

#if !DEBUG
                await token.WaitHandle.WaitOneAsync(1000, token).ConfigureAwait(false);
#endif
            }

            if (result.Count == 0)
                throw new PrtgRequestException($"Could not resolve '{address}' to an actual address.");

            var location = result.First();

            if (!string.IsNullOrWhiteSpace(label))
                location.Label = label;

            return location;
        }

        #endregion

        [ExcludeFromCodeCoverage]
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
