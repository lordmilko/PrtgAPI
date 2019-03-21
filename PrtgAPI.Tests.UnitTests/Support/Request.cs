using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Tests.UnitTests.Support
{
    [Flags]
    public enum UrlFlag
    {
        Columns = 1,
        Count = 2
    }

    public static class UnitRequest
    {
        #region Objects

        internal static string SensorCount => Count("sensors");
        internal static string Sensors(string url = "", UrlFlag? flags = UrlFlag.Columns | UrlFlag.Count) =>
            RequestObject(url, flags, "sensors", DefaultSensorProperties);

        internal static string DeviceCount => Count("devices");
        internal static string Devices(string url = "", UrlFlag? flags = UrlFlag.Columns | UrlFlag.Count) =>
            RequestObject(url, flags, "devices", DefaultDeviceProperties);

        internal static string GroupCount => Count("groups");
        internal static string Groups(string url = "", UrlFlag? flags = UrlFlag.Columns | UrlFlag.Count) =>
            RequestObject(url, flags, "groups", DefaultGroupProperties);

        internal static string ProbeCount => Get("api/table.xml?content=probenode&count=0&filter_parentid=0");
        internal static string Probes(string url = "", UrlFlag? flags = UrlFlag.Columns | UrlFlag.Count) =>
            RequestObject(url, flags, "probenode", DefaultProbeProperties);

        internal static string LogCount => Get("api/table.xml?content=messages&count=1&columns=objid,name");
        internal static string Logs(string url = "", UrlFlag? flags = UrlFlag.Columns | UrlFlag.Count) =>
            RequestObject(url, flags, "messages", DefaultLogProperties);

        internal static string Channels(int sensorId) =>
            RequestObject($"count=*&id={sensorId}", UrlFlag.Columns, "channels", DefaultChannelProperties);
        internal static string ChannelProperties(int sensorId, int channelId) =>
            Get($"controls/channeledit.htm?id={sensorId}&channel={channelId}");

        public static string Status() =>
            Get("api/getstatus.htm?id=0");

        private static string Count(string content) => Get($"api/table.xml?content={content}&count=0");

        static string RequestObject(string url, UrlFlag? flags, string content, Func<string> defaultProperties)
        {
            Func<string, string> getDelim = f => f != string.Empty ? "&" : "";

            var builder = new StringBuilder();

            builder.Append($"content={content}");

            if ((flags & UrlFlag.Columns) == UrlFlag.Columns)
                builder.Append($"&columns={defaultProperties()}");

            if ((flags & UrlFlag.Count) == UrlFlag.Count)
                builder.Append("&count=500");

            builder.Append($"{getDelim(url)}{url}");

            return $"https://prtg.example.com/api/table.xml?{builder}&username=username&passhash=12345678";
        }

        #region Columns

        internal static string DefaultSensorProperties()
        {
            return "objid,name,probe,group,favorite,lastvalue,device,downtime,downtimetime,downtimesince,uptime,uptimetime,uptimesince,knowntime,cumsince,lastcheck,lastup,lastdown,minigraph,schedule,basetype,baselink,parentid,notifiesx,intervalx,access,dependency,position,status,comments,priority,message,tags,type,active";
        }

        internal static string DefaultDeviceProperties()
        {
            return "objid,name,location,host,group,probe,favorite,condition,upsens,downsens,downacksens,partialdownsens,warnsens,pausedsens,unusualsens,undefinedsens,totalsens,schedule,basetype,baselink,parentid,notifiesx,intervalx,access,dependency,position,status,comments,priority,message,tags,type,active";
        }

        internal static string DefaultGroupProperties()
        {
            return "objid,name,probe,condition,fold,groupnum,devicenum,upsens,downsens,downacksens,partialdownsens,warnsens,pausedsens,unusualsens,undefinedsens,totalsens,schedule,basetype,baselink,parentid,notifiesx,intervalx,access,dependency,position,status,comments,priority,message,tags,type,active";
        }

        internal static string DefaultProbeProperties()
        {
            return "objid,name,condition,fold,groupnum,devicenum,upsens,downsens,downacksens,partialdownsens,warnsens,pausedsens,unusualsens,undefinedsens,totalsens,schedule,basetype,baselink,parentid,notifiesx,intervalx,access,dependency,position,status,comments,priority,message,tags,type,active";
        }

        internal static string DefaultLogProperties()
        {
            return "objid,name,datetime,parent,status,sensor,device,group,probe,message,priority,type,tags,active";
        }

        internal static string DefaultChannelProperties()
        {
            return "objid,name,lastvalue";
        }

        #endregion
        #endregion

        public static string AddSensor(string url) =>
            Get($"addsensor5.htm?{url}");

        public static string BeginAddSensorQuery(int deviceId, string sensorType) =>
            Get($"controls/addsensor2.htm?id={deviceId}&sensortype={sensorType}");

        public static string AddSensorProgress(int deviceId, int tmpId, bool step = false) =>
            $"https://prtg.example.com/api/getaddsensorprogress.htm?id={deviceId}&tmpid={tmpId}{(step ? "&step=3" : string.Empty)}";

        public static string EndAddSensorQuery(int deviceId, int tmpId) =>
            $"https://prtg.example.com/addsensor4.htm?id={deviceId}&tmpid={tmpId}";

        internal static string SetChannelProperty(string url) =>
            Get($"editsettings?{url}");

        internal static string Get(string url) => $"https://prtg.example.com/{url}&username=username&passhash=12345678";
    }
}
