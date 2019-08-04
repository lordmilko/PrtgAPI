using System.ComponentModel;
using PrtgAPI.Attributes;

namespace PrtgAPI
{
    enum CommandFunction
    {
        /// <summary>
        /// Renames an object.
        /// </summary>
        [Description("rename.htm")]
        Rename,

        [Description("setpriority.htm")]
        SetPriority,

        [Description("setobjectproperty.htm")]
        SetObjectProperty,

        /// <summary>
        /// Pauses an object indefinitely or resumes an object from a paused or simulated error state.
        /// </summary>
        [Description("pause.htm")]
        Pause,

        /// <summary>
        /// Pauses an object for a specified duration (in minutes).
        /// </summary>
        [Description("pauseobjectfor.htm")]
        PauseObjectFor,

        /// <summary>
        /// Simulates an error on a sensor.
        /// </summary>
        [Description("simulate.htm")]
        Simulate,

        /// <summary>
        /// Acknowledges a sensor in a <see cref="Status.Down"/> state.
        /// </summary>
        [Description("acknowledgealarm.htm")]
        AcknowledgeAlarm,

        /// <summary>
        /// Refreshes an object (as well as any child objects).
        /// </summary>
        [Description("scannow.htm")]
        ScanNow,

        /// <summary>
        /// Starts an auto-discovery operation.
        /// </summary>
        [Description("discovernow.htm")]
        DiscoverNow,

        /// <summary>
        /// Repositions an object under its parent object.
        /// </summary>
        [Description("setposition.htm")]
        SetPosition,

        [Description("reportaddsensor.htm")]
        ReportAddSensor,

        [Description("notificationtest.htm")]
        NotificationTest,

        /// <summary>
        /// Permanently removes an object from PRTG.
        /// </summary>
        [Description("deleteobject.htm")]
        DeleteObject,

        /// <summary>
        /// Clones an object and all children to under another object.
        /// </summary>
        [Description("duplicateobject.htm")]
        DuplicateObject,

        [Description("setlonlat.htm")]
        SetLonLat,

        /// <summary>
        /// Moves an object to under a new parent object.
        /// </summary>
        [Undocumented]
        [Description("moveobjectnow.htm")]
        MoveObjectNow,

        /// <summary>
        /// Sorts the children of an object in alphabetical order.
        /// </summary>
        [Description("sortsubobjects.htm")]
        SortSubObjects,

        /// <summary>
        /// Initiates a query for device specific information required to create a sensor.
        /// </summary>
        [Undocumented]
        [Description("controls/addsensor2.htm")]
        AddSensor2,

        /// <summary>
        /// Processes a query for device specific information required to create a sensor.
        /// </summary>
        [Undocumented]
        [Description("controls/addsensor3.htm")]
        AddSensor3,

        /// <summary>
        /// Creates a new sensor.
        /// </summary>
        [Undocumented]
        [Description("addsensor5.htm")]
        AddSensor5,

        /// <summary>
        /// Creates a new device.
        /// </summary>
        [Undocumented]
        [Description("adddevice2.htm")]
        AddDevice2,

        /// <summary>
        /// Creates a new group.
        /// </summary>
        [Undocumented]
        [Description("addgroup2.htm")]
        AddGroup2,

        /// <summary>
        /// Approve/Deny new probes
        /// </summary>
        [Description("probestate.htm")]
        ProbeState,

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
        LoadLookups,

        [Description("foldobject.htm")]
        FoldObject,

        [Description("sysinfochecknow.json")]
        SysInfoCheckNow
    }
}
