using System.Xml.Serialization;
using DH = PrtgAPI.Objects.Deserialization.DeserializationHelpers;

namespace PrtgAPI
{
    /// <summary>
    /// Indicates the total number of each type of sensor on a PRTG Server.
    /// </summary>
    public class SensorTotals
    {
        /// <summary>
        /// Number of sensors in <see cref="SensorStatus.Up"/> state.
        /// </summary>
        public int UpSensors => DH.StrToInt(upSensors);

        /// <summary>
        /// Raw number of up sensors. This field is for internal use only.
        /// </summary>
        [XmlElement("upsens")]
        protected string upSensors { get; set; }

        /// <summary>
        /// Number of sensors in <see cref="SensorStatus.Down"/> state.
        /// </summary>
        public int DownSensors => DH.StrToInt(downSensors);

        /// <summary>
        /// Raw number of down sensors. This field is for internal use only.
        /// </summary>
        [XmlElement("downsens")]
        protected string downSensors { get; set; }

        /// <summary>
        /// Number of sensors in <see cref="SensorStatus.Warning"/> state.
        /// </summary>
        public int WarningSensors => DH.StrToInt(warningSensors);

        /// <summary>
        /// Raw number of warning sensors. This field is for internal use only.
        /// </summary>
        [XmlElement("warnsens")]
        protected string warningSensors { get; set; }

        /// <summary>
        /// Number of sensors in <see cref="SensorStatus.DownAcknowledged"/> state.
        /// </summary>
        public int DownAcknowledgedSensors => DH.StrToInt(downAcknowledgedSensors);

        /// <summary>
        /// Raw number of down acknowledged sensors. This field is for internal use only.
        /// </summary>
        [XmlElement("downacksens")]
        protected string downAcknowledgedSensors { get; set; }

        /// <summary>
        /// Number of sensors in <see cref="SensorStatus.DownPartial"/> state.
        /// </summary>
        public int PartialDownSensors => DH.StrToInt(partialDownSensors);

        /// <summary>
        /// Raw number of partial down sensors. This field is for internal use only.
        /// </summary>
        [XmlElement("partialdownsens")]
        protected string partialDownSensors { get; set; }

        /// <summary>
        /// Number of sensors in <see cref="SensorStatus.Unusual"/> state.
        /// </summary>
        public int UnusualSensors => DH.StrToInt(unusualSensors);

        /// <summary>
        /// Raw number of unusual sensors. This field is for internal use only.
        /// </summary>
        [XmlElement("unusualsens")]
        protected string unusualSensors { get; set; }

        /// <summary>
        /// Number of sensors in <see cref="SensorStatus.PausedByUser"/>, <see cref="SensorStatus.PausedByDependency"/>, <see cref="SensorStatus.PausedBySchedule"/> or <see cref="SensorStatus.PausedByLicense"/> state.
        /// </summary>
        public int PausedSensors => DH.StrToInt(pausedSensors);

        /// <summary>
        /// Raw number of paused sensors. This field is for internal use only.
        /// </summary>
        [XmlElement("pausedsens")]
        protected string pausedSensors { get; set; }

        /// <summary>
        /// Number of sensors in <see cref="SensorStatus.Unknown"/> state.
        /// </summary>
        public int UndefinedSensors => DH.StrToInt(undefinedSensors);

        /// <summary>
        /// Raw number of undefined sensors. This field is for internal use only.
        /// </summary>
        [XmlElement("undefinedsens")]
        protected string undefinedSensors { get; set; }

        /// <summary>
        /// Total number of sensors in any <see cref="SensorStatus"/> state.
        /// </summary>
        public int TotalSensors => DH.StrToInt(totalSensors);

        /// <summary>
        /// Raw number of total sensors. This field is for internal use only.
        /// </summary>
        [XmlElement("totalsens")]
        protected string totalSensors { get; set; }
    }
}
