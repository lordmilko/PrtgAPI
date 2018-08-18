using System;
using System.Xml.Linq;

namespace PrtgAPI
{
    class ChannelSettings : ObjectSettings
    {
        internal static XElement GetChannelXml(string response, int channelId)
        {
            var basicMatchRegex = "<input.+?name=\".*?_.+?\".+?value=\".*?\".+?>";
            var nameRegex = "(.+?name=\")(.+?)(\".+)";

            Func<string, string> nameTransformer = n => n.Replace($"_{channelId}", "");

            var inputXml = GetInputXml(response, basicMatchRegex, nameRegex, nameTransformer);
            var ddlXml = GetDropDownListXml(response, nameRegex, nameTransformer);
            var elm = new XElement("properties", inputXml, ddlXml);
            return elm;
        }
    }
}
