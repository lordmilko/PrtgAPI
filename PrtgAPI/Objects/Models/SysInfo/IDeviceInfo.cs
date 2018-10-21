using System;

namespace PrtgAPI
{
    /// <summary>
    /// Represents an abstract piece of system information.
    /// </summary>
    public interface IDeviceInfo
    {
        /// <summary>
        /// ID of the device this object applies to.
        /// </summary>
        int DeviceId { get; set; }

        /// <summary>
        /// Type of system information represented by this object.
        /// </summary>
        DeviceInfoType Type { get; }

        /// <summary>
        /// Time this information was last received by PRTG from the target device.
        /// </summary>
        DateTime LastUpdated { get; }

        /// <summary>
        /// Display name of this object.
        /// </summary>
        string DisplayName { get; }
    }
}
