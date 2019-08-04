namespace PrtgAPI
{
    /// <summary>
    /// Specifies types of system information that can exist on a device.
    /// </summary>
    public enum DeviceInfoType
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
        User,

        /// <summary>
        /// System process information.
        /// </summary>
        Process,

        /// <summary>
        /// System service information.
        /// </summary>
        Service
    }
}
