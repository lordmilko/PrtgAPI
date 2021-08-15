using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using PrtgAPI.Utilities;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    public class MultiTypeResponse : IWebStreamResponse
    {
        private StringEnum<SensorType> newSensorType;

        public MultiTypeResponse()
        {
        }

        public MultiTypeResponse(Dictionary<Content, int> countOverride)
        {
            CountOverride = countOverride;
        }

        public MultiTypeResponse(Dictionary<Content, BaseItem[]> items)
        {
            ItemOverride = items;
        }

        public Dictionary<Content, int> CountOverride { get; set; }
        public Dictionary<Content, Action<BaseItem>> PropertyManipulator { get; set; }
        public Dictionary<Content, BaseItem[]> ItemOverride { get; set; }
        private Dictionary<string, int> hitCount = new Dictionary<string, int>();
        public Func<string, string, string> ResponseTextManipulator { get; set; }

        public int[] HasSchedule { get; set; }

        public int? FixedCountOverride { get; set; }

        public TriggerType[] ForceTriggerUninherited { get; set; } = new TriggerType[0];

        public string GetResponseText(ref string address)
        {
            var function = GetFunction(address);

            if (hitCount.ContainsKey(function))
                hitCount[function]++;
            else
                hitCount.Add(function, 1);

            var text = GetResponse(ref address, function).GetResponseText(ref address);

            if (ResponseTextManipulator != null)
                return ResponseTextManipulator(text, address);

            return text;
        }

        public async Task<string> GetResponseTextStream(string address)
        {
            var function = GetFunction(address);

            if (hitCount.ContainsKey(function))
                hitCount[function]++;
            else
                hitCount.Add(function, 1);

            var text = await GetResponseStream(address, function).GetResponseTextStream(address);

            if (ResponseTextManipulator != null)
                return ResponseTextManipulator(address, text);

            return text;
        }

        protected virtual IWebResponse GetResponse(ref string address, string function)
        {
            switch (function)
            {
                case nameof(XmlFunction.TableData):
                case nameof(XmlFunction.HistoricData):
                    return GetTableResponse(address, function, false);
                case nameof(CommandFunction.Pause):
                case nameof(CommandFunction.PauseObjectFor):
                    return new BasicResponse("<a data-placement=\"bottom\" title=\"Resume\" href=\"#\" onclick=\"var self=this; _Prtg.objectTools.pauseObject.call(this,'1234',1);return false;\"><i class=\"icon-play icon-dark\"></i></a>");
                case nameof(HtmlFunction.ChannelEdit):
                    var components = UrlUtilities.CrackUrl(address);

                    if (components["channel"] != "99")
                    {
                        ChannelItem item;

                        if (CountOverride != null && CountOverride.ContainsKey(Content.Channels))
                            item = new ChannelItem(objId: components["channel"], name: $"Percent Available Memory{components["channel"]}");
                        else
                            item = new ChannelItem();

                        return new ChannelResponse(item);
                    }
                    return new BasicResponse(string.Empty);
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

                    var sensorTypeStr = UrlUtilities.CrackUrl(address)["sensortype"];

                    try
                    {
                        
                        var sensorTypeEnum = sensorTypeStr.XmlToEnum<SensorType>();
                        newSensorType = new StringEnum<SensorType>(sensorTypeEnum);
                    }
                    catch
                    {
                        newSensorType = new StringEnum<SensorType>(sensorTypeStr);
                    }

                    address = "http://prtg.example.com/controls/addsensor3.htm?id=9999&tmpid=2";
                    return new BasicResponse(string.Empty);
                case nameof(HtmlFunction.EditNotification):
                    return new NotificationActionResponse(new NotificationActionItem())
                    {
                        HasSchedule = HasSchedule
                    };
                case nameof(JsonFunction.GetAddSensorProgress):
                    var progress = hitCount[function] % 2 == 0 ? 100 : 50;

                    return new BasicResponse($"{{\"progress\":\"{progress}\",\"targeturl\":\" /addsensor4.htm?id=4251&tmpid=119\"}}");
                case nameof(HtmlFunction.AddSensor4):
                    return GetSensorTargetResponse();
                case nameof(XmlFunction.GetTreeNodeStats):
                    return new SensorTotalsResponse(new SensorTotalsItem());
                case nameof(JsonFunction.GeoLocator):
                    return new GeoLocatorResponse();
                case nameof(HtmlFunction.HistoricDataReport):
                    return new SensorHistoryReportResponse(true);
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
                case nameof(CommandFunction.ProbeState):
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
                case nameof(CommandFunction.SysInfoCheckNow):
                    return new BasicResponse(string.Empty);
                default:
                    throw GetUnknownFunctionException(function);
            }
        }

        protected virtual IWebStreamResponse GetResponseStream(string address, string function)
        {
            switch (function)
            {
                case nameof(XmlFunction.TableData):
                    return (IWebStreamResponse)GetTableResponse(address, function, true);
                default:
                    throw GetUnknownFunctionException(function, true);
            }
        }

        private IWebResponse GetTableResponse(string address, string function, bool async)
        {
            var components = UrlUtilities.CrackUrl(address);

            Content? content;

            try
            {
                content = components["content"].DescriptionToEnum<Content>();
            }
            catch
            {
                content = null;
            }

            var count = GetCount(components, content);

            //Hack to make test "forces streaming with a date filter and returns no results" work
            if (content == Content.Logs && count == 0 && components["columns"] == "objid,name")
            {
                count = 501;
                address = address.Replace("count=1", "count=501");
            }

            if (function == nameof(XmlFunction.HistoricData))
                return new SensorHistoryResponse(GetItems(i => new SensorHistoryItem(), count));

            var columns = components["columns"]?.Split(',');

            switch (content)
            {
                case Content.Sensors: return Sensors(count, columns, address, async);
                case Content.Devices: return Devices(count, columns, address, async);
                case Content.Groups:  return Groups(count, columns, address, async);
                case Content.Probes:  return Probes(count, columns, address, async);
                case Content.Logs:
                    if (IsGetTotalLogs(address))
                        return TotalLogsResponse();

                    return Messages(i => new MessageItem($"WMI Remote Ping{i}"), count);
                case Content.History: return new ModificationHistoryResponse(new ModificationHistoryItem());
                case Content.Notifications: return Notifications(CreateNotification, count);
                case Content.Schedules: return Schedules(CreateSchedule, count);
                case Content.Channels:

                    if (!(CountOverride != null && CountOverride.TryGetValue(Content.Channels, out count)))
                        count = 1;

                    return AdvancedItem<ChannelItem, Channel>(i =>
                    {
                        if (count == 1)
                            return new ChannelItem();

                        return new ChannelItem(objId: i.ToString(), name: $"Percent Available Memory{i}");
                    }, i => new ChannelResponse(i), Content.Channels, count, columns, address, async);
                case Content.Objects:
                    return Objects(address, function, components);
                case Content.Triggers:
                    return new NotificationTriggerResponse(
                        NotificationTriggerItem.StateTrigger(onNotificationAction: "300|Email to all members of group PRTG Users Group",                                                                                  parentId: ForceTriggerUninherited.Contains(TriggerType.State) ? components["id"] : "0"),
                        NotificationTriggerItem.ThresholdTrigger(onNotificationAction: "300|Email to all members of group PRTG Users Group", offNotificationAction: "300|Email to all members of group PRTG Users Group", parentId: ForceTriggerUninherited.Contains(TriggerType.Threshold) ? components["id"] : "1"),
                        NotificationTriggerItem.SpeedTrigger(onNotificationAction: "300|Email to all members of group PRTG Users Group", offNotificationAction: "300|Email to all members of group PRTG Users Group",     parentId: ForceTriggerUninherited.Contains(TriggerType.Speed) ? components["id"] : "1"),
                        NotificationTriggerItem.VolumeTrigger(onNotificationAction: "300|Email to all members of group PRTG Users Group",                                                                                 parentId: ForceTriggerUninherited.Contains(TriggerType.Volume) ? components["id"] : "1"),
                        NotificationTriggerItem.ChangeTrigger(onNotificationAction: "300|Email to all members of group PRTG Users Group",                                                                                 parentId: ForceTriggerUninherited.Contains(TriggerType.Change) ? components["id"] : "1")
                    );
                case Content.SysInfo:
                    return new SystemInfoResponse(
                        SystemInfoItem.SystemItem(), SystemInfoItem.HardwareItem(), SystemInfoItem.SoftwareItem(),
                        SystemInfoItem.ProcessItem(), SystemInfoItem.ServiceItem(), SystemInfoItem.UserItem()
                    );
                default:
                    throw new NotImplementedException($"Unknown content '{content}' requested from {nameof(MultiTypeResponse)}");
            }
        }

        private IWebStreamResponse Sensors(int count, string[] columns, string address, bool async) =>
            AdvancedItem<SensorItem, Sensor>(CreateSensor, i => new SensorResponse(i), Content.Sensors, count, columns, address, async);

        private IWebStreamResponse Devices(int count, string[] columns, string address, bool async) =>
            AdvancedItem<DeviceItem, Device>(CreateDevice, i => new DeviceResponse(i), Content.Devices, count, columns, address, async);

        private IWebStreamResponse Groups(int count, string[] columns, string address, bool async) =>
            AdvancedItem<GroupItem, Group>(CreateGroup, i => new GroupResponse(i), Content.Groups, count, columns, address, async);

        private IWebStreamResponse Probes(int count, string[] columns, string address, bool async) =>
            AdvancedItem<ProbeItem, Probe>(CreateProbe, i => new ProbeResponse(i), Content.Probes, count, columns, address, async);

        private IWebStreamResponse AdvancedItem<TItem, TObject>(Func<int, TItem> createItem,
            Func<TItem[], IWebStreamResponse> createResponse,
            Content content,
            int count,
            string[] columns,
            string address,
            bool async)
            where TItem : BaseItem
            where TObject : IObject
        {
            return new AdvancedItemGenerator<TItem, TObject>(
                createItem,
                createResponse,
                content,
                count,
                columns,
                address,
                async,
                this
            ).GetResponse();
        }

        private IWebResponse Messages(Func<int, MessageItem> func, int count) => new MessageResponse(GetItems(func, count));
        private IWebResponse Notifications(Func<int, NotificationActionItem> func, int count) => new NotificationActionResponse(GetItems(func, count));
        private IWebResponse Schedules(Func<int, ScheduleItem> func, int count) => new ScheduleResponse(GetItems(func, count));

        private bool IsGetTotalLogs(string address)
        {
            if (address.Contains("content=messages&count=1&columns=objid,name"))
                return true;

            return false;
        }

        private IWebResponse TotalLogsResponse()
        {
            int count;

            if (!(CountOverride != null && CountOverride.TryGetValue(Content.Logs, out count)))
                count = 1000000;

            return new BasicResponse(new XElement("messages",
                new XAttribute("listend", 1),
                new XAttribute("totalcount", count),
                new XElement("prtg-version", "1.2.3.4"),
                null
            ).ToString());
        }

        private SensorItem CreateSensor(int i)
        {
            var item = new SensorItem(
                name: $"Volume IO _Total{i}",
                typeRaw: "aggregation",
                objid: (4000 + i).ToString(),
                downtimeTimeRaw: (1220 + i).ToString(),
                messageRaw: "OK1" + i,
                lastUpRaw: new DateTime(2000, 10, 1, 4, 2, 1, DateTimeKind.Utc).AddDays(i).ToUniversalTime().ToOADate().ToString(CultureInfo.InvariantCulture)
            );

            return AdjustProperties(item, Content.Sensors);
        }

        private DeviceItem CreateDevice(int i)
        {
            var item = new DeviceItem(
                name: $"Probe Device{i}",
                objid: (3000 + i).ToString(),
                messageRaw: "OK1" + i
            );

            return AdjustProperties(item, Content.Devices);
        }

        private GroupItem CreateGroup(int i)
        {
            var item = new GroupItem(
                name: $"Windows Infrastructure{i}",
                totalsens: "2",
                groupnum: "0",
                objid: (2000 + i).ToString(),
                messageRaw: "OK1" + i
            );

            return AdjustProperties(item, Content.Groups);
        }

        private ProbeItem CreateProbe(int i)
        {
            var item = new ProbeItem(
                name: $"127.0.0.1{i}",
                objid: (1000 + i).ToString(),
                messageRaw: "OK" + i
            );

            return AdjustProperties(item, Content.Probes);
        }

        private NotificationActionItem CreateNotification(int i)
        {
            var item = new NotificationActionItem();

            return AdjustProperties(item, Content.Notifications);
        }

        private ScheduleItem CreateSchedule(int i)
        {
            var item = new ScheduleItem();

            return AdjustProperties(item, Content.Schedules);
        }

        private TItem AdjustProperties<TItem>(TItem item, Content content) where TItem : BaseItem
        {
            if (PropertyManipulator != null)
            {
                Action<BaseItem> action;

                if (PropertyManipulator.TryGetValue(content, out action))
                    action(item);
            }

            return item;
        }

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

                    var text = new ObjectResponse(new SensorItem(objid: "7000", baseType: "", typeRaw: "basenode")).GetResponseText(ref address);

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

            if (FixedCountOverride != null)
                return FixedCountOverride.Value;

            if (CountOverride == null) //question: will this cause issues with streaming cos the second page have the wrong count
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
                if (content != null && CountOverride.ContainsKey(content.Value))
                    count = CountOverride[content.Value];
            }

            return count;
        }

        private IWebResponse GetObjectDataResponse(string address)
        {
            var components = UrlUtilities.CrackUrl(address);

            var objectType = components["objecttype"]?.ToEnum<ObjectType>() ?? ObjectType.Sensor;

            if (components["id"] == "810")
                objectType = ObjectType.WebServerOptions;

            switch (objectType)
            {
                case ObjectType.Sensor:
                    return new SensorSettingsResponse();
                case ObjectType.Device:
                    return new DeviceSettingsResponse();
                case ObjectType.Notification:
                    return new NotificationActionResponse(new NotificationActionItem())
                    {
                        HasSchedule = HasSchedule
                    };
                case ObjectType.Schedule:
                    return new ScheduleResponse();
                case ObjectType.WebServerOptions:
                    return new WebServerOptionsResponse();
                default:
                    throw new NotImplementedException($"Unknown object type '{objectType}' requested from {nameof(MultiTypeResponse)}");
            }
        }

        private IWebResponse GetRawObjectProperty(string address)
        {
            var components = UrlUtilities.CrackUrl(address);

            if (components["subid"] != null)
                return GetRawSubObjectProperty(components);

            if (components["name"] == "name")
                return new RawPropertyResponse("testName");

            if (components["name"] == "active")
                return new RawPropertyResponse("1");

            if (components["name"] == "tags")
                return new RawPropertyResponse("tag1 tag2");

            if (components["name"] == "accessgroup")
                return new RawPropertyResponse("1");

            if (components["name"] == "intervalgroup")
                return new RawPropertyResponse(null);

            if (components["name"] == "dbauth")
                return new RawPropertyResponse("1");

            if (components["name"] == "comments")
                return new RawPropertyResponse("Do not turn off!");

            if (components["name"] == "banana")
                return new RawPropertyResponse("(Property not found)");

            if (components["name"] == "aggregationchannel")
                return new RawPropertyResponse("#1:Channel\nchannel(4001, 0)\n#2:Channel\nchannel(4002, 0)");

            if (components["name"] == "authorized")
            {
                var str = string.Empty;

                switch(components["id"])
                {
                    case "1":
                    case "1001": //Not approved yet
                        str = "0";
                        break;
                    case "1002": //Already approved
                        str = "1";
                        break;
                    case "9001": //Not a probe (English)
                        str = "(Property not found)";
                        break;
                    default:
                        str = "(Test)";
                        break;
                }

                return new RawPropertyResponse(str);
            }

            components.Remove("username");
            components.Remove("passhash");
            components.Remove("id");

            throw new NotImplementedException($"Unknown raw object property '{components[0]}' passed to {GetType().Name}");
        }

        private IWebResponse GetRawSubObjectProperty(NameValueCollection components)
        {
            if (components["name"] == "name")
                return new RawPropertyResponse("testName");

            if (components["name"] == "limitmaxerror")
                return new RawPropertyResponse("90");

            if (components["name"] == "limitminerror")
                return new RawPropertyResponse("30");

            components.Remove("username");
            components.Remove("passhash");
            components.Remove("id");

            throw new NotImplementedException($"Unknown raw sub object property '{components[0]}' passed to {GetType().Name}");
        }

        private IWebResponse GetSensorTargetResponse()
        {
            switch (newSensorType.Value)
            {
                case SensorType.ExeXml:
                    return new ExeFileTargetResponse();
                case SensorType.WmiService:
                    return new WmiServiceTargetResponse();
                case SensorType.Http:
                    return new HttpTargetResponse();
                default:
                    if (newSensorType.StringValue == "snmplibrary_nolist")
                        return new ExeFileTargetResponse(); //We won't actually be utilizing the response

                    throw new NotSupportedException($"Sensor type {newSensorType} not supported");
            }
        }

        public static Content GetContent(string address)
        {
            var components = UrlUtilities.CrackUrl(address);

            Content content = components["content"].DescriptionToEnum<Content>();

            return content;
        }

        protected T[] GetItems<T>(Func<int, T> func, int count)
        {
            return Enumerable.Range(0, count).Select(func).ToArray();
        }

        public static Enum GetFunctionEnum(string address)
        {
            var page = GetPage(address);

            XmlFunction xmlFunc;
            if (TryParseEnumDescription(page, out xmlFunc))
                return xmlFunc;

            CommandFunction cmdFunc;
            if (TryParseEnumDescription(page, out cmdFunc))
                return cmdFunc;

            JsonFunction jsonFunc;
            if (TryParseEnumDescription(page, out jsonFunc))
                return jsonFunc;

            HtmlFunction htmlFunc;
            if (TryParseEnumDescription(page, out htmlFunc))
                return htmlFunc;

            throw new NotImplementedException($"Don't know what the type of function '{page}' is");
        }

        public static string GetFunction(string address)
        {
            return GetFunctionEnum(address).ToString();
        }

        private static string GetPage(string address)
        {
            var queries = UrlUtilities.ParseQueryString(address);

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

        protected Exception GetUnknownFunctionException(string function, bool async = false)
        {
            return new NotImplementedException($"Unknown {(async ? "async " : "")}function '{function}' passed to {GetType().Name}");
        }
    }
}
