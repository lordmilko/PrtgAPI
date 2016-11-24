using System;
using System.Management.Automation;
using System.Xml.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Objects.Shared;
using DH = PrtgAPI.Objects.Deserialization.DeserializationHelpers;

namespace PrtgAPI
{
    /// <summary>
    /// An object that monitors and collects information according to a defined schedule.
    /// </summary>
    public class Sensor : SensorOrDeviceOrGroupOrProbe
    {
        // ################################## Sensors, Devices, Groups ##################################
        //Also in Group because group must be derived from GroupOrProbe
        //Also in Device because device must be derived from DeviceOrGroupOrProbe

        /// <summary>
        /// Probe that manages the execution of this sensor.
        /// </summary>
        [XmlElement("probe")]
        [PropertyParameter(nameof(Property.Probe))]
        public string Probe { get; set; }

        // ################################## Sensors, Devices ##################################
        // There is a copy in both Sensor and Device

        /// <summary>
        /// Group this sensor's <see cref="Device"/> is contained in.
        /// </summary>
        [XmlElement("group")]
        [PropertyParameter(nameof(Property.Group))]
        public string Group { get; set; }

        // ################################## Sensors, Channel ##################################
        // There is a copy in both Sensor and Channel

        private string lastvalue;

        /// <summary>
        /// Last value of this sensor's primary channel. If this sensor's primary channel has been recently changed, the sensor may need to be paused and unpause (otherwise it may just display "No Data").
        /// </summary>
        [XmlElement("lastvalue")]
        [PropertyParameter(nameof(Property.LastValue))]
        public string LastValue
        {
            get { return lastvalue; }
            set { lastvalue = value == string.Empty ? null : value; }
        }

        // ################################## Sensor Only ##################################

        /// <summary>
        /// Device this sensor monitors.
        /// </summary>
        [XmlElement("device")]
        [PropertyParameter(nameof(Property.Device))]
        public string Device { get; set; }

        /// <summary>
        /// Percentage indicating overall downtime of this object over its entire lifetime. See also: <see cref="Uptime"/>.
        /// </summary>
        [XmlElement("downtime")]
        [PropertyParameter(nameof(Property.Downtime))]
        public string Downtime { get; set; }

        /// <summary>
        /// Total amount of time sensor has ever been in a down state.
        /// </summary>
        [PropertyParameter(nameof(Property.DowntimeTime))]
        public TimeSpan? TotalDowntime => DH.ConvertPrtgTimeSpan(_RawTotalDowntime);

        /// <summary>
        /// Raw value used for <see cref="TotalDowntime"/> attribute. This property should not be used.
        /// </summary>
        [Hidden]
        [XmlElement("downtimetime_raw")]
        public double? _RawTotalDowntime { get; set; }

        /// <summary>
        /// Amount of time passed since sensor was last in an up state. If sensor is currently up, this value is null.
        /// </summary>
        [PropertyParameter(nameof(Property.DowntimeSince))]
        public TimeSpan? DownDuration => DH.ConvertPrtgTimeSpan(_RawDownDuration);

        /// <summary>
        /// Raw value used for <see cref="DownDuration"/> attribute. This property should not be used.
        /// </summary>
        [Hidden]
        [XmlElement("downtimesince_raw")]
        public double? _RawDownDuration { get; set; }

        /// <summary>
        /// Percentage indicating overall uptime of this object over its entire lifetime. See also: <see cref="Downtime"/>.
        /// </summary>
        [XmlElement("uptime")]
        [PropertyParameter(nameof(Property.Uptime))]
        public string Uptime { get; set; }

        /// <summary>
        /// Total amount of time sensor has ever been in an up state.
        /// </summary>
        [PropertyParameter(nameof(Property.UptimeTime))]
        public TimeSpan? TotalUptime => DH.ConvertPrtgTimeSpan(_RawTotalUptime);

        /// <summary>
        /// Raw value used for <see cref="TotalUptime"/> attribute. This property should not be used.
        /// </summary>
        [Hidden]
        [XmlElement("uptimetime_raw")]
        public double? _RawTotalUptime { get; set; }

        /// <summary>
        /// Amount of time passed since sensor was last in an down state. If sensor is currently down, this value is null.
        /// </summary>
        [PropertyParameter(nameof(Property.UptimeSince))]
        public TimeSpan? UpDuration => DH.ConvertPrtgTimeSpan(_RawUpDuration);

        /// <summary>
        /// Raw value used for <see cref="UpDuration"/> attribute. This property should not be used.
        /// </summary>
        [Hidden]
        [XmlElement("uptimesince_raw")]
        public double? _RawUpDuration { get; set; }

        /// <summary>
        /// Total amount of time this sensor has been in an up or down state.
        /// </summary>
        [PropertyParameter(nameof(Property.KnownTime))]
        public TimeSpan? TotalMonitorTime => DH.ConvertPrtgTimeSpan(_RawTotalMonitorTime);

        /// <summary>
        /// Raw value used for <see cref="TotalMonitorTime"/> attribute. This property should not be used.
        /// </summary>
        [Hidden]
        [XmlElement("knowntime_raw")]
        public double? _RawTotalMonitorTime { get; set; }

        /// <summary>
        /// When data collection on this sensor began.
        /// </summary>
        [PropertyParameter(nameof(Property.CumSince))]
        public DateTime? DataCollectedSince => DH.ConvertPrtgDateTime(_RawDataCollectedSince);

        /// <summary>
        /// Raw value used for <see cref="DataCollectedSince"/> attribute. This property should not be used.
        /// </summary>
        [Hidden]
        [XmlElement("cumsince_raw")]
        public double? _RawDataCollectedSince { get; set; }

        /// <summary>
        /// When this sensor last checked for a value.
        /// </summary>
        [PropertyParameter(nameof(Property.LastCheck))]
        public DateTime? LastCheck => DH.ConvertPrtgDateTime(_RawLastCheck);

        /// <summary>
        /// Raw value used for <see cref="LastCheck"/> attribute. This property should not be used.
        /// </summary>
        [Hidden]
        [XmlElement("lastcheck_raw")]
        public double? _RawLastCheck { get; set; }

        /// <summary>
        /// When this object was last in an up state.
        /// </summary>
        [PropertyParameter(nameof(Property.LastUp))]
        public DateTime? LastUp => DH.ConvertPrtgDateTime(_RawLastUp);

        /// <summary>
        /// Raw value used for <see cref="LastUp"/> attribute. This property should not be used.
        /// </summary>
        [Hidden]
        [XmlElement("lastup_raw")]
        public double? _RawLastUp { get; set; }

        /// <summary>
        /// When this value was last in a down state.
        /// </summary>
        [PropertyParameter(nameof(Property.LastDown))]
        public DateTime? LastDown => DH.ConvertPrtgDateTime(_RawLastDown);

        /// <summary>
        /// Raw value used for <see cref="LastDown"/> attribute. This property should not be used.
        /// </summary>
        [Hidden]
        [XmlElement("lastdown_raw")]
        public double? _RawLastDown { get; set; }

        /// <summary>
        /// CSV of sensor values for the past 24 hours. Numbers are stored as 5 minute averages. Value contains two sets of CSVs: measured values and errors. Sets are separated by a pipe . If MiniGraphs are disabled, this value is null.
        /// </summary>
        [Hidden]
        [XmlElement("minigraph")]
        [PropertyParameter(nameof(Property.MiniGraph))]
        public string MiniGraph { get; set; }       
    }
}
