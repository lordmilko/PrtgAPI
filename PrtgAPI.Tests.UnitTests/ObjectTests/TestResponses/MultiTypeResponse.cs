using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using PrtgAPI.Helpers;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestItems;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses
{
    public class MultiTypeResponse : IWebResponse
    {
        //what if we had an optional constructor that took a dictionary of content to int that said how many items each content should return

        public MultiTypeResponse()
        {
        }

        public MultiTypeResponse(Dictionary<Content, int> countOverride)
        {
            this.countOverride = countOverride;
        }

        protected Dictionary<Content, int> countOverride;

        public string GetResponseText(ref string address)
        {
            var function = GetFunction(address);

            return GetResponse(ref address, function).GetResponseText(ref address);
        }

        protected virtual IWebResponse GetResponse(ref string address, string function)
        {
            switch (function)
            {
                case nameof(XmlFunction.TableData):
                    return GetTableResponse(ref address);
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
                case nameof(JsonFunction.Triggers):
                    return new TriggerOverviewResponse();
                case nameof(HtmlFunction.ObjectData):
                    return GetObjectDataResponse(address);
                case nameof(XmlFunction.GetObjectProperty):
                    return GetRawObjectProperty(address);
                case nameof(CommandFunction.ClearCache):
                case nameof(CommandFunction.LoadLookups):
                case nameof(CommandFunction.RecalcCache):
                case nameof(CommandFunction.RestartServer):
                case nameof(CommandFunction.RestartProbes):
                case nameof(CommandFunction.ReloadFileLists):
                case nameof(CommandFunction.SaveNow):
                    return new BasicResponse(string.Empty);
                default:
                    throw GetUnknownFunctionException(function);
            }
        }

        private IWebResponse GetTableResponse(ref string address)
        {
            var components = UrlHelpers.CrackUrl(address);

            Content content = components["content"].ToEnum<Content>();

            var count = 2;

            if (countOverride == null) //question: will this cause issues with streaming cos the second page have the wrong count
            {
                var countStr = components["count"];

                if (countStr != null && countStr != "0" && countStr != "500" && countStr != "50000")
                    count = Convert.ToInt32(countStr);

                if (components["filter_objid"] != null)
                    count = 1;
            }
            else
            {
                if (countOverride.ContainsKey(content))
                    count = countOverride[content];
            }

            //Hack to make test "forces streaming with a date filter and returns no results" work
            if (content == Content.Messages && count == 0 && components["columns"] == "objid,name")
            {
                count = 501;
                address = address.Replace("count=1", "count=501");
            }

            switch (content)
            {
                case Content.Sensors:   return new SensorResponse(GetItems(i => new SensorItem(name: $"Volume IO _Total{i}", type: "Sensor Factory"), count));
                case Content.Devices:   return new DeviceResponse(GetItems(i => new DeviceItem(name: $"Probe Device{i}"), count));
                case Content.Groups:    return new GroupResponse(GetItems(i => new GroupItem(name: $"Windows Infrastructure{i}"), count));
                case Content.ProbeNode: return new ProbeResponse(GetItems(i => new ProbeItem(name: $"127.0.0.1{i}"), count));
                case Content.Messages:  return new MessageResponse(GetItems(i => new MessageItem($"WMI Remote Ping{i}"), count));
                case Content.Channels:  return new ChannelResponse(new ChannelItem());
                default:
                    throw new NotImplementedException($"Unknown content '{content}' requested from {nameof(MultiTypeResponse)}");
            }
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

            if (components["name"] != null)
                return new RawPropertyResponse("testName");

            components.Remove("username");
            components.Remove("passhash");
            components.Remove("id");

            throw new NotImplementedException($"Unknown raw object property '{components[0]}' passed to {GetType().Name}");
        }

        public static Content GetContent(string address)
        {
            var components = UrlHelpers.CrackUrl(address);

            Content content = components["content"].ToEnum<Content>();

            return content;
        }

        private T[] GetItems<T>(Func<int, T> func, int count)
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
            var start = first.LastIndexOf(".com/", StringComparison.InvariantCulture) + 5;

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

        public Task<string> GetResponseTextStream(string address)
        {
            throw new NotImplementedException();
        }

        public HttpStatusCode StatusCode { get; set; }
    }
}
