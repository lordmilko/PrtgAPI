using System.ComponentModel;
using PrtgAPI.Attributes;

namespace PrtgAPI
{
    /// <summary>
    /// <para type="description">Specifies the properties of sensor channels that can be interfaced with using the PRTG API.</para>
    /// </summary>
    public enum ChannelProperty
    {
        /// <summary>
        /// Whether spike filtering is enabled for this object. If filtering is enabled, channel results outside expected ranges will be rounded to their <see cref="SpikeFilterMin"/>  or <see cref="SpikeFilterMax"/> values.
        /// </summary>
        [Description("spikemode")]
        SpikeFilterEnabled,

        /// <summary>
        /// The maximum valid value of this channel. Results above this threshold will be rounded to this value.
        /// </summary>
        [Description("spikemax")]
        [DependentProperty(nameof(SpikeFilterEnabled))]
        SpikeFilterMax,

        /// <summary>
        /// The minimum valid value of this channel. Results below this threshold will be rounded to this value.
        /// </summary>
        [Description("spikemin")]
        [DependentProperty(nameof(SpikeFilterEnabled))]
        SpikeFilterMin,

        /// <summary>
        /// Whether limits are enabled for this object. If limits are disabled, limit thresholds will be ignored.
        /// </summary>
        [Description("limitmode")]
        LimitsEnabled,

        /// <summary>
        /// The maximum value allowed before the sensor goes into an error state.
        /// </summary>
        [Description("limitmaxerror")]
        [RequireValue(false)]
        [DependentProperty(nameof(LimitsEnabled))]
        UpperErrorLimit,
        
        /// <summary>
        /// The maximum value allowed before the sensor goes into a warning state.
        /// </summary>
        [Description("limitmaxwarning")]
        [RequireValue(false)]
        [DependentProperty(nameof(LimitsEnabled))]
        UpperWarningLimit,

        /// <summary>
        /// The minimum value allowed before the sensor goes into an error state.
        /// </summary>
        [RequireValue(false)]
        [Description("limitminerror")]
        [DependentProperty(nameof(LimitsEnabled))]
        LowerErrorLimit,

        /// <summary>
        /// The minimum value allowed before the sensor goes into a warning state.
        /// </summary>
        [RequireValue(false)]
        [Description("limitminwarning")]
        [DependentProperty(nameof(LimitsEnabled))]
        LowerWarningLimit,

        /// <summary>
        /// The message to display when this channel causes the sensor to go into an error state.
        /// </summary>
        [RequireValue(false)]
        [Description("limiterrormsg")]
        [DependentProperty(nameof(LimitsEnabled))]
        ErrorLimitMessage,

        /// <summary>
        /// The message to display when this channel causes the sensor to go into a warning state.
        /// </summary>
        [RequireValue(false)]
        [Description("limitwarningmsg")]
        [DependentProperty(nameof(LimitsEnabled))]
        WarningLimitMessage

        //todo: allow setting all other properties
    }
}
