using System.ComponentModel;
using PrtgAPI.Attributes;

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
        [DependentProperty(nameof(LimitsEnabled))]
        UpperErrorLimit,
        
        /// <summary>
        /// The maximum value allowed before the sensor goes into a warning state.
        /// </summary>
        [Description("limitmaxwarning")]
        [DependentProperty(nameof(LimitsEnabled))]
        UpperWarningLimit,

        /// <summary>
        /// The minimum value allowed before the sensor goes into an error state.
        /// </summary>
        [Description("limitminerror")]
        [DependentProperty(nameof(LimitsEnabled))]
        LowerErrorLimit,
            
        /// <summary>
        /// The minimum value allowed before the sensor goes into a warning state.
        /// </summary>
        [Description("limitminwarning")]
        [DependentProperty(nameof(LimitsEnabled))]
        LowerWarningLimit,
        
        /// <summary>
        /// The message to display when this channel causes the sensor to go into an error state.
        /// </summary>
        [Description("limiterrormsg")]
        [DependentProperty(nameof(LimitsEnabled))]
        ErrorLimitMessage,

        /// <summary>
        /// The message to display when this channel causes the sensor to go into a warning state.
        /// </summary>
        [Description("limitwarningmsg")]
        [DependentProperty(nameof(LimitsEnabled))]
        WarningLimitMessage,
        
        /// <summary>
        /// Whether limits are enabled for this object. If limits are disabled, limit thresholds will be ignored.
        /// </summary>
        [Description("limitmode")]
        LimitsEnabled

        //todo: allow setting all other properties
    }
}
