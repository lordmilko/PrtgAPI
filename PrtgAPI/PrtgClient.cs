using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using PrtgAPI.Helpers;
using PrtgAPI.Parameters;

namespace PrtgAPI
{
    /// <summary>
    /// Makes API requests against a PRTG Network Monitor server.
    /// </summary>
    public class PrtgClient
    {
        /// <summary>
        /// Gets the PRTG server API requests will be made against.
        /// </summary>
        public string Server { get; }

        /// <summary>
        /// Gets the Username that will be used to authenticate against PRTG.
        /// </summary>
        public string Username { get; }

        public string PassHash { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:PrtgAPI.PrtgClient"/> class.
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
            Debug.WriteLine(response.ToString());
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

            try
            {
                var data = Data<T>.Deserialize(response);

                return data.Items;
            }
            catch (InvalidOperationException ex)
            {
                Exception ex1 = null;

                try
                {
                    ex1 = GetInvalidXml(response, ex, typeof(T));
                }
                catch
                {
                }

                if (ex1 != null)
                    throw ex1;
                else
                    throw;
            }

        }

        private Exception GetInvalidXml(XDocument response, InvalidOperationException ex, Type type)
        {
            var stream = GetStream(response);

            var xmlReader = (XmlReader)new XmlTextReader(stream)
            {
                WhitespaceHandling = WhitespaceHandling.Significant,
                Normalization = true,
                XmlResolver = null
            };

            var regex = new Regex("(.+\\()(.+)(, )(.+)(\\).+)");
            var line = Convert.ToInt32(regex.Replace(ex.Message, "$2"));
            var position = Convert.ToInt32(regex.Replace(ex.Message, "$4"));

            while (xmlReader.Read())
            {
                IXmlLineInfo xmlLineInfo = (IXmlLineInfo)xmlReader;

                if (xmlLineInfo.LineNumber == line - 1)
                {
                    var xml = xmlReader.ReadOuterXml();

                    var prevSpace = xml.LastIndexOf(' ', position) + 1;
                    var nextSpace = xml.IndexOf(' ', position);

                    var length = nextSpace - prevSpace;

                    var str = length > 0 ? xml.Substring(prevSpace, length) : xml;

                    return new XmlDeserializationException(type, str, ex);
                }
            }

            return null;
        }

        private Stream GetStream(XDocument response)
        {
            var stream = new MemoryStream();
            response.Save(stream);
            stream.Position = 0;

            return stream;
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

        #region Pause / Resume

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
                    using (var reader = new StreamReader(webResponse.GetResponseStream(), System.Text.Encoding.UTF8))
                    {
                        response = reader.ReadToEnd();
                        
                        var xDoc = XDocument.Parse(response);
                        var errorMessage = xDoc.Descendants("error").First().Value;

                        throw new PrtgRequestException("PRTG was unable to complete the request. The server responded with the following error: " + errorMessage, ex);
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

        public List<Channel> GetChannels(int sensorId)
        {
            return GetObjects<Channel>(new ChannelParameters(sensorId));
        }

        /// <summary>
        /// Modify channel properties for a PRTG Sensor.
        /// </summary>
        /// <param name="sensorId">The ID of the sensor whose channels should be modified.</param>
        /// <param name="channelId">The ID of the channel to modify.</param>
        /// <param name="property">The property of the channel to modify</param>
        /// <param name="value">The value to set the channel's property to.</param>
        public void SetObjectProperty(int sensorId, int channelId, ChannelProperty property, object value)
        {
            SetObjectProperty(new SetChannelSettingParameters(sensorId, channelId, property, value));
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
            var parameters = new Parameters.Parameters()
            {
                [Parameter.Id] = id,
                [Parameter.Name] = cloneName,
                [Parameter.Host] = host,
                [Parameter.TargetId] = targetLocation
            };

            //todo: apparently the server replies with the url of the new page, which we could parse into an object containing the id of the new object and return from this method
        }

        //delete an object
        public void Delete(int id)
        {
            var parameters = new Parameters.Parameters()
            {
                [Parameter.Id] = id,
                [Parameter.Approve] = 1
            };

            ExecuteRequest(CommandFunction.DeleteObject, parameters);
        }
    }
}
