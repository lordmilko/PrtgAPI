using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Helpers;
using PrtgAPI.Objects.Shared;
using PrtgAPI.Objects.Undocumented;
using PrtgAPI.Parameters;
using DH = PrtgAPI.Objects.Deserialization.DeserializationHelpers;

namespace PrtgAPI
{
    /// <summary>
    /// A value within a sensor that contains the results of monitoring operations.
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
        public string LastValueDisplay
        {
            get { return lastvalue; }
            set { lastvalue = value == string.Empty ? null : value.Trim(); }
        }

        /// <summary>
        /// The numeric last value of this object.
        /// </summary>
        public double LastValue => Convert.ToDouble((Convert.ToDecimal(_RawLastValue)/10).ToString("F"));

        /// <summary>
        /// Raw value used for <see cref="LastValue"/> attribute. This property should not be used.
        /// </summary>
        [Hidden]
        [XmlElement("lastvalue_raw")]
        public string _RawLastValue { get; set; }

        /// <summary>
        /// ID of the sensor this channel belongs to.
        /// </summary>
        [XmlElement("injected_sensorId")]
        public int SensorId { get; set; }

        /// <summary>
        /// Whether this channel should be shown in graphs.
        /// </summary>
        [XmlElement("injected_showchart")]
        public Visibility GraphVisibility { get; set; } //Show, Hide

        /// <summary>
        /// Whether this channel should be shown in tables.
        /// </summary>
        [XmlElement("injected_show")]
        public Visibility TableVisibility { get; set; }

        /// <summary>
        /// Whether the line color of this channel in graphs should be automatically chosen or defined manually.
        /// </summary>
        [XmlElement("injected_colmode")]
        public AutoMode ColorMode { get; set; }

        /// <summary>
        /// The line color to use for this channel in graphs. Applies when <see cref="ColorMode"/> is <see cref="AutoMode.Manual"/>.
        /// </summary>
        [XmlElement("injected_color")]
        public string LineColor { get; } //Automatic or a value you specify in hex - todo
        //todo - need to validate input against doing a System.Drawing.ColorTranslator.FromHtml

        /// <summary>
        /// Specifies whether to display the actual value stored in the channel, or display the value as a percentage of a specified maximum.
        /// </summary>
        [XmlElement("injected_percent")]
        public PercentDisplay PercentDisplay { get; set; }

        /// <summary>
        /// The maximum to use for calculating the percentage to display this channel's value as. Applies when <see cref="PercentDisplay"/> is <see cref="PrtgAPI.PercentDisplay.PercentOfMax"/>.
        /// </summary>
        public double? PercentValue => DH.StrToNullableDouble(_RawPercentValue);

        /// <summary>
        /// Raw value used for <see cref="PercentValue"/> attribute. This property should not be used.
        /// </summary>
        [Hidden]
        [XmlElement("injected_ref100percent")]
        public string _RawPercentValue { get; set; }

        /// <summary>
        /// The width of this channel's graph line, in pixels.
        /// </summary>
        [XmlElement("injected_linewidth")]
        public int LineWidth { get; set; }

        /// <summary>
        /// Controls how values are displayed in historic data of a timespan.
        /// </summary>
        [XmlElement("injected_avgmode")]
        public HistoricValueMode ValueMode { get; set; }

        /// <summary>
        /// Controls how decimal places are displayed for the value of this channel.
        /// </summary>
        [XmlElement("injected_decimalmode")]
        public DecimalMode DecimalMode { get; set; }

        /// <summary>
        /// The number of decimal places use to display the value of this channel. Applies when <see cref="DecimalMode"/> is <see cref="PrtgAPI.DecimalMode.Custom"/>.
        /// </summary>
        [XmlElement("injected_decimaldigits")]
        public int DecimalPlaces { get; set; } //Automatic, All or a value you specify - todo

        /// <summary>
        /// Whether values that are too high or too low should be filtered out of graphs and tables.
        /// </summary>
        [XmlElement("injected_spikemode")]
        public bool SpikeFilterEnabled { get; set; }

        /// <summary>
        /// The upper limit for spike filtering. Values above this value will be ignored.
        /// </summary>
        public int? SpikeFilterMax => DH.StrToNullableInt(_RawSpikeFilterMax);

        /// <summary>
        /// Raw value used for <see cref="SpikeFilterMax"/> attribute. This property should not be used.
        /// </summary>
        [Hidden]
        [XmlElement("injected_spikemax")]
        public string _RawSpikeFilterMax { get; set; }

        /// <summary>
        /// The lower limit for spike filtering. Values below this value will be ignored.
        /// </summary>
        public int? SpikeFilterMin => DH.StrToNullableInt(_RawSpikeFilterMin);

        /// <summary>
        /// Raw value used for <see cref="SpikeFilterMin"/> attribute. This property should not be used.
        /// </summary>
        [Hidden]
        [XmlElement("injected_spikemin")]
        public string _RawSpikeFilterMin { get; set; }

        [XmlElement("injected_axismode")]
        public AutoMode VerticalAxisScaling { get; set; } //Automatic or Manual

        public int? VerticalAxisMax => DH.StrToNullableInt(_RawVerticalAxisMax);

        /// <summary>
        /// Raw value used for <see cref="VerticalAxisMax"/> attribute. This property should not be used.
        /// </summary>
        [Hidden]
        [XmlElement("injected_axismax")]
        public string _RawVerticalAxisMax { get; set; }

        public int? VerticalAxisMin => DH.StrToNullableInt(_RawVerticalAxisMin);

        /// <summary>
        /// Raw value used for <see cref="VerticalAxisMin"/> attribute. This property should not be used.
        /// </summary>
        [Hidden]
        [XmlElement("injected_axismin")]
        public string _RawVerticalAxisMin { get; set; }

        /// <summary>
        /// Whether warning or error limits are enabled for this channel. When this channel's value crosses these limits, the channel's sensor will transition into an error or warning state.
        /// </summary>
        [XmlElement("injected_limitmode")]
        public bool LimitsEnabled { get; set; }

        //todo - confirm every value in here thats an int cant be doubles in prtg

        /// <summary>
        /// The upper error for this channel. If the <see cref="LastValue"/> of this channel goes above this limit, the channel will begin transitioning into an error state.
        /// </summary>
        public double? UpperErrorLimit => DH.StrToNullableDouble(_RawUpperErrorLimit);

        /// <summary>
        /// Raw value used for <see cref="UpperErrorLimit"/> attribute. This property should not be used.
        /// </summary>
        [Hidden]
        [XmlElement("injected_limitmaxerror")]
        public string _RawUpperErrorLimit { get; set; }

        /// <summary>
        /// The upper warning limit of this channel. If the <see cref="LastValue"/> of this channel goes above this limit, the channel will immediately transition into a warning state.
        /// </summary>
        public double? UpperWarningLimit => DH.StrToNullableDouble(_RawUpperWarningLimit);

        /// <summary>
        /// Raw value used for <see cref="UpperWarningLimit"/> attribute. This property should not be used.
        /// </summary>
        [Hidden]
        [XmlElement("injected_limitmaxwarning")]
        public string _RawUpperWarningLimit { get; set; }

        /// <summary>
        /// The lower error for this channel. If the <see cref="LastValue"/> of this channel goes below this limit, the channel will begin transitioning into an error state.
        /// </summary>
        [XmlElement("injected_limitminerror")]
        public double? LowerErrorLimit => DH.StrToNullableDouble(_RawLowerErrorLimit);

        /// <summary>
        /// Raw value used for <see cref="LowerErrorLimit"/> attribute. This property should not be used.
        /// </summary>
        [Hidden]
        [XmlElement("injected_limitminerror")]
        public string _RawLowerErrorLimit { get; set; }

        /// <summary>
        /// The lower warning limit of this channel. If the <see cref="LastValue"/> of this channel goes below this limit, the channel will immediately transition into a warning state.
        /// </summary>
        public double? LowerWarningLimit => DH.StrToNullableDouble(_RawLowerWarningLimit);

        /// <summary>
        /// Raw value used for <see cref="LowerWarningLimit"/> attribute. This property should not be used.
        /// </summary>
        [Hidden]
        [XmlElement("injected_limitminwarning")]
        public string _RawLowerWarningLimit { get; set; }

        [XmlElement("injected_limiterrormsg")]
        public string ErrorLimitMessage { get; set; }

        [XmlElement("injected_limitwarningmsg")]
        public string WarningLimitMessage { get; set; }

        internal static CustomParameter CreateCustomParameter(ChannelProperty property, int channelId, object value)
        {
            return new CustomParameter($"{property.GetDescription()}_{channelId}", value.ToString());
        }
    }
}
