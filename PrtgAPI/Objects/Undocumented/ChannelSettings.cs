using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PrtgAPI.Objects.Undocumented
{
    class ChannelSettings : ObjectSettings
    {
        internal static XElement GetXml(string response, int channelId)
        {
            var basicMatchRegex = "<input.+?name=\".*?_.+?\".+?value=\".*?\".+?>";
            var nameRegex = "(.+?name=\")(.+?)(\".+)";

            return GetXmlInternal(response, channelId, basicMatchRegex, nameRegex, n => n.Replace($"_{channelId}", ""));
        }

        protected static XElement GetXmlInternal(string response, int channelId, string basicMatchRegex, string nameRegex, Func<string, string> nameTransformer)
        {
            var inputXml = GetInputXml(response, basicMatchRegex, nameRegex, nameTransformer);
            //var ddlXml = GetDropDownListXml(response, nameRegex);
            //var dependencyXml = GetDependency(response); //if the dependency xml is null does that cause an issue for the xelement we create below?

            var elm = new XElement("properties", inputXml);
            return elm;
        }
    }
}
