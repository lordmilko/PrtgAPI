using System.ComponentModel;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies the type of content to retrieve from a PRTG API Request.
    /// </summary>
    public enum Content
    {
        //SensorTree,

        /// <summary>
        /// System objects internally used by PRTG.
        /// </summary>
        [Description("basenode")]
        System,

        /// <summary>
        /// Site containing PRTG Network Monitor software used to monitor a network.
        /// </summary>
        [Description("probenode")]
        Probes,

        /// <summary>
        /// Groups used to organize devices.
        /// </summary>
        Groups,

        /// <summary>
        /// Computers and pieces of equipment that contain sensors monitored by PRTG.
        /// </summary>
        Devices,

        /// <summary>
        /// Objects that monitor and collect data - the fundamental components of PRTG.
        /// </summary>
        Sensors,

        /// <summary>
        /// Values within sensors that hold the results of monitoring operations.
        /// </summary>
        Channels,

        /// <summary>
        /// Notification actions for use with notification triggers.
        /// </summary>
        Notifications,

        /// <summary>
        /// Schedules that indicate when monitoring should be active.
        /// </summary>
        Schedules,

        /// <summary>
        /// All notification triggers defined on an object, including inherited triggers.
        /// </summary>
        Triggers,

        // <summary>
        // Notification triggers inherited by an object.
        // </summary>
        //Trigger,

        /// <summary>
        /// Historical data of a PRTG Sensor.
        /// </summary>
        Values,

        /// <summary>
        /// Object modification history.
        /// </summary>
        History,

        /// <summary>
        /// Event logs specific to an object and its children.
        /// </summary>
        [Description("messages")]
        Logs,

        /// <summary>
        /// A report used to report on the values of sensors.
        /// </summary>
        Report,

        /// <summary>
        /// A library used to organized objects.
        /// </summary>
        Library,

        /// <summary>
        /// Web Server settings.
        /// </summary>
        UnifiedOptions,

        /// <summary>
        /// A dashboard.
        /// </summary>
        Map,

        /*Tickets,
        TicketData,        
        Reports,
        TopLists,*/

        /// <summary>
        /// All content types, including those unsupported by PRTG. Note: PRTG does not recognize "objects" as a valid content type, and as such this property should be used with caution.
        /// </summary>
        Objects,

        /// <summary>
        /// A user account capable of logging into PRTG.
        /// </summary>
        User,

        /// <summary>
        /// A group used for organizing user accounts.
        /// </summary>
        UserGroup,

        /// <summary>
        /// Device system information.
        /// </summary>
        SysInfo
    }
}
