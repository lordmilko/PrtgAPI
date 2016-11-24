using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrtgAPI.Attributes;

namespace PrtgAPI
{
    enum HtmlFunction
    {
        [Undocumented]
        [Description("controls/channeledit.htm")]
        ChannelEdit,

        [Undocumented]
        [Description("controls/objectdata.htm")]
        ObjectData,

        [Undocumented]
        [Description("editsettings")]
        EditSettings,
    }
}
