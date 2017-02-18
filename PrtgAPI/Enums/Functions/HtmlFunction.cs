using System.ComponentModel;
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

        [Undocumented]
        [Description("deletesub.htm")]
        RemoveSubObject
    }
}
