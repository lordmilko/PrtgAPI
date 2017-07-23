using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies whether auto-discovery should be periodically run according to a specified schedule.
    /// </summary>
    public enum DiscoverySchedule
    {
        /// <summary>
        /// Only run auto-discovery once. This is the default.
        /// </summary>
        [XmlEnum("0")]
        Once,

        /// <summary>
        /// Periodically perform an auto-discovery once an hour.
        /// </summary>
        [XmlEnum("1")]
        Hourly,

        /// <summary>
        /// Periodically perform an auto-discovery once a day.
        /// </summary>
        [XmlEnum("2")]
        Daily,

        /// <summary>
        /// Periodically perform an auto-discovery once a week.
        /// </summary>
        [XmlEnum("3")]
        Weekly
    }
}
