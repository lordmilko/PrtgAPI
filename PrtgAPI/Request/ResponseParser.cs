using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using PrtgAPI.Helpers;
using PrtgAPI.Objects.Deserialization;
using PrtgAPI.Objects.Shared;
using PrtgAPI.Parameters;

namespace PrtgAPI.Request
{
    static class ResponseParser
    {
        #region Notifications

        internal static XElement GroupNotificationActionProperties(XElement xml)
        {
            var regex = new Regex("(.+)_(\\d+)");

            var properties = xml.Descendants().Where(d => regex.Match(d.Name.ToString()).Success).ToList();

            var categorized = properties.Select(p =>
            {
                var id = Convert.ToInt32(regex.Replace(p.Name.ToString(), "$2"));

                if (Enum.IsDefined(typeof(NotificationType), id))
                {
                    var e = (NotificationType)id;

                    return new
                    {
                        Name = regex.Replace(p.Name.ToString(), "$1"),
                        Category = e,
                        Value = p.Value
                    };
                }

                return null;
            }).Where(p => p != null).ToList();

            var grouped = categorized.GroupBy(c => c.Category).ToList();

            var newXml = grouped.Select(g => new XElement($"category_{g.Key.ToString().ToLower()}",
                g.Select(i => new XElement(i.Name, i.Value)))
            );

            properties.Remove();

            xml.Add(newXml);

            return xml;
        }

        internal static List<NotificationTrigger> ParseNotificationTriggerResponse(int objectId, XDocument xmlResponse)
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

        internal static List<IGrouping<int, NotificationAction>> GroupTriggerActions(List<NotificationTrigger> triggers)
        {
            var actions = triggers.SelectMany(
                t => t.GetType().GetProperties().Where(p => p.PropertyType == typeof(NotificationAction)).Select(p => (NotificationAction)p.GetValue(t)
            ).Where(a => a != null && a.Id != -1)).GroupBy(a => a.Id).ToList();

            return actions;
        }

        #endregion
        #region Add Objects

        internal static List<T> ExceptTableObject<T>(List<T> before, List<T> after) where T : SensorOrDeviceOrGroupOrProbe
        {
            var beforeIds = before.Select(b => b.Id).ToList();

            return after.Where(a => !beforeIds.Contains(a.Id)).ToList();
        }

        internal static List<NotificationTrigger> ExceptTrigger(List<NotificationTrigger> before, List<NotificationTrigger> after, TriggerParameters parameters)
        {
            return after.Where(a => !before.Any(b => a.ObjectId == b.ObjectId && a.SubId == b.SubId) && a.OnNotificationAction.Id == parameters.OnNotificationAction.Id).ToList();
        }

        #endregion
        #region Clone Object

        internal static string CloneRequestParser(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.BadRequest)
                return null;

            var message = response.RequestMessage.RequestUri.ToString();

            if (message.Contains("the object is currently not valid"))
                RequestEngine.SetErrorUrlAsRequestUri(response);

            return message;
        }

        internal static int CloneResponseParser(string response)
        {
            var decodedResponse = HttpUtility.UrlDecode(response);

            var id = Convert.ToInt32(Regex.Replace(decodedResponse, "(.+id=)(\\d+)(&.*)?", "$2"));

            return id;
        }

        #endregion
        #region Get Object Properties

        internal static T GetTypedProperty<T>(object val)
        {
            if (val is T)
                return (T)val;

            var underlying = Nullable.GetUnderlyingType(typeof(T));

            if (underlying != null)
            {
                if (val == null)
                    return default(T);

                return (T)val;
            }

            var typeName = val?.GetType().Name ?? "null";

            throw new InvalidCastException($"Cannot convert a value of type '{typeName}' to type '{typeof(T)}'");
        }

        internal static string ValidateRawObjectProperty(XDocument response, GetObjectPropertyRawParameters parameters)
        {
            var value = response.Descendants("result").First().Value;

            if (value == "(Property not found)")
                throw new PrtgRequestException($"PRTG was unable to complete the request. A value for property '{parameters.Name}' could not be found.");

            return value;
        }

        internal static T GetObjectProperties<T>(string response)
        {
            var xml = ObjectSettings.GetXml(response);
            var xDoc = new XDocument(xml);

            var items = XmlDeserializer<T>.DeserializeType(xDoc);

            return items;
        }

        #endregion
        #region Set Object Properties

        internal static string ParseSetObjectPropertyUrl(int numObjectIds, HttpResponseMessage response)
        {
            if (numObjectIds > 1)
            {
                if (response.RequestMessage?.RequestUri?.AbsolutePath == "/error.htm")
                    RequestEngine.SetErrorUrlAsRequestUri(response);
            }

            return null;
        }

        #endregion
        #region System Administration

        internal static void UpdateProbeStatus(List<RestartProbeProgress> probes, List<Log> logs)
        {
            foreach (var probe in probes)
            {
                if (!probe.Disconnected)
                {
                    //If we got a log saying the probe disconnected, or the probe was already disconnected, flag it as having disconnected
                    if (logs.Any(log => log.Status == LogStatus.Disconnected && log.Id == probe.Id) || probe.InitialCondition == ProbeStatus.Disconnected)
                        probe.Disconnected = true;
                }
                if (probe.Disconnected && !probe.Reconnected) //If it's already disconnected and hasn't reconnected, check its status
                {
                    //If the probe has disconnected and we see it's reconnected, flag it as such. If it was already disconnected though,
                    //it'll never reconnect, so let it through
                    if (logs.Any(log => log.Status == LogStatus.Connected && log.Id == probe.Id) || probe.InitialCondition == ProbeStatus.Disconnected)
                        probe.Reconnected = true;
                }
            }
        }

        #endregion
        #region Sensor History

        internal static void ValidateSensorHistoryResponse(string response)
        {
            if (response == "Not enough monitoring data")
                throw new PrtgRequestException($"PRTG was unable to complete the request. The server responded with the following error: {response}");
        }

        internal static List<SensorHistoryData> ParseSensorHistoryResponse(List<SensorHistoryData> items, int sensorId)
        {
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

        #endregion
        #region Sensor Targets

        internal static string GetSensorTargetTmpId(HttpResponseMessage message)
        {
            var id = Regex.Replace(message.RequestMessage.RequestUri.ToString(), "(.+)(tmpid=)(.+)", "$3");

            return id;
        }

        internal static void ValidateSensorTargetProgressResult(SensorTargetProgress p)
        {
            if (p.TargetUrl.StartsWith("addsensorfailed"))
            {
                var parts = UrlHelpers.CrackUrl(p.TargetUrl);
                var message = parts["errormsg"];

                if (message.StartsWith("Incomplete connection settings"))
                    throw new PrtgRequestException("Failed to retrieve data from device; required credentials for sensor type may be missing. See PRTG UI for further details.");

                throw new PrtgRequestException($"An exception occurred while trying to resolve sensor targets: {message}");
            }
        }

        #endregion
        #region Device Templates

        internal static List<DeviceTemplate> GetTemplates(string response)
        {
            var checkboxes = ObjectSettings.GetInput(response).Where(
                t => t.Type == Html.InputType.Checkbox && t.Name == Parameter.DeviceTemplate.GetDescription()
            ).ToList();

            var templates = checkboxes.Select(c => new DeviceTemplate(c.Value)).ToList();

            return templates;
        }

        #endregion

        internal static string ValidateHasContent(HttpResponseMessage message)
        {
            var responseText = message.Content.ReadAsStringAsync().Result;

            if (responseText.Contains("class=\"no-content\""))
                return "[]";

            return responseText;
        }

        internal static async Task<string> ValidateHasContentAsync(HttpResponseMessage message)
        {
            var responseText = await message.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (responseText.Contains("class=\"no-content\""))
                return "[]";

            return responseText;
        }
    }
}
