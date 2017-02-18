using System.Xml.Linq;

namespace PrtgAPI.Objects.Undocumented
{
    class ChannelSettings : ObjectSettings
    {
        internal static XElement GetXml(string response, int channelId)
        {
            var basicMatchRegex = "<input.+?name=\".*?_.+?\".+?value=\".*?\".+?>";
            var nameRegex = "(.+?name=\")(.+?)(\".+)";

            var inputXml = GetInputXml(response, basicMatchRegex, nameRegex, n => n.Replace($"_{channelId}", ""));
            var elm = new XElement("properties", inputXml);
            return elm;
        }
    }
}
