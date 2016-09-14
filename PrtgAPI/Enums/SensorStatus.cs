using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies states capable of being held by sensors.
    /// </summary>
    public enum SensorStatus
    {
        /// <summary>
        /// Sensor is transitioning between states.
        /// </summary>
        [XmlEnum("0")] None = 0,

        /// <summary>
        /// Sensor is unable to collect data.
        /// </summary>
        [XmlEnum("1")] Unknown = 1,
        [XmlEnum("2")] Collecting = 2,

        /// <summary>
        /// Sensor is in an up and working state.
        /// </summary>
        [XmlEnum("3")] Up = 3,

        /// <summary>
        /// Sensor is behaving abnormally, but has not yet failed.
        /// </summary>
        [XmlEnum("4")] Warning = 4,

        /// <summary>
        /// Sensor is down and has failed.
        /// </summary>
        [XmlEnum("5")] Down = 5,
        [XmlEnum("6")] NoProbe = 6,

        /// <summary>
        /// Sensor has been paused indefinitely by a user.
        /// </summary>
        [XmlEnum("7")] PausedbyUser = 7,

        /// <summary>
        /// Sensor has been paused due to a dependency on another sensor (e.g. the device is not pingable)
        /// </summary>
        [XmlEnum("8")] PausedbyDependency = 8,

        /// <summary>
        /// Sensor has been paused automatically by a schedule used to control monitoring windows.
        /// </summary>
        [XmlEnum("9")] PausedbySchedule = 9,

        /// <summary>
        /// Sensor data is outside of normal ranges, potentially indicating an issue.
        /// </summary>
        [XmlEnum("10")] Unusual = 10,

        /// <summary>
        /// Sensor has been paused due to sensor limits imposed by a chance in license.
        /// </summary>
        [XmlEnum("11")] PausedbyLicense = 11,

        /// <summary>
        /// Sensor has been paused temporarily by a user until a specified time period.
        /// </summary>
        [XmlEnum("12")] PausedUntil = 12,

        /// <summary>
        /// Sensor is down but has been acknowledged by a user.
        /// </summary>
        [XmlEnum("13")] DownAcknowledged = 13,

        /// <summary>
        /// Sensor is down for at least one node in a PRTG Cluster.
        /// </summary>
        [XmlEnum("14")] DownPartial = 14
    }
}
