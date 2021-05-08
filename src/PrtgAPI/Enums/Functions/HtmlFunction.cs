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
        RemoveSubObject,

        /// <summary>
        /// Contains the content returned by a request for additional device information from <see cref="CommandFunction.AddSensor2"/>.
        /// </summary>
        [Undocumented]
        [Description("addsensor4.htm")]
        AddSensor4,

        [Undocumented]
        [Description("controls/editnotification.htm")]
        EditNotification,

        [Undocumented]
        [Description("addsensorfailed.htm")]
        AddSensorFailed,

        [Undocumented]
        [Description("historicdata_html.htm")]
        HistoricDataReport
    }
}
