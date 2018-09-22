using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using PrtgAPI.Attributes;
using PrtgAPI.Helpers;
using PrtgAPI.Linq;
using PrtgAPI.Parameters;
using PrtgAPI.Request;
using PrtgAPI.Request.Serialization;

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

        internal void Log(string message, LogLevel logLevel)
        {
            if((LogLevel & logLevel) == logLevel)
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
        /// <param name="ignoreSSL">Whether to ignore all SSL errors retuned by <paramref name="server"/>. Affects all requests to your server for the life of your program.</param>
        public PrtgClient(string server, string username, string password, AuthMode authMode = AuthMode.Password, bool ignoreSSL = false)
            : this(server, username, password, authMode, new PrtgWebClient(ignoreSSL, server))
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

        #region Channel

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
        #region Notification Actions
        
        private XElement GetNotificationActionProperties(int id)
        {
            var xml = RequestEngine.ExecuteRequest(new GetObjectPropertyParameters(id, ObjectType.Notification), ObjectSettings.GetXml);

            xml = ResponseParser.GroupNotificationActionProperties(xml);

            return xml;
        }

        private async Task<XElement> GetNotificationActionPropertiesAsync(int id)
        {
            var xml = await RequestEngine.ExecuteRequestAsync(new GetObjectPropertyParameters(id, ObjectType.Notification), ObjectSettings.GetXml, token).ConfigureAwait(false);

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

        private List<NotificationTrigger> GetNotificationTriggersInternal(int objectId)
        {
            var xmlResponse = RequestEngine.ExecuteRequest(new NotificationTriggerParameters(objectId));

            var parsed = ResponseParser.ParseNotificationTriggerResponse(objectId, xmlResponse);

            UpdateTriggerChannels(parsed);
            UpdateTriggerActions(parsed);

            return parsed;
        }

        private async Task<List<NotificationTrigger>> GetNotificationTriggersInternalAsync(int objectId)
        {
            var xmlResponse = await RequestEngine.ExecuteRequestAsync(new NotificationTriggerParameters(objectId)).ConfigureAwait(false);

            var parsed = ResponseParser.ParseNotificationTriggerResponse(objectId, xmlResponse);

            var updateTriggerChannels = UpdateTriggerChannelsAsync(parsed);
            var updateTriggerActions = UpdateTriggerActionsAsync(parsed);

            await Task.WhenAll(updateTriggerChannels, updateTriggerActions).ConfigureAwait(false);

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

            var tasks = actions.Select(g => GetNotificationActionPropertiesAsync(g.Key, token));
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
        #region Sensor History

        internal Tuple<List<SensorHistoryData>, int> GetSensorHistoryInternal(SensorHistoryParameters parameters)
        {
            var raw = ObjectEngine.GetObjectsRaw<SensorHistoryData>(parameters, ResponseParser.ValidateSensorHistoryResponse);

            var data = ResponseParser.ParseSensorHistoryResponse(raw.Items, parameters.SensorId);

            return Tuple.Create(data, raw.TotalCount);
        }

        internal async Task<List<SensorHistoryData>> GetSensorHistoryInternalAsync(SensorHistoryParameters parameters)
        {
            var items = await ObjectEngine.GetObjectsAsync<SensorHistoryData>(parameters, ResponseParser.ValidateSensorHistoryResponse).ConfigureAwait(false);

            return ResponseParser.ParseSensorHistoryResponse(items, parameters.SensorId);
        }

        private IEnumerable<SensorHistoryData> StreamSensorHistoryInternal(SensorHistoryParameters parameters, bool serial)
        {
            return ObjectEngine.StreamObjects(
                parameters,
                serial,
                () => GetSensorHistoryTotals(parameters),
                GetSensorHistoryInternalAsync,
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

        #endregion
    #endregion
    #region Object Manipulation
        #region Clone Object

        private int CloneObject(CloneParameters parameters) =>
            ResponseParser.Amend(RequestEngine.ExecuteRequest(parameters, ResponseParser.CloneRequestParser), ResponseParser.CloneResponseParser);

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

        private Dictionary<string, string> GetObjectPropertiesRawDictionary(int objectId, object objectType) =>
            ObjectSettings.GetDictionary(GetObjectPropertiesRawInternal(objectId, objectType));

        private async Task<Dictionary<string, string>> GetObjectPropertiesRawDictionaryAsync(int objectId, object objectType) =>
            ObjectSettings.GetDictionary(await GetObjectPropertiesRawInternalAsync(objectId, objectType).ConfigureAwait(false));

        private string GetObjectPropertiesRawInternal(int objectId, object objectType) =>
            RequestEngine.ExecuteRequest(new GetObjectPropertyParameters(objectId, objectType));

        private async Task<string> GetObjectPropertiesRawInternalAsync(int objectId, object objectType) =>
            await RequestEngine.ExecuteRequestAsync(new GetObjectPropertyParameters(objectId, objectType)).ConfigureAwait(false);

            #endregion
            #region Get Single Raw Property

        private string GetObjectPropertyRawInternal(int objectId, string property, bool text)
        {
            var parameters = new GetObjectPropertyRawParameters(objectId, property, text);

            var response = RequestEngine.ExecuteRequest(
                parameters,
                responseParser: m => ResponseParser.ParseGetObjectPropertyResponse(
                    m.Content.ReadAsStringAsync().Result,
                    property
                )
            );

            return ResponseParser.ValidateRawObjectProperty(response, parameters);
        }

        private async Task<string> GetObjectPropertyRawInternalAsync(int objectId, string property, bool text)
        {
            var parameters = new GetObjectPropertyRawParameters(objectId, property, text);

            var response = await RequestEngine.ExecuteRequestAsync(
                parameters,
                responseParser: async m => ResponseParser.ParseGetObjectPropertyResponse(
                    await m.Content.ReadAsStringAsync().ConfigureAwait(false),
                    property
                )
            ).ConfigureAwait(false);

            return ResponseParser.ValidateRawObjectProperty(response, parameters);
        }

            #endregion
        #endregion
        #region Set Object Properties

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
        
        private void RestartProbeInternal(int[] probeIds, bool waitForRestart, Func<ProbeRestartProgress[], bool> progressCallback)
        {
            var restartTime = waitForRestart ? (DateTime?) GetStatus().DateTime : null;

            if (probeIds != null && probeIds.Length > 1)
            {
                foreach (var probeId in probeIds)
                    RequestEngine.ExecuteRequest(new RestartProbeParameters(probeId));
            }
            else
                RequestEngine.ExecuteRequest(new RestartProbeParameters(probeIds?.Cast<int?>().FirstOrDefault()));

            if (waitForRestart)
            {
                var probe = probeIds == null || probeIds.Length == 0 ? GetProbes() : GetProbes(Property.Id, probeIds);
                WaitForProbeRestart(restartTime.Value, probe, progressCallback);
            }
        }

        private async Task RestartProbeInternalAsync(int[] probeIds, bool waitForRestart, Func<ProbeRestartProgress[], bool> progressCallback)
        {
            var restartTime = waitForRestart ? (DateTime?)(await GetStatusAsync().ConfigureAwait(false)).DateTime : null;

            if (probeIds != null && probeIds.Length > 1)
            {
                var tasks = probeIds.Select(probeId => RequestEngine.ExecuteRequestAsync(new RestartProbeParameters(probeId)));

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            else
                RequestEngine.ExecuteRequest(new RestartProbeParameters(probeIds?.Cast<int?>().FirstOrDefault()));

            if (waitForRestart)
            {
                var probe = probeIds == null || probeIds.Length == 0 ? await GetProbesAsync().ConfigureAwait(false) : await GetProbesAsync(Property.Id, probeIds).ConfigureAwait(false);
                await WaitForProbeRestartAsync(restartTime.Value, probe, progressCallback).ConfigureAwait(false);
            }
        }

        private void RestartCoreInternal(bool waitForRestart, Func<RestartCoreStage, bool> progressCallback)
        {
            var restartTime = waitForRestart ? (DateTime?)GetStatus().DateTime : null;

            RequestEngine.ExecuteRequest(new CommandFunctionParameters(CommandFunction.RestartServer));

            if (waitForRestart)
                WaitForCoreRestart(restartTime.Value, progressCallback);
        }

        private async Task RestartCoreInternalAsync(bool waitForRestart, Func<RestartCoreStage, bool> progressCallback)
        {
            var restartTime = waitForRestart ? (DateTime?)(await GetStatusAsync().ConfigureAwait(false)).DateTime : null;

            await RequestEngine.ExecuteRequestAsync(new CommandFunctionParameters(CommandFunction.RestartServer)).ConfigureAwait(false);

            if (waitForRestart)
                await WaitForCoreRestartAsync(restartTime.Value, progressCallback).ConfigureAwait(false);
        }

        #endregion
    #endregion
#endregion

#region Unsorted

        //todo: check all arguments we can in this file and make sure we validate input. when theres a chain of methods, validate on the inner most one except if we pass a parameter object, in which case validate both

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
