using System;
using System.Xml.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Objects.Shared;

namespace PrtgAPI
{
    /// <summary>
    /// <para type="description">A value within a sensor that contains the results of monitoring operations.</para>
    /// </summary>
    public class Channel : PrtgObject
    {
        // ################################## Sensors, Channel ##################################
        // There is a copy in both Sensor and Channel

        private string lastvalue;

        /// <summary>
        /// Last value of this sensor's primary channel. If this sensor's primary channel has been recently changed, the sensor may need to be paused and unpause (otherwise it may just display "No Data").
        /// </summary>
        [XmlElement("lastvalue")]
        [PropertyParameter(nameof(Property.LastValue))]
        public string LastValue
        {
            get { return lastvalue; }
            set { lastvalue = string.IsNullOrEmpty(value) ? null : value.Trim(); }
        }

        /// <summary>
        /// The numeric last value of this object.
        /// </summary>
        public double LastValueNumeric => Convert.ToDouble((Convert.ToDecimal(lastValueNumeric) /10).ToString("F"));

        [XmlElement("lastvalue_raw")]
        internal string lastValueNumeric { get; set; }

        /// <summary>
        /// A value that multiplies the raw value of this channel.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_factorm")]
        [PropertyParameter(nameof(ChannelProperty.ScalingMultiplication))]
        public double? ScalingMultiplication { get; set; }

        /// <summary>
        /// A value that divides the raw value of this channel.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_factord")]
        [PropertyParameter(nameof(ChannelProperty.ScalingDivision))]
        public double? ScalingDivision { get; set; }

        /// <summary>
        /// ID of the sensor this channel belongs to.
        /// </summary>
        [XmlElement("injected_sensorId")]
        public int SensorId { get; set; }

        /// <summary>
        /// Whether this channel should be shown in graphs.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_showchart")]
        [PropertyParameter(nameof(ChannelProperty.ShowInGraph))]
        public bool ShowInGraph { get; set; } //Show, Hide

        /// <summary>
        /// Whether this channel should be shown in tables and API responses. If this value is set to false you will be unable to restore visibility without manual intervention or an existing reference to this object.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_show")]
        public bool ShowInTable { get; set; }

        /// <summary>
        /// Whether the line color of this channel in graphs should be automatically chosen or defined manually.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_colmode")]
        [PropertyParameter(nameof(ChannelProperty.ColorMode))]
        public AutoMode ColorMode { get; set; }

        /// <summary>
        /// The line color to use for this channel in graphs. Applies when <see cref="ColorMode"/> is <see cref="AutoMode.Manual"/>.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_color")]
        [PropertyParameter(nameof(ChannelProperty.LineColor))]
        public string LineColor { get; set; } //Automatic or a value you specify in hex - todo
        //todo - need to validate input against doing a System.Drawing.ColorTranslator.FromHtml

        /// <summary>
        /// Specifies whether to display the actual value stored in the channel, or display the value as a percentage of a specified maximum.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_percent")]
        [PropertyParameter(nameof(ChannelProperty.PercentMode))]
        public PercentDisplay? PercentMode { get; set; }

        /// <summary>
        /// The maximum to use for calculating the percentage to display this channel's value as. Applies when <see cref="PercentMode"/> is <see cref="PrtgAPI.PercentDisplay.PercentOfMax"/>.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_ref100percent")]
        [PropertyParameter(nameof(ChannelProperty.PercentValue))]
        public double? PercentValue { get; set; }

        /// <summary>
        /// The width of this channel's graph line, in pixels.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_linewidth")]
        [PropertyParameter(nameof(ChannelProperty.LineWidth))]
        public int LineWidth { get; set; }

        private string unit;

        /// <summary>
        /// The unit this channel's value is measured in.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_customunit")]
        [PropertyParameter(nameof(ChannelProperty.Unit))]
        public string Unit
        {
            get { return unit; }
            set
            {
                if (value != null)
                    unit = value;
                else
                {
                    if (LastValue == null || LastValue == "No data")
                        unit = null;
                    else
                    {
                        unit = LastValue?.Substring(LastValue.LastIndexOf(' ') + 1);
                    }
                }
            }
        }

        [XmlElement("injected_valuelookup")]
        internal string valueLookup { get; set; }

        /// <summary>
        /// A standard or custom value lookup that allows this sensor's value to be displayed as text in a gauge or a switch.
        /// </summary>
        [Undocumented]
        [PropertyParameter(nameof(ChannelProperty.ValueLookup))]
        public string ValueLookup => valueLookup?.Substring(valueLookup.IndexOf("|") + 1);

        /// <summary>
        /// Controls how values are displayed in historic data of a timespan.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_avgmode")]
        [PropertyParameter(nameof(ChannelProperty.HistoricValueMode))]
        public HistoricValueMode? HistoricValueMode { get; set; }

        /// <summary>
        /// Controls how decimal places are displayed for the value of this channel.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_decimalmode")]
        [PropertyParameter(nameof(ChannelProperty.DecimalMode))]
        public DecimalMode DecimalMode { get; set; }

        /// <summary>
        /// The number of decimal places use to display the value of this channel. Applies when <see cref="DecimalMode"/> is <see cref="PrtgAPI.DecimalMode.Custom"/>.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_decimaldigits")]
        [PropertyParameter(nameof(ChannelProperty.DecimalPlaces))]
        public double? DecimalPlaces { get; set; } //Automatic, All or a value you specify - todo

        /// <summary>
        /// Whether values that are too high or too low should be filtered out of graphs and tables.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_spikemode")]
        [PropertyParameter(nameof(ChannelProperty.SpikeFilterEnabled))]
        public bool? SpikeFilterEnabled { get; set; }

        /// <summary>
        /// The upper limit for spike filtering. Values above this value will be ignored.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_spikemax")]
        [PropertyParameter(nameof(ChannelProperty.SpikeFilterMax))]
        public double? SpikeFilterMax { get; set; }

        /// <summary>
        /// The lower limit for spike filtering. Values below this value will be ignored.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_spikemin")]
        [PropertyParameter(nameof(ChannelProperty.SpikeFilterMin))]
        public double? SpikeFilterMin { get; set; }

        /// <summary>
        /// Indicates how to scale this channel in graphs.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_axismode")]
        //[PropertyParameter(nameof(ChannelProperty.VerticalAxisScaling))]
        public AutoMode VerticalAxisScaling { get; set; } //Automatic or Manual

        /// <summary>
        /// Maximum value to display on the graphs vertical axis. If <see cref="VerticalAxisScaling"/> is <see cref="AutoMode.Automatic"/> this value has no effect.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_axismax")]
        //[PropertyParameter(nameof(ChannelProperty.VerticalAxisMax))]
        public double? VerticalAxisMax { get; set; }

        /// <summary>
        /// Minimum value to display on the graphs vertical axis. If <see cref="VerticalAxisScaling"/> is <see cref="AutoMode.Automatic"/> this value has no effect.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_axismin")]
        //[PropertyParameter(nameof(ChannelProperty.VerticalAxisMin))]
        public double? VerticalAxisMin { get; set; }

        /// <summary>
        /// Whether warning or error limits are enabled for this channel. When this channel's value crosses these limits, the channel's sensor will transition into an error or warning state.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_limitmode")]
        [PropertyParameter(nameof(ChannelProperty.LimitsEnabled))]
        public bool? LimitsEnabled { get; set; }

        /// <summary>
        /// The upper error for this channel. If the <see cref="LastValue"/> of this channel goes above this limit, the channel will begin transitioning into an error state.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_limitmaxerror")]
        [PropertyParameter(nameof(ChannelProperty.UpperErrorLimit))]
        public double? UpperErrorLimit { get; set; }

        /// <summary>
        /// The upper warning limit of this channel. If the <see cref="LastValue"/> of this channel goes above this limit, the channel will immediately transition into a warning state.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_limitmaxwarning")]
        [PropertyParameter(nameof(ChannelProperty.UpperWarningLimit))]
        public double? UpperWarningLimit { get; set; }

        /// <summary>
        /// The lower error for this channel. If the <see cref="LastValue"/> of this channel goes below this limit, the channel will begin transitioning into an error state.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_limitminerror")]
        [PropertyParameter(nameof(ChannelProperty.LowerErrorLimit))]
        public double? LowerErrorLimit { get; set; }

        /// <summary>
        /// The lower warning limit of this channel. If the <see cref="LastValue"/> of this channel goes below this limit, the channel will immediately transition into a warning state.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_limitminwarning")]
        [PropertyParameter(nameof(ChannelProperty.LowerWarningLimit))]
        public double? LowerWarningLimit { get; set; }

        /// <summary>
        /// The message to display when the <see cref="LastValue"/> of this channel goes above or below the <see cref="UpperErrorLimit"/> or <see cref="LowerErrorLimit"/> respectively.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_limiterrormsg")]
        [PropertyParameter(nameof(ChannelProperty.ErrorLimitMessage))]
        public string ErrorLimitMessage { get; set; }

        /// <summary>
        /// The message to display when the <see cref="LastValue"/> of this channel goes above or below the <see cref="UpperWarningLimit"/> or <see cref="LowerWarningLimit"/> respectively.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_limitwarningmsg")]
        [PropertyParameter(nameof(ChannelProperty.WarningLimitMessage))]
        public string WarningLimitMessage { get; set; }
    }
}
