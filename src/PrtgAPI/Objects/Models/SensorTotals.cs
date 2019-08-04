using System.Xml.Serialization;
using PrtgAPI.Request.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// <para type="description">Indicates the total number of each type of sensor on a PRTG Server.</para>
    /// </summary>
    public class SensorTotals
    {
        /// <summary>
        /// Number of sensors in <see cref="Status.Up"/> state.
        /// </summary>
        public int UpSensors => TypeHelpers.StrToInt(upSensors);

        [XmlElement("upsens")]
        internal string upSensors { get; set; }

        /// <summary>
        /// Number of sensors in <see cref="Status.Down"/> state.
        /// </summary>
        public int DownSensors => TypeHelpers.StrToInt(downSensors);

        [XmlElement("downsens")]
        internal string downSensors { get; set; }

        /// <summary>
        /// Number of sensors in <see cref="Status.Warning"/> state.
        /// </summary>
        public int WarningSensors => TypeHelpers.StrToInt(warningSensors);

        [XmlElement("warnsens")]
        internal string warningSensors { get; set; }

        /// <summary>
        /// Number of sensors in <see cref="Status.DownAcknowledged"/> state.
        /// </summary>
        public int DownAcknowledgedSensors => TypeHelpers.StrToInt(downAcknowledgedSensors);

        [XmlElement("downacksens")]
        internal string downAcknowledgedSensors { get; set; }

        /// <summary>
        /// Number of sensors in <see cref="Status.DownPartial"/> state.
        /// </summary>
        public int PartialDownSensors => TypeHelpers.StrToInt(partialDownSensors);

        [XmlElement("partialdownsens")]
        internal string partialDownSensors { get; set; }

        /// <summary>
        /// Number of sensors in <see cref="Status.Unusual"/> state.
        /// </summary>
        public int UnusualSensors => TypeHelpers.StrToInt(unusualSensors);

        [XmlElement("unusualsens")]
        internal string unusualSensors { get; set; }

        /// <summary>
        /// Number of sensors in <see cref="Status.PausedByUser"/>, <see cref="Status.PausedByDependency"/>, <see cref="Status.PausedBySchedule"/> or <see cref="Status.PausedByLicense"/> state.
        /// </summary>
        public int PausedSensors => TypeHelpers.StrToInt(pausedSensors);

        [XmlElement("pausedsens")]
        internal string pausedSensors { get; set; }

        /// <summary>
        /// Number of sensors in <see cref="Status.Unknown"/> state.
        /// </summary>
        public int UndefinedSensors => TypeHelpers.StrToInt(undefinedSensors);

        [XmlElement("undefinedsens")]
        internal string undefinedSensors { get; set; }

        /// <summary>
        /// Total number of sensors in any <see cref="Status"/> state.
        /// </summary>
        public int TotalSensors => TypeHelpers.StrToInt(totalSensors);

        [XmlElement("totalsens")]
        internal string totalSensors { get; set; }
    }
}
