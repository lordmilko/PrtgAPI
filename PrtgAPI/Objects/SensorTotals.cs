using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public int UpSensors => DH.StrToInt(_RawUpSensors);

        /// <summary>
        /// Raw value used for <see cref="UpSensors"/> attribute. This property should not be used.
        /// </summary>
        [XmlElement("upsens")]
        public string _RawUpSensors { get; set; }

        /// <summary>
        /// Number of sensors in <see cref="SensorStatus.Down"/> state.
        /// </summary>
        public int DownSensors => DH.StrToInt(_RawDownSensors);

        /// <summary>
        /// Raw value used for <see cref="DownSensors"/> attribute. This property should not be used.
        /// </summary>
        [XmlElement("downsens")]
        public string _RawDownSensors { get; set; }

        /// <summary>
        /// Number of sensors in <see cref="SensorStatus.Warning"/> state.
        /// </summary>
        public int WarningSensors => DH.StrToInt(_RawWarningSensors);

        /// <summary>
        /// Raw value used for <see cref="WarningSensors"/> attribute. This property should not be used.
        /// </summary>
        [XmlElement("warnsens")]
        public string _RawWarningSensors { get; set; }

        /// <summary>
        /// Number of sensors in <see cref="SensorStatus.DownAcknowledged"/> state.
        /// </summary>
        public int DownAcknowledgedSensors => DH.StrToInt(_RawDownAcknowledgedSensors);

        /// <summary>
        /// Raw value used for <see cref="DownAcknowledgedSensors"/> attribute. This property should not be used.
        /// </summary>
        [XmlElement("downacksens")]
        public string _RawDownAcknowledgedSensors { get; set; }

        /// <summary>
        /// Number of sensors in <see cref="SensorStatus.DownPartial"/> state.
        /// </summary>
        public int PartialDownSensors => DH.StrToInt(_RawPartialDownSensors);

        /// <summary>
        /// Raw value used for <see cref="PartialDownSensors"/> attribute. This property should not be used.
        /// </summary>
        [XmlElement("partialdownsens")]
        public string _RawPartialDownSensors { get; set; }

        /// <summary>
        /// Number of sensors in <see cref="SensorStatus.Unusual"/> state.
        /// </summary>
        public int UnusualSensors => DH.StrToInt(_RawUnusualSensors);

        /// <summary>
        /// Raw value used for <see cref="UnusualSensors"/> attribute. This property should not be used.
        /// </summary>
        [XmlElement("unusualsens")]
        public string _RawUnusualSensors { get; set; }

        /// <summary>
        /// Number of sensors in <see cref="SensorStatus.PausedByUser"/>, <see cref="SensorStatus.PausedByDependency"/>, <see cref="SensorStatus.PausedBySchedule"/> or <see cref="SensorStatus.PausedByLicense"/> state.
        /// </summary>
        public int PausedSensors => DH.StrToInt(_RawPausedSensors);

        /// <summary>
        /// Raw value used for <see cref="PausedSensors"/> attribute. This property should not be used.
        /// </summary>
        [XmlElement("pausedsens")]
        public string _RawPausedSensors { get; set; }

        /// <summary>
        /// Number of sensors in <see cref="SensorStatus.Unknown"/> state.
        /// </summary>
        public int UndefinedSensors => DH.StrToInt(_RawUndefinedSensors);

        /// <summary>
        /// Raw value used for <see cref="UndefinedSensors"/> attribute. This property should not be used.
        /// </summary>
        [XmlElement("undefinedsens")]
        public string _RawUndefinedSensors { get; set; }

        /// <summary>
        /// Total number of sensors in any <see cref="SensorStatus"/> state.
        /// </summary>
        public int TotalSensors => DH.StrToInt(_RawTotalSensors);

        /// <summary>
        /// Raw value used for <see cref="TotalSensors"/> attribute. This property should not be used.
        /// </summary>
        [XmlElement("totalsens")]
        public string _RawTotalSensors { get; set; }
    }
}
