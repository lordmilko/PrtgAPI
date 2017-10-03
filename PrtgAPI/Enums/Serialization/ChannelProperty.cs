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
        /// The unit that is displayed next to this sensor's value. Note that only certain sensor types (such as SNMP) support modifying the channel unit.
        /// </summary>
        Unit,

        /// <summary>
        /// A standard or custom value lookup that allows this sensor's value to be displayed as text in a gauge or a switch.
        /// </summary>
        [Description("valuelookup")]
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

        //ShowInTable,

        /// <summary>
        /// Whether the line color of this channel in graphs should be automatically chosen or defined manually.
        /// </summary>
        ColorMode,

        /// <summary>
        /// The line color to use for this channel in graphs. Applies when <see cref="ColorMode"/> is <see cref="AutoMode.Manual"/>.
        /// </summary>
        [DependentProperty(nameof(ColorMode), AutoMode.Manual, true)]
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

        /// <summary>
        /// Specifies whether to display the actual value stored in the channel, or display the value as a percentage of a specified maximum.
        /// </summary>
        PercentMode,

        /// <summary>
        /// The maximum to use for calculating the percentage to display this channel's value as. Applies when <see cref="PercentDisplay"/> is <see cref="PrtgAPI.PercentDisplay.PercentOfMax"/>.
        /// </summary>
        [DependentProperty(nameof(PercentMode), PercentDisplay.PercentOfMax)]
        PercentValue,

        /*VerticalAxisScaling,

        [DependentProperty(nameof(VerticalAxisScaling), AutoMode.Manual, true)]
        VerticalAxisMax,

        [DependentProperty(nameof(VerticalAxisScaling), AutoMode.Manual, true)]
        VerticalAxisMin,*/

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
