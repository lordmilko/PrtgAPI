using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using PrtgAPI.Attributes;
using PrtgAPI.Helpers;
using PrtgAPI.Html;
using PrtgAPI.Objects.Undocumented;
using PrtgAPI.Parameters;

namespace PrtgAPI
{
    /// <summary>
    /// Makes API requests against a PRTG Network Monitor server.
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
        public string Username { get; }

        /// <summary>
        /// The PassHash that will be used to authenticate with, in place of a password.
        /// </summary>
        public string PassHash { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrtgClient"/> class.
        /// </summary>
        public PrtgClient(string server, string username, string pass, AuthMode authMode = AuthMode.Password)
        {
            if (server == null)
                throw new ArgumentNullException(nameof(server));

            if (username == null)
                throw new ArgumentNullException(nameof(username));

            if (pass == null)
                throw new ArgumentNullException(nameof(pass));

            Server = server;
            Username = username;

            PassHash = authMode == AuthMode.Password ? GetPassHash(pass) : pass;
        }

        #region Requests

        private string GetPassHash(string password)
        {
            var parameters = new Parameters.Parameters()
            {
                [Parameter.Password] = password
            };

            var response = ExecuteRequest(JsonFunction.GetPassHash, parameters);

            return response;
        }

        private List<T> GetObjects<T>(Parameters.Parameters parameters)
        {
            var response = ExecuteRequest(XmlFunction.TableData, parameters);

#if DEBUG
            //Debug.WriteLine(response.ToString());
#endif

            //todo: change the Property enum to have a description attribute that lets you change the property name to something else
            //then, change ObjId to just Id

            //todo - check the xml doesnt say there was an error
            //it looks like we already do this when its an xml request, however theres also a bug here in that we automatically try to deserialize
            //some xml without checking whether its xml or not; this could result in an exception in the exception handler!
            //we need to be able to handle errors on json requests or otherwise
            //it looks like these properties are ultimately parsed by prtgurl, so we'd need to change prtgurl to try and get the description property
            //if it detects a parameters object type is an enum

            //todo: upload our empty settings file then say dont track it
            //git update-index --assume-unchanged <file> and --no-assume-unchanged

            //redundant: deserializelist already does this exception handling

            var data = Data<T>.DeserializeList(response);

            return data.Items;
        }

        #region Sensors

        /// <summary>
        /// Retrieve all sensors from a PRTG Server.
        /// </summary>
        /// <returns></returns>
        public List<Sensor> GetSensors()
        {
            return GetObjects<Sensor>(new SensorParameters());
        }

        #region SensorStatus
        
        /// <summary>
        /// Retrieve sensors from a PRTG Server of one or more statuses.
        /// </summary>
        /// <param name="sensorStatuses">A list of sensor statuses to filter for.</param>
        /// <returns></returns>
        public List<Sensor> GetSensors(params SensorStatus[] sensorStatuses)
        {
            return GetSensors(new SensorParameters { StatusFilter = sensorStatuses });
        }

        /// <summary>
        /// Retrieve the number of sensors of each sensor type in the system.
        /// </summary>
        /// <returns></returns>
        public SensorTotals GetSensorTotals()
        {
            var response = ExecuteRequest(XmlFunction.GetTreeNodeStats, new Parameters.Parameters());

            return Data<SensorTotals>.DeserializeType(response);
        }

        #endregion 

        #region SearchFilter

        /// <summary>
        /// Retrieve sensors from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns></returns>
        public List<Sensor> GetSensors(Property property, object value)
        {
            return GetSensors(new SearchFilter(property, value));
        }

        /// <summary>
        /// Retrieve sensors from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="operator">Operator to compare value and property value with.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns></returns>
        public List<Sensor> GetSensors(Property property, FilterOperator @operator, object value)
        {
            return GetSensors(new SearchFilter(property, @operator, value));
        }

        /// <summary>
        /// Retrieve sensors from a PRTG Server based on the values of multiple properties.
        /// </summary>
        /// <param name="filters">One or more filters used to limit search results.</param>
        /// <returns></returns>
        public List<Sensor> GetSensors(params SearchFilter[] filters)
        {
            return GetSensors(new SensorParameters { SearchFilter = filters });
        }

        #endregion

        /// <summary>
        /// Retrieve sensors from a PRTG Server using a custom set of parameters.
        /// </summary>
        /// <param name="parameters">A custom set of parameters used to retrieve PRTG Sensors.</param>
        /// <returns></returns>
        public List<Sensor> GetSensors(SensorParameters parameters)
        {
            return GetObjects<Sensor>(parameters);
        }

        #endregion

        #region Devices

        /// <summary>
        /// Retrieve all devices from a PRTG Server.
        /// </summary>
        /// <returns></returns>
        public List<Device> GetDevices()
        {
            return GetObjects<Device>(new DeviceParameters());
        }

        /// <summary>
        /// Retrieve devices from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns></returns>
        public List<Device> GetDevices(Property property, object value)
        {
            return GetDevices(new SearchFilter(property, value));
        }

        /// <summary>
        /// Retrieve devices from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="operator">Operator to compare value and property value with.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns></returns>
        public List<Device> GetDevices(Property property, FilterOperator @operator, string value)
        {
            return GetDevices(new SearchFilter(property, @operator, value));
        }

        /// <summary>
        /// Retrieve devices from a PRTG Server based on the values of multiple properties.
        /// </summary>
        /// <param name="filters">One or more filters used to limit search results.</param>
        /// <returns></returns>
        public List<Device> GetDevices(params SearchFilter[] filters)
        {
            return GetDevices(new DeviceParameters { SearchFilter = filters });
        }

        /// <summary>
        /// Retrieve devices from a PRTG Server using a custom set of parameters.
        /// </summary>
        /// <param name="parameters">A custom set of parameters used to retrieve PRTG Devices.</param>
        /// <returns></returns>
        public List<Device> GetDevices(DeviceParameters parameters)
        {
            return GetObjects<Device>(parameters);
        }

        #endregion

        #region Groups

        /// <summary>
        /// Retrieve all groups from a PRTG Server.
        /// </summary>
        /// <returns></returns>
        public List<Group> GetGroups()
        {
            return GetGroups(new GroupParameters());
        }

        /// <summary>
        /// Retrieve groups from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns></returns>
        public List<Group> GetGroups(Property property, object value)
        {
            return GetGroups(new SearchFilter(property, value));
        }

        /// <summary>
        /// Retrieve groups from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="operator">Operator to compare value and property value with.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns></returns>
        public List<Group> GetGroups(Property property, FilterOperator @operator, string value)
        {
            return GetGroups(new SearchFilter(property, @operator, value));
        }

        /// <summary>
        /// Retrieve groups from a PRTG Server based on the values of multiple properties.
        /// </summary>
        /// <param name="filters">One or more filters used to limit search results.</param>
        /// <returns></returns>
        public List<Group> GetGroups(params SearchFilter[] filters)
        {
            return GetGroups(new GroupParameters { SearchFilter = filters });
        }

        /// <summary>
        /// Retrieve groups from a PRTG Server using a custom set of parameters.
        /// </summary>
        /// <param name="parameters">A custom set of parameters used to retrieve PRTG Groups.</param>
        public List<Group> GetGroups(GroupParameters parameters)
        {
            return GetObjects<Group>(parameters);
        }

        #endregion

        #region Probes

        /// <summary>
        /// Retrieve all probes from a PRTG Server.
        /// </summary>
        /// <returns></returns>
        public List<Probe> GetProbes()
        {
            return GetObjects<Probe>(new ProbeParameters());
        }

        /// <summary>
        /// Retrieve probes from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns></returns>
        public List<Probe> GetProbes(Property property, object value)
        {
            return GetProbes(new SearchFilter(property, value));
        }

        /// <summary>
        /// Retrieve probes from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="operator">Operator to compare value and property value with.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns></returns>
        public List<Probe> GetProbes(Property property, FilterOperator @operator, string value)
        {
            return GetProbes(new SearchFilter(property, @operator, value));
        }

        /// <summary>
        /// Retrieve probes from a PRTG Server based on the values of multiple properties.
        /// </summary>
        /// <param name="filters">One or more filters used to limit search results.</param>
        /// <returns></returns>
        public List<Probe> GetProbes(params SearchFilter[] filters)
        {
            return GetObjects<Probe>(new ProbeParameters {SearchFilter = filters});
        }

        /// <summary>
        /// Retrieve probes from a PRTG Server using a custom set of parameters.
        /// </summary>
        /// <param name="parameters">A custom set of parameters used to retrieve PRTG Probes.</param>
        public List<Probe> GetProbes(ProbeParameters parameters)
        {
            return GetObjects<Probe>(parameters);
        }

        #endregion

        #region Channel

        /// <summary>
        /// Retrieve all channels of a sensor.
        /// </summary>
        /// <param name="sensorId">The ID of the sensor to retrieve channels for.</param>
        /// <returns></returns>
        public List<Channel> GetChannels(int sensorId)
        {
            var response = ExecuteRequest(XmlFunction.TableData, new ChannelParameters(sensorId));

            var items = response.Descendants("item").ToList();
            var ids = response.Descendants("objid").Select(v => v.Value);

            foreach (var item in items)
            {
                var id = Convert.ToInt32(item.Element("objid").Value);

                var properties = GetChannelProperties(sensorId, id);

                item.Add(properties.Nodes());
                item.Add(new XElement("injected_sensorId", sensorId));
            }

            return Data<Channel>.DeserializeList(response).Items;

            //response.FirstNode.AddAfterSelf(GetChannelProperties(sensorId))

            //var obj = GetObjects<Channel>(new ChannelParameters(sensorId));

            /*foreach (var o in obj)
            {
                o.SensorId = sensorId;
            }

            return obj;*/
            return null;
        }

        internal XElement GetChannelProperties(int sensorId, int channelId)
        {
            var parameters = new Parameters.Parameters()
            {
                [Parameter.Hjax] = true,
                [Parameter.Id] = sensorId,
                [Parameter.Channel] = channelId
            };

            var response = ExecuteRequest(HtmlFunction.ChannelEdit, parameters);

            return ChannelSettings.GetXml(response, channelId);
        }

        internal SensorSettings GetSensorSettings(int sensorId)
        {
            var parameters = new Parameters.Parameters
            {
                [Parameter.Id] = sensorId,
                [Parameter.ObjectType] = BaseType.Sensor
            };

            //we'll need to add support for dropdown lists too

            var response = ExecuteRequest(HtmlFunction.ObjectData, parameters);

            var blah = SensorSettings.GetXml(response, sensorId);

            var doc = new XDocument(blah);

            var aaaa = Data<SensorSettings>.DeserializeType(doc);

            //maybe instead of having an enum for my schedule and scanninginterval we have a class with a special getter that removes the <num>|component when you try and retrieve the property
            //the thing is, the enum IS actually dynamic - we need a list of valid options

            //todo: whenever we use _raw attributes dont we need to add xmlignore on the one that accesses it/

            return aaaa;
        }

        public void blah()
        {
            var parameters = new Parameters.Parameters
            {
                [Parameter.Custom] = new CustomParameter("limitmaxerror_0", ""),
                //prtgurl needs to throw an exception if you dont use a customparameter with parameter.custom and detect if it contains a list
                [Parameter.Id] = 2196
            };

            var response = ExecuteRequest(HtmlFunction.EditSettings, parameters);
        }

        

        #endregion

        #region Pause / Resume

        /// <summary>
        /// Mark a <see cref="SensorStatus.Down"/> sensor as <see cref="SensorStatus.DownAcknowledged"/>. If an acknowledged sensor returns to <see cref="SensorStatus.Up"/>, it will not be acknowledged when it goes down again.
        /// </summary>
        /// <param name="objectId">ID of the sensor to acknowledge.</param>
        /// <param name="message">Message to display on the acknowledged sensor.</param>
        /// <param name="duration">Duration (in minutes) to acknowledge the object for. If null, sensor will be paused indefinitely.</param>
        public void AcknowledgeSensor(int objectId, string message = null, int? duration = null)
        {
            var parameters = new Parameters.Parameters
            {
                [Parameter.Id] = objectId,
                //[Parameter.AcknowledgeMessage] = message,
            };

            if (message != null)
                parameters[Parameter.AcknowledgeMessage] = message;

            if (duration != null)
                parameters[Parameter.Duration] = duration;

            ExecuteRequest(CommandFunction.AcknowledgeAlarm, parameters);
        }

        /// <summary>
        /// Pause a PRTG Object (sensor, device, etc).
        /// </summary>
        /// <param name="objectId">ID of the object to pause.</param>
        /// <param name="pauseMessage">Message to display on the paused object.</param>
        /// <param name="durationMinutes">Duration (in minutes) to pause the object for. If null, object will be paused indefinitely.</param>
        public void Pause(int objectId, string pauseMessage = null, int? durationMinutes = null)
        {
            PauseParametersBase parameters;

            if (durationMinutes == null)
            {
                parameters = new PauseParameters(objectId);
            }
            else
            {
                parameters = new PauseForDurationParameters(objectId, (int)durationMinutes);
            }

            if (pauseMessage != null)
                parameters.PauseMessage = pauseMessage;

            ExecuteRequest(CommandFunction.Pause, parameters);
        }

        /// <summary>
        /// Resume a PRTG Object (e.g. sensor or device) from a Paused or Simulated Error state.
        /// </summary>
        /// <param name="objectId">ID of the object to resume.</param>
        public void Resume(int objectId)
        {
            var parameters = new PauseParameters(objectId, PauseAction.Resume);

            ExecuteRequest(CommandFunction.Pause, parameters);
        }

        /// <summary>
        /// Simulate an error state for a sensor.
        /// </summary>
        /// <param name="sensorId">ID of the sensor to simulate an error for.</param>
        public void SimulateError(int sensorId)
        {
            ExecuteRequest(CommandFunction.Simulate, new SimulateErrorParameters(sensorId));
        }

        #endregion

        #region ExecuteRequest

        private string ExecuteRequest(JsonFunction function, Parameters.Parameters parameters)
        {
            var url = new PrtgUrl(Server, Username, PassHash, function, parameters);

            var response = ExecuteRequest(url);

            return response;
        }

        private XDocument ExecuteRequest(XmlFunction function, Parameters.Parameters parameters)
        {
            var url = new PrtgUrl(Server, Username, PassHash, function, parameters);

            var response = ExecuteRequest(url);

            return XDocument.Parse(XDocumentHelpers.SanitizeXml(response));
        }

        private void ExecuteRequest(CommandFunction function, Parameters.Parameters parameters)
        {
            var url = new PrtgUrl(Server, Username, PassHash, function, parameters);

            var response = ExecuteRequest(url);
        }

        private string ExecuteRequest(HtmlFunction function, Parameters.Parameters parameters)
        {
            var url = new PrtgUrl(Server, Username, PassHash, function, parameters);

            var response = ExecuteRequest(url);

            return response;
        }

        private string ExecuteRequest(PrtgUrl url)
        {
            string response;

            try
            {
                var client = new WebClient();
                response = client.DownloadString(url.Url);
            }
            catch (WebException ex)
            {
                if (ex.Response == null)
                    throw;

                var webResponse = (HttpWebResponse)ex.Response;

                if (webResponse.StatusCode == HttpStatusCode.BadRequest)
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream(), Encoding.UTF8))
                    {
                        response = reader.ReadToEnd();
                        
                        var xDoc = XDocument.Parse(response);
                        var errorMessage = xDoc.Descendants("error").First().Value;

                        throw new PrtgRequestException($"PRTG was unable to complete the request. The server responded with the following error: {errorMessage}", ex);
                    }
                }

                throw;
            }

            return response;
        }

        #endregion

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
        public void SetObjectProperty(int sensorId, int channelId, ChannelProperty property, object value)
        {
            var customParams = GetChannelSetObjectPropertyCustomParams(channelId, property, value);

            var parameters = new Parameters.Parameters
            {
                [Parameter.Custom] = customParams,
                [Parameter.Id] = sensorId
            };

            ExecuteRequest(HtmlFunction.EditSettings, parameters);
        }

        private List<CustomParameter> GetChannelSetObjectPropertyCustomParams(int channelId, ChannelProperty property, object value)
        {
            bool valAsBool;
            var valIsBool = bool.TryParse(value.ToString(), out valAsBool);

            List<CustomParameter> customParams = new List<CustomParameter>();

            if (valIsBool)
            {
                if (valAsBool)
                {
                    value = 1;
                }

                else //if we're disabling a property, check if there are values dependent on us. if so, disable them too!
                {
                    value = 0;

                    var associatedProperties = property.GetDependentProperties<ChannelProperty>();

                    customParams.AddRange(
                        associatedProperties.Select(prop => Channel.CreateCustomParameter(prop, channelId, string.Empty)));
                }
            }
            else //if we're enabling a property, check if there are values we depend on. if so, enable them!
            {
                var props = property.GetEnumAttribute<DependentPropertyAttribute>();

                if (props != null)
                {
                    customParams.Add(Channel.CreateCustomParameter(props.Name.ToEnum<ChannelProperty>(), channelId, "1"));
                }
            }

            customParams.Add(Channel.CreateCustomParameter(property, channelId, value));

            return customParams;
        }

        #endregion

        private void SetObjectProperty<T>(SetObjectSettingParameters<T> parameters)
        {
            ExecuteRequest(CommandFunction.SetObjectProperty, parameters);
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
            var response = ExecuteRequest(XmlFunction.GetObjectProperty, parameters);

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

        #endregion

        #region Miscellaneous

        //todo: check all arguments we can in this file and make sure we validate input. when theres a chain of methods, validate on the inner most one except if we pass a parameter object, in which case validate both

        /// <summary>
        /// Request an object or any children of an object refresh themselves immediately.
        /// </summary>
        /// <param name="objectId">The ID of the sensor, or the ID of a Probe, Group or Device whose child sensors should be refreshed.</param>
        public void CheckNow(int objectId)
        {
            var parameters = new Parameters.Parameters()
            {
                [Parameter.Id] = objectId
            };

            ExecuteRequest(CommandFunction.ScanNow, parameters);
        }

        /// <summary>
        /// Automatically create sensors under an object based on the object's (or it's children's) device type.
        /// </summary>
        /// <param name="objectId">The object to run Auto-Discovery for (such as a device or group).</param>
        public void AutoDiscover(int objectId)
        {
            var parameters = new Parameters.Parameters()
            {
                [Parameter.Id] = objectId
            };
            
            ExecuteRequest(CommandFunction.DiscoverNow, parameters);
        }

        /// <summary>
        /// Modify the position of an object up or down within the PRTG User Interface.
        /// </summary>
        /// <param name="objectId">The object to reposition.</param>
        /// <param name="position">The direction to move in.</param>
        public void SetPosition(int objectId, Position position)
        {
            var parameters = new Parameters.Parameters()
            {
                [Parameter.Id] = objectId,
                [Parameter.NewPos] = position
            };

            ExecuteRequest(CommandFunction.SetPosition, parameters);
        }

        //clone a sensor or group
        public void Clone(int id, string cloneName, int targetLocation)
        {
            if (cloneName == null)
                throw new ArgumentNullException(nameof(cloneName));

            var parameters = new Parameters.Parameters()
            {
                [Parameter.Id] = id,
                [Parameter.Name] = cloneName,
                [Parameter.TargetId] = targetLocation
            };

            //todo: need to implement simulateerrorparameters or get rid of it?

            ExecuteRequest(CommandFunction.DuplicateObject, parameters);

            //todo: apparently the server replies with the url of the new page, which we could parse into an object containing the id of the new object and return from this method
        }

        //clone a device
        public void Clone(int id, string cloneName, string host, int targetLocation)
        {
            if (cloneName == null)
                throw new ArgumentNullException(nameof(cloneName));

            if (host == null)
                throw new ArgumentNullException(nameof(host));

            var parameters = new Parameters.Parameters()
            {
                [Parameter.Id] = id,
                [Parameter.Name] = cloneName,
                [Parameter.Host] = host,
                [Parameter.TargetId] = targetLocation
            };

            //todo: apparently the server replies with the url of the new page, which we could parse into an object containing the id of the new object and return from this method
        }

        /// <summary>
        /// Permanently delete an object from PRTG. This cannot be undone.
        /// </summary>
        /// <param name="id">ID of the object to delete.</param>
        public void Delete(int id)
        {
            var parameters = new Parameters.Parameters
            {
                [Parameter.Id] = id,
                [Parameter.Approve] = 1
            };

            ExecuteRequest(CommandFunction.DeleteObject, parameters);
        }

        /// <summary>
        /// Rename an object.
        /// </summary>
        /// <param name="objectId">ID of the object to rename.</param>
        /// <param name="name">New name to give the object.</param>
        public void Rename(int objectId, string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            var parameters = new Parameters.Parameters
            {
                [Parameter.Id] = objectId,
                [Parameter.Value] = name
            };

            ExecuteRequest(CommandFunction.Rename, parameters);
        }

        public ServerStatus GetStatus()
        {
            var parameters = new Parameters.Parameters
            {
                [Parameter.Id] = 0
            };

            var response = ExecuteRequest(XmlFunction.GetStatus, parameters);

            return Data<ServerStatus>.DeserializeType(response);
        }

        #endregion
    }
}
