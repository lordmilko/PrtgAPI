using System.Text;
using System.Xml.Linq;
using PrtgAPI.Utilities;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    public class ScheduleResponse : BaseResponse<ScheduleItem>
    {
        internal ScheduleResponse(params ScheduleItem[] schedules) : base("schedules", schedules)
        {
        }

        string GetSettingsResponseText(string address)
        {
            var builder = new StringBuilder();

            builder.Append("<form id=\"objectdataform\" enctype=\"multipart/form-data\" action=\"/editsettings\" method=\"post\" class=\"prtg-form prtg-plugin\" data-plugin=\"prtg-form\" data-ajaxsubmit=\"true\">");
            builder.Append("      <fieldset>");
            builder.Append("<legend class=\"prtg-header\">Basic Settings</legend><div class=\"control-group\">");
            builder.Append("<label class=\"control-label has_help \" for=\"name_\">Schedule Name</label>");
            builder.Append("<div class=\"controls \" data-placement=\"top\" data-helptext=\"Enter a meaningful name for the schedule to identify it later.<p><a class='morehelp nohjax' target='_blank' href='/help/schedules_settings.htm#basic'>Further Help (Manual)</a></p>\">");
            builder.Append("<input class=\"text\"  data-rule-required=\"true\" type=\"text\" name=\"name_\" id=\"name_\" autocomplete=\"off\" value=\"Weekdays [GMT+0800]\" ></div>");
            builder.Append("<label class=\"control-label has_help \" for=\"tags_\">Tags</label>");
            builder.Append("<div class=\"controls \" data-placement=\"top\" data-helptext=\"Enter a list of tags (not case sensitive) for filtering purposes (for example, the top 10 lists use these tags). Use spaces or commas as separators.<br/><br/> <b>Note:</b> Plus (+) or minus (-) are not allowed as first characters. You cannot use round parentheses or angle brackets in tags.<p><a class='morehelp nohjax' target='_blank' href='/help/schedules_settings.htm#basic'>Further Help (Manual)</a></p>\">");
            builder.Append("<input class=\"text\"  type=\"text\" name=\"tags_\" id=\"tags_\" autocomplete=\"off\" value=\"\" ></div>");
            builder.Append("<label class=\"control-label has_help \" for=\"fineschedmode_\">Edit Mode</label>");
            builder.Append("<div class=\"controls \" data-placement=\"top\" data-helptext=\"Select how you want to define your schedules. The <b>Use weekday/hour time table</b> option has only an hourly resolution, but gives better visual feedback and is easy to use. The <b>Use list of period definitions</b> option allows you to define a more precise time span.<p><a class='morehelp nohjax' target='_blank' href='/help/schedules_settings.htm#basic'>Further Help (Manual)</a></p>\">");
            builder.Append("<div class=\"radio-control\">");
            builder.Append("<input type=\"radio\" class=\"hidden radio  GroupShowHide\" name=\"fineschedmode_\" value=\"0\" checked  id=\"fineschedmode0\" >");
            builder.Append("<label for=\"fineschedmode0\" class=\"radio-control-label\">");
            builder.Append("<i class=\"icon-gray icon-radio-on\"></i>");
            builder.Append("Use weekday/hour time table");
            builder.Append("</label>");
            builder.Append("<input type=\"radio\" class=\"hidden radio  GroupShowHide\" name=\"fineschedmode_\" value=\"1\" id=\"fineschedmode1\" >");
            builder.Append("<label for=\"fineschedmode1\" class=\"radio-control-label\">");
            builder.Append("<i class=\"icon-gray icon-radio-on\"></i>");
            builder.Append("Use list of period definitions");
            builder.Append("</label>");
            builder.Append("</div>");
            builder.Append("</div>");
            builder.Append("<label class=\"control-label has_help  Showfineschedmode0  Hidefineschedmode1  groupshowhideelement\" for=\"timetable_\">Time Table (active time slots)</label>");
            builder.Append("<div class=\"controls  Showfineschedmode0  Hidefineschedmode1  groupshowhideelement\" data-placement=\"top\" data-helptext=\"A checked box means 'sensor/notification is active in this hour'. Uncheck a box to pause sensors or notifications during the respective hour. Click the buttons to enable/disable hours or days.<p><a class='morehelp nohjax' target='_blank' href='/help/schedules_settings.htm#basic'>Further Help (Manual)</a></p>\">");
            builder.Append("<table class=\"table hoverable schedule prevent-datatables prtg-plugin\" data-plugin=\"form-schedule\" ><thead><tr>");
            builder.Append("<th>");
            builder.Append("<a class=\"schedulebutton\" id=\"scheduleAll\" data-targets=\"scheduleAll\" data-toggle=\"ON\">All</a></th>");
            builder.Append("<th>");
            builder.Append("<a class=\"schedulebutton\" id=\"scheduleMo\" data-targets=\"scheduleMo\" data-toggle=\"ON\">Mo</a></th>");
            builder.Append("<th>");
            builder.Append("<a class=\"schedulebutton\" id=\"scheduleTu\" data-targets=\"scheduleTu\" data-toggle=\"ON\">Tu</a></th>");
            builder.Append("<th>");
            builder.Append("<a class=\"schedulebutton\" id=\"scheduleWe\" data-targets=\"scheduleWe\" data-toggle=\"ON\">We</a></th>");
            builder.Append("<th>");
            builder.Append("<a class=\"schedulebutton\" id=\"scheduleTh\" data-targets=\"scheduleTh\" data-toggle=\"ON\">Th</a></th>");
            builder.Append("<th>");
            builder.Append("<a class=\"schedulebutton\" id=\"scheduleFr\" data-targets=\"scheduleFr\" data-toggle=\"ON\">Fr</a></th>");
            builder.Append("<th>");
            builder.Append("<a class=\"schedulebutton\" id=\"scheduleSa\" data-targets=\"scheduleSa\" data-toggle=\"ON\">Sa</a></th>");
            builder.Append("<th>");
            builder.Append("<a class=\"schedulebutton\" id=\"scheduleSu\" data-targets=\"scheduleSu\" data-toggle=\"ON\">Su</a></th>");
            builder.Append("</tr></thead>");
            builder.Append("<tr><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("</td>");
            builder.Append("<td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("</td>");
            builder.Append("<td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("</td>");
            builder.Append("<td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("</td>");
            builder.Append("<td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("</td>");
            builder.Append("<td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("</td>");
            builder.Append("<td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("</td>");
            builder.Append("<td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("</td>");
            builder.Append("</tr>");
            builder.Append("<tr><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<a class=\"schedulebutton\" id=\"schedule0\" data-targets=\"schedule0\" data-toggle=\"ON\">0<span class=\"tiny\">:00</span></a><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleMo schedule0 scheduleAll\" value=\"0\" name=\"timetable\" id=\"timetable_0\" ><label for=\"timetable_0\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTu schedule0 scheduleAll\" value=\"24\" name=\"timetable\" id=\"timetable_24\"  checked=\"checked\" ><label for=\"timetable_24\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleWe schedule0 scheduleAll\" value=\"48\" name=\"timetable\" id=\"timetable_48\"  checked=\"checked\" ><label for=\"timetable_48\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTh schedule0 scheduleAll\" value=\"72\" name=\"timetable\" id=\"timetable_72\"  checked=\"checked\" ><label for=\"timetable_72\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleFr schedule0 scheduleAll\" value=\"96\" name=\"timetable\" id=\"timetable_96\"  checked=\"checked\" ><label for=\"timetable_96\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSa schedule0 scheduleAll\" value=\"120\" name=\"timetable\" id=\"timetable_120\"  checked=\"checked\" ><label for=\"timetable_120\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSu schedule0 scheduleAll\" value=\"144\" name=\"timetable\" id=\"timetable_144\" ><label for=\"timetable_144\"></label></tr>");
            builder.Append("<tr><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<a class=\"schedulebutton\" id=\"schedule1\" data-targets=\"schedule1\" data-toggle=\"ON\">1<span class=\"tiny\">:00</span></a><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleMo schedule1 scheduleAll\" value=\"1\" name=\"timetable\" id=\"timetable_1\" ><label for=\"timetable_1\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTu schedule1 scheduleAll\" value=\"25\" name=\"timetable\" id=\"timetable_25\"  checked=\"checked\" ><label for=\"timetable_25\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleWe schedule1 scheduleAll\" value=\"49\" name=\"timetable\" id=\"timetable_49\"  checked=\"checked\" ><label for=\"timetable_49\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTh schedule1 scheduleAll\" value=\"73\" name=\"timetable\" id=\"timetable_73\"  checked=\"checked\" ><label for=\"timetable_73\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleFr schedule1 scheduleAll\" value=\"97\" name=\"timetable\" id=\"timetable_97\"  checked=\"checked\" ><label for=\"timetable_97\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSa schedule1 scheduleAll\" value=\"121\" name=\"timetable\" id=\"timetable_121\"  checked=\"checked\" ><label for=\"timetable_121\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSu schedule1 scheduleAll\" value=\"145\" name=\"timetable\" id=\"timetable_145\" ><label for=\"timetable_145\"></label></tr>");
            builder.Append("<tr><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<a class=\"schedulebutton\" id=\"schedule2\" data-targets=\"schedule2\" data-toggle=\"ON\">2<span class=\"tiny\">:00</span></a><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleMo schedule2 scheduleAll\" value=\"2\" name=\"timetable\" id=\"timetable_2\" ><label for=\"timetable_2\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTu schedule2 scheduleAll\" value=\"26\" name=\"timetable\" id=\"timetable_26\"  checked=\"checked\" ><label for=\"timetable_26\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleWe schedule2 scheduleAll\" value=\"50\" name=\"timetable\" id=\"timetable_50\"  checked=\"checked\" ><label for=\"timetable_50\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTh schedule2 scheduleAll\" value=\"74\" name=\"timetable\" id=\"timetable_74\"  checked=\"checked\" ><label for=\"timetable_74\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleFr schedule2 scheduleAll\" value=\"98\" name=\"timetable\" id=\"timetable_98\"  checked=\"checked\" ><label for=\"timetable_98\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSa schedule2 scheduleAll\" value=\"122\" name=\"timetable\" id=\"timetable_122\"  checked=\"checked\" ><label for=\"timetable_122\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSu schedule2 scheduleAll\" value=\"146\" name=\"timetable\" id=\"timetable_146\" ><label for=\"timetable_146\"></label></tr>");
            builder.Append("<tr><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<a class=\"schedulebutton\" id=\"schedule3\" data-targets=\"schedule3\" data-toggle=\"ON\">3<span class=\"tiny\">:00</span></a><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleMo schedule3 scheduleAll\" value=\"3\" name=\"timetable\" id=\"timetable_3\" ><label for=\"timetable_3\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTu schedule3 scheduleAll\" value=\"27\" name=\"timetable\" id=\"timetable_27\"  checked=\"checked\" ><label for=\"timetable_27\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleWe schedule3 scheduleAll\" value=\"51\" name=\"timetable\" id=\"timetable_51\"  checked=\"checked\" ><label for=\"timetable_51\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTh schedule3 scheduleAll\" value=\"75\" name=\"timetable\" id=\"timetable_75\"  checked=\"checked\" ><label for=\"timetable_75\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleFr schedule3 scheduleAll\" value=\"99\" name=\"timetable\" id=\"timetable_99\"  checked=\"checked\" ><label for=\"timetable_99\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSa schedule3 scheduleAll\" value=\"123\" name=\"timetable\" id=\"timetable_123\"  checked=\"checked\" ><label for=\"timetable_123\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSu schedule3 scheduleAll\" value=\"147\" name=\"timetable\" id=\"timetable_147\" ><label for=\"timetable_147\"></label></tr>");
            builder.Append("<tr><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<a class=\"schedulebutton\" id=\"schedule4\" data-targets=\"schedule4\" data-toggle=\"ON\">4<span class=\"tiny\">:00</span></a><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleMo schedule4 scheduleAll\" value=\"4\" name=\"timetable\" id=\"timetable_4\" ><label for=\"timetable_4\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTu schedule4 scheduleAll\" value=\"28\" name=\"timetable\" id=\"timetable_28\"  checked=\"checked\" ><label for=\"timetable_28\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleWe schedule4 scheduleAll\" value=\"52\" name=\"timetable\" id=\"timetable_52\"  checked=\"checked\" ><label for=\"timetable_52\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTh schedule4 scheduleAll\" value=\"76\" name=\"timetable\" id=\"timetable_76\"  checked=\"checked\" ><label for=\"timetable_76\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleFr schedule4 scheduleAll\" value=\"100\" name=\"timetable\" id=\"timetable_100\"  checked=\"checked\" ><label for=\"timetable_100\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSa schedule4 scheduleAll\" value=\"124\" name=\"timetable\" id=\"timetable_124\"  checked=\"checked\" ><label for=\"timetable_124\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSu schedule4 scheduleAll\" value=\"148\" name=\"timetable\" id=\"timetable_148\" ><label for=\"timetable_148\"></label></tr>");
            builder.Append("<tr><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<a class=\"schedulebutton\" id=\"schedule5\" data-targets=\"schedule5\" data-toggle=\"ON\">5<span class=\"tiny\">:00</span></a><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleMo schedule5 scheduleAll\" value=\"5\" name=\"timetable\" id=\"timetable_5\" ><label for=\"timetable_5\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTu schedule5 scheduleAll\" value=\"29\" name=\"timetable\" id=\"timetable_29\"  checked=\"checked\" ><label for=\"timetable_29\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleWe schedule5 scheduleAll\" value=\"53\" name=\"timetable\" id=\"timetable_53\"  checked=\"checked\" ><label for=\"timetable_53\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTh schedule5 scheduleAll\" value=\"77\" name=\"timetable\" id=\"timetable_77\"  checked=\"checked\" ><label for=\"timetable_77\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleFr schedule5 scheduleAll\" value=\"101\" name=\"timetable\" id=\"timetable_101\"  checked=\"checked\" ><label for=\"timetable_101\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSa schedule5 scheduleAll\" value=\"125\" name=\"timetable\" id=\"timetable_125\"  checked=\"checked\" ><label for=\"timetable_125\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSu schedule5 scheduleAll\" value=\"149\" name=\"timetable\" id=\"timetable_149\" ><label for=\"timetable_149\"></label></tr>");
            builder.Append("<tr><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<a class=\"schedulebutton\" id=\"schedule6\" data-targets=\"schedule6\" data-toggle=\"ON\">6<span class=\"tiny\">:00</span></a><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleMo schedule6 scheduleAll\" value=\"6\" name=\"timetable\" id=\"timetable_6\" ><label for=\"timetable_6\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTu schedule6 scheduleAll\" value=\"30\" name=\"timetable\" id=\"timetable_30\"  checked=\"checked\" ><label for=\"timetable_30\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleWe schedule6 scheduleAll\" value=\"54\" name=\"timetable\" id=\"timetable_54\"  checked=\"checked\" ><label for=\"timetable_54\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTh schedule6 scheduleAll\" value=\"78\" name=\"timetable\" id=\"timetable_78\"  checked=\"checked\" ><label for=\"timetable_78\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleFr schedule6 scheduleAll\" value=\"102\" name=\"timetable\" id=\"timetable_102\"  checked=\"checked\" ><label for=\"timetable_102\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSa schedule6 scheduleAll\" value=\"126\" name=\"timetable\" id=\"timetable_126\"  checked=\"checked\" ><label for=\"timetable_126\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSu schedule6 scheduleAll\" value=\"150\" name=\"timetable\" id=\"timetable_150\" ><label for=\"timetable_150\"></label></tr>");
            builder.Append("<tr><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<a class=\"schedulebutton\" id=\"schedule7\" data-targets=\"schedule7\" data-toggle=\"ON\">7<span class=\"tiny\">:00</span></a><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleMo schedule7 scheduleAll\" value=\"7\" name=\"timetable\" id=\"timetable_7\" ><label for=\"timetable_7\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTu schedule7 scheduleAll\" value=\"31\" name=\"timetable\" id=\"timetable_31\"  checked=\"checked\" ><label for=\"timetable_31\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleWe schedule7 scheduleAll\" value=\"55\" name=\"timetable\" id=\"timetable_55\"  checked=\"checked\" ><label for=\"timetable_55\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTh schedule7 scheduleAll\" value=\"79\" name=\"timetable\" id=\"timetable_79\"  checked=\"checked\" ><label for=\"timetable_79\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleFr schedule7 scheduleAll\" value=\"103\" name=\"timetable\" id=\"timetable_103\"  checked=\"checked\" ><label for=\"timetable_103\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSa schedule7 scheduleAll\" value=\"127\" name=\"timetable\" id=\"timetable_127\"  checked=\"checked\" ><label for=\"timetable_127\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSu schedule7 scheduleAll\" value=\"151\" name=\"timetable\" id=\"timetable_151\" ><label for=\"timetable_151\"></label></tr>");
            builder.Append("<tr><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<a class=\"schedulebutton\" id=\"schedule8\" data-targets=\"schedule8\" data-toggle=\"ON\">8<span class=\"tiny\">:00</span></a><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleMo schedule8 scheduleAll\" value=\"8\" name=\"timetable\" id=\"timetable_8\" ><label for=\"timetable_8\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTu schedule8 scheduleAll\" value=\"32\" name=\"timetable\" id=\"timetable_32\"  checked=\"checked\" ><label for=\"timetable_32\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleWe schedule8 scheduleAll\" value=\"56\" name=\"timetable\" id=\"timetable_56\"  checked=\"checked\" ><label for=\"timetable_56\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTh schedule8 scheduleAll\" value=\"80\" name=\"timetable\" id=\"timetable_80\"  checked=\"checked\" ><label for=\"timetable_80\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleFr schedule8 scheduleAll\" value=\"104\" name=\"timetable\" id=\"timetable_104\"  checked=\"checked\" ><label for=\"timetable_104\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSa schedule8 scheduleAll\" value=\"128\" name=\"timetable\" id=\"timetable_128\"  checked=\"checked\" ><label for=\"timetable_128\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSu schedule8 scheduleAll\" value=\"152\" name=\"timetable\" id=\"timetable_152\" ><label for=\"timetable_152\"></label></tr>");
            builder.Append("<tr><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<a class=\"schedulebutton\" id=\"schedule9\" data-targets=\"schedule9\" data-toggle=\"ON\">9<span class=\"tiny\">:00</span></a><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleMo schedule9 scheduleAll\" value=\"9\" name=\"timetable\" id=\"timetable_9\" ><label for=\"timetable_9\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTu schedule9 scheduleAll\" value=\"33\" name=\"timetable\" id=\"timetable_33\"  checked=\"checked\" ><label for=\"timetable_33\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleWe schedule9 scheduleAll\" value=\"57\" name=\"timetable\" id=\"timetable_57\"  checked=\"checked\" ><label for=\"timetable_57\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTh schedule9 scheduleAll\" value=\"81\" name=\"timetable\" id=\"timetable_81\"  checked=\"checked\" ><label for=\"timetable_81\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleFr schedule9 scheduleAll\" value=\"105\" name=\"timetable\" id=\"timetable_105\"  checked=\"checked\" ><label for=\"timetable_105\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSa schedule9 scheduleAll\" value=\"129\" name=\"timetable\" id=\"timetable_129\"  checked=\"checked\" ><label for=\"timetable_129\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSu schedule9 scheduleAll\" value=\"153\" name=\"timetable\" id=\"timetable_153\" ><label for=\"timetable_153\"></label></tr>");
            builder.Append("<tr><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<a class=\"schedulebutton\" id=\"schedule10\" data-targets=\"schedule10\" data-toggle=\"ON\">10<span class=\"tiny\">:00</span></a><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleMo schedule10 scheduleAll\" value=\"10\" name=\"timetable\" id=\"timetable_10\" ><label for=\"timetable_10\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTu schedule10 scheduleAll\" value=\"34\" name=\"timetable\" id=\"timetable_34\"  checked=\"checked\" ><label for=\"timetable_34\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleWe schedule10 scheduleAll\" value=\"58\" name=\"timetable\" id=\"timetable_58\"  checked=\"checked\" ><label for=\"timetable_58\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTh schedule10 scheduleAll\" value=\"82\" name=\"timetable\" id=\"timetable_82\"  checked=\"checked\" ><label for=\"timetable_82\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleFr schedule10 scheduleAll\" value=\"106\" name=\"timetable\" id=\"timetable_106\"  checked=\"checked\" ><label for=\"timetable_106\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSa schedule10 scheduleAll\" value=\"130\" name=\"timetable\" id=\"timetable_130\"  checked=\"checked\" ><label for=\"timetable_130\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSu schedule10 scheduleAll\" value=\"154\" name=\"timetable\" id=\"timetable_154\" ><label for=\"timetable_154\"></label></tr>");
            builder.Append("<tr><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<a class=\"schedulebutton\" id=\"schedule11\" data-targets=\"schedule11\" data-toggle=\"ON\">11<span class=\"tiny\">:00</span></a><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleMo schedule11 scheduleAll\" value=\"11\" name=\"timetable\" id=\"timetable_11\" ><label for=\"timetable_11\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTu schedule11 scheduleAll\" value=\"35\" name=\"timetable\" id=\"timetable_35\"  checked=\"checked\" ><label for=\"timetable_35\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleWe schedule11 scheduleAll\" value=\"59\" name=\"timetable\" id=\"timetable_59\"  checked=\"checked\" ><label for=\"timetable_59\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTh schedule11 scheduleAll\" value=\"83\" name=\"timetable\" id=\"timetable_83\"  checked=\"checked\" ><label for=\"timetable_83\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleFr schedule11 scheduleAll\" value=\"107\" name=\"timetable\" id=\"timetable_107\"  checked=\"checked\" ><label for=\"timetable_107\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSa schedule11 scheduleAll\" value=\"131\" name=\"timetable\" id=\"timetable_131\"  checked=\"checked\" ><label for=\"timetable_131\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSu schedule11 scheduleAll\" value=\"155\" name=\"timetable\" id=\"timetable_155\" ><label for=\"timetable_155\"></label></tr>");
            builder.Append("<tr><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<a class=\"schedulebutton\" id=\"schedule12\" data-targets=\"schedule12\" data-toggle=\"ON\">12<span class=\"tiny\">:00</span></a><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleMo schedule12 scheduleAll\" value=\"12\" name=\"timetable\" id=\"timetable_12\" ><label for=\"timetable_12\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTu schedule12 scheduleAll\" value=\"36\" name=\"timetable\" id=\"timetable_36\"  checked=\"checked\" ><label for=\"timetable_36\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleWe schedule12 scheduleAll\" value=\"60\" name=\"timetable\" id=\"timetable_60\"  checked=\"checked\" ><label for=\"timetable_60\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTh schedule12 scheduleAll\" value=\"84\" name=\"timetable\" id=\"timetable_84\"  checked=\"checked\" ><label for=\"timetable_84\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleFr schedule12 scheduleAll\" value=\"108\" name=\"timetable\" id=\"timetable_108\"  checked=\"checked\" ><label for=\"timetable_108\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSa schedule12 scheduleAll\" value=\"132\" name=\"timetable\" id=\"timetable_132\"  checked=\"checked\" ><label for=\"timetable_132\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSu schedule12 scheduleAll\" value=\"156\" name=\"timetable\" id=\"timetable_156\" ><label for=\"timetable_156\"></label></tr>");
            builder.Append("<tr><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<a class=\"schedulebutton\" id=\"schedule13\" data-targets=\"schedule13\" data-toggle=\"ON\">13<span class=\"tiny\">:00</span></a><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleMo schedule13 scheduleAll\" value=\"13\" name=\"timetable\" id=\"timetable_13\" ><label for=\"timetable_13\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTu schedule13 scheduleAll\" value=\"37\" name=\"timetable\" id=\"timetable_37\"  checked=\"checked\" ><label for=\"timetable_37\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleWe schedule13 scheduleAll\" value=\"61\" name=\"timetable\" id=\"timetable_61\"  checked=\"checked\" ><label for=\"timetable_61\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTh schedule13 scheduleAll\" value=\"85\" name=\"timetable\" id=\"timetable_85\"  checked=\"checked\" ><label for=\"timetable_85\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleFr schedule13 scheduleAll\" value=\"109\" name=\"timetable\" id=\"timetable_109\"  checked=\"checked\" ><label for=\"timetable_109\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSa schedule13 scheduleAll\" value=\"133\" name=\"timetable\" id=\"timetable_133\"  checked=\"checked\" ><label for=\"timetable_133\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSu schedule13 scheduleAll\" value=\"157\" name=\"timetable\" id=\"timetable_157\" ><label for=\"timetable_157\"></label></tr>");
            builder.Append("<tr><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<a class=\"schedulebutton\" id=\"schedule14\" data-targets=\"schedule14\" data-toggle=\"ON\">14<span class=\"tiny\">:00</span></a><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleMo schedule14 scheduleAll\" value=\"14\" name=\"timetable\" id=\"timetable_14\"  checked=\"checked\" ><label for=\"timetable_14\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTu schedule14 scheduleAll\" value=\"38\" name=\"timetable\" id=\"timetable_38\"  checked=\"checked\" ><label for=\"timetable_38\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleWe schedule14 scheduleAll\" value=\"62\" name=\"timetable\" id=\"timetable_62\"  checked=\"checked\" ><label for=\"timetable_62\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTh schedule14 scheduleAll\" value=\"86\" name=\"timetable\" id=\"timetable_86\"  checked=\"checked\" ><label for=\"timetable_86\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleFr schedule14 scheduleAll\" value=\"110\" name=\"timetable\" id=\"timetable_110\"  checked=\"checked\" ><label for=\"timetable_110\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSa schedule14 scheduleAll\" value=\"134\" name=\"timetable\" id=\"timetable_134\" ><label for=\"timetable_134\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSu schedule14 scheduleAll\" value=\"158\" name=\"timetable\" id=\"timetable_158\" ><label for=\"timetable_158\"></label></tr>");
            builder.Append("<tr><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<a class=\"schedulebutton\" id=\"schedule15\" data-targets=\"schedule15\" data-toggle=\"ON\">15<span class=\"tiny\">:00</span></a><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleMo schedule15 scheduleAll\" value=\"15\" name=\"timetable\" id=\"timetable_15\"  checked=\"checked\" ><label for=\"timetable_15\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTu schedule15 scheduleAll\" value=\"39\" name=\"timetable\" id=\"timetable_39\"  checked=\"checked\" ><label for=\"timetable_39\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleWe schedule15 scheduleAll\" value=\"63\" name=\"timetable\" id=\"timetable_63\"  checked=\"checked\" ><label for=\"timetable_63\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTh schedule15 scheduleAll\" value=\"87\" name=\"timetable\" id=\"timetable_87\"  checked=\"checked\" ><label for=\"timetable_87\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleFr schedule15 scheduleAll\" value=\"111\" name=\"timetable\" id=\"timetable_111\"  checked=\"checked\" ><label for=\"timetable_111\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSa schedule15 scheduleAll\" value=\"135\" name=\"timetable\" id=\"timetable_135\" ><label for=\"timetable_135\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSu schedule15 scheduleAll\" value=\"159\" name=\"timetable\" id=\"timetable_159\" ><label for=\"timetable_159\"></label></tr>");
            builder.Append("<tr><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<a class=\"schedulebutton\" id=\"schedule16\" data-targets=\"schedule16\" data-toggle=\"ON\">16<span class=\"tiny\">:00</span></a><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleMo schedule16 scheduleAll\" value=\"16\" name=\"timetable\" id=\"timetable_16\"  checked=\"checked\" ><label for=\"timetable_16\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTu schedule16 scheduleAll\" value=\"40\" name=\"timetable\" id=\"timetable_40\"  checked=\"checked\" ><label for=\"timetable_40\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleWe schedule16 scheduleAll\" value=\"64\" name=\"timetable\" id=\"timetable_64\"  checked=\"checked\" ><label for=\"timetable_64\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTh schedule16 scheduleAll\" value=\"88\" name=\"timetable\" id=\"timetable_88\"  checked=\"checked\" ><label for=\"timetable_88\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleFr schedule16 scheduleAll\" value=\"112\" name=\"timetable\" id=\"timetable_112\"  checked=\"checked\" ><label for=\"timetable_112\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSa schedule16 scheduleAll\" value=\"136\" name=\"timetable\" id=\"timetable_136\" ><label for=\"timetable_136\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSu schedule16 scheduleAll\" value=\"160\" name=\"timetable\" id=\"timetable_160\" ><label for=\"timetable_160\"></label></tr>");
            builder.Append("<tr><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<a class=\"schedulebutton\" id=\"schedule17\" data-targets=\"schedule17\" data-toggle=\"ON\">17<span class=\"tiny\">:00</span></a><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleMo schedule17 scheduleAll\" value=\"17\" name=\"timetable\" id=\"timetable_17\"  checked=\"checked\" ><label for=\"timetable_17\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTu schedule17 scheduleAll\" value=\"41\" name=\"timetable\" id=\"timetable_41\"  checked=\"checked\" ><label for=\"timetable_41\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleWe schedule17 scheduleAll\" value=\"65\" name=\"timetable\" id=\"timetable_65\"  checked=\"checked\" ><label for=\"timetable_65\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTh schedule17 scheduleAll\" value=\"89\" name=\"timetable\" id=\"timetable_89\"  checked=\"checked\" ><label for=\"timetable_89\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleFr schedule17 scheduleAll\" value=\"113\" name=\"timetable\" id=\"timetable_113\"  checked=\"checked\" ><label for=\"timetable_113\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSa schedule17 scheduleAll\" value=\"137\" name=\"timetable\" id=\"timetable_137\" ><label for=\"timetable_137\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSu schedule17 scheduleAll\" value=\"161\" name=\"timetable\" id=\"timetable_161\" ><label for=\"timetable_161\"></label></tr>");
            builder.Append("<tr><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<a class=\"schedulebutton\" id=\"schedule18\" data-targets=\"schedule18\" data-toggle=\"ON\">18<span class=\"tiny\">:00</span></a><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleMo schedule18 scheduleAll\" value=\"18\" name=\"timetable\" id=\"timetable_18\"  checked=\"checked\" ><label for=\"timetable_18\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTu schedule18 scheduleAll\" value=\"42\" name=\"timetable\" id=\"timetable_42\"  checked=\"checked\" ><label for=\"timetable_42\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleWe schedule18 scheduleAll\" value=\"66\" name=\"timetable\" id=\"timetable_66\"  checked=\"checked\" ><label for=\"timetable_66\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTh schedule18 scheduleAll\" value=\"90\" name=\"timetable\" id=\"timetable_90\"  checked=\"checked\" ><label for=\"timetable_90\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleFr schedule18 scheduleAll\" value=\"114\" name=\"timetable\" id=\"timetable_114\"  checked=\"checked\" ><label for=\"timetable_114\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSa schedule18 scheduleAll\" value=\"138\" name=\"timetable\" id=\"timetable_138\" ><label for=\"timetable_138\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSu schedule18 scheduleAll\" value=\"162\" name=\"timetable\" id=\"timetable_162\" ><label for=\"timetable_162\"></label></tr>");
            builder.Append("<tr><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<a class=\"schedulebutton\" id=\"schedule19\" data-targets=\"schedule19\" data-toggle=\"ON\">19<span class=\"tiny\">:00</span></a><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleMo schedule19 scheduleAll\" value=\"19\" name=\"timetable\" id=\"timetable_19\"  checked=\"checked\" ><label for=\"timetable_19\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTu schedule19 scheduleAll\" value=\"43\" name=\"timetable\" id=\"timetable_43\"  checked=\"checked\" ><label for=\"timetable_43\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleWe schedule19 scheduleAll\" value=\"67\" name=\"timetable\" id=\"timetable_67\"  checked=\"checked\" ><label for=\"timetable_67\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTh schedule19 scheduleAll\" value=\"91\" name=\"timetable\" id=\"timetable_91\"  checked=\"checked\" ><label for=\"timetable_91\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleFr schedule19 scheduleAll\" value=\"115\" name=\"timetable\" id=\"timetable_115\"  checked=\"checked\" ><label for=\"timetable_115\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSa schedule19 scheduleAll\" value=\"139\" name=\"timetable\" id=\"timetable_139\" ><label for=\"timetable_139\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSu schedule19 scheduleAll\" value=\"163\" name=\"timetable\" id=\"timetable_163\" ><label for=\"timetable_163\"></label></tr>");
            builder.Append("<tr><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<a class=\"schedulebutton\" id=\"schedule20\" data-targets=\"schedule20\" data-toggle=\"ON\">20<span class=\"tiny\">:00</span></a><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleMo schedule20 scheduleAll\" value=\"20\" name=\"timetable\" id=\"timetable_20\"  checked=\"checked\" ><label for=\"timetable_20\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTu schedule20 scheduleAll\" value=\"44\" name=\"timetable\" id=\"timetable_44\"  checked=\"checked\" ><label for=\"timetable_44\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleWe schedule20 scheduleAll\" value=\"68\" name=\"timetable\" id=\"timetable_68\"  checked=\"checked\" ><label for=\"timetable_68\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTh schedule20 scheduleAll\" value=\"92\" name=\"timetable\" id=\"timetable_92\"  checked=\"checked\" ><label for=\"timetable_92\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleFr schedule20 scheduleAll\" value=\"116\" name=\"timetable\" id=\"timetable_116\"  checked=\"checked\" ><label for=\"timetable_116\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSa schedule20 scheduleAll\" value=\"140\" name=\"timetable\" id=\"timetable_140\" ><label for=\"timetable_140\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSu schedule20 scheduleAll\" value=\"164\" name=\"timetable\" id=\"timetable_164\" ><label for=\"timetable_164\"></label></tr>");
            builder.Append("<tr><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<a class=\"schedulebutton\" id=\"schedule21\" data-targets=\"schedule21\" data-toggle=\"ON\">21<span class=\"tiny\">:00</span></a><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleMo schedule21 scheduleAll\" value=\"21\" name=\"timetable\" id=\"timetable_21\"  checked=\"checked\" ><label for=\"timetable_21\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTu schedule21 scheduleAll\" value=\"45\" name=\"timetable\" id=\"timetable_45\"  checked=\"checked\" ><label for=\"timetable_45\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleWe schedule21 scheduleAll\" value=\"69\" name=\"timetable\" id=\"timetable_69\"  checked=\"checked\" ><label for=\"timetable_69\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTh schedule21 scheduleAll\" value=\"93\" name=\"timetable\" id=\"timetable_93\"  checked=\"checked\" ><label for=\"timetable_93\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleFr schedule21 scheduleAll\" value=\"117\" name=\"timetable\" id=\"timetable_117\"  checked=\"checked\" ><label for=\"timetable_117\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSa schedule21 scheduleAll\" value=\"141\" name=\"timetable\" id=\"timetable_141\" ><label for=\"timetable_141\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSu schedule21 scheduleAll\" value=\"165\" name=\"timetable\" id=\"timetable_165\" ><label for=\"timetable_165\"></label></tr>");
            builder.Append("<tr><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<a class=\"schedulebutton\" id=\"schedule22\" data-targets=\"schedule22\" data-toggle=\"ON\">22<span class=\"tiny\">:00</span></a><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleMo schedule22 scheduleAll\" value=\"22\" name=\"timetable\" id=\"timetable_22\"  checked=\"checked\" ><label for=\"timetable_22\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTu schedule22 scheduleAll\" value=\"46\" name=\"timetable\" id=\"timetable_46\"  checked=\"checked\" ><label for=\"timetable_46\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleWe schedule22 scheduleAll\" value=\"70\" name=\"timetable\" id=\"timetable_70\"  checked=\"checked\" ><label for=\"timetable_70\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTh schedule22 scheduleAll\" value=\"94\" name=\"timetable\" id=\"timetable_94\"  checked=\"checked\" ><label for=\"timetable_94\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleFr schedule22 scheduleAll\" value=\"118\" name=\"timetable\" id=\"timetable_118\"  checked=\"checked\" ><label for=\"timetable_118\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSa schedule22 scheduleAll\" value=\"142\" name=\"timetable\" id=\"timetable_142\" ><label for=\"timetable_142\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSu schedule22 scheduleAll\" value=\"166\" name=\"timetable\" id=\"timetable_166\" ><label for=\"timetable_166\"></label></tr>");
            builder.Append("<tr><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<a class=\"schedulebutton\" id=\"schedule23\" data-targets=\"schedule23\" data-toggle=\"ON\">23<span class=\"tiny\">:00</span></a><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleMo schedule23 scheduleAll\" value=\"23\" name=\"timetable\" id=\"timetable_23\"  checked=\"checked\" ><label for=\"timetable_23\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTu schedule23 scheduleAll\" value=\"47\" name=\"timetable\" id=\"timetable_47\"  checked=\"checked\" ><label for=\"timetable_47\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleWe schedule23 scheduleAll\" value=\"71\" name=\"timetable\" id=\"timetable_71\"  checked=\"checked\" ><label for=\"timetable_71\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleTh schedule23 scheduleAll\" value=\"95\" name=\"timetable\" id=\"timetable_95\"  checked=\"checked\" ><label for=\"timetable_95\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleFr schedule23 scheduleAll\" value=\"119\" name=\"timetable\" id=\"timetable_119\"  checked=\"checked\" ><label for=\"timetable_119\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSa schedule23 scheduleAll\" value=\"143\" name=\"timetable\" id=\"timetable_143\" ><label for=\"timetable_143\"></label><td><input type=\"hidden\" name=\"timetable_\">");
            builder.Append("<input type=\"checkbox\" class=\"checkbox hidden scheduleSu schedule23 scheduleAll\" value=\"167\" name=\"timetable\" id=\"timetable_167\" ><label for=\"timetable_167\"></label></tr>");
            builder.Append("</table>");
            builder.Append("</div>");
            builder.Append("<label class=\"control-label has_help  Showfineschedmode1  Hidefineschedmode0  InitialDisplayNone  groupshowhideelement\" for=\"fineschedlist_\">Period List (inactive time slots)</label>");
            builder.Append("<div class=\"controls  Showfineschedmode1  Hidefineschedmode0  InitialDisplayNone  groupshowhideelement\" data-placement=\"top\" data-helptext=\"Define a start-end period when you want a sensor or notification to be inactive. Use the syntax ww:hh:mm-ww:hh:mm. 'ww' is a two letter shortcut for the weekdays from Monday to Sunday: mo,tu,we,th,fr,sa,su. 'hh' defines the hour (in a 24 hour format, no AM/PM allowed)and 'mm' defines the minute. For example, 'fr:17:00-mo:09:00' means a scheduled pause from Friday 17:00 (5:00 pm) to Monday 9:00 (9:00 am). Use a new line for each period you define.           <br/><br/><b>Note:</b> The schedule you define here applies to the timezone of the computer on which your PRTG core server runs, <b>not</b> to the timezone of your PRTG user account!<p><a class='morehelp nohjax' target='_blank' href='/help/schedules_settings.htm#basic'>Further Help (Manual)</a></p>\">");
            builder.Append("<textarea class=\"textarea\"  data-rule-required=\"true\" id=\"fineschedlist_\" name=\"fineschedlist_\" rows=\"2\" ></textarea></div>");
            builder.Append("<span class=\" InitialDisplayNone \"><div class=\"wiz1-3\"><input type=\"hidden\" name=\"comments\" value=\"\"></div></span>");
            builder.Append("</div></fieldset><fieldset>");
            builder.Append("<legend class=\"prtg-header\">Access Rights</legend><div class=\"control-group\">");
            builder.Append("<label class=\"control-label has_help \" for=\"accessrights_\">User Group Access</label>");
            builder.Append("<div class=\"controls \" data-placement=\"top\" data-helptext=\"Set user group access rights for this monitoring object. You cannot remove the rights defined for the parent node of this object. All the rights you define here are inherited to all child nodes of this object.<p><a class='morehelp nohjax' target='_blank' href='/help/schedules_settings.htm#accessgroup'>Further Help (Manual)</a></p>\">");
            builder.Append("<table class=\"table hoverable\" ><thead><tr><th>User Group<input type=\"hidden\" name=\"accessrights_\" value=\"1\"></th><th>Rights</th></tr></thead><tr class=\"even\"><td><div class=\"unitselect\">PRTG Users Group</div></td><td><select class=\"rightsselect\" name=\"accessrights_201\" id=\"accessrights_201\" ><option value=\"0\">None</option><option value=\"100\" selected=\"selected\" >Read</option><option value=\"200\">Write</option><option value=\"400\">Full</option></select></td></tr></table></div>");
            builder.Append("</div></fieldset>");
            builder.Append("      <input type=\"hidden\" name=\"id\" value=\"620\">");
            builder.Append("      <div style='' class='save-action quick-action-wrapper \"'>");
            builder.Append("    <button class=\"submit actionbutton quick-action-button\" type=\"submit\">Save</button>");
            builder.Append("    <button class=\"quick-action-badge\" type=\"submit\"></button>");
            builder.Append("</div>");
            builder.Append("      <div class=\"quick-links\">");
            builder.Append("        <a class=\"printbutton\" href=\"/config_report_object.htm?id=620\" target=\"_blank\" style=' display:none;'");
            builder.Append("            data-placement=\"top\" title='Prepare Configuration Report for Printout'>");
            builder.Append("          <i class=\"icon-print icon-dark\"></i>");
            builder.Append("        </a>");
            builder.Append("    </div>");
            builder.Append("</form>");

            return builder.ToString();
        }

        public override XElement GetItem(ScheduleItem item)
        {
            var xml = new XElement("item",
                new XElement("baselink", item.BaseLink),
                new XElement("baselink_raw", item.BaseLinkRaw),
                new XElement("parentid", item.ParentId),
                new XElement("type", item.Type),
                new XElement("type_raw", item.TypeRaw),
                new XElement("active", item.Active),
                new XElement("active_raw", item.ActiveRaw),
                new XElement("objid", item.ObjId),
                new XElement("name", item.Name)
            );

            return xml;
        }

        public override string GetResponseText(ref string address)
        {
            if (address.Contains(HtmlFunction.ObjectData.GetDescription()))
            {
                return GetSettingsResponseText(address);
            }

            return base.GetResponseText(ref address);
        }
    }
}
