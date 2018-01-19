using System.ComponentModel;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies types of objects that can be targeted by API requests for retrieving and modifying object properties.
    /// </summary>
    public enum ObjectType
    {
        /// <summary>
        /// PRTG Device Sensors.
        /// </summary>
        Sensor,

        /// <summary>
        /// PRTG Devices.
        /// </summary>
        Device,

        /// <summary>
        /// PRTG Groups.
        /// </summary>
        Group,

        /// <summary>
        /// PRTG Probes.
        /// </summary>
        [Description("ProbeNode")]
        Probe
    }
}
