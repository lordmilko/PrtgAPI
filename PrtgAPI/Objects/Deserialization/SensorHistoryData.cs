using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Represents historical monitoring data for a sensor at a specified time period.
    /// </summary>
    [Description("Sensor History")]
    public class SensorHistoryData
    {
        //todo: Downtime property not currently exposed by PRTG API

        /// <summary>
        /// The date and time to which this object's historical values apply.
        /// </summary>
        [XmlElement("datetime_raw")]
        public DateTime DateTime { get; set; }

        /// <summary>
        /// The ID of the sensor to which the historic channel data applies.
        /// </summary>
        public int SensorId { get; set; }

        /// <summary>
        /// A collection of channels and their associated values at the DateTime indicated by this object.
        /// </summary>
        [XmlElement("value")]
        public List<ChannelHistoryRecord> ChannelRecords { get; set; }

        [XmlElement("coverage_raw")]
        internal int coverage { get; set; }

        /// <summary>
        /// Indicates the percentage of time over the history averaging interval during which PRTG was successfully monitoring the sensor.
        /// </summary>
        public int Coverage => coverage/100;

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return DateTime.ToString();
        }
    }
}
