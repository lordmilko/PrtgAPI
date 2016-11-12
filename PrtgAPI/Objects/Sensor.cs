using System;
using System.Xml.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Objects.Shared;

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
        [PSVisible(true)]
        public string Probe { get; set; }

        // ################################## Sensors, Devices ##################################
        // There is a copy in both Sensor and Device

        /// <summary>
        /// Group this sensor's <see cref="Device"/> is contained in.
        /// </summary>
        [XmlElement("group")]
        [PropertyParameter(nameof(Property.Group))]
        [PSVisible(true)]
        public string Group { get; set; }

        // ################################## Sensors, Channel ##################################
        // There is a copy in both Sensor and Channel

        private string lastvalue;

        /// <summary>
        /// Last value of this sensor's primary channel. If this sensor's primary channel has been recently changed, the sensor may need to be paused and unpause (otherwise it may just display "No Data").
        /// </summary>
        [XmlElement("lastvalue")]
        [PropertyParameter(nameof(Property.LastValue))]
        [PSVisible(true)]
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
        [PSVisible(true)]
        public string Device { get; set; }

        /// <summary>
        /// Percentage indicating overall downtime of this object over its entire lifetime. See also: <see cref="Uptime"/>.
        /// </summary>
        [XmlElement("downtime")]
        [PropertyParameter(nameof(Property.Downtime))]
        [PSVisible(true)]
        public string Downtime { get; set; }

        /// <summary>
        /// Total amount of time sensor has ever been in a down state.
        /// </summary>
        [PropertyParameter(nameof(Property.DowntimeTime))]
        [PSVisible(true)]
        public TimeSpan? TotalDowntime => ConvertPrtgTimeSpan(_RawTotalDowntime);

        /// <summary>
        /// Raw value used for <see cref="TotalDowntime"/> attribute. This property should not be used.
        /// </summary>
        [XmlElement("downtimetime_raw")]
        [PSVisible(false)]
        public double? _RawTotalDowntime { get; set; }

        /// <summary>
        /// Amount of time passed since sensor was last in an up state. If sensor is currently up, this value is null.
        /// </summary>
        [PropertyParameter(nameof(Property.DowntimeSince))]
        [PSVisible(true)]
        public TimeSpan? DownDuration => ConvertPrtgTimeSpan(_RawDownDuration);

        /// <summary>
        /// Raw value used for <see cref="DownDuration"/> attribute. This property should not be used.
        /// </summary>
        [XmlElement("downtimesince_raw")]
        [PSVisible(false)]
        public double? _RawDownDuration { get; set; }

        /// <summary>
        /// Percentage indicating overall uptime of this object over its entire lifetime. See also: <see cref="Downtime"/>.
        /// </summary>
        [XmlElement("uptime")]
        [PropertyParameter(nameof(Property.Uptime))]
        [PSVisible(true)]
        public string Uptime { get; set; }

        /// <summary>
        /// Total amount of time sensor has ever been in an up state.
        /// </summary>
        [PropertyParameter(nameof(Property.UptimeTime))]
        [PSVisible(true)]
        public TimeSpan? TotalUptime => ConvertPrtgTimeSpan(_RawTotalUptime);

        /// <summary>
        /// Raw value used for <see cref="TotalUptime"/> attribute. This property should not be used.
        /// </summary>
        [XmlElement("uptimetime_raw")]
        [PSVisible(false)]
        public double? _RawTotalUptime { get; set; }

        /// <summary>
        /// Amount of time passed since sensor was last in an down state. If sensor is currently down, this value is null.
        /// </summary>
        [PropertyParameter(nameof(Property.UptimeSince))]
        [PSVisible(true)]
        public TimeSpan? UpDuration => ConvertPrtgTimeSpan(_RawUpDuration);

        /// <summary>
        /// Raw value used for <see cref="UpDuration"/> attribute. This property should not be used.
        /// </summary>
        [XmlElement("uptimesince_raw")]
        [PSVisible(false)]
        public double? _RawUpDuration { get; set; }

        /// <summary>
        /// Total amount of time this sensor has been in an up or down state.
        /// </summary>
        [PropertyParameter(nameof(Property.KnownTime))]
        [PSVisible(true)]
        public TimeSpan? TotalMonitorTime => ConvertPrtgTimeSpan(_RawTotalMonitorTime);

        /// <summary>
        /// Raw value used for <see cref="TotalMonitorTime"/> attribute. This property should not be used.
        /// </summary>
        [XmlElement("knowntime_raw")]
        [PSVisible(false)]
        public double? _RawTotalMonitorTime { get; set; }

        /// <summary>
        /// When data collection on this sensor began.
        /// </summary>
        [PropertyParameter(nameof(Property.CumSince))]
        [PSVisible(true)]
        public DateTime? DataCollectedSince => ConvertPrtgDateTime(_RawDataCollectedSince);

        /// <summary>
        /// Raw value used for <see cref="DataCollectedSince"/> attribute. This property should not be used.
        /// </summary>
        [XmlElement("cumsince_raw")]
        [PSVisible(false)]
        public double? _RawDataCollectedSince { get; set; }

        /// <summary>
        /// When this sensor last checked for a value.
        /// </summary>
        [PropertyParameter(nameof(Property.LastCheck))]
        [PSVisible(true)]
        public DateTime? LastCheck => ConvertPrtgDateTime(_RawLastCheck);

        /// <summary>
        /// Raw value used for <see cref="LastCheck"/> attribute. This property should not be used.
        /// </summary>
        [XmlElement("lastcheck_raw")]
        [PSVisible(false)]
        public double? _RawLastCheck { get; set; }

        /// <summary>
        /// When this object was last in an up state.
        /// </summary>
        [PropertyParameter(nameof(Property.LastUp))]
        [PSVisible(true)]
        public DateTime? LastUp => ConvertPrtgDateTime(_RawLastUp);

        /// <summary>
        /// Raw value used for <see cref="LastUp"/> attribute. This property should not be used.
        /// </summary>
        [XmlElement("lastup_raw")]
        [PSVisible(false)]
        public double? _RawLastUp { get; set; }

        /// <summary>
        /// When this value was last in a down state.
        /// </summary>
        [PropertyParameter(nameof(Property.LastDown))]
        [PSVisible(true)]
        public DateTime? LastDown => ConvertPrtgDateTime(_RawLastDown);

        /// <summary>
        /// Raw value used for <see cref="LastDown"/> attribute. This property should not be used.
        /// </summary>
        [XmlElement("lastdown_raw")]
        [PSVisible(false)]
        public double? _RawLastDown { get; set; }

        /// <summary>
        /// CSV of sensor values for the past 24 hours. Numbers are stored as 5 minute averages. Value contains two sets of CSVs: measured values and errors. Sets are separated by a pipe . If MiniGraphs are disabled, this value is null.
        /// </summary>
        [XmlElement("minigraph")]
        [PropertyParameter(nameof(Property.MiniGraph))]
        [PSVisible(false)]
        public string MiniGraph { get; set; }       
    }
}
