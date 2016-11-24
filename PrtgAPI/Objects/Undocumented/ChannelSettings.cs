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
    }
}
