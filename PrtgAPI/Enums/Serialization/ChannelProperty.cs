using System.ComponentModel;
using PrtgAPI.Attributes;

namespace PrtgAPI
{
    /// <summary>
    /// <para type="description">Specifies the properties of sensor channels that can be interfaced with using the PRTG API.</para>
    /// </summary>
    public enum ChannelProperty
    {
        Unit,

        [Description("valuelookup")]
        ValueLookup,

        ScalingMultiplication,

        ScalingDivision,

        ShowInGraph,

        //ShowInTable,

        ColorMode,

        [DependentProperty(nameof(ColorMode), AutoMode.Manual, true)]
        LineColor,

        LineWidth,

        ValueMode,

        DecimalMode,

        [DependentProperty(nameof(DecimalMode), PrtgAPI.DecimalMode.Custom, true)]
        DecimalPlaces,

        /// <summary>
        /// Whether spike filtering is enabled for this object. If filtering is enabled, channel results outside expected ranges will be rounded to their <see cref="SpikeFilterMin"/>  or <see cref="SpikeFilterMax"/> values.
        /// </summary>
        SpikeFilterEnabled,

        /// <summary>
        /// The maximum valid value of this channel. Results above this threshold will be rounded to this value.
        /// </summary>
        [DependentProperty(nameof(SpikeFilterEnabled), true)]
        SpikeFilterMax,

        /// <summary>
        /// The minimum valid value of this channel. Results below this threshold will be rounded to this value.
        /// </summary>
        [DependentProperty(nameof(SpikeFilterEnabled), true)]
        SpikeFilterMin,

        PercentDisplay,

        [DependentProperty(nameof(PercentDisplay), PrtgAPI.PercentDisplay.PercentOfMax)]
        PercentValue,

        VerticalAxisScaling,

        [DependentProperty(nameof(VerticalAxisScaling), AutoMode.Manual, true)]
        VerticalAxisMax,

        [DependentProperty(nameof(VerticalAxisScaling), AutoMode.Manual, true)]
        VerticalAxisMin,

        /// <summary>
        /// Whether limits are enabled for this object. If limits are disabled, limit thresholds will be ignored.
        /// </summary>
        LimitsEnabled,

        /// <summary>
        /// The maximum value allowed before the sensor goes into an error state.
        /// </summary>
        [DependentProperty(nameof(LimitsEnabled), true)]
        UpperErrorLimit,
        
        /// <summary>
        /// The maximum value allowed before the sensor goes into a warning state.
        /// </summary>
        [DependentProperty(nameof(LimitsEnabled), true)]
        UpperWarningLimit,

        /// <summary>
        /// The minimum value allowed before the sensor goes into an error state.
        /// </summary>
        [DependentProperty(nameof(LimitsEnabled), true)]
        LowerErrorLimit,

        /// <summary>
        /// The minimum value allowed before the sensor goes into a warning state.
        /// </summary>
        [DependentProperty(nameof(LimitsEnabled), true)]
        LowerWarningLimit,

        /// <summary>
        /// The message to display when this channel causes the sensor to go into an error state.
        /// </summary>
        [DependentProperty(nameof(LimitsEnabled), true)]
        ErrorLimitMessage,

        /// <summary>
        /// The message to display when this channel causes the sensor to go into a warning state.
        /// </summary>
        [DependentProperty(nameof(LimitsEnabled), true)]
        WarningLimitMessage
    }
}
