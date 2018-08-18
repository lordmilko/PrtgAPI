using System.ComponentModel;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies types of objects that can be targeted by API requests for retrieving and modifying object properties.
    /// </summary>
    public enum ObjectType
    {
        /// <summary>
        /// Objects used to monitor and collect information according to a specified scanning interval.
        /// </summary>
        Sensor,

        /// <summary>
        /// Objects used to organize and specify the physical systems that <see cref="Sensor"/> objects should target.
        /// </summary>
        Device,

        /// <summary>
        /// Organizational units used to group <see cref="Device"/> objects.
        /// </summary>
        Group,

        /// <summary>
        /// Physical systems used to distribute monitoring responsibilities across multiple nodes.
        /// </summary>
        [Description("probenode")]
        Probe,

        /// <summary>
        /// System dashboards.
        /// </summary>
        Map,

        /// <summary>
        /// An object used to generate reports based on one or more sensors on demand or according to a specified schedule.
        /// </summary>
        Report,

        /// <summary>
        /// A user account capable of authenticating to PRTG.
        /// </summary>
        User,

        /// <summary>
        /// A user group used to organize <see cref="User"/> objects based on role or responsibility.
        /// </summary>
        UserGroup,

        /// <summary>
        /// A notification action describing one or more actions to perform when alerting <see cref="User"/> objects to system activities.
        /// </summary>
        Notification,

        /// <summary>
        /// A schedule describing when monitoring should be active or paused.
        /// </summary>
        Schedule,

        /// <summary>
        /// A custom view of sensors of a specified type.
        /// </summary>
        Library,

        /// <summary>
        /// Root level system nodes used by PRTG.
        /// </summary>
        [Description("basenode")]
        System,

        /// <summary>
        /// Options used for configuring the PRTG Web Server.
        /// </summary>
        [Description("unifiedoptions")]
        WebServerOptions,

        /// <summary>
        /// A <see cref="User"/> or <see cref="Group"/> used for authenticating locally against PRTG.
        /// </summary>
        [Description("0")]
        PrtgUserOrGroup,

        /// <summary>
        /// A <see cref="User"/> or <see cref="Group"/> used for authenticating against PRTG via Active Directory.
        /// </summary>
        [Description("1")]
        ActiveDirectoryUserOrGroup

        //aggregation - apparently an objecttype?
    }
}
