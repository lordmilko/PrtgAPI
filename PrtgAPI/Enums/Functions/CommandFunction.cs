using System.ComponentModel;

namespace PrtgAPI
{
    enum CommandFunction
    {
        [Description("rename.htm")]
        Rename,

        [Description("setpriority.htm")]
        SetPriority,

        [Description("setobjectproperty.htm")]
        SetObjectProperty,

        [Description("pause.htm")]
        Pause,

        [Description("pauseobjectfor.htm")]
        PauseObjectFor,

        [Description("simulate.htm")]
        Simulate,

        [Description("acknowledgealarm.htm")]
        AcknowledgeAlarm,

        [Description("scannow.htm")]
        ScanNow,

        [Description("discovernow.htm")]
        DiscoverNow,

        [Description("setposition.htm")]
        SetPosition,

        [Description("reportaddsensor.htm")]
        ReportAddSensor,

        [Description("notificationtest.htm")]
        NotificationTest,

        [Description("deleteobject.htm")]
        DeleteObject,

        [Description("duplicateobject.htm")]
        DuplicateObject,

        [Description("setlonlat.htm")]
        SetLonLat,
    }
}
