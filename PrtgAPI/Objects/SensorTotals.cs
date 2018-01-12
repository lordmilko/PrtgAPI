using System.Xml.Serialization;
using DH = PrtgAPI.Objects.Deserialization.DeserializationHelpers;

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
        public int UpSensors => DH.StrToInt(upSensors);

        [XmlElement("upsens")]
        internal string upSensors { get; set; }

        /// <summary>
        /// Number of sensors in <see cref="Status.Down"/> state.
        /// </summary>
        public int DownSensors => DH.StrToInt(downSensors);

        [XmlElement("downsens")]
        internal string downSensors { get; set; }

        /// <summary>
        /// Number of sensors in <see cref="Status.Warning"/> state.
        /// </summary>
        public int WarningSensors => DH.StrToInt(warningSensors);

        [XmlElement("warnsens")]
        internal string warningSensors { get; set; }

        /// <summary>
        /// Number of sensors in <see cref="Status.DownAcknowledged"/> state.
        /// </summary>
        public int DownAcknowledgedSensors => DH.StrToInt(downAcknowledgedSensors);

        [XmlElement("downacksens")]
        internal string downAcknowledgedSensors { get; set; }

        /// <summary>
        /// Number of sensors in <see cref="Status.DownPartial"/> state.
        /// </summary>
        public int PartialDownSensors => DH.StrToInt(partialDownSensors);

        [XmlElement("partialdownsens")]
        internal string partialDownSensors { get; set; }

        /// <summary>
        /// Number of sensors in <see cref="Status.Unusual"/> state.
        /// </summary>
        public int UnusualSensors => DH.StrToInt(unusualSensors);

        [XmlElement("unusualsens")]
        internal string unusualSensors { get; set; }

        /// <summary>
        /// Number of sensors in <see cref="Status.PausedByUser"/>, <see cref="Status.PausedByDependency"/>, <see cref="Status.PausedBySchedule"/> or <see cref="Status.PausedByLicense"/> state.
        /// </summary>
        public int PausedSensors => DH.StrToInt(pausedSensors);

        [XmlElement("pausedsens")]
        internal string pausedSensors { get; set; }

        /// <summary>
        /// Number of sensors in <see cref="Status.Unknown"/> state.
        /// </summary>
        public int UndefinedSensors => DH.StrToInt(undefinedSensors);

        [XmlElement("undefinedsens")]
        internal string undefinedSensors { get; set; }

        /// <summary>
        /// Total number of sensors in any <see cref="Status"/> state.
        /// </summary>
        public int TotalSensors => DH.StrToInt(totalSensors);

        [XmlElement("totalsens")]
        internal string totalSensors { get; set; }
    }
}
