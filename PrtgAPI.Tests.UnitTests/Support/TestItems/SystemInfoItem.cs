using System.Linq;
using System.Text;

namespace PrtgAPI.Tests.UnitTests.Support.TestItems
{
    public class SystemInfoItem
    {
        public SystemInfoType Type { get; set; }

        public string Content { get; set; }

        public static SystemInfoItem SystemItem(string key = "\"IPAddress / vmxnet3 Ethernet Adapter\"",
            string value = "\"fdd6:8a79:a465:d577:1c60:344f:6714:d010\"", string id = "2", string adapter = "\"vmxnet3 Ethernet Adapter\"",
            string receiveTime = "19-10-2018 23:19:40.683", string displayName = "\"IPAddress#2: fdd6:8a79:a465:d577:1c60:344f:6714:d010\"")
        {
            var content = MakeObject(key, value, id, adapter, receiveTime, displayName);

            return new SystemInfoItem
            {
                Type = SystemInfoType.System,
                Content = content
            };
        }

        public static SystemInfoItem HardwareItem(string name = "\"\\\\\\\\.\\\\PHYSICALDRIVE0\"", string description = "\"Disk drive\"",
            string @class = "\"DiskDrive\"", string caption = "\"VMware Virtual disk SCSI Disk Device\"",
            string state = "\"<img src=\\\"/images/state_ok.png\\\"> OK\"",
            string serialNumber = "\"\"", string capacity = "\"\"", string receiveTime = "19-10-2018 23:19:40.683", string displayName = "\"\\\\\\\\.\\\\PHYSICALDRIVE0 (Disk drive)\"")
        {
            var content = MakeObject(name, description, @class, caption, state, serialNumber, capacity, receiveTime, displayName);

            return new SystemInfoItem
            {
                Type = SystemInfoType.Hardware,
                Content = content
            };
        }

        public static SystemInfoItem SoftwareItem(string name = "\"Configuration Manager Client\"", string vendor = "\"Microsoft Corporation\"",
            string version = "\"5.00.8498.1000\"", string date = "\"2017-05-23-00-00-00\"", string size = "38048",
            string receiveTime = "19-10-2018 23:19:40.683", string displayName = "\"Microsoft Corporation Configuration Manager Client\"")
        {
            var content = MakeObject(name, vendor, version, date, size, receiveTime, displayName);

            return new SystemInfoItem
            {
                Type = SystemInfoType.Software,
                Content = content
            };
        }

        public static SystemInfoItem ProcessItem(string processId = "1044", string caption = "\"WmiPrvSE.exe\"",
            string creationDate = "\"2018-08-31 19:36:26\"", string receiveTime = "19-10-2018 23:19:40.683", string displayName = "\"WmiPrvSE.exe\"")
        {
            var content = MakeObject(processId, caption, creationDate, receiveTime, displayName);

            return new SystemInfoItem
            {
                Type = SystemInfoType.Processes,
                Content = content
            };
        }

        public static SystemInfoItem ServiceItem(string name = "\"wuauserv\"", string description = "\"Enables the detection, download, and installation of updates for Windows and other programs. If this service is disabled, users of this computer will not be able to use Windows Update or its automatic updating feature, and programs will not be able to use the Windows Update Agent (WUA) API.\"",
            string startName = "\"LocalSystem\"", string startMode = "\"Manual\"", string state = "\"<img src=\\\"/images/state_stopped.png\\\"> Stopped\"",
            string receiveTime = "19-10-2018 23:19:40.683", string displayName = "\"wuauserv\"")
        {
            var content = MakeObject(name, description, startName, startMode, state, receiveTime, displayName);

            var builder = new StringBuilder();

            return new SystemInfoItem
            {
                Type = SystemInfoType.Services,
                Content = content
            };
        }

        public static SystemInfoItem UserItem(string domain = "\"PRTG-1\"", string user = "\"NETWORK SERVICE\"", string receiveTime = "19-10-2018 23:19:40.683", string displayName = "\"PRTG-1\\\\NETWORK SERVICE\"")
        {
            var content = MakeObject(domain, user, receiveTime, displayName);

            return new SystemInfoItem
            {
                Type = SystemInfoType.Users,
                Content = content
            };
        }

        private static string MakeObject(params string[] properties)
        {
            var pairs = properties.Where(p => p != null).Select(p => $"\"\":{p},\"_raw\":{p}");

            var joined = string.Join(",", pairs);

            var builder = new StringBuilder();
            builder.Append("{").Append(joined).Append("}");
            return builder.ToString();
        }
    }
}
