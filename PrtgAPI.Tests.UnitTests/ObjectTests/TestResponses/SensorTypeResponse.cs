using System.Text;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses
{
    public class SensorTypeResponse : IWebResponse
    {
        public string GetResponseText(ref string address)
        {
            var builder = new StringBuilder();

            builder.Append("{  ");
            builder.Append("   \"sensortypes\":[  ");
            builder.Append("      {  ");
            builder.Append("         \"id\":\"ptfadsreplfailurexml\",");
            builder.Append("         \"name\":\"Active Directory Replication Errors\",");
            builder.Append("         \"family\":[  ");
            builder.Append("            \"Availability/Uptime\",");
            builder.Append("            \"Network Infrastructure\",");
            builder.Append("            \"WMI\",");
            builder.Append("            \"Windows\"");
            builder.Append("         ],");
            builder.Append("         \"description\":\"Checks Windows domain controllers for replication errors\",");
            builder.Append("         \"help\":\"Needs .NET 4.0 installed on the computer running the PRTG probe and valid credentials for Windows systems defined in the parent device or group settings.\",");
            builder.Append("         \"manuallink\":\"/help/active_directory_replication_errors_sensor.htm\",");
            builder.Append("         \"needsprobe\":true,");
            builder.Append("         \"needslocalprobe\":false,");
            builder.Append("         \"needsprobedevice\":false,");
            builder.Append("         \"needsvm\":false,");
            builder.Append("         \"needslinux\":false,");
            builder.Append("         \"needswindows\":true,");
            builder.Append("         \"dotnetversion\":40,");
            builder.Append("         \"notincluster\":false,");
            builder.Append("         \"notonpod\":true,");
            builder.Append("         \"ipv6\":true,");
            builder.Append("         \"top10\":0,");
            builder.Append("         \"message\":\"\",");
            builder.Append("         \"resourceusage\":4,");
            builder.Append("         \"linux\":false,");
            builder.Append("         \"metascan\":true,");
            builder.Append("         \"templatesupport\":true,");
            builder.Append("         \"preselection\":false");
            builder.Append("      },");
            builder.Append("      {  ");
            builder.Append("         \"id\":\"adosqlv2\",");
            builder.Append("         \"name\":\"ADO SQL v2\",");
            builder.Append("         \"family\":[  ");
            builder.Append("            \"LAN\",");
            builder.Append("            \"Availability/Uptime\",");
            builder.Append("            \"Database\",");
            builder.Append("            \"Windows\"");
            builder.Append("         ],");
            builder.Append("         \"description\":\"Monitors any data source that is available via OLE DB or ODBC\",");
            builder.Append("         \"help\":\"Needs .NET 4.0 installed on the computer running the PRTG probe.\",");
            builder.Append("         \"manuallink\":\"/help/ado_sql_v2_sensor.htm\",");
            builder.Append("         \"needsprobe\":true,");
            builder.Append("         \"needslocalprobe\":false,");
            builder.Append("         \"needsprobedevice\":false,");
            builder.Append("         \"needsvm\":false,");
            builder.Append("         \"needslinux\":false,");
            builder.Append("         \"needswindows\":false,");
            builder.Append("         \"dotnetversion\":40,");
            builder.Append("         \"notincluster\":false,");
            builder.Append("         \"notonpod\":true,");
            builder.Append("         \"ipv6\":true,");
            builder.Append("         \"top10\":0,");
            builder.Append("         \"message\":\"\",");
            builder.Append("         \"resourceusage\":4,");
            builder.Append("         \"linux\":false,");
            builder.Append("         \"metascan\":true,");
            builder.Append("         \"templatesupport\":true,");
            builder.Append("         \"preselection\":false");
            builder.Append("      },");
            builder.Append("      {  ");
            builder.Append("         \"id\":\"cloudwatchalarm\",");
            builder.Append("         \"name\":\"Amazon CloudWatch Alarm BETA\",");
            builder.Append("         \"family\":[  ");
            builder.Append("            \"WAN\",");
            builder.Append("            \"Bandwidth/Traffic\",");
            builder.Append("            \"Speed/Performance\",");
            builder.Append("            \"Disk Usage\",");
            builder.Append("            \"CPU Usage\",");
            builder.Append("            \"TCP\",");
            builder.Append("            \"Cloud Services\"");
            builder.Append("         ],");
            builder.Append("         \"description\":\"Monitors the status of an Amazon CloudWatch alarm by reading its data from the AWS CloudWatch API\",");
            builder.Append("         \"help\":\"Provide your credentials for Amazon CloudWatch in the settings of the parent device. You need .NET 4.5 installed on the computer running the PRTG probe.\",");
            builder.Append("         \"manuallink\":\"/help/amazon_cloudwatch_alarm_sensor.htm\",");
            builder.Append("         \"needsprobe\":true,");
            builder.Append("         \"needslocalprobe\":false,");
            builder.Append("         \"needsprobedevice\":false,");
            builder.Append("         \"needsvm\":false,");
            builder.Append("         \"needslinux\":false,");
            builder.Append("         \"needswindows\":false,");
            builder.Append("         \"dotnetversion\":45,");
            builder.Append("         \"notincluster\":false,");
            builder.Append("         \"notonpod\":false,");
            builder.Append("         \"ipv6\":true,");
            builder.Append("         \"top10\":0,");
            builder.Append("         \"message\":\"\",");
            builder.Append("         \"resourceusage\":3,");
            builder.Append("         \"linux\":false,");
            builder.Append("         \"metascan\":true,");
            builder.Append("         \"templatesupport\":true,");
            builder.Append("         \"preselection\":false");
            builder.Append("      }");
            builder.Append("   ]");
            builder.Append("}");

            return builder.ToString();
        }
    }
}
