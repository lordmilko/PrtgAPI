using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        Primary,

        /// <summary>
        /// The "Total" channel (where applicable)
        /// </summary>
        Total,

        /// <summary>
        /// The "TrafficIn" channel (where applicable)
        /// </summary>
        TrafficIn,

        /// <summary>
        /// The "TrafficOut" channel (where applicable)
        /// </summary>
        TrafficOut
    }
}
