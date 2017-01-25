using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies the sensor channels a notification trigger can monitor.
    /// </summary>
    public enum TriggerChannel
    {
        /// <summary>
        /// The primary channel.
        /// </summary>
        [XmlEnum("-999")]
        Primary,

        /// <summary>
        /// The "Total" channel (where applicable)
        /// </summary>
        [XmlEnum("-1")]
        Total,

        /// <summary>
        /// The "TrafficIn" channel (where applicable)
        /// </summary>
        [XmlEnum("0")]
        [XmlEnumAlternateName("Traffic In")]
        TrafficIn,

        /// <summary>
        /// The "TrafficOut" channel (where applicable)
        /// </summary>
        [XmlEnum("1")]
        [XmlEnumAlternateName("Traffic Out")]
        TrafficOut
    }
}
