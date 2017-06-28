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
using PrtgAPI.Tests.UnitTests.ObjectTests.Items;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.Responses
{
    public class MultiTypeResponse : IWebResponse
    {
        public string GetResponseText(string address)
        {
            return GetResponse(address).GetResponseText(address);
        }

        private IWebResponse GetResponse(string address)
        {
            var function = GetFunction(address);

            switch (function)
            {
                case nameof(XmlFunction.TableData):
                    return GetTableResponse(address);
                case nameof(CommandFunction.Pause):
                case nameof(CommandFunction.PauseObjectFor):
                    return new BasicResponse("<a data-placement=\"bottom\" title=\"Resume\" href=\"#\" onclick=\"var self=this; _Prtg.objectTools.pauseObject.call(this,'1234',1);return false;\"><i class=\"icon-play icon-dark\"></i></a>");
                default:
                    throw new NotImplementedException($"Unknown function '{function}'");
            }
        }

        private IWebResponse GetTableResponse(string address)
        {
            var content = GetContent(address);

            switch (content)
            {
                case Content.Sensors:   return new SensorResponse(new[] { new SensorItem() });
                case Content.Devices:   return new DeviceResponse(new[] { new DeviceItem() });
                case Content.Groups:    return new GroupResponse(new[] { new GroupItem() });
                case Content.ProbeNode: return new ProbeResponse(new[] { new ProbeItem() });
                default:
                    throw new NotImplementedException($"Unknown content '{content}'");
            }
        }

        private string GetFunction(string address)
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

        private string GetPage(string address)
        {
            var queries = HttpUtility.ParseQueryString(address);

            var items = queries.AllKeys.SelectMany(queries.GetValues, (k, v) => new { Key = k, Value = v });

            var first = items.First().Key;

            var query = first.LastIndexOf('?');

            var end = query > 0 ? query : first.Length - 1;
            var start = first.LastIndexOf('/') + 1;

            var page = first.Substring(start, end - start);

            return page;
        }

        private Content GetContent(string address)
        {
            var components = UrlHelpers.CrackUrl(address);

            Content content = components["content"].ToEnum<Content>();

            return content;
        }

        private string GetContent1(string address)
        {
            var components = UrlHelpers.CrackUrl(address);

            if (components["content"] != null)
                return components["content"];

            throw new NotImplementedException("unknown address");
        }

        private bool TryParseEnumDescription<TEnum>(string description, out TEnum result)
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

        public Task<string> GetResponseTextStream(string address)
        {
            throw new NotImplementedException();
        }

        public HttpStatusCode StatusCode { get; set; }
    }
}
