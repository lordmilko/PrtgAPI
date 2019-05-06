using System.Text;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class SensorQueryTargetParametersResponse : IWebResponse
    {
        public string GetResponseText(ref string address)
        {
            var builder = new StringBuilder();

            builder.Append("<form action=\"/controls/addsensor3.htm\" method=\"POST\" class=\"prtg-form prtg-plugin\" data-plugin=\"prtg-form\">");
            builder.Append("  <fieldset>");
            builder.Append("<legend class=\"prtg-header\">Oracle Specific</legend><div class=\"control-group\">");
            builder.Append("<label class=\"control-label has_help \" for=\"database_\">Identifier</label>");
            builder.Append("<div class=\"controls \" data-placement=\"top\" data-helptext=\"Enter the Oracle SID or SERVICE_NAME of your database instance. It is defined in the 'CONNECT_DATA' part of the 'tnsnames.ora' file on the Oracle server.<p><a class='morehelp nohjax' target='_blank' href='/help/oracle_tablespace_sensor.htm#dbtablespacecredentials'>Further Help (Manual)</a></p>\">");
            builder.Append("<input class=\"text\"  type=\"text\" name=\"database_\" id=\"database_\" autocomplete=\"off\" value=\"\" ></div>");
            builder.Append("<label class=\"control-label has_help \" for=\"sid_type_\">Identification Method</label>");
            builder.Append("<div class=\"controls \" data-placement=\"top\" data-helptext=\"Choose the type of the identifier that you enter above. It depends on the configuration of your Oracle server which type of identifier you have to use.<p><a class='morehelp nohjax' target='_blank' href='/help/oracle_tablespace_sensor.htm#dbtablespacecredentials'>Further Help (Manual)</a></p>\">");
            builder.Append("<div class=\"radio-control\">");
            builder.Append("<input type=\"radio\" class=\"hidden radio \" name=\"sid_type_\" value=\"0\" checked  id=\"sid_type0\" >");
            builder.Append("<label for=\"sid_type0\" class=\"radio-control-label\">");
            builder.Append("<i class=\"icon-gray icon-radio-on\"></i>");
            builder.Append("Use SID as identifier (default)");
            builder.Append("</label>");
            builder.Append("<input type=\"radio\" class=\"hidden radio \" name=\"sid_type_\" value=\"1\" id=\"sid_type1\" >");
            builder.Append("<label for=\"sid_type1\" class=\"radio-control-label\">");
            builder.Append("<i class=\"icon-gray icon-radio-on\"></i>");
            builder.Append("Use SERVICE_NAME as identifier");
            builder.Append("</label>");
            builder.Append("</div>");
            builder.Append("</div>");
            builder.Append("<label class=\"control-label has_help \" for=\"prefix_\">Sensor Name Prefix</label>");
            builder.Append("<div class=\"controls \" data-placement=\"top\" data-helptext=\"Choose if you want to add the SERVICE_NAME to the beginning of the sensor name. This helps you to distinguish the monitored tablespaces if you have multiple databases on your machine.<p><a class='morehelp nohjax' target='_blank' href='/help/oracle_tablespace_sensor.htm#dbtablespacecredentials'>Further Help (Manual)</a></p>\">");
            builder.Append("<div class=\"radio-control\">");
            builder.Append("<input type=\"radio\" class=\"hidden radio \" name=\"prefix_\" value=\"0\" checked  id=\"prefix0\" >");
            builder.Append("<label for=\"prefix0\" class=\"radio-control-label\">");
            builder.Append("<i class=\"icon-gray icon-radio-on\"></i>");
            builder.Append("Do not use a prefix for the sensor name");
            builder.Append("</label>");
            builder.Append("<input type=\"radio\" class=\"hidden radio \" name=\"prefix_\" value=\"1\" id=\"prefix1\" >");
            builder.Append("<label for=\"prefix1\" class=\"radio-control-label\">");
            builder.Append("<i class=\"icon-gray icon-radio-on\"></i>");
            builder.Append("Use SERVICE_NAME as prefix for the sensor name");
            builder.Append("</label>");
            builder.Append("</div>");
            builder.Append("</div>");
            builder.Append("</div></fieldset><input type=\"hidden\" name=\"id\" value=\"2055\"><input type=\"hidden\" name=\"tmpid\" value=\"7\">");
            builder.Append("</form>");

            return builder.ToString();
        }
    }
}
