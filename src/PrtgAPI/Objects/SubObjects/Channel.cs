using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI
{
    /// <summary>
    /// <para type="description">A value within a sensor that contains the results of monitoring operations.</para>
    /// </summary>
    public class Channel : ISubObject
    {
        /// <summary>
        /// Unique identifier of this channel within its parent sensor.
        /// </summary>
        [XmlElement("objid")]
        [PropertyParameter(Property.Id)]
        public int Id { get; set; }

        [ExcludeFromCodeCoverage]
        int ISubObject.ObjectId => SensorId;

        [ExcludeFromCodeCoverage]
        int ISubObject.SubId => Id;

        /// <summary>
        /// Name of this channel.
        /// </summary>
        [XmlElement("name")]
        [PropertyParameter(Property.Name)]
        [PropertyParameter(ChannelProperty.Name)]
        public string Name { get; set; }

        // ################################## Sensors, Channel ##################################
        // There is a copy in both Sensor and Channel

        private string displayLastvalue;

        /// <summary>
        /// Last display value of this channel. If this channel's sensor is currently paused, this may display "No data"
        /// </summary>
        [XmlElement("lastvalue")]
        public string DisplayLastValue
        {
            get { return displayLastvalue; }
            set { displayLastvalue = string.IsNullOrEmpty(value) ? null : value.Trim(); }
        }

        /// <summary>
        /// The raw last value of this channel.<para/>
        /// This value is represented in the smallest unit the <see cref="Unit"/> can be divided into (seconds, bits, bytes, etc)
        /// </summary>
        [XmlElement("lastvalue_raw")]
        [PropertyParameter(Property.LastValue)]
        public double? LastValue { get; set; }

        /// <summary>
        /// A value that multiplies the raw value of this channel.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_factorm")]
        [PropertyParameter(ChannelProperty.ScalingMultiplication)]
        public double? ScalingMultiplication { get; set; }

        /// <summary>
        /// A value that divides the raw value of this channel.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_factord")]
        [PropertyParameter(ChannelProperty.ScalingDivision)]
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
        [PropertyParameter(ChannelProperty.ShowInGraph)]
        public bool? ShowInGraph { get; set; } //Show, Hide

        /// <summary>
        /// Whether this channel should be shown in tables and API responses. Note: PrtgAPI will always force hidden channels to be returned even if they are marked as hidden.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_show")]
        [PropertyParameter(ChannelProperty.ShowInTable)]
        public bool? ShowInTable { get; set; }

        /// <summary>
        /// Whether the line color of this channel in graphs should be automatically chosen or defined manually.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_colmode")]
        [PropertyParameter(ChannelProperty.ColorMode)]
        public AutoMode? ColorMode { get; set; }

        /// <summary>
        /// The line color to use for this channel in graphs. Applies when <see cref="ColorMode"/> is <see cref="AutoMode.Manual"/>.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_color")]
        [PropertyParameter(ChannelProperty.LineColor)]
        public string LineColor { get; set; } //Automatic or a value you specify in hex - todo
        //todo - need to validate input against doing a System.Drawing.ColorTranslator.FromHtml

        /// <summary>
        /// Specifies whether to display the actual value stored in the channel, or display the value as a percentage of a specified maximum.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_percent")]
        [PropertyParameter(ChannelProperty.PercentMode)]
        public PercentDisplay? PercentMode { get; set; }

        /// <summary>
        /// The maximum to use for calculating the percentage to display this channel's value as. Applies when <see cref="PercentMode"/> is <see cref="PrtgAPI.PercentDisplay.PercentOfMax"/>.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_ref100percent")]
        [PropertyParameter(ChannelProperty.PercentValue)]
        public double? PercentValue { get; set; }

        [Undocumented]
        [XmlElement("injected_ref100percent_factor")]
        [PropertyParameter(ChannelPropertyInternal.PercentValueFactor)]
        internal double? PercentFactor { get; set; }
        
        /// <summary>
        /// Value multiplication factor for this object's <see cref="Unit"/>. Used when modifying certain properties.<para/>
        /// For internal use only.
        /// </summary>
        public double? Factor => PercentFactor; //Every internal factor should be the same

        /// <summary>
        /// The width of this channel's graph line, in pixels.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_linewidth")]
        [PropertyParameter(ChannelProperty.LineWidth)]
        public int? LineWidth { get; set; }

        private string unit;

        /// <summary>
        /// The unit this channel's value is measured in.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_customunit")]
        [PropertyParameter(ChannelProperty.Unit)]
        public string Unit
        {
            get { return unit; }
            set
            {
                //Property will be set to null by deserialization engine
                if (value != null)
                    unit = value;
                else
                {
                    if (LastValue == null)
                        unit = null;
                    else
                    {
                        unit = DisplayLastValue?.Substring(DisplayLastValue.LastIndexOf(' ') + 1);
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
        [PropertyParameter(ChannelProperty.ValueLookup)]
        public string ValueLookup => valueLookup?.Substring(valueLookup.IndexOf("|", StringComparison.Ordinal) + 1);

        /// <summary>
        /// Controls how values are displayed in historic data of a timespan.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_avgmode")]
        [PropertyParameter(ChannelProperty.HistoricValueMode)]
        public HistoricValueMode? HistoricValueMode { get; set; }

        /// <summary>
        /// Controls how decimal places are displayed for the value of this channel.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_decimalmode")]
        [PropertyParameter(ChannelProperty.DecimalMode)]
        public DecimalMode? DecimalMode { get; set; }

        /// <summary>
        /// The number of decimal places use to display the value of this channel. Applies when <see cref="DecimalMode"/> is <see cref="PrtgAPI.DecimalMode.Custom"/>.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_decimaldigits")]
        [PropertyParameter(ChannelProperty.DecimalPlaces)]
        public double? DecimalPlaces { get; set; } //Automatic, All or a value you specify - todo

        /// <summary>
        /// Whether values that are too high or too low should be filtered out of graphs and tables.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_spikemode")]
        [PropertyParameter(ChannelProperty.SpikeFilterEnabled)]
        public bool? SpikeFilterEnabled { get; set; }

        /// <summary>
        /// The upper limit for spike filtering. Values above this value will be ignored.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_spikemax")]
        [PropertyParameter(ChannelProperty.SpikeFilterMax)]
        public double? SpikeFilterMax { get; set; }

        [Undocumented]
        [XmlElement("injected_spikemax_factor")]
        [PropertyParameter(ChannelPropertyInternal.SpikeFilterMaxFactor)]
        internal double? SpikeFilterMaxFactor { get; set; }

        /// <summary>
        /// The lower limit for spike filtering. Values below this value will be ignored.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_spikemin")]
        [PropertyParameter(ChannelProperty.SpikeFilterMin)]
        public double? SpikeFilterMin { get; set; }

        [Undocumented]
        [XmlElement("injected_spikemin_factor")]
        [PropertyParameter(ChannelPropertyInternal.SpikeFilterMinFactor)]
        internal double? SpikeFilterMinFactor { get; set; }

        /// <summary>
        /// Indicates how to scale this channel in graphs.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_axismode")]
        //[PropertyParameter(ChannelProperty.VerticalAxisScaling)]
        public AutoMode? VerticalAxisScaling { get; set; } //Automatic or Manual

        /// <summary>
        /// Maximum value to display on the graphs vertical axis. If <see cref="VerticalAxisScaling"/> is <see cref="AutoMode.Automatic"/> this value has no effect.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_axismax")]
        //[PropertyParameter(ChannelProperty.VerticalAxisMax)]
        public double? VerticalAxisMax { get; set; }

        [Undocumented]
        [XmlElement("injected_axismax_factor")]
        [PropertyParameter(ChannelPropertyInternal.VerticalAxisMaxFactor)]
        internal double? VerticalAxisMaxFactor { get; set; }

        /// <summary>
        /// Minimum value to display on the graphs vertical axis. If <see cref="VerticalAxisScaling"/> is <see cref="AutoMode.Automatic"/> this value has no effect.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_axismin")]
        //[PropertyParameter(ChannelProperty.VerticalAxisMin)]
        public double? VerticalAxisMin { get; set; }

        [Undocumented]
        [XmlElement("injected_axismin_factor")]
        [PropertyParameter(ChannelPropertyInternal.VerticalAxisMinFactor)]
        internal double? VerticalAxisMinFactor { get; set; }

        /// <summary>
        /// Whether warning or error limits are enabled for this channel. When this channel's value crosses these limits, the channel's sensor will transition into an error or warning state.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_limitmode")]
        [PropertyParameter(ChannelProperty.LimitsEnabled)]
        public bool? LimitsEnabled { get; set; }

        /// <summary>
        /// The upper error for this channel. If the <see cref="LastValue"/> of this channel goes above this limit, the channel will begin transitioning into an error state.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_limitmaxerror")]
        [PropertyParameter(ChannelProperty.UpperErrorLimit)]
        public double? UpperErrorLimit { get; set; }

        [Undocumented]
        [XmlElement("injected_limitmaxerror_factor")]
        [PropertyParameter(ChannelPropertyInternal.UpperErrorLimitFactor)]
        internal double? UpperErrorLimitFactor { get; set; }

        /// <summary>
        /// The upper warning limit of this channel. If the <see cref="LastValue"/> of this channel goes above this limit, the channel will immediately transition into a warning state.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_limitmaxwarning")]
        [PropertyParameter(ChannelProperty.UpperWarningLimit)]
        public double? UpperWarningLimit { get; set; }

        [Undocumented]
        [XmlElement("injected_limitmaxwarning_factor")]
        [PropertyParameter(ChannelPropertyInternal.UpperWarningLimitFactor)]
        internal double? UpperWarningLimitFactor { get; set; }

        /// <summary>
        /// The lower error for this channel. If the <see cref="LastValue"/> of this channel goes below this limit, the channel will begin transitioning into an error state.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_limitminerror")]
        [PropertyParameter(ChannelProperty.LowerErrorLimit)]
        public double? LowerErrorLimit { get; set; }

        [Undocumented]
        [XmlElement("injected_limitminerror_factor")]
        [PropertyParameter(ChannelPropertyInternal.LowerErrorLimitFactor)]
        internal double? LowerErrorLimitFactor { get; set; }

        /// <summary>
        /// The lower warning limit of this channel. If the <see cref="LastValue"/> of this channel goes below this limit, the channel will immediately transition into a warning state.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_limitminwarning")]
        [PropertyParameter(ChannelProperty.LowerWarningLimit)]
        public double? LowerWarningLimit { get; set; }

        [Undocumented]
        [XmlElement("injected_limitminwarning_factor")]
        [PropertyParameter(ChannelPropertyInternal.LowerWarningLimitFactor)]
        internal double? LowerWarningLimitFactor { get; set; }

        /// <summary>
        /// The message to display when the <see cref="LastValue"/> of this channel goes above or below the <see cref="UpperErrorLimit"/> or <see cref="LowerErrorLimit"/> respectively.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_limiterrormsg")]
        [PropertyParameter(ChannelProperty.ErrorLimitMessage)]
        public string ErrorLimitMessage { get; set; }

        /// <summary>
        /// The message to display when the <see cref="LastValue"/> of this channel goes above or below the <see cref="UpperWarningLimit"/> or <see cref="LowerWarningLimit"/> respectively.
        /// </summary>
        [Undocumented]
        [XmlElement("injected_limitwarningmsg")]
        [PropertyParameter(ChannelProperty.WarningLimitMessage)]
        public string WarningLimitMessage { get; set; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
