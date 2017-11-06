using System.Xml.Serialization;
using PrtgAPI.Attributes;

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

        /// <summary>
        /// A remote probe whose GID has been blocked attempted to connect to the PRTG Core.
        /// </summary>
        [XmlEnum("5")]
        ProbeGidDenied,

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

        /// <summary>
        /// Indicates a "Notify Changed" event occurred on an EXE/XML sensor, or that a WMI sensor encountered an error.
        /// </summary>
        [XmlEnum("303")]
        Notify,

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
        /// An FTP connection could not be established using the specified mode.
        /// </summary>
        [XmlEnum("407")]
        FTPMode,

        /// <summary>
        /// Object was created.
        /// </summary>
        [XmlEnum("501")]
        Created,

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

        /// <summary>
        /// A remote probe connected to the PRTG Core.
        /// </summary>
        [XmlEnum("612")]
        ProbeConnected,

        /// <summary>
        /// A remote probe disconnected from the PRTG Core.
        /// </summary>
        [XmlEnum("613")]
        ProbeDisconnected,

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
        /// A new probe connected to PRTG requiring user approval.
        /// </summary>
        [XmlEnum("710")]
        NewProbe,

        /// <summary>
        /// Indicates an error with PRTG's SMTP Configuration.
        /// </summary>
        [XmlEnum("711")]
        SmtpConfiguration,

        /// <summary>
        /// Indicates that a new cluster node has attempted to connect to PRTG and is pending approval.
        /// </summary>
        [XmlEnum("713")]
        NewClusterNode,

        /// <summary>
        /// Indicates that a PRTG update was successfully applied.
        /// </summary>
        [XmlEnum("720")]
        UpdateApplied,

        /// <summary>
        /// Authentication attempts have been throttled due to excessive failed logins.
        /// </summary>
        [XmlEnum("721")]
        ThrottleAuthentication,

        /// <summary>
        /// A reminder from PRTG or Paessler about an upcoming event or change.
        /// </summary>
        [XmlEnum("729")]
        Reminder,

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
        /// Informational event regarding the status of an active cluster connection, including error messages.
        /// </summary>
        [XmlEnum("858")]
        ClusterInfo

        //todo: KNOWN: 3 - "status message"
        //todo: KNOWN: 4 - "probe related"
        //todo: KNOWN: 6 - "status message"
        //todo: KNOWN: 803 - "auto-discovery related"
        //todo: KNOWN: 857 - "cluster related"
        //todo: KNOWN: 859
    }
}