using System;
using System.Text;

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

        public const UrlFlag DefaultObjectFlags = UrlFlag.Columns | UrlFlag.Count;

        public static string SensorCount => Count("sensors");
        public static string Sensors(string url = "", UrlFlag? flags = DefaultObjectFlags) =>
            RequestObject(url, flags, "sensors", DefaultSensorProperties);

        public static string DeviceCount => Count("devices");
        public static string Devices(string url = "", UrlFlag? flags = DefaultObjectFlags) =>
            RequestObject(url, flags, "devices", DefaultDeviceProperties);

        public static string DeviceProperties(int id) => RequestObjectData(id, "device");

        public static string GroupCount => Count("groups");
        public static string Groups(string url = "", UrlFlag? flags = DefaultObjectFlags) =>
            RequestObject(url, flags, "groups", DefaultGroupProperties);

        public static string ProbeCount => Get("api/table.xml?content=probenode&count=0&filter_parentid=0");
        public static string Probes(string url = "", UrlFlag? flags = DefaultObjectFlags) =>
            RequestObject(url, flags, "probenode", DefaultProbeProperties);

        public static string LogCount => Get("api/table.xml?content=messages&count=1&columns=objid,name");
        public static string Logs(string url = "", UrlFlag? flags = DefaultObjectFlags) =>
            RequestObject(url, flags, "messages", DefaultLogProperties);

        public static string Channels(int sensorId) =>
            RequestObject($"id={sensorId}", DefaultObjectFlags, "channels", DefaultChannelProperties);
        public static string ChannelProperties(int sensorId, int channelId) =>
            Get($"controls/channeledit.htm?id={sensorId}&channel={channelId}");

        public static string Notifications(string url = "", UrlFlag? flags = DefaultObjectFlags) =>
            RequestObject(url, flags, "notifications", DefaultNotificationProperties);

        public static string NotificationProperties(int id) => RequestObjectData(id, "notification");

        public static string Objects(string url = "", UrlFlag? flags = DefaultObjectFlags) =>
            RequestObject(url, flags, "objects", DefaultObjectProperties);

        public static string Schedules(string url = "", UrlFlag? flags = DefaultObjectFlags) =>
            RequestObject(url, flags, "schedules", DefaultScheduleProperties);

        public static string ScheduleProperties(int id) => RequestObjectData(id, "schedule");

        public static string Triggers(int id) =>
            Get($"api/table.xml?id={id}&content=triggers&columns=content,objid");

        public static string TriggerTypes(int objectId) =>
            Get($"api/triggers.json?id={objectId}");

        public static string SensorTypes(int id) =>
            Get($"api/sensortypes.json?id={id}");

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
                builder.Append("&count=*");

            builder.Append($"{getDelim(url)}{url}");

            return $"https://prtg.example.com/api/table.xml?{builder}&username=username&passhash=12345678";
        }

        static string RequestObjectData(int id, string objectType) =>
            Get($"controls/objectdata.htm?id={id}&objecttype={objectType}");

        public static string RequestObjectData(int id) =>
            Get($"controls/objectdata.htm?id={id}");

        #region Columns

        internal static string DefaultSensorProperties()
        {
            return "objid,name,probe,group,favorite,lastvalue,device,downtime,downtimetime,downtimesince,uptime,uptimetime,uptimesince,knowntime,cumsince,lastcheck,lastup,lastdown,minigraph,schedule,basetype,baselink,notifiesx,intervalx,access,dependency,position,status,comments,priority,message,parentid,tags,type,active";
        }

        internal static string DefaultDeviceProperties()
        {
            return "objid,name,location,host,group,probe,favorite,condition,upsens,downsens,downacksens,partialdownsens,warnsens,pausedsens,unusualsens,undefinedsens,totalsens,schedule,basetype,baselink,notifiesx,intervalx,access,dependency,position,status,comments,priority,message,parentid,tags,type,active";
        }

        internal static string DefaultGroupProperties()
        {
            return "objid,name,probe,condition,fold,groupnum,devicenum,upsens,downsens,downacksens,partialdownsens,warnsens,pausedsens,unusualsens,undefinedsens,totalsens,schedule,basetype,baselink,notifiesx,intervalx,access,dependency,position,status,comments,priority,message,parentid,tags,type,active";
        }

        internal static string DefaultProbeProperties()
        {
            return "objid,name,condition,fold,groupnum,devicenum,upsens,downsens,downacksens,partialdownsens,warnsens,pausedsens,unusualsens,undefinedsens,totalsens,schedule,basetype,baselink,notifiesx,intervalx,access,dependency,position,status,comments,priority,message,parentid,tags,type,active";
        }

        internal static string DefaultLogProperties()
        {
            return "objid,name,datetime,parent,status,sensor,device,group,probe,message,priority,type,tags,active";
        }

        internal static string DefaultObjectProperties()
        {
            return "objid,name,parentid,tags,type,active,basetype";
        }

        internal static string DefaultChannelProperties()
        {
            return "objid,name,lastvalue";
        }

        private static string DefaultNotificationProperties()
        {
            return "objid,name,baselink,parentid,tags,type,active,basetype";
        }

        private static string DefaultScheduleProperties()
        {
            return "objid,name,baselink,parentid,tags,type,active,basetype";
        }

        #endregion
        #endregion

        public static string GetObjectProperty(int id, string property) =>
            Get($"api/getobjectproperty.htm?id={id}&name={property}");

        public static string AddSensor(string url) =>
            Get($"addsensor5.htm?{url}");

        public static string BeginAddSensorQuery(int deviceId, string sensorType, string preselection = null) =>
            Get($"controls/addsensor2.htm?id={deviceId}{(preselection != null ? "&preselection_" + sensorType + "=" + preselection : "")}&sensortype={sensorType}");

        public static string ContinueAddSensorQuery(int deviceId, int tmpId, string parameters) =>
            GetWithCookie($"controls/addsensor3.htm?id={deviceId}&tmpid={tmpId}&{parameters}");
            

        public static string AddSensorProgress(int deviceId, int tmpId, bool step = false) =>
            GetWithCookie($"api/getaddsensorprogress.htm?id={deviceId}&tmpid={tmpId}{(step ? "&step=3" : string.Empty)}");

        public static string EndAddSensorQuery(int deviceId, int tmpId) =>
            GetWithCookie($"addsensor4.htm?id={deviceId}&tmpid={tmpId}");

        public static string EditSettings(string url) =>
            Get($"editsettings?{url}");

        public static string Get(string url) => $"https://prtg.example.com/{url}{(url.EndsWith("?") ? "" : "&")}username=username&passhash=12345678";

        public static string GetWithCookie(string url) => $"https://prtg.example.com/{url}";
    }
}
