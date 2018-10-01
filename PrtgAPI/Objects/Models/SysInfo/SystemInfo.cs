using System.Collections.Generic;

namespace PrtgAPI
{
    /// <summary>
    /// Represents System Information used to describe the state of a device.
    /// </summary>
    public class SystemInfo
    {
        /// <summary>
        /// ID of the device this information applies to.
        /// </summary>
        public int DeviceId { get; }

        /// <summary>
        /// Core system information of a device.
        /// </summary>
        public List<DeviceSystemInfo> System { get; }

        /// <summary>
        /// Hardware installed in a device.
        /// </summary>
        public List<DeviceHardwareInfo> Hardware { get; }

        /// <summary>
        /// Software installed on a device.
        /// </summary>
        public List<DeviceSoftwareInfo> Software { get; }

        /// <summary>
        /// Processes running on a device.
        /// </summary>
        public List<DeviceProcessInfo> Processes { get; }

        /// <summary>
        /// Services installed on a device.
        /// </summary>
        public List<DeviceServiceInfo> Services { get; }

        /// <summary>
        /// Users logged into a device.
        /// </summary>
        public List<DeviceUserInfo> Users { get; }

        internal SystemInfo(int deviceId, List<DeviceSystemInfo> system, List<DeviceHardwareInfo> hardware, List<DeviceSoftwareInfo> software,
            List<DeviceProcessInfo> processes, List<DeviceServiceInfo> services, List<DeviceUserInfo> users)
        {
            DeviceId = deviceId;
            System = system;
            Hardware = hardware;
            Software = software;
            Processes = processes;
            Services = services;
            Users = users;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"System Information (ID: {DeviceId})";
        }
    }
}
