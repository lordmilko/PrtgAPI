using System;
using System.Xml.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI
{
    /// <summary>
    /// <para type="description">An object that monitors and collects information according to a defined scanning interval.</para>
    /// </summary>
    public class Sensor : SensorOrDeviceOrGroupOrProbe, ISensor
    {
        // ################################## Sensors, Devices, Groups ##################################
        //Also in Group because group must be derived from GroupOrProbe
        //Also in Device because device must be derived from DeviceOrGroupOrProbe

        /// <summary>
        /// Probe that manages the execution of this sensor.
        /// </summary>
        [XmlElement("probe")]
        [PropertyParameter(Property.Probe)]
        public string Probe { get; set; }

        // ################################## Sensors, Devices ##################################
        // There is a copy in both Sensor and Device

        /// <summary>
        /// Group this sensor's <see cref="Device"/> is contained in.
        /// </summary>
        [XmlElement("group")]
        [PropertyParameter(Property.Group)]
        public string Group { get; set; }

        /// <summary>
        /// Whether this object has been marked as a favorite.
        /// </summary>
        [XmlElement("favorite_raw")]
        [PropertyParameter(Property.Favorite)]
        public bool Favorite { get; set; }

        // ################################## Sensors, Channel ##################################
        // There is a copy in both Sensor and Channel

        private string displayLastValue;

        /// <summary>
        /// Last value of this sensor's primary channel with value unit. If this sensor's primary channel has been recently changed, the sensor may need to be paused and unpaused (otherwise it may just display "No data").
        /// </summary>
        [XmlElement("lastvalue")]
        public string DisplayLastValue
        {
            get { return displayLastValue; }
            set { displayLastValue = value == string.Empty || value == "-" ? null : value; }
        }

        /// <summary>
        /// The raw last value of this sensor's primary channel.
        /// </summary>
        [XmlElement("lastvalue_raw")]
        [PropertyParameter(Property.LastValue)]
        public double? LastValue { get; set; }

        // ################################## Sensor Only ##################################

        /// <summary>
        /// Device this sensor monitors.
        /// </summary>
        [XmlElement("device")]
        [PropertyParameter(Property.Device)]
        public string Device { get; set; }

        /// <summary>
        /// Percentage indicating overall downtime of this object over its entire lifetime. See also: <see cref="Uptime"/>.
        /// </summary>
        [XmlElement("downtime_raw")]
        [PropertyParameter(Property.Downtime)]
        public double? Downtime { get; set; }

        /// <summary>
        /// Total amount of time sensor has ever been in a <see cref="Status.Down"/> or <see cref="Status.DownAcknowledged"/> state.
        /// </summary>
        [XmlElement("downtimetime_raw")]
        [PropertyParameter(Property.TotalDowntime)]
        public TimeSpan? TotalDowntime { get; set; }

        /// <summary>
        /// Amount of time passed since this object was last in an <see cref="Status.Up"/> state. If this object is currently <see cref="Status.Up"/>, this value is null.
        /// </summary>
        [XmlElement("downtimesince_raw")]
        [PropertyParameter(Property.DownDuration)]
        public TimeSpan? DownDuration { get; set; }

        /// <summary>
        /// Percentage indicating overall uptime of this object over its entire lifetime. See also: <see cref="Downtime"/>.
        /// </summary>
        [XmlElement("uptime_raw")]
        [PropertyParameter(Property.Uptime)]
        public double? Uptime { get; set; }

        /// <summary>
        /// Total amount of time sensor has ever been in a <see cref="Status.Up"/> state.
        /// </summary>
        [XmlElement("uptimetime_raw")]
        [PropertyParameter(Property.TotalUptime)]
        public TimeSpan? TotalUptime { get; set; }

        /// <summary>
        /// Amount of time passed since sensor was last in a <see cref="Status.Down"/> state. If sensor is currently <see cref="Status.Down"/>, this value is null.
        /// </summary>
        [XmlElement("uptimesince_raw")]
        [PropertyParameter(Property.UpDuration)]
        public TimeSpan? UpDuration { get; set; }

        /// <summary>
        /// Total amount of time this sensor has been in an up or down state.
        /// </summary>
        [XmlElement("knowntime_raw")]
        [PropertyParameter(Property.TotalMonitorTime)]
        public TimeSpan TotalMonitorTime { get; set; }

        /// <summary>
        /// When data collection on this sensor began.
        /// </summary>
        [PropertyParameter(Property.DataCollectedSince)]
        [XmlElement("cumsince_raw")]
        public DateTime? DataCollectedSince { get; set; }

        /// <summary>
        /// When this sensor last checked for a value.
        /// </summary>
        [XmlElement("lastcheck_raw")]
        [PropertyParameter(Property.LastCheck)]
        public DateTime? LastCheck { get; set; }

        /// <summary>
        /// When this object was last in an <see cref="Status.Up"/> state.
        /// </summary>
        [XmlElement("lastup_raw")]
        [PropertyParameter(Property.LastUp)]
        public DateTime? LastUp { get; set; }
        
        /// <summary>
        /// When this value was last in a down state.
        /// </summary>
        [XmlElement("lastdown_raw")]
        [PropertyParameter(Property.LastDown)]
        public DateTime? LastDown { get; set; }

        /// <summary>
        /// CSV of sensor values for the past 24 hours. Numbers are stored as 5 minute averages. Value contains two sets of CSVs: measured values and errors. Sets are separated by a pipe . If MiniGraphs are disabled, this value is null.
        /// </summary>
        [XmlElement("minigraph")]
        [PropertyParameter(Property.MiniGraph)]
        public string MiniGraph { get; set; }       
    }
}
