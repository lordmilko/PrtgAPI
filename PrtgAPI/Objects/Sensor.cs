using System;
using System.Xml.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Objects.Shared;
using DH = PrtgAPI.Objects.Deserialization.DeserializationHelpers;

namespace PrtgAPI
{
    /// <summary>
    /// <para type="description">An object that monitors and collects information according to a defined schedule.</para>
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

        /// <summary>
        /// Whether this object has been marked as a favorite.
        /// </summary>
        [XmlElement("favorite_raw")]
        [PropertyParameter(nameof(Property.Favorite))]
        public bool Favorite { get; set; }

        // ################################## Sensors, Channel ##################################
        // There is a copy in both Sensor and Channel

        private string lastValue;

        /// <summary>
        /// Last value of this sensor's primary channel. If this sensor's primary channel has been recently changed, the sensor may need to be paused and unpause (otherwise it may just display "No Data").
        /// </summary>
        [XmlElement("lastvalue")]
        [PropertyParameter(nameof(Property.LastValue))]
        public string LastValue
        {
            get { return lastValue; }
            set { lastValue = value == string.Empty || value == "-" ? null : value; }
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
        [XmlElement("downtimetime_raw")]
        [PropertyParameter(nameof(Property.DowntimeTime))]
        public TimeSpan? TotalDowntime { get; set; }

        /// <summary>
        /// Amount of time passed since sensor was last in an up state. If sensor is currently up, this value is null.
        /// </summary>
        [XmlElement("downtimesince_raw")]
        [PropertyParameter(nameof(Property.DowntimeSince))]
        public TimeSpan? DownDuration { get; set; }

        /// <summary>
        /// Percentage indicating overall uptime of this object over its entire lifetime. See also: <see cref="Downtime"/>.
        /// </summary>
        [XmlElement("uptime")]
        [PropertyParameter(nameof(Property.Uptime))]
        public string Uptime { get; set; }

        /// <summary>
        /// Total amount of time sensor has ever been in an up state.
        /// </summary>
        [XmlElement("uptimetime_raw")]
        [PropertyParameter(nameof(Property.UptimeTime))]
        public TimeSpan? TotalUptime { get; set; }

        /// <summary>
        /// Amount of time passed since sensor was last in an down state. If sensor is currently down, this value is null.
        /// </summary>
        [XmlElement("uptimesince_raw")]
        [PropertyParameter(nameof(Property.UptimeSince))]
        public TimeSpan? UpDuration { get; set; }

        /// <summary>
        /// Total amount of time this sensor has been in an up or down state.
        /// </summary>
        [XmlElement("knowntime_raw")]
        [PropertyParameter(nameof(Property.KnownTime))]
        public TimeSpan TotalMonitorTime { get; set; }

        /// <summary>
        /// When data collection on this sensor began.
        /// </summary>
        [PropertyParameter(nameof(Property.CumSince))]
        [XmlElement("cumsince_raw")]
        public DateTime? DataCollectedSince { get; set; }

        /// <summary>
        /// When this sensor last checked for a value.
        /// </summary>
        [XmlElement("lastcheck_raw")]
        [PropertyParameter(nameof(Property.LastCheck))]
        public DateTime? LastCheck { get; set; }

        /// <summary>
        /// When this object was last in an up state.
        /// </summary>
        [XmlElement("lastup_raw")]
        [PropertyParameter(nameof(Property.LastUp))]
        public DateTime? LastUp { get; set; }
        
        /// <summary>
        /// When this value was last in a down state.
        /// </summary>
        [XmlElement("lastdown_raw")]
        [PropertyParameter(nameof(Property.LastDown))]
        public DateTime? LastDown { get; set; }

        /// <summary>
        /// CSV of sensor values for the past 24 hours. Numbers are stored as 5 minute averages. Value contains two sets of CSVs: measured values and errors. Sets are separated by a pipe . If MiniGraphs are disabled, this value is null.
        /// </summary>
        [XmlElement("minigraph")]
        [PropertyParameter(nameof(Property.MiniGraph))]
        public string MiniGraph { get; set; }       
    }
}
