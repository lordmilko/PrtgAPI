using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies logging event categories.
    /// </summary>
    public enum LogStatus
    {
        /// <summary>
        /// PRTG Core Service started.
        /// </summary>
        [XmlEnum("1")]
        SystemStart,

        /// <summary>
        /// PRTG Core Service shut down.
        /// </summary>
        [XmlEnum("2")]
        SystemStop,

        //[XmlEnum("3")]
        //SystemReset,

        //[XmlEnum("4")]
        //ProbeIPDenied,

        /// <summary>
        /// A remote probe whose GID has been blocked attempted to connect to the PRTG Core.
        /// </summary>
        [XmlEnum("5")]
        ProbeGidDenied,

        //[XmlEnum("6")]
        //ConfigSaveError,

        /// <summary>
        /// Web server status message, potentially indicating issue starting PRTG Core Service.
        /// </summary>
        [XmlEnum("7")]
        WebServer,

        /// <summary>
        /// PRTG detected an update is available for install after having performed an <see cref="UpdateCheck"/>.
        /// </summary>
        [XmlEnum("8")]
        UpdateAvailable,

        /// <summary>
        /// Graph cache is being recalculated.
        /// </summary>
        [XmlEnum("9")]
        GraphCache,

        /// <summary>
        /// An internal exception occurred within PRTG while attempting to perform an operation.
        /// </summary>
        [XmlEnum("10")]
        Data,

        /// <summary>
        /// Informational message about a core system operation.
        /// </summary>
        [XmlEnum("11")]
        SystemInfo,

        /// <summary>
        /// A new probe that attempted to connect to PRTG was denied.
        /// </summary>
        [XmlEnum("12")]
        ProbeDenied,

        /// <summary>
        /// Notification trigger was activated on object.
        /// </summary>
        [XmlEnum("13")]
        NotificationItem,

        /// <summary>
        /// A remote probe with an incorrect version or invalid access key attempted to connect to the PRTG Core.
        /// </summary>
        [XmlEnum("14")]
        ProbeKeyDenied,

        /*[XmlEnum("101")]
        DataError,

        [XmlEnum("102")]
        ResponseError,

        [XmlEnum("103")]
        InternalError,

        [XmlEnum("104")]
        SocketError,

        [XmlEnum("105")]
        ICMPError,

        [XmlEnum("108")]
        WMIError,

        [XmlEnum("109")]
        UnsupportedSensor,

        [XmlEnum("201")]
        CounterValue,

        [XmlEnum("202")]
        IntegerValue,

        [XmlEnum("203")]
        FloatValue,

        [XmlEnum("301")]
        NotifyContent,

        [XmlEnum("302")]
        NotifySize,*/

        /// <summary>
        /// Indicates a "Notify Changed" event occurred on an EXE/XML sensor, or that a WMI sensor encountered an error.
        /// </summary>
        [XmlEnum("303")]
        NotifyChanged,

        //[XmlEnum("401")]
        //Text,

        /// <summary>
        /// Miscellaneous informational item, including certain ticket creation events.
        /// </summary>
        [XmlEnum("402")]
        Info,

        /// <summary>
        /// A sensor has fallen back to using WMI as Performance Counters are malfunctioning.
        /// </summary>
        [XmlEnum("404")]
        WMIFallback,

        /// <summary>
        /// A sensor that was previously in status <see cref="WMIFallback"/> has resumed working normally.
        /// </summary>
        [XmlEnum("405")]
        UsePerfCounters,

        /// <summary>
        /// The type of mode that was used to establish the FTP connection (e.g. implicit, explicit).
        /// </summary>
        [XmlEnum("406")] 
        FtpConnectionMode, 

        /// <summary>
        /// An FTP connection could not be established using the specified mode.
        /// </summary>
        [XmlEnum("407")]
        FtpMode,

        /// <summary>
        /// Object was created.
        /// </summary>
        [XmlEnum("501")]
        Created,

        /// <summary>
        /// Object with the same ID was previously deleted.<para/>
        /// Under normal circumstances object IDs cannot be reused, however this status can occur when the PRTG Configuration
        /// is reverted to before an object was initially created.
        /// </summary>
        [XmlEnum("502")]
        Deleted,

        /// <summary>
        /// An object was moved to another parent. States where the object was moved from and to. Occurs in conjunction with <see cref="ChildObjectMoved"/>.
        /// </summary>
        [XmlEnum("503")]
        Moved,

        /// <summary>
        /// Object settings were modified.
        /// </summary>
        [XmlEnum("504")]
        Edited,

        /// <summary>
        /// A new child object (such as a sensor) was created.
        /// </summary>
        [XmlEnum("505")]
        NewChildObject,

        /// <summary>
        /// A child object (such as a sensor) was deleted.
        /// </summary>
        [XmlEnum("506")]
        ChildObjectDeleted,

        /// <summary>
        /// An child of this object was moved to another parent. Event states what was moved. Occurs in conjunction with <see cref="Moved"/>.
        /// </summary>
        [XmlEnum("507")]
        ChildObjectMoved,

        /// <summary>
        /// An object that was <see cref="Moved"/> was inserted.
        /// </summary>
        [XmlEnum("508")]
        MovedObjectInserted,

        /// <summary>
        /// A new subnode (such as a notification trigger) was created.
        /// </summary>
        [XmlEnum("509")]
        SubnodeCreated,

        /// <summary>
        /// A subnode (such as a notification trigger) was deleted
        /// </summary>
        [XmlEnum("510")]
        SubnodeDeleted,

        /// <summary>
        /// A subnode (such as a channel or notification trigger) was edited.
        /// </summary>
        [XmlEnum("511")]
        SubnodeEdited,

        /// <summary>
        /// Object was created from cloning another object.
        /// </summary>
        [XmlEnum("512")]
        CreatedFromClone,

        //[XmlEnum("513")]
        //LicenseEdited,

        /// <summary>
        /// A license could not be found.
        /// </summary>
        [XmlEnum("514")]
        Activation,

        /// <summary>
        /// Sensor is unable to collect data.
        /// </summary>
        [XmlEnum("601")]
        Unknown,

        /// <summary>
        /// Monitoring of a previously paused object is in the process of being resumed.
        /// </summary>
        [XmlEnum("602")]
        Resuming,

        /// <summary>
        /// Object is in the process of being paused. Shows the pause message that was specified for the object (if applicable)
        /// </summary>
        [XmlEnum("603")]
        Pausing,

        /// <summary>
        /// Object has been paused indefinitely by a user.
        /// </summary>
        [XmlEnum("604")]
        PausedByUser,

        /// <summary>
        /// Object has been paused due to a dependency on another sensor (e.g. the device is not pingable)
        /// </summary>
        [XmlEnum("605")]
        PausedByDependency,

        /// <summary>
        /// Object has been paused automatically by a schedule used to control monitoring windows.
        /// </summary>
        [XmlEnum("606")]
        PausedBySchedule,

        /// <summary>
        /// Object is in an up and working state.
        /// </summary>
        [XmlEnum("607")]
        Up,

        /// <summary>
        /// Object is down and has failed.
        /// </summary>
        [XmlEnum("608")]
        Down,

        /// <summary>
        /// Sensor is behaving abnormally, but has not yet failed.
        /// </summary>
        [XmlEnum("609")]
        Warning,

        /// <summary>
        /// Sensor data is outside of normal ranges, potentially indicating an issue.
        /// </summary>
        [XmlEnum("610")]
        Unusual,

        //[XmlEnum("611")]
        //OK,

        /// <summary>
        /// A remote probe connected to the PRTG Core.
        /// </summary>
        [XmlEnum("612")]
        Connected,

        /// <summary>
        /// A remote probe disconnected from the PRTG Core.
        /// </summary>
        [XmlEnum("613")]
        Disconnected,

        /// <summary>
        /// Probe status message (e.g. restart occurred)
        /// </summary>
        [XmlEnum("614")]
        ProbeInfo,

        /// <summary>
        /// Sensor has been paused due to sensor limits imposed by a chance in license.
        /// </summary>
        [XmlEnum("615")]
        PausedByLicense,

        //[XmlEnum("616")]
        //NoProbe,

        /// <summary>
        /// Previously paused object has completed <see cref="Resuming"/> and is now active.
        /// </summary>
        [XmlEnum("618")]
        Active,

        /// <summary>
        /// Sensor is down but has been acknowledged by a user.
        /// </summary>
        [XmlEnum("619")]
        DownAcknowledged,

        /// <summary>
        /// Sensor is down for at least one node in a PRTG Cluster.
        /// </summary>
        [XmlEnum("620")]
        DownPartial,

        /// <summary>
        /// A new probe was approved and added to PRTG.
        /// </summary>
        [XmlEnum("701")]
        ProbeApproved,

        /*[XmlEnum("702")]
        NewDevice,

        [XmlEnum("703")]
        NewSensor,

        [XmlEnum("704")]
        DiskFull,

        [XmlEnum("705")]
        EmailFailed,*/

        /// <summary>
        /// A software update check was performed.
        /// </summary>
        [XmlEnum("706")]
        UpdateCheck,

        /// <summary>
        /// Could not complete operation due to critical error (such as performance issue).
        /// </summary>
        [XmlEnum("707")]
        SystemError,

        /// <summary>
        /// A scheduled report has completed.
        /// </summary>
        [XmlEnum("708")]
        ReportDone,

        //[XmlEnum("709")]
        //LicenseError,

        /// <summary>
        /// A new probe connected to PRTG requiring user approval.
        /// </summary>
        [XmlEnum("710")]
        NewProbe,

        /// <summary>
        /// Indicates an error with PRTG's SMTP Configuration.
        /// </summary>
        [XmlEnum("711")]
        SmtpConfiguration,

        //[XmlEnum("712")]
        //Dependency,

        /// <summary>
        /// Indicates that a new cluster node has attempted to connect to PRTG and is pending approval.
        /// </summary>
        [XmlEnum("713")]
        NewClusterNode,

        /*[XmlEnum("714")]
        NodeNotActivated,

        [XmlEnum("715")]
        ClusterSynchronization,

        [XmlEnum("716")]
        BrowserVersion,

        [XmlEnum("717")]
        ConfigConflict,

        [XmlEnum("718")]
        ConfigUpdate,

        [XmlEnum("719")]
        SoftwareMaintenance,*/

        /// <summary>
        /// Indicates that a PRTG update was successfully applied.
        /// </summary>
        [XmlEnum("720")]
        UpdateApplied,

        /// <summary>
        /// Authentication attempts have been throttled due to excessive failed logins.
        /// </summary>
        [XmlEnum("721")]
        OverloadProtection,

        /*[XmlEnum("722")]
        UpgradeOffer,

        [XmlEnum("723")]
        TrialReminder,

        [XmlEnum("728")]
        InvalidPushDevice,*/

        /// <summary>
        /// A reminder from PRTG or Paessler about an upcoming event or change.
        /// </summary>
        [XmlEnum("729")]
        Reminder,

        //[XmlEnum("730")]
        //SmsConfiguration,

        /// <summary>
        /// A group auto-discovery operation started.
        /// </summary>
        [XmlEnum("801")]
        GroupDiscoveryStarted,

        /// <summary>
        /// A group auto-discovery operation completed.
        /// </summary>
        [XmlEnum("802")]
        GroupDiscoveryFinished,

        /// <summary>
        /// A new device was discovered by auto-discovery
        /// </summary>
        [XmlEnum("804")]
        DiscoveryDeviceFound,

        /// <summary>
        /// An auto-discovery template was applied to a device.
        /// </summary>
        [XmlEnum("805")]
        DiscoveryTemplateApplied,

        /// <summary>
        /// An auto-discovery operation failed.
        /// </summary>
        [XmlEnum("806")]
        DiscoveryFailed,

        /// <summary>
        /// The clustering engine has started.
        /// </summary>
        [XmlEnum("851")]
        ClusterStart,

        /// <summary>
        /// The clustering engine has stopped.
        /// </summary>
        [XmlEnum("852")]
        ClusterStop,

        /// <summary>
        /// Indicates clustering has initialized on a single cluster node.
        /// </summary>
        [XmlEnum("853")]
        ClusterInit,

        /// <summary>
        /// Indicates that two initialized cluster nodes have successfully connected to one another
        /// </summary>
        [XmlEnum("854")]
        ClusterConnection,

        /// <summary>
        /// Indicates that a cluster successfully connected to a new cluster node.
        /// </summary>
        [XmlEnum("855")]
        ClusterNodeConnected,

        /// <summary>
        /// Indicates that a cluster node disconnected from a cluster.
        /// </summary>
        [XmlEnum("856")]
        ClusterNodeDisconnected,

        /// <summary>
        /// The master node of a PRTG Cluster changed.
        /// </summary>
        [XmlEnum("857")]
        ClusterMasterChanged,

        /// <summary>
        /// Informational event regarding the status of an active cluster connection, including error messages.
        /// </summary>
        [XmlEnum("858")]
        ClusterInfo,

        /// <summary>
        /// Multiple PRTG Cluster master nodes were detected.
        /// </summary>
        [XmlEnum("859")]
        MultipleClusterMasters,

        //[XmlEnum("901")]
        //UserTicket,

        //[XmlEnum("902")]
        //NotificationTicket
    }
}
