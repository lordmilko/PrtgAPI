using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies the properties of sensor channels that can be interfaced with using the PRTG API.
    /// </summary>
    public enum ChannelProperty
    {
        /// <summary>
        /// The maximum value allowed before the sensor goes into an error state.
        /// </summary>
        [Description("limitmaxerror")]
        UpperErrorLimit,

        /// <summary>
        /// The maximum value allowed before the sensor goes into a warning state.
        /// </summary>
        [Description("limitmaxwarning")]
        UpperWarningLimit,

        /// <summary>
        /// The minimum value allowed before the sensor goes into an error state.
        /// </summary>
        [Description("limitminerror")]
        LowerErrorLimit,
            
        /// <summary>
        /// The minimum value allowed before the sensor goes into a warning state.
        /// </summary>
        [Description("limitminwarning")]
        LowerWarningLimit,
        
        /// <summary>
        /// The message to display when this channel causes the sensor to go into an error state.
        /// </summary>
        [Description("limiterrormsg")]
        ErrorLimitMessage,

        /// <summary>
        /// The message to display when this channel causes the sensor to go into a warning state.
        /// </summary>
        [Description("limitwarningmsg")]
        WarningLimitMessage
    }
}
