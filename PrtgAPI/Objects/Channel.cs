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
        public string LastValue
        {
            get { return lastvalue; }
            set { lastvalue = string.IsNullOrEmpty(value) ? null : value.Trim(); }
        }

        /// <summary>
        /// The numeric last value of this object.
        /// </summary>
        public double LastValueNumeric => Convert.ToDouble((Convert.ToDecimal(lastValueNumeric) /10).ToString("F"));

        /// <summary>
        /// The raw numeric last value of this object. This field is for internal use only.
        /// </summary>
        [XmlElement("lastvalue_raw")]
        protected string lastValueNumeric { get; set; }

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
        /// Whether this channel should be shown in tables and API responses. If this value is set to <see cref="Visibility.NotVisible"/> you will be unable to restore visibility without manual intervention or an existing reference to this object.
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
        public string LineColor { get; set; } //Automatic or a value you specify in hex - todo
        //todo - need to validate input against doing a System.Drawing.ColorTranslator.FromHtml

        /// <summary>
        /// Specifies whether to display the actual value stored in the channel, or display the value as a percentage of a specified maximum.
        /// </summary>
        [XmlElement("injected_percent")]
        public PercentDisplay? PercentDisplay { get; set; }

        /// <summary>
        /// The maximum to use for calculating the percentage to display this channel's value as. Applies when <see cref="PercentDisplay"/> is <see cref="PrtgAPI.PercentDisplay.PercentOfMax"/>.
        /// </summary>
        [XmlElement("injected_ref100percent")]
        public double? PercentValue { get; set; }

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
        public bool? SpikeFilterEnabled { get; set; }

        /// <summary>
        /// The upper limit for spike filtering. Values above this value will be ignored.
        /// </summary>
        [XmlElement("injected_spikemax")]
        public int? SpikeFilterMax { get; set; }

        /// <summary>
        /// The lower limit for spike filtering. Values below this value will be ignored.
        /// </summary>
        [XmlElement("injected_spikemin")]
        public int? SpikeFilterMin { get; set; }

        [XmlElement("injected_axismode")]
        public AutoMode VerticalAxisScaling { get; set; } //Automatic or Manual

        [XmlElement("injected_axismax")]
        public int? VerticalAxisMax { get; set; }

        [XmlElement("injected_axismin")]
        public int? VerticalAxisMin { get; set; }

        /// <summary>
        /// Whether warning or error limits are enabled for this channel. When this channel's value crosses these limits, the channel's sensor will transition into an error or warning state.
        /// </summary>
        [XmlElement("injected_limitmode")]
        public bool? LimitsEnabled { get; set; }

        //todo - confirm every value in here thats an int cant be doubles in prtg

        /// <summary>
        /// The upper error for this channel. If the <see cref="LastValue"/> of this channel goes above this limit, the channel will begin transitioning into an error state.
        /// </summary>
        [XmlElement("injected_limitmaxerror")]
        public double? UpperErrorLimit { get; set; }

        /// <summary>
        /// The upper warning limit of this channel. If the <see cref="LastValue"/> of this channel goes above this limit, the channel will immediately transition into a warning state.
        /// </summary>
        [XmlElement("injected_limitmaxwarning")]
        public double? UpperWarningLimit { get; set; }

        /// <summary>
        /// The lower error for this channel. If the <see cref="LastValue"/> of this channel goes below this limit, the channel will begin transitioning into an error state.
        /// </summary>
        [XmlElement("injected_limitminerror")]
        public double? LowerErrorLimit { get; set; }

        /// <summary>
        /// The lower warning limit of this channel. If the <see cref="LastValue"/> of this channel goes below this limit, the channel will immediately transition into a warning state.
        /// </summary>
        [XmlElement("injected_limitminwarning")]
        public double? LowerWarningLimit { get; set; }

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
