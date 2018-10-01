using System.ComponentModel;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies types of system information that can be retrieved for an object.
    /// </summary>
    public enum SystemInfoType
    {
        /// <summary>
        /// System information including IP/MAC Addresses, serial numbers and operating system versions.
        /// </summary>
        System,

        /// <summary>
        /// Installed software information.
        /// </summary>
        Software,

        /// <summary>
        /// Device hardware information, including CPU, Memory, Disk and Network Adapter info.
        /// </summary>
        Hardware,

        /// <summary>
        /// Logged on user information.
        /// </summary>
        [Description("loggedonusers")]
        Users,

        /// <summary>
        /// System process information.
        /// </summary>
        Processes,

        /// <summary>
        /// System service information.
        /// </summary>
        Services
    }
}
