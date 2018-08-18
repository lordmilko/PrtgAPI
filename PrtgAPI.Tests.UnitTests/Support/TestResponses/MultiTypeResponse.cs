using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Web;
using PrtgAPI.Helpers;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestItems;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses
{
    public class MultiTypeResponse : IWebResponse
    {
        private SensorType? newSensorType;

        public MultiTypeResponse()
        {
        }

        public MultiTypeResponse(Dictionary<Content, int> countOverride)
        {
            this.countOverride = countOverride;
        }

        protected Dictionary<Content, int> countOverride;
        private Dictionary<string, int> hitCount = new Dictionary<string, int>();

        public string GetResponseText(ref string address)
        {
            var function = GetFunction(address);

            if (hitCount.ContainsKey(function))
                hitCount[function]++;
            else
                hitCount.Add(function, 1);

            return GetResponse(ref address, function).GetResponseText(ref address);
        }

        protected virtual IWebResponse GetResponse(ref string address, string function)
        {
            switch (function)
            {
                case nameof(XmlFunction.TableData):
                case nameof(XmlFunction.HistoricData):
                    return GetTableResponse(ref address, function);
                case nameof(CommandFunction.Pause):
                case nameof(CommandFunction.PauseObjectFor):
                    return new BasicResponse("<a data-placement=\"bottom\" title=\"Resume\" href=\"#\" onclick=\"var self=this; _Prtg.objectTools.pauseObject.call(this,'1234',1);return false;\"><i class=\"icon-play icon-dark\"></i></a>");
                case nameof(HtmlFunction.ChannelEdit):
                    return new ChannelResponse(new ChannelItem());
                case nameof(CommandFunction.DuplicateObject):
                    address = "https://prtg.example.com/public/login.htm?loginurl=/object.htm?id=9999&errormsg=";
                    return new BasicResponse(string.Empty);
                case nameof(HtmlFunction.EditSettings):
                    return new BasicResponse(string.Empty);
                case nameof(JsonFunction.GetStatus):
                    return new ServerStatusResponse(new ServerStatusItem());
                case nameof(JsonFunction.Triggers):
                    return new TriggerOverviewResponse();
                case nameof(JsonFunction.SensorTypes):
                    return new SensorTypeResponse();
                case nameof(HtmlFunction.ObjectData):
                    return GetObjectDataResponse(address);
                case nameof(XmlFunction.GetObjectProperty):
                case nameof(XmlFunction.GetObjectStatus):
                    return GetRawObjectProperty(address);
                case nameof(CommandFunction.AddSensor2):
                    newSensorType = UrlHelpers.CrackUrl(address)["sensortype"].ToEnum<SensorType>();
                    address = "http://prtg.example.com/controls/addsensor3.htm?id=9999&tmpid=2";
                    return new BasicResponse(string.Empty);
                case nameof(HtmlFunction.EditNotification):
                    return new NotificationActionResponse(new NotificationActionItem());
                case nameof(JsonFunction.GetAddSensorProgress):
                    var progress = hitCount[function] % 2 == 0 ? 100 : 50;

                    return new BasicResponse($"{{\"progress\":\"{progress}\",\"targeturl\":\" /addsensor4.htm?id=4251&tmpid=119\"}}");
                case nameof(HtmlFunction.AddSensor4):
                    return GetSensorTargetResponse();
                case nameof(CommandFunction.AcknowledgeAlarm):
                case nameof(CommandFunction.AddSensor5):
                case nameof(CommandFunction.AddDevice2):
                case nameof(CommandFunction.AddGroup2):
                case nameof(CommandFunction.ClearCache):
                case nameof(CommandFunction.DeleteObject):
                case nameof(HtmlFunction.RemoveSubObject):
                case nameof(CommandFunction.DiscoverNow):
                case nameof(CommandFunction.LoadLookups):
                case nameof(CommandFunction.MoveObjectNow):
                case nameof(CommandFunction.RecalcCache):
                case nameof(CommandFunction.Rename):
                case nameof(CommandFunction.RestartServer):
                case nameof(CommandFunction.RestartProbes):
                case nameof(CommandFunction.ReloadFileLists):
                case nameof(CommandFunction.SaveNow):
                case nameof(CommandFunction.ScanNow):
                case nameof(CommandFunction.SetPosition):
                case nameof(CommandFunction.Simulate):
                case nameof(CommandFunction.SortSubObjects):
                    return new BasicResponse(string.Empty);
                default:
                    throw GetUnknownFunctionException(function);
            }
        }

        private IWebResponse GetTableResponse(ref string address, string function)
        {
            var components = UrlHelpers.CrackUrl(address);

            Content c;
            Content? content = null;

            if (Enum.TryParse(components["content"], true, out c))
                content = c;

            var count = GetCount(components, content);

            //Hack to make test "forces streaming with a date filter and returns no results" work
            if (content == Content.Messages && count == 0 && components["columns"] == "objid,name")
            {
                count = 501;
                address = address.Replace("count=1", "count=501");
            }

            if (function == nameof(XmlFunction.HistoricData))
                return new SensorHistoryResponse(GetItems(i => new SensorHistoryItem(), count));

            switch (content)
            {
                case Content.Sensors:   return Sensors(i => new SensorItem(name: $"Volume IO _Total{i}", type: "Sensor Factory", objid: (4000 + i).ToString()), count);
                case Content.Devices:   return Devices(i => new DeviceItem(name: $"Probe Device{i}", objid: (3000 + i).ToString()), count);
                case Content.Groups:    return Groups(i => new GroupItem(name: $"Windows Infrastructure{i}", totalsens: "2", groupnum: "0", objid: (2000 + i).ToString()), count);
                case Content.ProbeNode: return Probes(i => new ProbeItem(name: $"127.0.0.1{i}", objid: (1000 + i).ToString()), count);
                case Content.Messages:  return Messages(i => new MessageItem($"WMI Remote Ping{i}"), count);
                case Content.Notifications: return new NotificationActionResponse(new NotificationActionItem());
                case Content.Schedules: return Schedules(i => new ScheduleItem(), count);
                case Content.Channels:  return new ChannelResponse(new ChannelItem());
                case Content.Objects:
                    return Objects(address, function, components);
                default:
                    throw new NotImplementedException($"Unknown content '{content}' requested from {nameof(MultiTypeResponse)}");
            }
        }

        private SensorResponse Sensors(Func<int, SensorItem> func, int count) => new SensorResponse(GetItems(func, count));
        private DeviceResponse Devices(Func<int, DeviceItem> func, int count) => new DeviceResponse(GetItems(func, count));
        private GroupResponse Groups(Func<int, GroupItem> func, int count) => new GroupResponse(GetItems(func, count));
        private ProbeResponse Probes(Func<int, ProbeItem> func, int count) => new ProbeResponse(GetItems(func, count));
        private MessageResponse Messages(Func<int, MessageItem> func, int count) => new MessageResponse(GetItems(func, count));
        private ScheduleResponse Schedules(Func<int, ScheduleItem> func, int count) => new ScheduleResponse(GetItems(func, count));
        private IWebResponse Objects(string address, string function, NameValueCollection components)
        {
            var idStr = components["filter_objid"];

            if (idStr != null)
            {
                var ids = idStr.Split(',').Select(v => Convert.ToInt32(v));

                var items = ids.SelectMany(id =>
                {
                    if (id < 400)
                        return GetObject("notifications", address, function);
                    if (id < 700)
                        return GetObject("schedules", address, function);
                    if (id < 2000)
                        return GetObject("probenode", address, function);
                    if (id < 3000)
                        return GetObject("groups", address, function);
                    if (id < 4000)
                        return GetObject("devices", address, function);
                    if (id < 5000)
                        return GetObject("sensors", address, function);

                    var text = new ObjectResponse(new SensorItem()).GetResponseText(ref address);

                    return XDocument.Parse(text).Descendants("item").ToList();
                }).ToArray();

                var xml = new XElement("objects",
                    new XAttribute("listend", 1),
                    new XAttribute("totalcount", items.Length),
                    new XElement("prtg-version", "1.2.3.4"),
                    items
                );

                return new BasicResponse(xml.ToString());
            }

            return new ObjectResponse(
                new SensorItem(),
                new DeviceItem(),
                new GroupItem(),
                new ProbeItem(),
                new ScheduleItem(),
                new NotificationActionItem()
            );
        }

        private List<XElement> GetObject(string newContent, string address, string function)
        {
            var r = GetTableResponse(address.Replace("content=objects", $"content={newContent}").Replace("count=*", "count=1"), function, false);

            var text = r.GetResponseText(ref address);

            return XDocument.Parse(text).Descendants("item").ToList();
        }

        private int GetCount(NameValueCollection components, Content? content)
        {
            var count = 2;

            if (countOverride == null) //question: will this cause issues with streaming cos the second page have the wrong count
            {
                var countStr = components["count"];

                if (countStr != null && countStr != "0" && countStr != "500" && countStr != "*")
                    count = Convert.ToInt32(countStr);

                if (components["filter_objid"] != null)
                {
                    count = 1;

                    var values = components.GetValues("filter_objid");

                    if (values?.Length > 1)
                        count = 2;
                    else
                    {

                        if (values?.First() == "-2")
                        {
                            if (content != Content.Devices)
                                count = 0;
                        }
                        if (values?.First() == "-3")
                        {
                            if (content != Content.Groups)
                                count = 0;
                        }
                        else if (values?.First() == "-4")
                            count = 0;
                    }
                }
            }
            else
            {
                if (content != null && countOverride.ContainsKey(content.Value))
                    count = countOverride[content.Value];
            }

            return count;
        }

        private IWebResponse GetObjectDataResponse(string address)
        {
            var components = UrlHelpers.CrackUrl(address);

            var objectType = components["objecttype"].ToEnum<ObjectType>();

            switch (objectType)
            {
                case ObjectType.Sensor:
                    return new SensorSettingsResponse();
                case ObjectType.Device:
                    return new DeviceSettingsResponse();
                default:
                    throw new NotImplementedException($"Unknown object type '{objectType}' requested from {nameof(MultiTypeResponse)}");
            }
        }

        private IWebResponse GetRawObjectProperty(string address)
        {
            var components = UrlHelpers.CrackUrl(address);

            if (components["name"] == "name")
                return new RawPropertyResponse("testName");

            if (components["name"] == "tags")
                return new RawPropertyResponse("tag1 tag2");

            if (components["name"] == "accessgroup")
                return new RawPropertyResponse("1");

            if (components["name"] == "intervalgroup")
                return new RawPropertyResponse(null);

            if (components["name"] == "comments")
                return new RawPropertyResponse("Do not turn off!");

            if (components["name"] == "banana")
                return new RawPropertyResponse("(Property not found)");

            components.Remove("username");
            components.Remove("passhash");
            components.Remove("id");

            throw new NotImplementedException($"Unknown raw object property '{components[0]}' passed to {GetType().Name}");
        }

        private IWebResponse GetSensorTargetResponse()
        {
            switch (newSensorType)
            {
                case SensorType.ExeXml:
                    return new ExeFileTargetResponse();
                case SensorType.WmiService:
                    return new WmiServiceTargetResponse();
                case SensorType.Http:
                    return new HttpTargetResponse();
                default:
                    throw new NotSupportedException($"Sensor type {newSensorType} not supported");
            }
        }

        public static Content GetContent(string address)
        {
            var components = UrlHelpers.CrackUrl(address);

            Content content = components["content"].ToEnum<Content>();

            return content;
        }

        protected T[] GetItems<T>(Func<int, T> func, int count)
        {
            return Enumerable.Range(0, count).Select(func).ToArray();
        }

        public static string GetFunction(string address)
        {
            var page = GetPage(address);

            XmlFunction xmlFunc;
            if (TryParseEnumDescription(page, out xmlFunc))
                return xmlFunc.ToString();

            CommandFunction cmdFunc;
            if (TryParseEnumDescription(page, out cmdFunc))
                return cmdFunc.ToString();

            JsonFunction jsonFunc;
            if (TryParseEnumDescription(page, out jsonFunc))
                return jsonFunc.ToString();

            HtmlFunction htmlFunc;
            if (TryParseEnumDescription(page, out htmlFunc))
                return htmlFunc.ToString();

            throw new NotImplementedException($"Don't know what the type of function '{page}' is");
        }

        private static string GetPage(string address)
        {
            var queries = HttpUtility.ParseQueryString(address);

            var items = queries.AllKeys.SelectMany(queries.GetValues, (k, v) => new { Key = k, Value = v });

            var first = items.First().Key;

            var query = first.LastIndexOf('?');

            var end = query > 0 ? query : first.Length - 1;
            var start = first.IndexOf('/', 9) + 1;

            var page = first.Substring(start, end - start);

            if (page.StartsWith("api/"))
                page = page.Substring(4);

            return page;
        }

        private static bool TryParseEnumDescription<TEnum>(string description, out TEnum result)
        {
            result = default(TEnum);

            foreach (var field in typeof(TEnum).GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;

                if (attribute != null)
                {
                    if (attribute.Description.ToLower() == description.ToLower())
                    {
                        result = (TEnum)field.GetValue(null);

                        return true;
                    }
                }
            }

            return false;
        }

        protected Exception GetUnknownFunctionException(string function)
        {
            return new NotImplementedException($"Unknown function '{function}' passed to {GetType().Name}");
        }
    }
}
