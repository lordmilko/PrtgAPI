using System.ComponentModel;
using PrtgAPI.Attributes;
using PrtgAPI.Request.Serialization.ValueConverters;

namespace PrtgAPI
{
    internal enum ChannelPropertyInternal
    {
        [LiteralValue]
        PercentValueFactor,

        [LiteralValue]
        SpikeFilterMaxFactor,

        [LiteralValue]
        SpikeFilterMinFactor,

        [LiteralValue]
        VerticalAxisMaxFactor,

        [LiteralValue]
        VerticalAxisMinFactor,

        [LiteralValue]
        UpperErrorLimitFactor,

        [LiteralValue]
        UpperWarningLimitFactor,

        [LiteralValue]
        LowerErrorLimitFactor,

        [LiteralValue]
        LowerWarningLimitFactor
    }

    /// <summary>
    /// <para type="description">Specifies the properties of sensor channels that can be interfaced with using the PRTG API.</para>
    /// </summary>
    public enum ChannelProperty
    {
        /// <summary>
        /// The name of this channel.
        /// </summary>
        Name,

        /// <summary>
        /// The unit that is displayed next to this sensor's value. Note that only certain sensor types (such as SNMP) support modifying the channel unit.
        /// </summary>
        Unit,

        /// <summary>
        /// A standard or custom value lookup that allows this sensor's value to be displayed as text in a gauge or a switch.
        /// </summary>
        [Description("valuelookup")]
        [ValueConverter(typeof(ValueLookupConverter))]
        ValueLookup,

        /// <summary>
        /// A value that multiplies the raw value of this channel.
        /// </summary>
        ScalingMultiplication,

        /// <summary>
        /// A value that divides the raw value of this channel.
        /// </summary>
        ScalingDivision,

        /// <summary>
        /// Whether this channel should be shown in graphs.
        /// </summary>
        ShowInGraph,

        /// <summary>
        /// Whether this channel should be shown in tables.
        /// </summary>
        ShowInTable,

        /// <summary>
        /// Whether the line color of this channel in graphs should be automatically chosen or defined manually.
        /// </summary>
        ColorMode,

        /// <summary>
        /// The line color to use for this channel in graphs. Applies when <see cref="ColorMode"/> is <see cref="AutoMode.Manual"/>.
        /// </summary>
        [DependentProperty(ColorMode, AutoMode.Manual, true)]
        LineColor,

        /// <summary>
        /// The width of this channel's graph line, in pixels.
        /// </summary>
        LineWidth,

        /// <summary>
        /// Controls how values are displayed in historic data of a timespan.
        /// </summary>
        HistoricValueMode,

        /// <summary>
        /// Controls how decimal places are displayed for the value of this channel.
        /// </summary>
        DecimalMode,

        /// <summary>
        /// The number of decimal places use to display the value of this channel. Applies when <see cref="DecimalMode"/> is <see cref="PrtgAPI.DecimalMode.Custom"/>.
        /// </summary>
        [DependentProperty(DecimalMode, PrtgAPI.DecimalMode.Custom, true)]
        DecimalPlaces,

        /// <summary>
        /// Whether spike filtering is enabled for this object. If filtering is enabled, channel results outside expected ranges will be rounded to their <see cref="SpikeFilterMin"/> or <see cref="SpikeFilterMax"/> values.
        /// </summary>
        SpikeFilterEnabled,

        /// <summary>
        /// The maximum valid value of this channel. Results above this threshold will be rounded to this value.
        /// </summary>
        [DependentProperty(SpikeFilterEnabled, true)]
        SpikeFilterMax,

        /// <summary>
        /// The minimum valid value of this channel. Results below this threshold will be rounded to this value.
        /// </summary>
        [DependentProperty(SpikeFilterEnabled, true)]
        SpikeFilterMin,

        /// <summary>
        /// Specifies whether to display the actual value stored in the channel, or display the value as a percentage of a specified maximum.
        /// </summary>
        PercentMode,

        /// <summary>
        /// The maximum to use for calculating the percentage to display this channel's value as. Applies when <see cref="PercentDisplay"/> is <see cref="PrtgAPI.PercentDisplay.PercentOfMax"/>.
        /// </summary>
        [Factor(ChannelPropertyInternal.PercentValueFactor)]
        [DependentProperty(PercentMode, PercentDisplay.PercentOfMax, true)]
        PercentValue,

        /*VerticalAxisScaling,

        [Factor(ChannelPropertyInternal.VerticalAxisMaxFactor)]
        [DependentProperty(VerticalAxisScaling, AutoMode.Manual, true)]
        VerticalAxisMax,

        [Factor(ChannelPropertyInternal.VerticalAxisMinFactor)]
        [DependentProperty(VerticalAxisScaling, AutoMode.Manual, true)]
        VerticalAxisMin,*/

        /// <summary>
        /// Whether limits are enabled for this object. If limits are disabled, limit thresholds will be ignored.
        /// </summary>
        [Version(RequestVersion.v18_1)]
        LimitsEnabled,

        /// <summary>
        /// The maximum value allowed before the sensor goes into an error state.
        /// </summary>
        [DependentProperty(LimitsEnabled, true)]
        [Factor(ChannelPropertyInternal.UpperErrorLimitFactor)]
        UpperErrorLimit,

        /// <summary>
        /// The maximum value allowed before the sensor goes into a warning state.
        /// </summary>
        [DependentProperty(LimitsEnabled, true)]
        [Factor(ChannelPropertyInternal.UpperWarningLimitFactor)]
        UpperWarningLimit,

        /// <summary>
        /// The minimum value allowed before the sensor goes into an error state.
        /// </summary>
        [DependentProperty(LimitsEnabled, true)]
        [Factor(ChannelPropertyInternal.LowerErrorLimitFactor)]
        LowerErrorLimit,

        /// <summary>
        /// The minimum value allowed before the sensor goes into a warning state.
        /// </summary>
        [DependentProperty(LimitsEnabled, true)]
        [Factor(ChannelPropertyInternal.LowerWarningLimitFactor)]
        LowerWarningLimit,

        /// <summary>
        /// The message to display when this channel's <see cref="UpperErrorLimit"/> or <see cref="LowerErrorLimit"/> is exceeded.
        /// </summary>
        [Version(RequestVersion.v18_1)]
        [DependentProperty(LimitsEnabled, true)]
        ErrorLimitMessage,

        /// <summary>
        /// The message to display when this channel's <see cref="UpperWarningLimit"/> or <see cref="LowerWarningLimit"/> is exceeded.
        /// </summary>
        [Version(RequestVersion.v18_1)]
        [DependentProperty(LimitsEnabled, true)]
        WarningLimitMessage
    }
}
