using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.UnitTests.Support
{
    [Flags]
    public enum UrlFlag
    {
        Columns = 1,
        Count = 2
    }

    public static class TestHelpers
    {
        internal static string RequestSensorCount => RequestCount("sensors");
        internal static string RequestDeviceCount => RequestCount("devices");
        internal static string RequestGroupCount => RequestCount("groups");
        internal static string RequestProbeCount => "https://prtg.example.com/api/table.xml?content=probenode&count=0&filter_parentid=0&username=username&passhash=12345678";
        internal static string RequestLogCount => "https://prtg.example.com/api/table.xml?content=messages&count=1&columns=objid,name&username=username&passhash=12345678";

        private static string RequestCount(string content) => $"https://prtg.example.com/api/table.xml?content={content}&count=0&username=username&passhash=12345678";

        internal static string RequestSensor(string url = "", UrlFlag? flags = UrlFlag.Columns | UrlFlag.Count) =>
            RequestObject(url, flags, "sensors", DefaultSensorProperties);

        internal static string RequestDevice(string url = "", UrlFlag? flags = UrlFlag.Columns | UrlFlag.Count) =>
            RequestObject(url, flags, "devices", DefaultDeviceProperties);

        internal static string RequestGroup(string url = "", UrlFlag? flags = UrlFlag.Columns | UrlFlag.Count) =>
            RequestObject(url, flags, "groups", DefaultGroupProperties);

        internal static string RequestProbe(string url = "", UrlFlag? flags = UrlFlag.Columns | UrlFlag.Count) =>
            RequestObject(url, flags, "probenode", DefaultProbeProperties);

        internal static string RequestLog(string url = "", UrlFlag? flags = UrlFlag.Columns | UrlFlag.Count) =>
            RequestObject(url, flags, "messages", DefaultLogProperties);

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

        public static List<MethodInfo> GetTests(Type type)
        {
            return type.GetMethods().Where(m => m.GetCustomAttribute<TestMethodAttribute>() != null).ToList();
        }

        public static void Assert_TestClassHasMethods(Type testClass, List<string> expectedMethods)
        {
            var methods = GetTests(testClass);

            var missing = expectedMethods.Where(e => !methods.Any(
                    m => m.Name == e || m.Name.StartsWith($"{e}_")
                )
            ).OrderBy(m => m).ToList();

            if (missing.Count > 0)
                Assert.Fail($"{missing.Count}/{expectedMethods.Count} tests are missing: " + string.Join(", ", missing));
        }

        internal static string GetProjectRoot(bool solution = false)
        {
            var dll = new Uri(typeof(PrtgClient).Assembly.CodeBase);
            var root = dll.Host + dll.PathAndQuery + dll.Fragment;
            var rootStr = Uri.UnescapeDataString(root);

            var thisProject = Assembly.GetExecutingAssembly().GetName().Name;

            var prefix = rootStr.IndexOf(thisProject, StringComparison.InvariantCulture);

            var solutionPath = rootStr.Substring(0, prefix);

            if (solution)
                return solutionPath;

            return solutionPath + "PrtgAPI";
        }
    }
}
