using System;
using System.Text;
using System.Text.RegularExpressions;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    public class SensorSettingsResponse : IWebResponse
    {
        private Func<string, string> propertyChanger;

        public SensorSettingsResponse(Func<string, string> propertyChanger = null)
        {
            this.propertyChanger = propertyChanger;
        }

        public string GetResponseText(ref string address)
        {
            var builder = new StringBuilder();

            builder.Append("<!-- First two lines must be left empty! -->\n");
            builder.Append("<!-- Help with <manuallink> from OSK, if settingspage for a sensor -->\n");
            builder.Append("<div class=\"contexthelpbox prtg-plugin\" data-id=\"id579218321\" data-plugin=\"contexthelp\" data-plugin-function=\"add\" data-plugin-target=\"#header_help\" data-show=\"true\"><div class=\"helpheader\">Settings</div><p><p>Define settings specific to this sensor. There is a manual page available with detailed information: <br><a href='/help/sensor_factory_sensor.htm' target='_blank'>Help: Sensor Settings</a></p></p></div>\n");
            builder.Append("<!-- Hilfe, falls Settingseite fuer \"My Account\" ID 100 -->\n");
            builder.Append("<!-- Hilfe, falls Settingseite fuer \"System & Website\" ID 800 -->\n");
            builder.Append("<!-- Hilfe, falls Settingseite fuer \"monitoring\" ID 810 -->\n");
            builder.Append("<!-- Hilfe, falls Settingseite fuer \"Notification Delivery\" ID 802 -->\n");
            builder.Append("<!-- Hilfe, falls Settingseite fuer \"Probes\" ID 801 -->\n");
            builder.Append("<form id=\"objectdataform\" enctype=\"multipart/form-data\" action=\"/editsettings\" method=\"post\" class=\"prtg-form prtg-plugin\" data-plugin=\"prtg-form\" data-ajaxsubmit=\"true\">\n");
            builder.Append("  <fieldset>\n");
            builder.Append("<legend class=\"prtg-header\">Basic Sensor Settings</legend><div class=\"control-group\">\n");
            builder.Append("<label class=\"control-label has_help \" for=\"name_\">Sensor Name</label>\n");
            builder.Append("<div class=\"controls  \" data-placement=\"right\" title=\"The sensor's name.\">\n");
            builder.Append("<input class=\"text\"  data-rule-required=\"true\" type=\"text\" name=\"name_\" id=\"name_\" autocomplete=\"off\" value=\"Server CPU Usage\" ></div>\n");
            builder.Append("<label class=\"control-label has_help \" for=\"tags_\">Tags</label>\n");
            builder.Append("<div class=\"controls  \" data-placement=\"right\" title=\"Enter a list of tags (not case sensitive) for filtering purposes (e.g. the top 10 lists use these tags). Use space or comma as separators.\">\n");
            builder.Append("<input class=\"text\"  type=\"text\" name=\"tags_\" id=\"tags_\" autocomplete=\"off\" value=\"factorysensor\" jsmarker=\"tag\"></div>\n");
            builder.Append("<label class=\"control-label has_help \" for=\"priority_\">Priority</label>\n");
            builder.Append("<div class=\"controls  \" data-placement=\"right\" title=\"Use this value in order to sort this object within lists.\">\n");
            builder.Append("<select name=\"priority_\" class=\"combo\" id=\"priority_\" >\n");
            builder.Append("<option value=\"5\" id=\"priority5\">***** (highest)</option>\n");
            builder.Append("<option value=\"4\" id=\"priority4\">****</option>\n");
            builder.Append("<option value=\"3\" selected=\"selected\"  id=\"priority3\">***</option>\n");
            builder.Append("<option value=\"2\" id=\"priority2\">**</option>\n");
            builder.Append("<option value=\"1\" id=\"priority1\">* (lowest)</option></select></div>\n");
            builder.Append("</div></fieldset><fieldset>\n");
            builder.Append("<legend class=\"prtg-header\">Sensor Factory Specific Settings</legend><div class=\"control-group\">\n");
            builder.Append("<label class=\"control-label has_help \" for=\"aggregationchannel_\">Channel Definition</label>\n");
            builder.Append("<div class=\"controls  \" data-placement=\"right\" title=\"The basic syntax for a channel definition looks like this:<br/><pre>#id:name [unit]<br/>formula</pre><br/>For each channel one section - which begins with the # sign - is used. Example with two channels:<br/><pre>#1:Sample<br/>Channel(1000,0)<br/>#2:Response Time[ms]<br/>Channel(1001,1)</pre><br/>The first parameter of the channel() function is the sensor ID, the second the channel ID. See the <a href='/help/sensor_factory_sensor.htm#sensor_channels' target='_blank'>manual</a> for details.\">\n");
            builder.Append("<textarea class=\"textarea\"  id=\"aggregationchannel_\" name=\"aggregationchannel_\" wrap=\"Off\"  rows=\"2\" >#1:Highest CPU\n");
            builder.Append("max(channel(2834,0),max(channel(2716,0),max(channel(2800,0),max(channel(2374,0),max(channel(2360,0),max(channel(2097,0),max(channel(2354,0),max(channel(2695,0),max(channel(2091,0),max(channel(2085,0),max(channel(2864,0),max(channel(2778,0),max(channel(2348,0),max(channel(2340,0),max(channel(2302,0),max(channel(2310,0),max(channel(2293,0),max(channel(2279,0),max(channel(2069,0),max(channel(2047,0),max(channel(2788,0),max(channel(2874,0),max(channel(2489,0),max(channel(2485,0),max(channel(2794,0),max(channel(2059,0),max(channel(2314,0),max(channel(2286,0),max(channel(2471,0),max(channel(2020,0),channel(2110,0)))))))))))))))))))))))))))))))\n");
            builder.Append("#2:ca-1\n");
            builder.Append("channel(2110,0)\n");
            builder.Append("#3:dc-1\n");
            builder.Append("channel(2020,0)\n");
            builder.Append("#4:dc-2\n");
            builder.Append("channel(2471,0)\n");
            builder.Append("#5:exch-1\n");
            builder.Append("channel(2286,0)\n");
            builder.Append("#6:exch-2\n");
            builder.Append("channel(2314,0)\n");
            builder.Append("#7:fs-1\n");
            builder.Append("channel(2059,0)\n");
            builder.Append("#8:fs-2\n");
            builder.Append("channel(2794,0)\n");
            builder.Append("#9:hypv-1\n");
            builder.Append("channel(2485,0)\n");
            builder.Append("#10:hypv-2\n");
            builder.Append("channel(2489,0)\n");
            builder.Append("#11:iis-1\n");
            builder.Append("channel(2874,0)\n");
            builder.Append("#12:one-1\n");
            builder.Append("channel(2788,0)\n");
            builder.Append("#13:prtg-1\n");
            builder.Append("channel(2047,0)\n");
            builder.Append("#14:prtg-2\n");
            builder.Append("channel(2069,0)\n");
            builder.Append("#15:rds-cb-1\n");
            builder.Append("channel(2279,0)\n");
            builder.Append("#16:rds-cb-2\n");
            builder.Append("channel(2293,0)\n");
            builder.Append("#17:rds-gw-1\n");
            builder.Append("channel(2310,0)\n");
            builder.Append("#18:rds-gw-2\n");
            builder.Append("channel(2302,0)\n");
            builder.Append("#19:rds-ts-1\n");
            builder.Append("channel(2340,0)\n");
            builder.Append("#20:rds-ts-2\n");
            builder.Append("channel(2348,0)\n");
            builder.Append("#21:sccm-1\n");
            builder.Append("channel(2778,0)\n");
            builder.Append("#22:skype-1\n");
            builder.Append("channel(2864,0)\n");
            builder.Append("#23:sql-1\n");
            builder.Append("channel(2085,0)\n");
            builder.Append("#24:sql-2\n");
            builder.Append("channel(2091,0)\n");
            builder.Append("#25:veeam-1\n");
            builder.Append("channel(2695,0)\n");
            builder.Append("#26:vmm-1\n");
            builder.Append("channel(2354,0)\n");
            builder.Append("#27:wsus-1\n");
            builder.Append("channel(2097,0)\n");
            builder.Append("#28:xap-dc-1\n");
            builder.Append("channel(2360,0)\n");
            builder.Append("#29:xap-dc-2\n");
            builder.Append("channel(2374,0)\n");
            builder.Append("#30:xap-pvs-1\n");
            builder.Append("channel(2800,0)\n");
            builder.Append("#31:xap-ts-1\n");
            builder.Append("channel(2716,0)\n");
            builder.Append("#32:xap-vdi-1\n");
            builder.Append("channel(2834,0)</textarea></div>\n");
            builder.Append("<label class=\"control-label has_help \" for=\"warnonerror_\">Error Handling</label>\n");
            builder.Append("<div class=\"controls  \" data-placement=\"right\" title=\"Specify the sensor's behavior if one of the defined sensors from above is in an error status.\">\n");
            builder.Append("<div class=\"radio-control\">\n");
            builder.Append("<input type=\"radio\" class=\"hidden radio  GroupShowHide\" name=\"warnonerror_\" value=\"0\" checked  id=\"warnonerror0\" >\n");
            builder.Append("<label for=\"warnonerror0\" class=\"radio-control-label\">\n");
            builder.Append("<i class=\"icon-gray icon-radio-on\"></i>\n");
            builder.Append("Factory sensor shows error status when one or more source sensors are in error status\n");
            builder.Append("</label>\n");
            builder.Append("<input type=\"radio\" class=\"hidden radio  GroupShowHide\" name=\"warnonerror_\" value=\"1\" id=\"warnonerror1\" >\n");
            builder.Append("<label for=\"warnonerror1\" class=\"radio-control-label\">\n");
            builder.Append("<i class=\"icon-gray icon-radio-on\"></i>\n");
            builder.Append("Factory sensor shows warning status when one or more source sensors are in error status\n");
            builder.Append("</label>\n");
            builder.Append("<input type=\"radio\" class=\"hidden radio  GroupShowHide\" name=\"warnonerror_\" value=\"2\" id=\"warnonerror2\" >\n");
            builder.Append("<label for=\"warnonerror2\" class=\"radio-control-label\">\n");
            builder.Append("<i class=\"icon-gray icon-radio-on\"></i>\n");
            builder.Append("Use custom formula\n");
            builder.Append("</label>\n");
            builder.Append("</div>\n");
            builder.Append("</div>\n");
            builder.Append("<label class=\"control-label has_help  Showwarnonerror2  Hidewarnonerror0  InitialDisplayNone  Hidewarnonerror1 \" for=\"aggregationstatus_\">Status Definition</label>\n");
            builder.Append("<div class=\"controls  Showwarnonerror2  Hidewarnonerror0  InitialDisplayNone  Hidewarnonerror1  \" data-placement=\"right\" title=\"Define when the factory sensor will change to a <b>Down</b> status. Enter a formula using the <i>status(SensorId)</i> function and Boolean operations (AND, OR, NOT) or keep the field empty for default. See <a href='/help/sensor_factory_sensor.htm#sensor_status' target='_blank'>user manual</a> for details.\">\n");
            builder.Append("<textarea class=\"textarea\"  id=\"aggregationstatus_\" name=\"aggregationstatus_\" wrap=\"Off\"  rows=\"2\" ></textarea></div>\n");
            builder.Append("<label class=\"control-label has_help \" for=\"missingdata_\">If a Sensor Has No Data</label>\n");
            builder.Append("<div class=\"controls  \" data-placement=\"right\" title=\"In case a sensor, that is used in a formula for one or more channels of this sensor factory, does not provide data for an interval (e.g. sensor is paused or did not exist) you have two options.\">\n");
            builder.Append("<div class=\"radio-control\">\n");
            builder.Append("<input type=\"radio\" class=\"hidden radio \" name=\"missingdata_\" value=\"0\" checked  id=\"missingdata0\" >\n");
            builder.Append("<label for=\"missingdata0\" class=\"radio-control-label\">\n");
            builder.Append("<i class=\"icon-gray icon-radio-on\"></i>\n");
            builder.Append("Do not calculate factory channels that use the sensor\n");
            builder.Append("</label>\n");
            builder.Append("<input type=\"radio\" class=\"hidden radio \" name=\"missingdata_\" value=\"1\" id=\"missingdata1\" >\n");
            builder.Append("<label for=\"missingdata1\" class=\"radio-control-label\">\n");
            builder.Append("<i class=\"icon-gray icon-radio-on\"></i>\n");
            builder.Append("Calculate the factory channels and use zero as source value\n");
            builder.Append("</label>\n");
            builder.Append("</div>\n");
            builder.Append("</div>\n");
            builder.Append("</div></fieldset><fieldset>\n");
            builder.Append("<legend class=\"prtg-header\">Sensor Display</legend><div class=\"control-group\">\n");
            builder.Append("<label class=\"control-label has_help \" for=\"primarychannel_\">Primary Channel</label>\n");
            builder.Append("<div class=\"controls  \" data-placement=\"right\" title=\"The last value of the primary channel will always be displayed for this sensor. The primary channel can also be used to trigger notifications.\">\n");
            builder.Append("<select name=\"primarychannel_\" id=\"primarychannel_\" >\n");
            builder.Append("<option value=\"1|Highest CPU (%)|\" selected=\"selected\" >Highest CPU (%)</option>\n");
            builder.Append("<option value=\"2|ca-1 (%)|\">ca-1 (%)</option>\n");
            builder.Append("<option value=\"3|dc-1 (%)|\">dc-1 (%)</option>\n");
            builder.Append("<option value=\"4|dc-2 (%)|\">dc-2 (%)</option>\n");
            builder.Append("<option value=\"5|exch-1 (%)|\">exch-1 (%)</option>\n");
            builder.Append("<option value=\"6|exch-2 (%)|\">exch-2 (%)</option>\n");
            builder.Append("<option value=\"7|fs-1 (%)|\">fs-1 (%)</option>\n");
            builder.Append("<option value=\"8|fs-2 (%)|\">fs-2 (%)</option>\n");
            builder.Append("<option value=\"9|hypv-1 (%)|\">hypv-1 (%)</option>\n");
            builder.Append("<option value=\"10|hypv-2 (%)|\">hypv-2 (%)</option>\n");
            builder.Append("<option value=\"11|iis-1 (%)|\">iis-1 (%)</option>\n");
            builder.Append("<option value=\"12|one-1 (%)|\">one-1 (%)</option>\n");
            builder.Append("<option value=\"13|prtg-1 (%)|\">prtg-1 (%)</option>\n");
            builder.Append("<option value=\"14|prtg-2 (%)|\">prtg-2 (%)</option>\n");
            builder.Append("<option value=\"15|rds-cb-1 (%)|\">rds-cb-1 (%)</option>\n");
            builder.Append("<option value=\"16|rds-cb-2 (%)|\">rds-cb-2 (%)</option>\n");
            builder.Append("<option value=\"17|rds-gw-1 (%)|\">rds-gw-1 (%)</option>\n");
            builder.Append("<option value=\"18|rds-gw-2 (%)|\">rds-gw-2 (%)</option>\n");
            builder.Append("<option value=\"19|rds-ts-1 (%)|\">rds-ts-1 (%)</option>\n");
            builder.Append("<option value=\"20|rds-ts-2 (%)|\">rds-ts-2 (%)</option>\n");
            builder.Append("<option value=\"21|sccm-1 (%)|\">sccm-1 (%)</option>\n");
            builder.Append("<option value=\"22|skype-1 (%)|\">skype-1 (%)</option>\n");
            builder.Append("<option value=\"23|sql-1 (%)|\">sql-1 (%)</option>\n");
            builder.Append("<option value=\"24|sql-2 (%)|\">sql-2 (%)</option>\n");
            builder.Append("<option value=\"25|veeam-1 (%)|\">veeam-1 (%)</option>\n");
            builder.Append("<option value=\"26|vmm-1 (%)|\">vmm-1 (%)</option>\n");
            builder.Append("<option value=\"27|wsus-1 (%)|\">wsus-1 (%)</option>\n");
            builder.Append("<option value=\"28|xap-dc-1 (%)|\">xap-dc-1 (%)</option>\n");
            builder.Append("<option value=\"29|xap-dc-2 (%)|\">xap-dc-2 (%)</option>\n");
            builder.Append("<option value=\"30|xap-pvs-1 (%)|\">xap-pvs-1 (%)</option>\n");
            builder.Append("<option value=\"31|xap-ts-1 (%)|\">xap-ts-1 (%)</option>\n");
            builder.Append("<option value=\"32|xap-vdi-1 (%)|\">xap-vdi-1 (%)</option>\n");
            builder.Append("<option value=\"-4|Downtime|\">Downtime</option></select></div>\n");
            builder.Append("<label class=\"control-label has_help \" for=\"stack_\">Chart Type</label>\n");
            builder.Append("<div class=\"controls  \" data-placement=\"right\" title=\"Select how the graph will be displayed. <b>Note:</b> If <i>Vertical Axis Scaling</i> is set to 'Manual Scaling' for a sensor channel, this will only be applied when using the default setting here, other settings will overwrite manual scaling.\">\n");
            builder.Append("<div class=\"radio-control\">\n");
            builder.Append("<input type=\"radio\" class=\"hidden radio  GroupShowHide\" name=\"stack_\" value=\"0\" checked  id=\"stack0\" >\n");
            builder.Append("<label for=\"stack0\" class=\"radio-control-label\">\n");
            builder.Append("<i class=\"icon-gray icon-radio-on\"></i>\n");
            builder.Append("Show channels independently (default)\n");
            builder.Append("</label>\n");
            builder.Append("<input type=\"radio\" class=\"hidden radio  GroupShowHide\" name=\"stack_\" value=\"1\" id=\"stack1\" >\n");
            builder.Append("<label for=\"stack1\" class=\"radio-control-label\">\n");
            builder.Append("<i class=\"icon-gray icon-radio-on\"></i>\n");
            builder.Append("Stack channels on top of each other\n");
            builder.Append("</label>\n");
            builder.Append("</div>\n");
            builder.Append("</div>\n");
            builder.Append("<label class=\"control-label has_help  Showstack1  Hidestack0  InitialDisplayNone  Hidestack2 \" for=\"stackunit_\">Stack Unit</label>\n");
            builder.Append("<div class=\"controls  Showstack1  Hidestack0  InitialDisplayNone  Hidestack2  \" data-placement=\"right\" title=\"Select a unit. All channels with this unit will be stacked on top of each other.\">\n");
            builder.Append("<div class=\"radio-control\">\n");
            builder.Append("<input type=\"radio\" class=\"radio hidden\" name=\"stackunit_\" value=\"?raw-s4|%\" checked=\"checked\"  id=\"stackunit0\"><label for=\"stackunit0\" class=\"radio-control-label\"><i class=\"icon-gray icon-radio-on\"></i>%</label>\n");
            builder.Append("</div>\n");
            builder.Append("</div>\n");
            builder.Append("</div></fieldset><fieldset class=\"collapsed\">\n");
            builder.Append("<legend class=\"prtg-header\">Scanning Interval\n");
            builder.Append("<span class=\"inheritedfrom\">\n");
            builder.Append("<input name=\"intervalgroup\" type=\"hidden\" value=\"0\"><input class=\"toggleFieldset GroupShowHide checkbox\" name=\"intervalgroup\" type=\"checkbox\" value=\"1\" id=\"Inheritintervalgroup\"  checked=\"checked\" ><label for=\"Inheritintervalgroup\">inherit from</label> <a  dependency=\"2\"  thisid=\"2881\" class=\"devicemenu isnotpaused  isnotfavorite\"  style=\"background-image:url(/icons/devices/a_server_1.png)\" id=\"2881\" href=\"device.htm?id=2881\">Network Overview</a> \n");
            builder.Append(" <span title=\"Scanning Interval: 60 seconds, Set sensor to warning for 1 interval, then set to &quot;down&quot; (recommended)\">(Scanning Interval: 60 seconds, Set sensor to ...)</span></span>\n");
            builder.Append("</legend><div class=\"control-group\">\n");
            builder.Append("<label class=\"control-label has_help \" for=\"interval_\">Scanning Interval</label>\n");
            builder.Append("<div class=\"controls  \" data-placement=\"right\" title=\"Time between two scans\">\n");
            builder.Append("<select name=\"interval_\" id=\"interval_\" >\n");
            builder.Append("<option value=\"30|30 seconds\">30 seconds</option>\n");
            builder.Append("<option value=\"60|60 seconds\" selected=\"selected\" >60 seconds</option>\n");
            builder.Append("<option value=\"300|5 minutes\">5 minutes</option>\n");
            builder.Append("<option value=\"600|10 minutes\">10 minutes</option>\n");
            builder.Append("<option value=\"900|15 minutes\">15 minutes</option>\n");
            builder.Append("<option value=\"1800|30 minutes\">30 minutes</option>\n");
            builder.Append("<option value=\"3600|1 hour\">1 hour</option>\n");
            builder.Append("<option value=\"14400|4 hours\">4 hours</option>\n");
            builder.Append("<option value=\"21600|6 hours\">6 hours</option>\n");
            builder.Append("<option value=\"43200|12 hours\">12 hours</option>\n");
            builder.Append("<option value=\"86400|24 hours\">24 hours</option></select></div>\n");
            builder.Append("<label class=\"control-label has_help \" for=\"errorintervalsdown_\">When a sensor reports an error</label>\n");
            builder.Append("<div class=\"controls  \" data-placement=\"right\" title=\"When a device reports an error, PRTG can try to reach the device again with the next scanning interval before the sensor is shown as 'down'. This can avoid false alarms if your device has temporary issues only. Note: WMI sensors always use at least 1 interval. Channel error limits will always set the sensor to 'down' immediately.\">\n");
            builder.Append("<select name=\"errorintervalsdown_\" class=\"combo\" id=\"errorintervalsdown_\" >\n");
            builder.Append("<option value=\"0\" id=\"errorintervalsdown0\">Set sensor to &quot;down&quot; immediately</option>\n");
            builder.Append("<option value=\"1\" selected=\"selected\"  id=\"errorintervalsdown1\">Set sensor to warning for 1 interval, then set to &quot;down&quot; (recommended)</option>\n");
            builder.Append("<option value=\"2\" id=\"errorintervalsdown2\">Set sensor to warning for 2 intervals, then set to &quot;down&quot;</option>\n");
            builder.Append("<option value=\"3\" id=\"errorintervalsdown3\">Set sensor to warning for 3 intervals, then set to &quot;down&quot;</option>\n");
            builder.Append("<option value=\"4\" id=\"errorintervalsdown4\">Set sensor to warning for 4 intervals, then set to &quot;down&quot;</option>\n");
            builder.Append("<option value=\"5\" id=\"errorintervalsdown5\">Set sensor to warning for 5 intervals, then set to &quot;down&quot;</option></select></div>\n");
            builder.Append("<span class=\"InitialDisplayNone\"><div class=\"wiz1-3\"><input type=\"hidden\" name=\"inherittriggers\" value=\"1\"></div></span>\n");
            builder.Append("</div></fieldset><fieldset class=\"collapsed\">\n");
            builder.Append("<legend class=\"prtg-header\">Schedules, Dependencies, and Maintenance Window\n");
            builder.Append("<span class=\"inheritedfrom\">\n");
            builder.Append("<input name=\"scheduledependency\" type=\"hidden\" value=\"0\"><input class=\"toggleFieldset GroupShowHide checkbox\" name=\"scheduledependency\" type=\"checkbox\" value=\"1\" id=\"Inheritscheduledependency\"  checked=\"checked\" ><label for=\"Inheritscheduledependency\">inherit from</label> <a  dependency=\"2\"  thisid=\"2881\" class=\"devicemenu isnotpaused  isnotfavorite\"  style=\"background-image:url(/icons/devices/a_server_1.png)\" id=\"2881\" href=\"device.htm?id=2881\">Network Overview</a> \n");
            builder.Append("</span>\n");
            builder.Append("</legend><div class=\"control-group\">\n");
            builder.Append("<div class=\"controls fillwidth\"><div class=\"controls-info\"><div class=\"readonlyproperty\" >Dependencies, schedules and maintenance windows always pause all sensors inside a group/device. This pausing is always inherited to all sub-objects and the inheritance can not be disabled. Below you can set additional schedules, maintenance windows or dependencies that will be used on top of any inherited setting.</div></div></div>\n");
            builder.Append("<label class=\"control-label has_help \" for=\"schedule_\">Schedule</label>\n");
            builder.Append("<div class=\"controls  \" data-placement=\"right\" title=\"Use schedules to only monitor within specific time spans (days, hours) throughout the week. You can edit schedules in the account settings.\">\n");
            builder.Append("<select name=\"schedule_\" id=\"schedule_\" >\n");
            builder.Append("<option value=\"-1|None|\">None</option>\n");
            builder.Append("<option value=\"623|Saturdays [GMT+0800]|\">Saturdays [GMT+0800]</option>\n");
            builder.Append("<option value=\"622|Sundays [GMT+0800]|\">Sundays [GMT+0800]</option>\n");
            builder.Append("<option value=\"620|Weekdays [GMT+0800]|\">Weekdays [GMT+0800]</option>\n");
            builder.Append("<option value=\"625|Weekdays Eight-To-Eight (8:00 - 20:00) [GMT+0800]|\">Weekdays Eight-To-Eight (8:00 - 20:00) [GMT+0800]</option>\n");
            builder.Append("<option value=\"627|Weekdays Nights (17:00 - 9:00) [GMT+0800]|\" selected=\"selected\" >Weekdays Nights (17:00 - 9:00) [GMT+0800]</option>\n");
            builder.Append("<option value=\"626|Weekdays Nights (20:00 - 8:00) [GMT+0800]|\">Weekdays Nights (20:00 - 8:00) [GMT+0800]</option>\n");
            builder.Append("<option value=\"624|Weekdays Nine-To-Five (9:00 - 17:00) [GMT+0800]|\">Weekdays Nine-To-Five (9:00 - 17:00) [GMT+0800]</option>\n");
            builder.Append("<option value=\"621|Weekends [GMT+0800]|\">Weekends [GMT+0800]</option></select></div>\n");
            builder.Append("<label class=\"control-label has_help \" for=\"maintenable_\">Maintenance Window</label>\n");
            builder.Append("<div class=\"controls  \" data-placement=\"right\" title=\"This setting allows you to set up a one-time maintenance window for this object. During a maintenance window this object and all child objects will not be monitored and will enter a paused state.\">\n");
            builder.Append("<div class=\"radio-control\">\n");
            builder.Append("<input type=\"radio\" class=\"hidden radio  GroupShowHide\" name=\"maintenable_\" value=\"0\" checked  id=\"maintenable0\" >\n");
            builder.Append("<label for=\"maintenable0\" class=\"radio-control-label\">\n");
            builder.Append("<i class=\"icon-gray icon-radio-on\"></i>\n");
            builder.Append("Not set (monitor continuously)\n");
            builder.Append("</label>\n");
            builder.Append("<input type=\"radio\" class=\"hidden radio  GroupShowHide\" name=\"maintenable_\" value=\"1\" id=\"maintenable1\" >\n");
            builder.Append("<label for=\"maintenable1\" class=\"radio-control-label\">\n");
            builder.Append("<i class=\"icon-gray icon-radio-on\"></i>\n");
            builder.Append("Set up a one-time maintenance window\n");
            builder.Append("</label>\n");
            builder.Append("</div>\n");
            builder.Append("</div>\n");
            builder.Append("<label class=\"control-label has_help  Showmaintenable1  Hidemaintenable0  InitialDisplayNone \" for=\"maintstart_\">Maintenance Begins At</label>\n");
            builder.Append("<div class=\"controls  Showmaintenable1  Hidemaintenable0  InitialDisplayNone  \" data-placement=\"right\" title=\"Enter the start date and time of the maintenance window.\">\n");
            builder.Append("<input class=\"prtg-plugin\" data-plugin=\"prtg-datetimepicker\" data-maxdate=\"+10y\" data-timepicker=\"true\" type=\"text\" name=\"maintstart_\" id=\"maintstart_\" value=\"2017,04,02,13,31,00\" ></div>\n");
            builder.Append("<label class=\"control-label has_help  Showmaintenable1  Hidemaintenable0  InitialDisplayNone \" for=\"maintend_\">Maintenance Ends At</label>\n");
            builder.Append("<div class=\"controls  Showmaintenable1  Hidemaintenable0  InitialDisplayNone  \" data-placement=\"right\" title=\"Enter the end date and time of the maintenance window.\">\n");
            builder.Append("<input class=\"prtg-plugin\" data-plugin=\"prtg-datetimepicker\" data-maxdate=\"+10y\" data-timepicker=\"true\" type=\"text\" name=\"maintend_\" id=\"maintend_\" value=\"2017,04,02,13,31,00\" ></div>\n");
            builder.Append("<label class=\"control-label has_help \" for=\"dependencytype_\">Dependency Type</label>\n");
            builder.Append("<div class=\"controls  \" data-placement=\"right\" title=\"Select the dependency behavior for this object. 'Use parent' will pause the object if the parent object is not 'UP'. 'Select object' allows to select an object from a drop-down list on which the current object will depend on. 'Master object' has a special meaning: If a sensor is the master object for a device then the device and all its other sensors will be paused until the master object comes back up. Selecting this option will make the sensor the 'master object'. Tip: It is a good idea to create at least one basic sensor (e.g. PING) with a short interval as the master sensor for a device in order to pause all sibling sensors in case the device can't even be pinged.\">\n");
            builder.Append("<div class=\"radio-control\">\n");
            builder.Append("<input type=\"radio\" class=\"hidden radio  GroupShowHide\" name=\"dependencytype_\" value=\"0\" checked  id=\"dependencytype0\" >\n");
            builder.Append("<label for=\"dependencytype0\" class=\"radio-control-label\">\n");
            builder.Append("<i class=\"icon-gray icon-radio-on\"></i>\n");
            builder.Append("Use parent\n");
            builder.Append("</label>\n");
            builder.Append("<input type=\"radio\" class=\"hidden radio  GroupShowHide\" name=\"dependencytype_\" value=\"1\" id=\"dependencytype1\" >\n");
            builder.Append("<label for=\"dependencytype1\" class=\"radio-control-label\">\n");
            builder.Append("<i class=\"icon-gray icon-radio-on\"></i>\n");
            builder.Append("Select object\n");
            builder.Append("</label>\n");
            builder.Append("<input type=\"radio\" class=\"hidden radio  GroupShowHide\" name=\"dependencytype_\" value=\"2\" id=\"dependencytype2\" >\n");
            builder.Append("<label for=\"dependencytype2\" class=\"radio-control-label\">\n");
            builder.Append("<i class=\"icon-gray icon-radio-on\"></i>\n");
            builder.Append("Master object for parent\n");
            builder.Append("</label>\n");
            builder.Append("</div>\n");
            builder.Append("</div>\n");
            builder.Append("<label class=\"control-label has_help  Showdependencytype1  Hidedependencytype0  InitialDisplayNone  Hidedependencytype2 \" for=\"dependency_\">Dependency</label>\n");
            builder.Append("<div class=\"controls  Showdependencytype1  Hidedependencytype0  InitialDisplayNone  Hidedependencytype2  \" data-placement=\"right\" title=\"Select an object upon which the current object shall depend on. If the selected sensor is not in an UP state (i.e. DOWN or PAUSED due to a further dependency) the current object will be paused. Choose a sensor with a short interval and timeout value. Use with caution!\">\n");
            builder.Append("<div class=\"sensorlookupnew prtg-plugin\" data-plugin=\"object-lookup\" data-inputname=\"dependency_\" data-rootid=\"0\" data-selid=\"null\" data-onlysensors=\"1\" data-sensors=\"1\" data-devices=\"1\" data-groups=\"0\" data-required=\"\" data-probes=\"0\" data-allowone=\"1\" data-allowself=\"0\" data-allowroot=\"0\" data-ownid=\"2884\"></div></div>\n");
            builder.Append("<label class=\"control-label has_help  Showdependencytype0  Showdependencytype1  Hidedependencytype2 \" for=\"depdelay_\">Delay (Seconds)</label>\n");
            builder.Append("<div class=\"controls  Showdependencytype0  Showdependencytype1  Hidedependencytype2  \" data-placement=\"right\" title=\"Enter a value (in seconds). Resuming of monitoring for this object will be additionally delayed after the master object for this dependency is 'Up' again. This is helpful for devices with sensors that need some time to resume after the restart of a device, while usually the dependency sensor (e.g. Ping) is already 'Up'. A delay can avoid false alarms.\">\n");
            builder.Append("<input class=\"text\"  type=\"text\" name=\"depdelay_\" id=\"depdelay_\" autocomplete=\"off\" value=\"0\" ></div>\n");
            builder.Append("</div></fieldset><fieldset class=\"collapsed\">\n");
            builder.Append("<legend class=\"prtg-header\">Access Rights\n");
            builder.Append("<span class=\"inheritedfrom\">\n");
            builder.Append("<input name=\"accessgroup\" type=\"hidden\" value=\"0\"><input class=\"toggleFieldset GroupShowHide checkbox\" name=\"accessgroup\" type=\"checkbox\" value=\"1\" id=\"Inheritaccessgroup\"  checked=\"checked\" ><label for=\"Inheritaccessgroup\">inherit from</label> <a  dependency=\"2\"  thisid=\"2881\" class=\"devicemenu isnotpaused  isnotfavorite\"  style=\"background-image:url(/icons/devices/a_server_1.png)\" id=\"2881\" href=\"device.htm?id=2881\">Network Overview</a> \n");
            builder.Append("</span>\n");
            builder.Append("</legend><div class=\"control-group\">\n");
            builder.Append("<label class=\"control-label has_help \" for=\"accessrights_\">User Group Access</label>\n");
            builder.Append("<div class=\"controls  \" data-placement=\"right\" title=\"Set user group access rights for this object. You cannot remove rights defined for a parent node. All rights are inherited to child nodes\">\n");
            builder.Append("<table class=\"table hoverable\" ><thead><tr><th>User Group<input type=\"hidden\" name=\"accessrights_\" value=\"1\"></th><th>Rights</th></tr></thead><tr class=\"even\"><td><div class=\"unitselect\">PRTG Users Group</div></td><td><select class=\"rightsselect\" name=\"accessrights_201\" id=\"accessrights_\" ><option value=\"-1\" selected=\"selected\" >Inherited (None)</option><option value=\"0\">None</option><option value=\"100\">Read</option><option value=\"200\">Write</option><option value=\"400\">Full</option></select></td></tr></table></div>\n");
            builder.Append("</div></fieldset>\n");
            builder.Append("  <input type=\"hidden\" name=\"id\" value=\"2884\">\n");
            builder.Append("  <div class=\"submitbuttonboxanchor\">\n");
            builder.Append("    <div class=\"submitbuttonbox\">\n");
            builder.Append("      <input style=\"\" type=\"submit\" class=\"submit button btngrey\" value=\"Save\">\n");
            builder.Append("      <input onclick=\"history.back();return(false)\" class=\"button btngrey hideinwingui\" type=\"reset\" value=\"Cancel\">\n");
            builder.Append("      <!--input class=\"button btngrey\" onclick=\"_Prtg.objectTools.copyClipBoard($('#objectdataform'));return false;\" value=\"Clipboard\" title=\"Copy settings to clipboard\" data-placement=\"right\"-->\n");
            builder.Append("    </div>\n");
            builder.Append("  </div>\n");
            builder.Append("</form>");

            var str = builder.ToString();

            str = propertyChanger?.Invoke(str) ?? str;

            return str;
        }

        public static string SetContainerTagContents(string body, string newContents, string tag, string nameAttrib)
        {
            var original = GetContainerTagContents(body, tag, nameAttrib);

            return body.Replace(original, newContents);
        }

        private static string GetContainerTagContents(string body, string tag, string nameAttrib)
        {
            var regex = new Regex($"(<{tag}[^>]+?name=\\\"{nameAttrib}\\\".+?>)(.+?)(<\\/{tag}>)", RegexOptions.Singleline);

            var match = regex.Match(body);

            if (match.Success)
            {
                var val = match.Value;

                return regex.Replace(val, "$2");
            }
            else
            {
                throw new NotImplementedException("Couldn't find match for specified tag. This should not be possible");
            }
        }
    }
}
