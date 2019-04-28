using System;
using System.Xml.Linq;
using PrtgAPI.Request;

namespace PrtgAPI
{
    class ChannelSettings : ObjectSettings
    {
        internal static XElement GetChannelXml(PrtgResponse response, int channelId)
        {
            var str = response.StringValue;

            var basicMatchRegex = "<input.+?name=\".*?_.+?\".+?value=\".*?\".*?>";
            var nameRegex = "(.+?name=\")(.+?)(\".+)";

            Func<string, string> nameTransformer = n => n.Replace($"_{channelId}", "");

            var inputXml = HtmlParser.Default.GetInputXml(str, basicMatchRegex, nameRegex, nameTransformer);
            var ddlXml = HtmlParser.Default.GetDropDownListXml(str, nameRegex, nameTransformer);
            var elm = new XElement("properties", inputXml, ddlXml);
            return elm;
        }
    }
}
