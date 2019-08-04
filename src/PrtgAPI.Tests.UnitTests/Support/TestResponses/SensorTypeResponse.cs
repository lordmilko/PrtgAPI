using System.Linq;
using System.Text;
using PrtgAPI.Utilities;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
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
            builder.Append("      },");
            builder.Append("      {  ");
            builder.Append("         \"id\":\"snmplibrary\",");
            builder.Append("         \"name\":\"SNMP Library\",");
            builder.Append("         \"family\":[  ");
            builder.Append("            \"LAN\",");
            builder.Append("            \"Hardware Parameters\",");
            builder.Append("            \"SNMP\",");
            builder.Append("            \"Windows\",");
            builder.Append("            \"Linux/MacOS\",");
            builder.Append("            \"Custom Sensors\"");
            builder.Append("         ],");
            builder.Append("         \"description\":\"Monitors a device using SNMP and compiled MIB files (\\\\'\\\"SNMP Libraries(oidlib)\\\\'\\\")\",");
            builder.Append("         \"help\":\"Monitors Cisco interfaces and queue, Dell systems and storages, APC UPS (battery ems status), Linux (AX BGP DisMan EtherLike Host Framework Proxy Noti v2 IP Net Noti OSPF RMON SMUX Source TCP UCD UDP), etc. as well as any other SNMP devices using your imported MIB files.\",");
            builder.Append("         \"manuallink\":\"/help/snmp_library_sensor.htm\",");
            builder.Append("         \"needsprobe\":true,");
            builder.Append("         \"needslocalprobe\":false,");
            builder.Append("         \"needsprobedevice\":false,");
            builder.Append("         \"needsvm\":false,");
            builder.Append("         \"needslinux\":false,");
            builder.Append("         \"needswindows\":false,");
            builder.Append("         \"dotnetversion\":-1,");
            builder.Append("         \"notincluster\":false,");
            builder.Append("         \"notonpod\":false,");
            builder.Append("         \"ipv6\":true,");
            builder.Append("         \"top10\":0,");
            builder.Append("         \"message\":\"\",");
            builder.Append("         \"resourceusage\":1,");
            builder.Append("         \"linux\":true,");
            builder.Append("         \"metascan\":true,");
            builder.Append("         \"templatesupport\":true,");
            builder.Append("         \"preselection\":true,");
            builder.Append("         \"preselectionlist\":\"%3Cselect%20class%3D%22combo%22%20%20name%3D%22preselection_snmplibrary%22%20id%3D%22preselection_snmplibrary%22%20%3E%3Coption%20value%3D%22%22%20selected%3D%22selected%22%20%3E%26lt%3Bplease%20select%20a%20file%26gt%3B%3C%2Foption%3E%3Coption%20value%3D%22APC%20UPS.oidlib%22%3EApc%20ups.oidlib%3C%2Foption%3E%3Coption%20value%3D%22APCSensorstationlib.oidlib%22%3EApcsensorstationlib.oidlib%3C%2Foption%3E%3Coption%20value%3D%22Basic%20Linux%20Library%20%28UCD-SNMP-MIB%29.oidlib%22%3EBasic%20linux%20library%20%28ucd-snmp-mib%29.oidlib%3C%2Foption%3E%3Coption%20value%3D%22cisco-interfaces.oidlib%22%3ECisco-interfaces.oidlib%3C%2Foption%3E%3Coption%20value%3D%22cisco-queue.oidlib%22%3ECisco-queue.oidlib%3C%2Foption%3E%3Coption%20value%3D%22Dell%20Storage%20Management.oidlib%22%3EDell%20storage%20management.oidlib%3C%2Foption%3E%3Coption%20value%3D%22Dell%20Systems%20Management%20Instrumentation.oidlib%22%3EDell%20systems%20management%20instrumentation.oidlib%3C%2Foption%3E%3Coption%20value%3D%22HP%20Laserjet%20Status.oidlib%22%3EHp%20laserjet%20status.oidlib%3C%2Foption%3E%3Coption%20value%3D%22Linux%20SNMP%20%28AX%20BGP%20DisMan%20EtherLike%20Host%29.oidlib%22%3ELinux%20snmp%20%28ax%20bgp%20disman%20etherlike%20host%29.oidlib%3C%2Foption%3E%3Coption%20value%3D%22Linux%20SNMP%20%28Framework%20Proxy%20Noti%20v2%29.oidlib%22%3ELinux%20snmp%20%28framework%20proxy%20noti%20v2%29.oidlib%3C%2Foption%3E%3Coption%20value%3D%22Linux%20SNMP%20%28IP%20Net%20SNMP%20Noti%20OSPF%20RMON%20SMUX%29.oidlib%22%3ELinux%20snmp%20%28ip%20net%20snmp%20noti%20ospf%20rmon%20smux%29.oidlib%3C%2Foption%3E%3Coption%20value%3D%22Linux%20SNMP%20%28Source%20TCP%20UCD%20UDP%29.oidlib%22%3ELinux%20snmp%20%28source%20tcp%20ucd%20udp%29.oidlib%3C%2Foption%3E%3Coption%20value%3D%22Paessler%20Common%20OID%20Library.oidlib%22%3EPaessler%20common%20oid%20library.oidlib%3C%2Foption%3E%3Coption%20value%3D%22PAN.oidlib%22%3EPan.oidlib%3C%2Foption%3E%3Coption%20value%3D%22SNMP%20Informant%20Std.oidlib%22%3ESnmp%20informant%20std.oidlib%3C%2Foption%3E%3C%2Fselect%3E\"");
            builder.Append("      }");

            builder.Append(AddSupported());

            builder.Append("   ]");
            builder.Append("}");

            return builder.ToString();
        }

        private string AddSupported()
        {
            var types = typeof(SensorType).GetEnumValues().Cast<SensorType>().Select(v => v.EnumToXml());

            var builder = new StringBuilder();

            foreach (var type in types)
            {
                builder.Append(",");
                builder.Append("      {  ");
                builder.Append($"         \"id\":\"{type}\",");
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
            }

            return builder.ToString();
        }
    }
}
