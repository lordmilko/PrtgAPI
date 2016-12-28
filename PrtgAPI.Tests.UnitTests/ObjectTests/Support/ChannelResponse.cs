using System.Text;
using System.Xml.Linq;
using PrtgAPI.Helpers;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.Support
{
    public class ChannelResponse : BaseResponse<ChannelItem>
    {
        string GetSettingsResponseText()
        {
            var builder = new StringBuilder();

            builder.Append("\n");
            builder.Append("<div>\n");
            builder.Append("  <div class=\"pseudoform prtg-form prtg-plugin\" data-plugin=\"channels\" data-template=\"channels.htm\" data-singleedit=\"true\" data-channelid=\"0\" data-objectid=\"2196\" data-container=\".channelsloadingform\">\n");
            builder.Append("    <h1 style=\"display: none\">\n");
            builder.Append("      Edit Channel\n");
            builder.Append("    </h1>\n");
            builder.Append("    <select name=\"channel\" id=\"channelsdropdown\" style=\"width: 100%\">\n");
            builder.Append("      <option value=\"-4\" data-channel-kind=\"5\">Downtime (ID -4)</option><option value=\"0\" data-channel-kind=\"5\">Percent Available Memory (ID 0)</option><option value=\"1\" data-channel-kind=\"2\">Available Memory (ID 1)</option>\n");
            builder.Append("    </select>\n");
            builder.Append("    <div class=\"channelsloadingform\">\n");
            builder.Append("    \n");
            builder.Append("<form id=\"channelsform\" action=\"/editsettings\" method=\"post\" class=\"prtg-form prtg-plugin\" data-ajaxsubmit=\"true\" data-redirect=\"false\" data-plugin=\"multiedit\">\n");
            builder.Append("	<fieldset>\n");
            builder.Append("		<legend class=\"prtg-header inline-hidden\">Edit Channel &quot;Percent Available Memory&quot;</legend>\n");
            builder.Append("		<div class=\"control-group\">\n");
            builder.Append("			<div class=\"checkboxbuttonset\"><label class=\"control-label has_help \" for=\"name_1\">Name</label>\n");
            builder.Append("<div class=\"controls \" data-placement=\"right\" data-helptext=\"Enter a meaningful name for the channel to identify it in data graphs and tables.\">\n");
            builder.Append("<input class=\"text\"  data-rule-required=\"true\" type=\"text\" name=\"name_1\" id=\"name_1\" autocomplete=\"off\" value=\"Percent Available Memory\" ></div>\n");
            builder.Append("\n");
            builder.Append("<label class=\"control-label has_help \" for=\"id_1\">ID</label>\n");
            builder.Append("<div class=\"controls \" data-placement=\"right\" data-helptext=\"Shows the ID of this channel. You cannot edit this ID. It is shown for your information only.\">\n");
            builder.Append("<div class=\"readonlyproperty\" >0</div></div>\n");
            builder.Append("\n");
            builder.Append("<label class=\"control-label has_help \" for=\"showchart_1\">Graph Rendering</label>\n");
            builder.Append("<div class=\"controls \" data-placement=\"right\" data-helptext=\"Define if you want to show this channel in data graphs.\">\n");
            builder.Append("<div class=\"radio-control\">\n");
            builder.Append("<input type=\"radio\" class=\"hidden radio \" name=\"showchart_1\" value=\"1\" checked  id=\"showchart1\" >\n");
            builder.Append("\n");
            builder.Append("<label for=\"showchart1\" class=\"radio-control-label\">\n");
            builder.Append("<i class=\"icon-gray icon-radio-on\"></i>\n");
            builder.Append("Show in Graphs\n");
            builder.Append("</label>\n");
            builder.Append("<input type=\"radio\" class=\"hidden radio \" name=\"showchart_1\" value=\"0\" id=\"showchart0\" >\n");
            builder.Append("\n");
            builder.Append("<label for=\"showchart0\" class=\"radio-control-label\">\n");
            builder.Append("<i class=\"icon-gray icon-radio-on\"></i>\n");
            builder.Append("Hide from Graphs\n");
            builder.Append("</label>\n");
            builder.Append("</div>\n");
            builder.Append("</div>\n");
            builder.Append("\n");
            builder.Append("<label class=\"control-label has_help \" for=\"show_1\">Table Rendering</label>\n");
            builder.Append("<div class=\"controls \" data-placement=\"right\" data-helptext=\"Define if you want to show this channel in data tables. <br/><br/><b>Important:</b> Only if you show this channel in data tables, PRTG will use it for the calculation of the sum (total) channel of this sensor! Channels that are shown in graphs only and not in tables will not contribute to the sum (total) of this sensor.\">\n");
            builder.Append("<div class=\"radio-control\">\n");
            builder.Append("<input type=\"radio\" class=\"hidden radio \" name=\"show_1\" value=\"1\" checked  id=\"show1\" >\n");
            builder.Append("\n");
            builder.Append("<label for=\"show1\" class=\"radio-control-label\">\n");
            builder.Append("<i class=\"icon-gray icon-radio-on\"></i>\n");
            builder.Append("Show in Tables\n");
            builder.Append("</label>\n");
            builder.Append("<input type=\"radio\" class=\"hidden radio \" name=\"show_1\" value=\"0\" id=\"show0\" >\n");
            builder.Append("\n");
            builder.Append("<label for=\"show0\" class=\"radio-control-label\">\n");
            builder.Append("<i class=\"icon-gray icon-radio-on\"></i>\n");
            builder.Append("Hide from Tables\n");
            builder.Append("</label>\n");
            builder.Append("</div>\n");
            builder.Append("</div>\n");
            builder.Append("\n");
            builder.Append("<label class=\"control-label has_help \" for=\"colmode_1\">Line Color</label>\n");
            builder.Append("<div class=\"controls \" data-placement=\"right\" data-helptext=\"Choose if you want an automatic line color for this channel in graphs or define the color manually.\">\n");
            builder.Append("<div class=\"radio-control\">\n");
            builder.Append("<input type=\"radio\" class=\"hidden radio  GroupShowHide\" name=\"colmode_1\" value=\"0\" id=\"colmode0\" >\n");
            builder.Append("\n");
            builder.Append("<label for=\"colmode0\" class=\"radio-control-label\">\n");
            builder.Append("<i class=\"icon-gray icon-radio-on\"></i>\n");
            builder.Append("Automatic\n");
            builder.Append("</label>\n");
            builder.Append("<input type=\"radio\" class=\"hidden radio  GroupShowHide\" name=\"colmode_1\" value=\"1\" checked id=\"colmode1\" >\n");
            builder.Append("\n");
            builder.Append("<label for=\"colmode1\" class=\"radio-control-label\">\n");
            builder.Append("<i class=\"icon-gray icon-radio-on\"></i>\n");
            builder.Append("Manual\n");
            builder.Append("</label>\n");
            builder.Append("</div>\n");
            builder.Append("</div>\n");
            builder.Append("\n");
            builder.Append("<label class=\"control-label has_help  Showcolmode1  Hidecolmode0  InitialDisplayNone  groupshowhideelement\" for=\"color_1\">Color (#rrggbb)</label>\n");
            builder.Append("<div class=\"controls  Showcolmode1  Hidecolmode0  InitialDisplayNone  groupshowhideelement\" data-placement=\"right\" data-helptext=\"Enter the color in hexadecimal RGB notation with a leading # (as in HTML/CSS, for example, #ff0000 for red, #0000ff for blue) or directly choose a color.\">\n");
            builder.Append("<input class=\"text colorselector\"  data-rule-required=\"true\" data-hexcolor=\"true\" autocomplete=\"off\" type=\"text\" name=\"color_1\" id=\"color_1\" value=\"#B441B1\" ></div>\n");
            builder.Append("\n");
            builder.Append("<label class=\"control-label has_help \" for=\"linewidth_1\">Line Width</label>\n");
            builder.Append("<div class=\"controls \" data-placement=\"right\" data-helptext=\"Enter the line width for large graphs.\">\n");
            builder.Append("<input class=\"text\"  data-rule-number=\"true\" data-rule-max=\"25\" data-rule-required=\"true\" type=\"text\" name=\"linewidth_1\" id=\"linewidth_1\" autocomplete=\"off\" value=\"1\" ></div>\n");
            builder.Append("\n");
            builder.Append("<label class=\"control-label has_help \" for=\"percent_1\">Data</label>\n");
            builder.Append("<div class=\"controls \" data-placement=\"right\" data-helptext=\"Choose if you want to show actual values with the given unit or to calculate and show a percentage based on the maximum value as defined below.\">\n");
            builder.Append("<div class=\"radio-control\">\n");
            builder.Append("<input type=\"radio\" class=\"hidden radio  GroupShowHide\" name=\"percent_1\" value=\"0\" id=\"percent0\">\n");
            builder.Append("<label for=\"percent0\" class=\"radio-control-label\">\n");
            builder.Append("<i class=\"icon-gray icon-radio-on\"></i>\n");
            builder.Append("Display actual values in %\n");
            builder.Append("</label>\n");
            builder.Append("<input type=\"radio\" class=\"hidden radio  GroupShowHide\" name=\"percent_1\" value=\"1\" checked=\"\" id=\"percent1\">\n");
            builder.Append("<label for=\"percent1\" class=\"radio-control-label\">\n");
            builder.Append("<i class=\"icon-gray icon-radio-on\"></i>\n");
            builder.Append("Display in percent of maximum\n");
            builder.Append("</label>\n");
            builder.Append("</div>\n");
            builder.Append("</div>\n");
            builder.Append("<label class=\"control-label has_help  Showpercent1  Hidepercent0  groupshowhideelement\" for=\"ref100percent_1\">Maximum (%)</label>\n");
            builder.Append("<div class=\"controls  Showpercent1  Hidepercent0  groupshowhideelement\" data-placement=\"right\" data-helptext=\"Enter a maximum value on which the percentage calculation is based.\">\n");
            builder.Append("<input type=\"text\" class=\"text\" data-rule-number=\"true\" autocomplete=\"off\" name=\"ref100percent_1\" id=\"ref100percent_1\" value=\"30\"><input type=\"hidden\" name=\"ref100percent_1_factor\" id=\"ref100percent_1_factor\" value=\"1\"></div>\n");
            builder.Append("\n");
            builder.Append("<label class=\"control-label has_help \" for=\"avgmode_1\">Value Mode</label>\n");
            builder.Append("<div class=\"controls \" data-placement=\"right\" data-helptext=\"Choose which values will be displayed as historic data for this channel. Instead of averages you can show maximum or minimum values of a time span.\">\n");
            builder.Append("<div class=\"radio-control\">\n");
            builder.Append("<input type=\"radio\" class=\"hidden radio \" name=\"avgmode_1\" value=\"0\" id=\"avgmode0\" >\n");
            builder.Append("\n");
            builder.Append("<label for=\"avgmode0\" class=\"radio-control-label\">\n");
            builder.Append("<i class=\"icon-gray icon-radio-on\"></i>\n");
            builder.Append("Average\n");
            builder.Append("</label>\n");
            builder.Append("<input type=\"radio\" class=\"hidden radio \" name=\"avgmode_1\" value=\"1\" checked id=\"avgmode1\" >\n");
            builder.Append("\n");
            builder.Append("<label for=\"avgmode1\" class=\"radio-control-label\">\n");
            builder.Append("<i class=\"icon-gray icon-radio-on\"></i>\n");
            builder.Append("Minimum\n");
            builder.Append("</label>\n");
            builder.Append("<input type=\"radio\" class=\"hidden radio \" name=\"avgmode_1\" value=\"2\" id=\"avgmode2\" >\n");
            builder.Append("\n");
            builder.Append("<label for=\"avgmode2\" class=\"radio-control-label\">\n");
            builder.Append("<i class=\"icon-gray icon-radio-on\"></i>\n");
            builder.Append("Maximum\n");
            builder.Append("</label>\n");
            builder.Append("</div>\n");
            builder.Append("</div>\n");
            builder.Append("\n");
            builder.Append("<label class=\"control-label has_help \" for=\"decimalmode_1\">Decimal Places</label>\n");
            builder.Append("<div class=\"controls \" data-placement=\"right\" data-helptext=\"Define the number of decimal places you want to display in this channel. You can enter the custom number below.\">\n");
            builder.Append("<div class=\"radio-control\">\n");
            builder.Append("<input type=\"radio\" class=\"hidden radio  GroupShowHide\" name=\"decimalmode_1\" value=\"0\" id=\"decimalmode0\" >\n");
            builder.Append("\n");
            builder.Append("<label for=\"decimalmode0\" class=\"radio-control-label\">\n");
            builder.Append("<i class=\"icon-gray icon-radio-on\"></i>\n");
            builder.Append("Automatic\n");
            builder.Append("</label>\n");
            builder.Append("<input type=\"radio\" class=\"hidden radio  GroupShowHide\" name=\"decimalmode_1\" value=\"1\" id=\"decimalmode1\" >\n");
            builder.Append("\n");
            builder.Append("<label for=\"decimalmode1\" class=\"radio-control-label\">\n");
            builder.Append("<i class=\"icon-gray icon-radio-on\"></i>\n");
            builder.Append("All\n");
            builder.Append("</label>\n");
            builder.Append("<input type=\"radio\" class=\"hidden radio  GroupShowHide\" name=\"decimalmode_1\" value=\"2\" checked id=\"decimalmode2\" >\n");
            builder.Append("\n");
            builder.Append("<label for=\"decimalmode2\" class=\"radio-control-label\">\n");
            builder.Append("<i class=\"icon-gray icon-radio-on\"></i>\n");
            builder.Append("Custom\n");
            builder.Append("</label>\n");
            builder.Append("</div>\n");
            builder.Append("</div>\n");
            builder.Append("\n");
            builder.Append("<label class=\"control-label no_help  Showdecimalmode2  Hidedecimalmode0  InitialDisplayNone  Hidedecimalmode1  groupshowhideelement\" for=\"decimaldigits_1\"></label>\n");
            builder.Append("<div class=\"controls  Showdecimalmode2  Hidedecimalmode0  InitialDisplayNone  Hidedecimalmode1  groupshowhideelement\" data-placement=\"right\" >\n");
            builder.Append("<input class=\"text\"  data-rule-required=\"true\" type=\"text\" name=\"decimaldigits_1\" id=\"decimaldigits_1\" autocomplete=\"off\" value=\"2\" ></div>\n");
            builder.Append("\n");
            builder.Append("<label class=\"control-label has_help \" for=\"spikemode_1\">Spike Filter</label>\n");
            builder.Append("<div class=\"controls \" data-placement=\"right\" data-helptext=\"Choose if you want to filter out incorrect values from data graphs and tables that are too high or too low. The filter applies to existing data. This can take a few minutes.\">\n");
            builder.Append("<div class=\"radio-control\">\n");
            builder.Append("<input type=\"radio\" class=\"hidden radio  GroupShowHide\" name=\"spikemode_1\" value=\"0\" id=\"spikemode0\" >\n");
            builder.Append("\n");
            builder.Append("<label for=\"spikemode0\" class=\"radio-control-label\">\n");
            builder.Append("<i class=\"icon-gray icon-radio-on\"></i>\n");
            builder.Append("Disable Filtering\n");
            builder.Append("</label>\n");
            builder.Append("<input type=\"radio\" class=\"hidden radio  GroupShowHide\" name=\"spikemode_1\" value=\"1\" checked id=\"spikemode1\" >\n");
            builder.Append("\n");
            builder.Append("<label for=\"spikemode1\" class=\"radio-control-label\">\n");
            builder.Append("<i class=\"icon-gray icon-radio-on\"></i>\n");
            builder.Append("Enable Filtering\n");
            builder.Append("</label>\n");
            builder.Append("</div>\n");
            builder.Append("</div>\n");
            builder.Append("\n");
            builder.Append("<label class=\"control-label has_help  Showspikemode1  Hidespikemode0  InitialDisplayNone  groupshowhideelement\" for=\"spikemax_1\">Spike Filter Max. Value (%)</label>\n");
            builder.Append("<div class=\"controls  Showspikemode1  Hidespikemode0  InitialDisplayNone  groupshowhideelement\" data-placement=\"right\" data-helptext=\"Define the upper spike filter. PRTG will disregard all data above this value.\">\n");
            builder.Append("<input type=\"text\" class=\"text\"  data-rule-number=\"true\" autocomplete=\"off\" name=\"spikemax_1\" id=\"spikemax_1\" value=\"100\" ><input type=\"hidden\" name=\"spikemax_1_factor\" id=\"spikemax_1_factor\" value=\"1\" ></div>\n");
            builder.Append("\n");
            builder.Append("<label class=\"control-label has_help  Showspikemode1  Hidespikemode0  InitialDisplayNone  groupshowhideelement\" for=\"spikemin_1\">Spike Filter Min. Value (%)</label>\n");
            builder.Append("<div class=\"controls  Showspikemode1  Hidespikemode0  InitialDisplayNone  groupshowhideelement\" data-placement=\"right\" data-helptext=\"Define the lower spike filter. PRTG will disregard all data below this value.\">\n");
            builder.Append("<input type=\"text\" class=\"text\"  data-rule-number=\"true\" autocomplete=\"off\" name=\"spikemin_1\" id=\"spikemin_1\" value=\"0\" ><input type=\"hidden\" name=\"spikemin_1_factor\" id=\"spikemin_1_factor\" value=\"1\" ></div>\n");
            builder.Append("\n");
            builder.Append("<label class=\"control-label has_help \" for=\"axismode_1\">Vertical Axis Scaling</label>\n");
            builder.Append("<div class=\"controls \" data-placement=\"right\" data-helptext=\"Define if you want to set the vertical axis in graphs for this channel automatically or set the axis minimum and maximum manually. <b>Note:</b> This setting will be ignored if the sensor&apos;s <i>Graph Type</i> is set to either &quot;Stack channels on top of each other&quot; or &quot;Show in and out traffic as positive and negative area graph&quot; (available for traffic sensors).\">\n");
            builder.Append("<div class=\"radio-control\">\n");
            builder.Append("<input type=\"radio\" class=\"hidden radio  GroupShowHide\" name=\"axismode_1\" value=\"0\" id=\"axismode0\" >\n");
            builder.Append("\n");
            builder.Append("<label for=\"axismode0\" class=\"radio-control-label\">\n");
            builder.Append("<i class=\"icon-gray icon-radio-on\"></i>\n");
            builder.Append("Automatic Scaling\n");
            builder.Append("</label>\n");
            builder.Append("<input type=\"radio\" class=\"hidden radio  GroupShowHide\" name=\"axismode_1\" value=\"1\" checked id=\"axismode1\" >\n");
            builder.Append("\n");
            builder.Append("<label for=\"axismode1\" class=\"radio-control-label\">\n");
            builder.Append("<i class=\"icon-gray icon-radio-on\"></i>\n");
            builder.Append("Manual Scaling\n");
            builder.Append("</label>\n");
            builder.Append("</div>\n");
            builder.Append("</div>\n");
            builder.Append("\n");
            builder.Append("<label class=\"control-label no_help  Showaxismode1  Hideaxismode0  InitialDisplayNone  groupshowhideelement\" for=\"axismax_1\">Vertical Axis Maximum (%)</label>\n");
            builder.Append("<div class=\"controls  Showaxismode1  Hideaxismode0  InitialDisplayNone  groupshowhideelement\" data-placement=\"right\" >\n");
            builder.Append("<input type=\"text\" class=\"text\"  data-rule-number=\"true\" data-rule-required=\"true\" autocomplete=\"off\" name=\"axismax_1\" id=\"axismax_1\" value=\"100\" ><input type=\"hidden\" name=\"axismax_1_factor\" id=\"axismax_1_factor\" value=\"1\" ></div>\n");
            builder.Append("\n");
            builder.Append("<label class=\"control-label no_help  Showaxismode1  Hideaxismode0  InitialDisplayNone  groupshowhideelement\" for=\"axismin_1\">Vertical Axis Minimum (%)</label>\n");
            builder.Append("<div class=\"controls  Showaxismode1  Hideaxismode0  InitialDisplayNone  groupshowhideelement\" data-placement=\"right\" >\n");
            builder.Append("<input type=\"text\" class=\"text\"  data-rule-number=\"true\" data-rule-required=\"true\" autocomplete=\"off\" name=\"axismin_1\" id=\"axismin_1\" value=\"0\" ><input type=\"hidden\" name=\"axismin_1_factor\" id=\"axismin_1_factor\" value=\"1\" ></div>\n");
            builder.Append("\n");
            builder.Append("<label class=\"control-label has_help \" for=\"limitmode_1\">Limits</label>\n");
            builder.Append("<div class=\"controls \" data-placement=\"right\" data-helptext=\"Define if you want to use thresholds for this channel. With limits you can put a sensor in a warning or down status depending on the current value of a channel.\">\n");
            builder.Append("<div class=\"radio-control\">\n");
            builder.Append("<input type=\"radio\" class=\"hidden radio  GroupShowHide\" name=\"limitmode_1\" value=\"0\" id=\"limitmode0\" >\n");
            builder.Append("\n");
            builder.Append("<label for=\"limitmode0\" class=\"radio-control-label\">\n");
            builder.Append("<i class=\"icon-gray icon-radio-on\"></i>\n");
            builder.Append("Disable Limits\n");
            builder.Append("</label>\n");
            builder.Append("<input type=\"radio\" class=\"hidden radio  GroupShowHide\" name=\"limitmode_1\" value=\"1\" checked  id=\"limitmode1\" >\n");
            builder.Append("\n");
            builder.Append("<label for=\"limitmode1\" class=\"radio-control-label\">\n");
            builder.Append("<i class=\"icon-gray icon-radio-on\"></i>\n");
            builder.Append("Enable Limits\n");
            builder.Append("</label>\n");
            builder.Append("</div>\n");
            builder.Append("</div>\n");
            builder.Append("\n");
            builder.Append("<label class=\"control-label has_help  Showlimitmode1  Hidelimitmode0  groupshowhideelement\" for=\"limitmaxerror_1\">Upper Error Limit (%)</label>\n");
            builder.Append("<div class=\"controls  Showlimitmode1  Hidelimitmode0  groupshowhideelement\" data-placement=\"right\" data-helptext=\"Enter an upper threshold. Values above this limit will set the sensor status to 'Down'.\">\n");
            builder.Append("<input type=\"text\" class=\"text\"  data-rule-number=\"true\" autocomplete=\"off\" name=\"limitmaxerror_1\" id=\"limitmaxerror_1\" value=\"100\" ><input type=\"hidden\" name=\"limitmaxerror_1_factor\" id=\"limitmaxerror_1_factor\" value=\"1\" ></div>\n");
            builder.Append("\n");
            builder.Append("<label class=\"control-label has_help  Showlimitmode1  Hidelimitmode0  groupshowhideelement\" for=\"limitmaxwarning_1\">Upper Warning Limit (%)</label>\n");
            builder.Append("<div class=\"controls  Showlimitmode1  Hidelimitmode0  groupshowhideelement\" data-placement=\"right\" data-helptext=\"Enter an upper threshold. Values above this limit will set the sensor status to 'Warning'.\">\n");
            builder.Append("<input type=\"text\" class=\"text\"  data-rule-number=\"true\" autocomplete=\"off\" name=\"limitmaxwarning_1\" id=\"limitmaxwarning_1\" value=\"80\" ><input type=\"hidden\" name=\"limitmaxwarning_1_factor\" id=\"limitmaxwarning_1_factor\" value=\"1\" ></div>\n");
            builder.Append("\n");
            builder.Append("<label class=\"control-label has_help  Showlimitmode1  Hidelimitmode0  groupshowhideelement\" for=\"limitminwarning_1\">Lower Warning Limit (%)</label>\n");
            builder.Append("<div class=\"controls  Showlimitmode1  Hidelimitmode0  groupshowhideelement\" data-placement=\"right\" data-helptext=\"Enter a lower threshold. Values below this limit will set the sensor status to 'Warning'.\">\n");
            builder.Append("<input type=\"text\" class=\"text\"  data-rule-number=\"true\" autocomplete=\"off\" name=\"limitminwarning_1\" id=\"limitminwarning_1\" value=\"30\" ><input type=\"hidden\" name=\"limitminwarning_1_factor\" id=\"limitminwarning_1_factor\" value=\"1\" ></div>\n");
            builder.Append("\n");
            builder.Append("<label class=\"control-label has_help  Showlimitmode1  Hidelimitmode0  groupshowhideelement\" for=\"limitminerror_1\">Lower Error Limit (%)</label>\n");
            builder.Append("<div class=\"controls  Showlimitmode1  Hidelimitmode0  groupshowhideelement\" data-placement=\"right\" data-helptext=\"Enter a lower threshold. Values below this limit will set the sensor status to 'Down'.\">\n");
            builder.Append("<input type=\"text\" class=\"text\"  data-rule-number=\"true\" autocomplete=\"off\" name=\"limitminerror_1\" id=\"limitminerror_1\" value=\"10\" ><input type=\"hidden\" name=\"limitminerror_1_factor\" id=\"limitminerror_1_factor\" value=\"1\" ></div>\n");
            builder.Append("\n");
            builder.Append("<label class=\"control-label has_help  Showlimitmode1  Hidelimitmode0  groupshowhideelement\" for=\"limiterrormsg_1\">Error Limit Message</label>\n");
            builder.Append("<div class=\"controls  Showlimitmode1  Hidelimitmode0  groupshowhideelement\" data-placement=\"right\" data-helptext=\"Enter a message that you want to add to the sensor message in down status.\">\n");
            builder.Append("<input class=\"text\"  type=\"text\" name=\"limiterrormsg_1\" id=\"limiterrormsg_1\" autocomplete=\"off\" value=\"An error has occurred\" ></div>\n");
            builder.Append("\n");
            builder.Append("<label class=\"control-label has_help  Showlimitmode1  Hidelimitmode0  groupshowhideelement\" for=\"limitwarningmsg_1\">Warning Limit Message</label>\n");
            builder.Append("<div class=\"controls  Showlimitmode1  Hidelimitmode0  groupshowhideelement\" data-placement=\"right\" data-helptext=\"Enter a message that you want to add to the sensor message in warning status.\">\n");
            builder.Append("<input class=\"text\"  type=\"text\" name=\"limitwarningmsg_1\" id=\"limitwarningmsg_1\" autocomplete=\"off\" value=\"A warning has occurred\" ></div>\n");
            builder.Append("\n");
            builder.Append("</div>\n");
            builder.Append("			<input type=\"hidden\" name=\"id\" value=\"2196\">\n");
            builder.Append("			<!-- <input type=\"hidden\" name=\"targeturl\" value=\"/sensor.htm?id=2196&tabid=11\"> -->\n");
            builder.Append("		</div>\n");
            builder.Append("	</fieldset>\n");
            builder.Append("	<div class=\"submitbuttonboxanchor\">\n");
            builder.Append("		<div class=\"submitbuttonbox\">\n");
            builder.Append("			<input style=\"\" id=\"mysubmit\" class=\"submit button btngrey\" type=\"submit\" value=\"Save\">\n");
            builder.Append("			<input style=\"\" onclick=\"history.back();return(false)\" class=\"cancel btngrey button hideinwingui\" type=\"reset\" value=\"Cancel\">\n");
            builder.Append("		</div>\n");
            builder.Append("	</div>\n");
            builder.Append("</form>\n");
            builder.Append("\n");
            builder.Append("    </div>\n");
            builder.Append("  </div>\n");
            builder.Append("</div>");

            return builder.ToString();
        }

        internal ChannelResponse(ChannelItem[] channels) : base("channels", channels)
        {
        }

        public override XElement GetItem(ChannelItem item)
        {
            var xml = new XElement("item",
                new XElement("lastvalue", item.LastValue),
                new XElement("lastvalue_raw", item.LastValueRaw),
                new XElement("objid", item.ObjId),
                new XElement("objid_raw", item.ObjIdRaw),
                new XElement("name", item.Name),
                new XElement("type", item.Type),
                new XElement("comments", item.Comments)
            );

            return xml;
        }

        public override string GetResponseText(string address)
        {
            if (address.Contains(HtmlFunction.ChannelEdit.GetDescription()))
            {
                return GetSettingsResponseText();
            }

            return base.GetResponseText(address);
        }
    }
}
