using System;
using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// <para type="description">Specifies states capable of being held by sensors.</para>
    /// </summary>
    [Flags]
    public enum SensorStatus
    {
        /// <summary>
        /// Sensor is transitioning between states or is <see cref="Unknown"/> but is malfunctioning.
        /// </summary>
        [XmlEnum("0")] None = 1,

        /// <summary>
        /// Sensor is unable to collect data.
        /// </summary>
        [XmlEnum("1")] Unknown = 2,

        /// <summary>
        /// This value is no longer used by PRTG, however is considered analogous to <see cref="Up"/>.
        /// </summary>
        [XmlEnum("2")] Collecting = 4,

        /// <summary>
        /// Sensor is in an up and working state.
        /// </summary>
        [XmlEnum("3")] Up = 8,

        /// <summary>
        /// Sensor is behaving abnormally, but has not yet failed.
        /// </summary>
        [XmlEnum("4")] Warning = 16,

        /// <summary>
        /// Sensor is down and has failed.
        /// </summary>
        [XmlEnum("5")] Down = 32,

        /// <summary>
        /// This value is no longer used by PRTG, however is considered analogous to <see cref="Unknown"/>.
        /// </summary>
        [XmlEnum("6")] NoProbe = 64,

        /// <summary>
        /// Sensor has been paused indefinitely by a user.
        /// </summary>
        [XmlEnum("7")] PausedByUser = 128,

        /// <summary>
        /// Sensor has been paused due to a dependency on another sensor (e.g. the device is not pingable)
        /// </summary>
        [XmlEnum("8")] PausedByDependency = 256,

        /// <summary>
        /// Sensor has been paused automatically by a schedule used to control monitoring windows.
        /// </summary>
        [XmlEnum("9")] PausedBySchedule = 512,

        /// <summary>
        /// Sensor data is outside of normal ranges, potentially indicating an issue.
        /// </summary>
        [XmlEnum("10")] Unusual = 1024,

        /// <summary>
        /// Sensor has been paused due to sensor limits imposed by a chance in license.
        /// </summary>
        [XmlEnum("11")] PausedByLicense = 2048,

        /// <summary>
        /// Sensor has been paused temporarily by a user until a specified time period.
        /// </summary>
        [XmlEnum("12")] PausedUntil = 4096,

        /// <summary>
        /// Sensor is down but has been acknowledged by a user.
        /// </summary>
        [XmlEnum("13")] DownAcknowledged = 8192,

        /// <summary>
        /// Sensor is down for at least one node in a PRTG Cluster.
        /// </summary>
        [XmlEnum("14")] DownPartial = 16384,

        /// <summary>
        /// Sensor is in any paused state.
        /// </summary>
        Paused = PausedByDependency | PausedByLicense | PausedBySchedule | PausedByUser | PausedUntil
    }
}
