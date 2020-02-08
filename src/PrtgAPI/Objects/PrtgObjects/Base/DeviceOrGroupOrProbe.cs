using System.Xml.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI
{
    /// <summary>
    /// <para type="description">Base class for Devices, Groups and Probes, containing properties that apply to all three object types.</para>
    /// </summary>
    public class DeviceOrGroupOrProbe : SensorOrDeviceOrGroupOrProbe, IDeviceOrGroupOrProbe
    {
        /// <summary>
        /// Number of sensors in <see cref="Status.Up"/> state.
        /// </summary>
        [XmlElement("upsens_raw")]
        [PropertyParameter(Property.UpSensors)]
        public int UpSensors { get; set; }

        /// <summary>
        /// Number of sensors in <see cref="Status.Down"/> state.
        /// </summary>
        [XmlElement("downsens_raw")]
        [PropertyParameter(Property.DownSensors)]
        public int DownSensors { get; set; }

        /// <summary>
        /// Number of sensors in <see cref="Status.DownAcknowledged"/> state.
        /// </summary>
        [XmlElement("downacksens_raw")]
        [PropertyParameter(Property.DownAcknowledgedSensors)]
        public int DownAcknowledgedSensors { get; set; }

        /// <summary>
        /// Number of sensors in <see cref="Status.DownPartial"/> state.
        /// </summary>
        [XmlElement("partialdownsens_raw")]
        [PropertyParameter(Property.PartialDownSensors)]
        public int PartialDownSensors { get; set; }

        /// <summary>
        /// Number of sensors in <see cref="Status.Warning"/> state.
        /// </summary>
        [XmlElement("warnsens_raw")]
        [PropertyParameter(Property.WarningSensors)]
        public int WarningSensors { get; set; }

        /// <summary>
        /// Number of sensors in <see cref="Status.PausedByUser"/>, <see cref="Status.PausedByDependency"/>, <see cref="Status.PausedBySchedule"/> or <see cref="Status.PausedByLicense"/> state.
        /// </summary>
        [XmlElement("pausedsens_raw")]
        [PropertyParameter(Property.PausedSensors)]
        public int PausedSensors { get; set; }

        /// <summary>
        /// Number of sensors in <see cref="Status.Unusual"/> state.
        /// </summary>
        [XmlElement("unusualsens_raw")]
        [PropertyParameter(Property.UnusualSensors)]
        public int UnusualSensors { get; set; }

        /// <summary>
        /// Number of sensors in <see cref="Status.Unknown"/> state.
        /// </summary>
        [XmlElement("undefinedsens_raw")]
        [PropertyParameter(Property.UnknownSensors)]
        public int UnknownSensors { get; set; }

        /// <summary>
        /// Total number of sensors contained under this object in any <see cref="Status"/> state.
        /// </summary>
        [XmlElement("totalsens")]
        [PropertyParameter(Property.TotalSensors)]
        public int TotalSensors { get; set; }
    }
}
