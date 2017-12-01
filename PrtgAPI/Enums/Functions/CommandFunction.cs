using System.ComponentModel;
using PrtgAPI.Attributes;

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

        [Undocumented]
        [Description("moveobjectnow.htm")]
        MoveObjectNow,

        [Description("sortsubobjects.htm")]
        SortSubObjects,

        [Undocumented]
        [Description("addsensor5.htm")]
        AddSensor5,

        [Undocumented]
        [Description("adddevice2.htm")]
        AddDevice2,

        [Undocumented]
        [Description("addgroup2.htm")]
        AddGroup2,

        [Description("restartprobes.htm")]
        RestartProbes,

        [Description("restartserver.htm")]
        RestartServer,

        [Description("savenow.htm")]
        SaveNow,

        [Description("clearcache.htm")]
        ClearCache,

        [Description("recalccache.htm")]
        RecalcCache,

        [Description("reloadfilelists.htm")]
        ReloadFileLists,

        [Description("loadlookups.htm")]
        LoadLookups
    }
}
