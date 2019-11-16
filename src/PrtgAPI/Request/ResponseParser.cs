using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using PrtgAPI.Request.Serialization;
using PrtgAPI.Parameters;
using PrtgAPI.Parameters.Helpers;
using PrtgAPI.Reflection;
using PrtgAPI.Schedules;
using PrtgAPI.Utilities;

namespace PrtgAPI.Request
{
    static class ResponseParser
    {
        /// <summary>
        /// Apply a modification function to each element of a response.
        /// </summary>
        /// <typeparam name="T">The type of objects returned by the response.</typeparam>
        /// <param name="objects">The collection of objects to amend.</param>
        /// <param name="action">A modification function to apply to each element of the collection.</param>
        /// <returns>A collection of modified objects.</returns>
        internal static List<T> Amend<T>(List<T> objects, Action<T> action)
        {
            foreach (var obj in objects)
            {
                action(obj);
            }

            return objects;
        }

        /// <summary>
        /// Apply a modification function to the properties of an object.
        /// </summary>
        /// <typeparam name="T">The type of object returned by the response.</typeparam>
        /// <param name="obj">The object to amend.</param>
        /// <param name="action">A modification function to apply to the object.</param>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        internal static T Amend<T>(T obj, Action<T> action)
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
        internal static TRet Amend<TSource, TRet>(TSource obj, Func<TSource, TRet> action)
        {
            var val = action(obj);

            return val;
        }

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
                        p.Value
                    };
                }

                return null;
            }).Where(p => p != null).ToList();

            var grouped = categorized.GroupBy(c => c.Category).ToList();

            List<XElement> newXml = new List<XElement>();

            foreach(var group in grouped)
            {
                if (group.Count() == 1 && group.Single().Name == "injected_active")
                    continue;

                newXml.Add(new XElement($"category_{group.Key.ToString().ToLower()}",
                    group.Select(i => new XElement(i.Name, i.Value))
                ));
            }

            properties.Remove();

            xml.Add(newXml);

            return xml;
        }

        internal static List<NotificationTrigger> ParseNotificationTriggerResponse(Either<IPrtgObject, int> objectOrId, XDocument xmlResponse)
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
                    o.ObjectId = objectOrId.GetId();
                }
            );

            return triggers;
        }

        internal static List<IGrouping<int, NotificationAction>> GroupTriggerActions(List<NotificationTrigger> triggers)
        {
            var actions = triggers.SelectMany(
                t => t.GetTypeCache().Properties.Where(p => p.Property.PropertyType == typeof(NotificationAction)).Select(p => (NotificationAction)p.Property.GetValue(t)
            ).Where(a => a != null && a.Id != -1)).GroupBy(a => a.Id).ToList();

            return actions;
        }

        internal static IEnumerable<IGrouping<int?, NotificationAction>> GroupActionSchedules(List<NotificationAction> actions)
        {
            var actionsWithSchedules = actions
                .GroupBy(a => a.lazyScheduleStr != null ? (int?)PrtgObject.GetId(a.lazyScheduleStr) : null).ToList();

            foreach (var group in actionsWithSchedules)
            {
                if (group.Key == -1)
                {
                    foreach (var action in group)
                        action.schedule = new LazyValue<Schedule>(action.lazyScheduleStr, () => new Schedule(action.lazyScheduleStr));
                }
                else
                    yield return group;
            }
        }

        #endregion
        #region Schedules

        internal static void LoadTimeTable(Schedule schedule, string response)
        {
            var input = HtmlParser.Default.GetInput(response, HtmlParser.DefaultBackwardsMatchRegex).Where(i => i.Name == "timetable").ToList();

            //If user is read only inputs is empty
            if (input.Count > 0)
                schedule.TimeTable = new TimeTable(input);
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

        internal static PrtgResponse CloneRequestParser(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.BadRequest)
                return null;

            var message = response.RequestMessage.RequestUri.ToString();

            //todo: does this work with other languages? Not sure how to replicate it
            if (message.Contains("the object is currently not valid"))
                RequestEngine.SetErrorUrlAsRequestUri(response);

            return message;
        }

        internal static int CloneResponseParser(PrtgResponse response)
        {
            var decodedResponse = WebUtility.UrlDecode(response.StringValue);

            if (decodedResponse.Contains(CommandFunction.DuplicateObject.GetDescription()))
                throw new PrtgRequestException("PRTG successfully cloned the object, however failed to return a response containing the location of the new object. This violates the Clone Object API contract and indicates a bug in PRTG.");

            var id = Convert.ToInt32(Regex.Replace(decodedResponse, "(.+?id=)(\\d+)(&.*)?", "$2"));

            return id;
        }

        #endregion
        #region Get Object Properties

        internal static string ParseGetObjectPropertyResponse(string response, string property)
        {
            //Responses may contain invalid XML characters if show=text was not specified. Abstract
            //away this behavior by reconstructing the XML (XElement will automatically escape invalid
            //characters)

            var versionRegex = "<version>(.+)<\\/version>";
            var resultRegex = "<result>(.+)<\\/result>";

            var versionMatch = Regex.Match(response, versionRegex);
            var resultMatch = Regex.Match(response, resultRegex);

            if (versionMatch.Success && resultMatch.Success)
            {
                var version = Regex.Replace(versionMatch.Value, versionRegex, "$1", RegexOptions.Multiline);
                var result = Regex.Replace(resultMatch.Value, resultRegex, "$1", RegexOptions.Multiline);

                var newResult = WebUtility.HtmlDecode(result);

                var xml = new XElement("prtg",
                    new XElement("version", version),
                    new XElement("result", newResult)
                );

                return xml.ToString();
            }

            return null;
        }

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

            if (typeof(T).IsArray && val == null)
            {
                return (T)Activator.CreateInstance(typeof(T), new object[] { 0 });
            }

            var typeName = val?.GetType().Name ?? "null";

            throw new InvalidCastException($"Cannot convert a value of type '{typeName}' to type '{typeof(T)}'.");
        }

        internal static string ValidateRawObjectProperty(XDocument response, GetObjectPropertyRawParameters parameters)
        {
            var value = response.Descendants("result").First().Value;

            //Does not work when the server's language is not English. API XmlDocs document this fact.
            if (value == "(Property not found)")
                throw new PrtgRequestException($"PRTG was unable to complete the request. A value for property '{parameters.Name}' could not be found.");

            return value;
        }

        internal static T GetObjectProperties<T>(PrtgResponse response, XmlEngine xmlEngine, ObjectProperty mandatoryProperty)
        {
            var xml = HtmlParser.Default.GetXml(response);
            var xDoc = new XDocument(xml);

            //If the response does not contain the mandatory property, we are executing as a read only user, and
            //should return null

            var name = HtmlParser.DefaultPropertyPrefix + ObjectPropertyParser.GetObjectPropertyName(mandatoryProperty).TrimEnd('_');

            if (xDoc.Descendants(name).ToList().Count > 0)
            {
                var items = xmlEngine.DeserializeObject<T>(xDoc.CreateReader());

                return items;
            }

            return default(T);
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

        internal static void UpdateProbeStatus(List<ProbeRestartProgress> probes, List<Log> logs)
        {
            foreach (var probe in probes)
            {
                if (!probe.Disconnected)
                {
                    //If we got a log saying the probe disconnected, or the probe was already disconnected, flag it as having disconnected
                    if (logs.Any(log => log.Status == LogStatus.Disconnected && log.Id == probe.Id) || probe.InitialStatus == ProbeStatus.Disconnected)
                        probe.Disconnected = true;
                }
                if (probe.Disconnected && !probe.Reconnected) //If it's already disconnected and hasn't reconnected, check its status
                {
                    //If the probe has disconnected and we see it's reconnected, flag it as such. If it was already disconnected though,
                    //it'll never reconnect, so let it through
                    if (logs.Any(log => log.Status == LogStatus.Connected && log.Id == probe.Id) || probe.InitialStatus == ProbeStatus.Disconnected)
                        probe.Reconnected = true;
                }
            }
        }

        #endregion
        #region Sensor History

        internal static PrtgResponse GetSensorHistoryResponse(HttpResponseMessage responseMessage, LogLevel logLevel, bool isDirty)
        {
            if (RequestEngine.NeedsStringResponse(responseMessage, logLevel, isDirty))
            {
                var response = responseMessage.Content.ReadAsStringAsync().Result;

                if (!response.Contains("<"))
                    throw new PrtgRequestException($"PRTG was unable to complete the request. The server responded with the following error: {response.EnsurePeriod()}");

                return new PrtgResponse(response, isDirty);
            }

            return new PrtgResponse(new SensorHistoryStream(responseMessage.Content.ReadAsStreamAsync().Result));
        }

        internal static async Task<PrtgResponse> GetSensorHistoryResponseAsync(HttpResponseMessage responseMessage, LogLevel logLevel, bool isDirty)
        {
            if (RequestEngine.NeedsStringResponse(responseMessage, logLevel, isDirty))
            {
                var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!response.Contains("<"))
                    throw new PrtgRequestException($"PRTG was unable to complete the request. The server responded with the following error: {response.EnsurePeriod()}");

                return new PrtgResponse(response, isDirty);
            }

            return new PrtgResponse(new SensorHistoryStream(await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false)));
        }

        internal static List<SensorHistoryRecord> ParseSensorHistoryResponse(List<SensorHistoryRecord> items, int sensorId)
        {
            foreach (var history in items)
            {
                history.SensorId = sensorId;

                foreach (var record in history.ChannelRecords)
                {
                    SetNewSensorHistoryChannelName(history, record);

                    record.Value = GetSensorHistoryChannelValue(history, record);
                    record.DateTime = history.DateTime;
                    record.SensorId = sensorId;
                }
            }

            return items;
        }

        private static void SetNewSensorHistoryChannelName(SensorHistoryRecord history, ChannelHistoryRecord record)
        {
            record.Name = record.Name.Replace(" ", "");

            string newName;

            if (TryGetNewSensorHistoryChannelName(record.Name, out newName))
            {
                if (history.ChannelRecords.Where(r => r != record).All(r =>
                {
                    //Is this new name unique amongst all the other channel names (after updating their names)?
                    string other;

                    if (!TryGetNewSensorHistoryChannelName(r.Name.Replace(" ", ""), out other))
                        other = r.Name;

                    return other != newName;
                }))
                {
                    record.Name = newName;
                }
            }
        }

        private static bool TryGetNewSensorHistoryChannelName(string oldName, out string newName)
        {
            var regex = new Regex("^(.+)(\\(.+\\))$");

            if (regex.Match(oldName).Success)
            {
                newName = regex.Replace(oldName, "$1");

                return true;
            }

            newName = null;
            return false;
        }

        private static double? GetSensorHistoryChannelValue(SensorHistoryRecord history, ChannelHistoryRecord record)
        {
            //PRTG does not return a raw record if the sensor did not return a value
            //(such as because it was in an error state)
            var rawRecord = history.ChannelRecordsRaw.FirstOrDefault(r => r.ChannelId == record.ChannelId);

            return rawRecord?.Value;
        }

        #endregion
        #region Sensor Targets

        internal static ContinueAddSensorQueryParameters GetProcessSensorQueryParameters(string content, string sensorType, SensorMultiQueryTargetParameters queryParameters)
        {
            //We didn't automatically redirect to addsensor3.htm, indicating that we are a sensor type
            //like Oracle Tablespace that wants to prefill some information prior to loading addsensor4.htm.
            //Ignore this information request; these fields will automatically be included in our dynamic sensor parameters (if applicable)

            var dictionary = HtmlParser.Default.GetDictionary(content);
            
            var htmlParser = new HtmlParser
            {
                StandardNameRegex = "(.+?name=\")(.+?_*)(\".+)"
            };

            var excluded = new[] { "id", "tmpid" };

            var dict = htmlParser.GetDictionary(content);

            string tmpIdStr;
            int tmpId;

            if (dict.TryGetValue("tmpid", out tmpIdStr))
                tmpId = Convert.ToInt32(tmpIdStr);
            else
                return null;

            var deviceId = Convert.ToInt32(dict["id"]);

            dict = dict.Where(kv => !excluded.Contains(kv.Key)).ToDictionary(i => i.Key, i => i.Value);

            if (queryParameters?.Parameters == null)
            {
                var str = string.Join(", ", dict.Select(v => $"'{v.Key}'"));

                throw new InvalidOperationException($"Failed to process request for sensor type '{sensorType}': sensor query target parameters are required, however none were specified. Please retry the request specifying the parameters {str}.");
            }

            var missing = new List<string>();

            foreach (var prop in dict.ToDictionary(i => i.Key, i => i.Value))
            {
                object v;

                //Only trim name if a trimmed version of this parameter doesn't exist
                var flexibleName = !dict.ContainsKey(prop.Key.ToLower().TrimEnd('_')); //todo: unit test having a trimmed version conflicting but we specified UPPERCASE_ non-trimmed

                if (queryParameters.Parameters.TryGetValue(flexibleName ? prop.Key : prop.Key.ToLower(), out v, flexibleName, flexibleName))
                {
                    if (v is ISerializable)
                        dict[prop.Key] = ((ISerializable) v).GetSerializedFormat();
                    else
                        dict[prop.Key] = v?.ToString();
                }
                else
                    missing.Add(prop.Key);
            }

            if (missing.Count > 0)
            {
                var plural = "parameter".Plural(missing.Count);

                throw new InvalidOperationException($"Failed to process request for sensor type '{sensorType}': sensor query target parameters did not include mandatory {plural} {missing.ToQuotedList()}.");
            }

            return new ContinueAddSensorQueryParameters(deviceId, tmpId, dict);
        }

        internal static PrtgResponse GetSensorTargetTmpId(HttpResponseMessage message) =>
            Regex.Replace(message.RequestMessage.RequestUri.ToString(), "(.+tmpid=)(\\d+)(.*)", "$2", RegexOptions.Singleline);

        internal static void ValidateAddSensorProgressResult(AddSensorProgress p, bool addFull)
        {
            if (p.TargetUrl.StartsWith("addsensorfailed"))
            {
                var parts = UrlUtilities.CrackUrl(p.TargetUrl);

                var message = parts["errormsg"];

                var action = addFull ? "add sensor" : "resolve sensor targets";

                if (message != null)
                {
                    message = message.Trim('\r', '\n');

                    //todo: does this work in other languages? Not sure how to replicate it
                    if (message.StartsWith("Incomplete connection settings"))
                        throw new PrtgRequestException("Failed to retrieve data from device; required credentials for sensor type may be missing. See PRTG UI for further details.");

                    throw new PrtgRequestException($"An exception occurred while trying to {action}: {message.EnsurePeriod()}");
                }

                throw new PrtgRequestException($"An unspecified error occurred while trying to {action}. Specified sensor type may not be valid on this device, or sensor query target parameters may be incorrect. Check the Device 'Host' is still valid or try adding sensor with the PRTG UI.");
            }

            if (addFull && p.Percent == -1)
                throw new PrtgRequestException($"PRTG was unable to complete the request. The server responded with the following error: '{p.Error.Replace("<br/><ul><li>", " ").Replace("</li></ul><br/>", " ")}'.");
        }

        #endregion
        #region Device Templates

        internal static List<DeviceTemplate> GetTemplates(string response)
        {
            var checkboxes = HtmlParser.Default.GetInput(response).Where(
                t => t.Type == Html.InputType.Checkbox && t.Name == Parameter.DeviceTemplate.GetDescription()
            ).ToList();

            var templates = checkboxes.Select(c => new DeviceTemplate(c.Value)).ToList();

            return templates;
        }

        #endregion
        #region Sensor Types

        internal static List<SensorTypeDescriptor> ParseSensorTypes(List<SensorTypeDescriptor> types)
        {
            if (types == null)
                return new List<SensorTypeDescriptor>();

            return types.GroupBy(t => t.Id).Select(g => g.First()).ToList();
        }

        #endregion
        #region Resolve Address

        internal static PrtgResponse ResolveParser(HttpResponseMessage message)
        {
            if (message.Content.Headers.ContentType.MediaType == "image/png" || message.StatusCode.ToString() == "530")
                throw new PrtgRequestException("Could not resolve the specified address; the PRTG map provider is not currently available.");

            return null;
        }

        #endregion

        internal static PrtgResponse ValidateHasContent(HttpResponseMessage message)
        {
            var responseText = message.Content.ReadAsStringAsync().Result;

            if (responseText.Contains("class=\"no-content\""))
                return "[]";

            return responseText;
        }

        internal static async Task<PrtgResponse> ValidateHasContentAsync(HttpResponseMessage message)
        {
            var responseText = await message.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (responseText.Contains("class=\"no-content\""))
                return "[]";

            return responseText;
        }
    }
}
