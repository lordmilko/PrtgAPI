using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using Prtg.Parameters;

using Prtg.Helpers;

namespace Prtg
{
    /// <summary>
    /// Makes API requests against a PRTG Network Monitor server.
    /// </summary>
    public class PrtgRequest
    {
        /// <summary>
        /// Gets the PRTG server API requests will be made against.
        /// </summary>
        public string Server { get; }

        /// <summary>
        /// Gets the Username that will be used to authenticate against PRTG.
        /// </summary>
        public string Username { get; }

        private readonly string passhash;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Prtg.PrtgRequest"/> class.
        /// </summary>
        public PrtgRequest(string server, string username, string pass, AuthMode authMode = AuthMode.Password)
        {
            Server = server;
            Username = username;

            passhash = authMode == AuthMode.Password ? GetPassHash(pass) : pass;
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

            //check its not an error

            var data = Data<T>.Deserialize(response);

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

        #endregion 

        #region ContentFilter

        /// <summary>
        /// Retrieve sensors from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns></returns>
        public List<Sensor> GetSensors(Property property, string value)
        {
            return GetSensors(new ContentFilter(property, value));
        }

        /// <summary>
        /// Retrieve sensors from a PRTG Server based on the value of a certain property.
        /// </summary>
        /// <param name="property">Property to search against.</param>
        /// <param name="operator">Operator to compare value and property value with.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns></returns>
        public List<Sensor> GetSensors(Property property, FilterOperator @operator, string value)
        {
            return GetSensors(new ContentFilter(property, @operator, value));
        }

        /// <summary>
        /// Retrieve sensors from a PRTG Server based on the values of multiple properties.
        /// </summary>
        /// <param name="filters">One or more filters used to limit search results.</param>
        /// <returns></returns>
        public List<Sensor> GetSensors(params ContentFilter[] filters)
        {
            return GetSensors(new SensorParameters { ContentFilter = filters });
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
        public List<Device> GetDevices(Property property, string value)
        {
            return GetDevices(new ContentFilter(property, value));
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
            return GetDevices(new ContentFilter(property, @operator, value));
        }

        /// <summary>
        /// Retrieve devices from a PRTG Server based on the values of multiple properties.
        /// </summary>
        /// <param name="filters">One or more filters used to limit search results.</param>
        /// <returns></returns>
        public List<Device> GetDevices(params ContentFilter[] filters)
        {
            return GetDevices(new DeviceParameters { ContentFilter = filters });
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
        public List<Group> GetGroups(Property property, string value)
        {
            return GetGroups(new ContentFilter(property, value));
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
            return GetGroups(new ContentFilter(property, @operator, value));
        }

        /// <summary>
        /// Retrieve groups from a PRTG Server based on the values of multiple properties.
        /// </summary>
        /// <param name="filters">One or more filters used to limit search results.</param>
        /// <returns></returns>
        public List<Group> GetGroups(params ContentFilter[] filters)
        {
            return GetGroups(new GroupParameters { ContentFilter = filters });
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
        public List<Probe> GetProbes(Property property, string value)
        {
            return GetProbes(new ContentFilter(property, value));
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
            return GetProbes(new ContentFilter(property, @operator, value));
        }

        /// <summary>
        /// Retrieve probes from a PRTG Server based on the values of multiple properties.
        /// </summary>
        /// <param name="filters">One or more filters used to limit search results.</param>
        /// <returns></returns>
        public List<Probe> GetProbes(params ContentFilter[] filters)
        {
            return GetObjects<Probe>(new ProbeParameters {ContentFilter = filters});
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

        #endregion

        #region ExecuteRequest

        private string ExecuteRequest(JsonFunction function, Parameters.Parameters parameters)
        {
            var url = new PrtgUrl(Server, Username, passhash, function, parameters);

            var response = ExecuteRequest(url);

            return response;
        }

        private XDocument ExecuteRequest(XmlFunction function, Parameters.Parameters parameters)
        {
            var url = new PrtgUrl(Server, Username, passhash, function, parameters);

            var response = ExecuteRequest(url);

            return XDocument.Parse(XDocumentHelpers.SanitizeXml(response));
        }

        private void ExecuteRequest(CommandFunction function, Parameters.Parameters parameters)
        {
            var url = new PrtgUrl(Server, Username, passhash, function, parameters);

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

        /// <summary>
        /// Modify basic object settings (name, tags, priority, etc.) for a PRTG Object.
        /// </summary>
        /// <param name="objectId">ID of the object to modify.</param>
        /// <param name="name">The setting to whose value will be overwritten.</param>
        /// <param name="value">Value of the setting to apply.</param>
        public void SetObjectProperty(int objectId, BasicObjectSetting name, int value)
        {
            SetObjectProperty(objectId, name, value.ToString());
        }

        #endregion

        #region ScanningInterval

        /// <summary>
        /// Modify scanning interval settings for a PRTG Object.
        /// </summary>
        /// <param name="objectId">ID of the object to modify.</param>
        /// <param name="name">The setting to whose value will be overwritten.</param>
        /// <param name="value">Value of the setting to apply.</param>
        private void SetObjectProperty(int objectId, ScanningInterval name, string value)
        {
            SetObjectProperty(new SetObjectSettingParameters<ScanningInterval>(objectId, name, value));
        }

        /// <summary>
        /// Modify scanning interval settings for a PRTG Object.
        /// </summary>
        /// <param name="objectId">ID of the object to modify.</param>
        /// <param name="name">The setting to whose value will be overwritten.</param>
        /// <param name="value">Value of the setting to apply.</param>
        private void SetObjectProperty(int objectId, ScanningInterval name, int value)
        {
            SetObjectProperty(objectId, name, value.ToString());
        }

        #endregion

        #region SensorDisplay

        /// <summary>
        /// Modify sensor display settings for a PRTG Object.
        /// </summary>
        /// <param name="objectId">ID of the object to modify.</param>
        /// <param name="name">The setting to whose value will be overwritten.</param>
        /// <param name="value">Value of the setting to apply.</param>
        private void SetObjectProperty(int objectId, SensorDisplay name, string value)
        {
            SetObjectProperty(new SetObjectSettingParameters<SensorDisplay>(objectId, name, value));
        }

        /// <summary>
        /// Modify sensor display settings for a PRTG Object.
        /// </summary>
        /// <param name="objectId">ID of the object to modify.</param>
        /// <param name="name">The setting to whose value will be overwritten.</param>
        /// <param name="value">Value of the setting to apply.</param>
        private void SetObjectProperty(int objectId, SensorDisplay name, int value)
        {
            SetObjectProperty(objectId, name, value.ToString());
        }

        #endregion

        #region ExeScriptSetting

        /// <summary>
        /// Modify EXE/Script settings for a PRTG Object.
        /// </summary>
        /// <param name="objectId">ID of the object to modify.</param>
        /// <param name="name">The setting to whose value will be overwritten.</param>
        /// <param name="value">Value of the setting to apply.</param>
        private void SetObjectProperty(int objectId, ExeScriptSetting name, string value)
        {
            SetObjectProperty(new SetObjectSettingParameters<ExeScriptSetting>(objectId, name, value));
        }

        /// <summary>
        /// Modify EXE/Script settings for a PRTG Object.
        /// </summary>
        /// <param name="objectId">ID of the object to modify.</param>
        /// <param name="name">The setting to whose value will be overwritten.</param>
        /// <param name="value">Value of the setting to apply.</param>
        private void SetObjectProperty(int objectId, ExeScriptSetting name, int value)
        {
            SetObjectProperty(objectId, name, value.ToString());
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
        /// <typeparam name="T">The return type suggested by the documentation for the <see cref="Prtg.BasicObjectSetting"/> specified in <paramref name="name"/>.</typeparam>
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
        /// <typeparam name="T">The return type suggested by the documentation for the <see cref="Prtg.ScanningInterval"/> specified in <paramref name="name"/>.</typeparam>
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
        /// <typeparam name="T">The return type suggested by the documentation for the <see cref="Prtg.SensorDisplay"/> specified in <paramref name="name"/>.</typeparam>
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
        /// <typeparam name="T">The return type suggested by the documentation for the <see cref="Prtg.ExeScriptSetting"/> specified in <paramref name="name"/>.</typeparam>
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
    }
}
