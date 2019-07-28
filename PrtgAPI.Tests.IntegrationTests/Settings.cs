using System;

namespace PrtgAPI.Tests.IntegrationTests
{
    public enum HttpProtocol
    {
        HTTP,
        HTTPS
    }

    /* Store settings in Settings.Local.cs to avoid syncing with GitHub
     * 
     * public static partial class Settings
     * {
     *     static Settings()
     *     {
     *         Server = "prtg.example.com";
     *         UserName = "prtgadmin";
     *         
     *         //etc
     *     }
     * }
     */
    public static partial class Settings
    {
#pragma warning disable CS0649 //Ignore 'field' is never assigned to.
        public static string ServerWithProto { get { return "http://" + Server; } } //C# 5 syntax required by New-Settings.ps1
        public static bool ResetAfterTests = true;

        //PRTG Server
        public static string Server = null; //Must not support HTTPS
        public static string UserName = null;
        public static string Password = null;

        public static string ReadOnlyUserName = null;
        public static string ReadOnlyPassword = null;

        //Local Server
        public static string WindowsUserName;
        public static string WindowsPassword;

        //Objects
        public static int Probe = -1;
        public static int Group = -1;
        public static int Device = -1;
        public static int Channel = -1;

        //Channel
        public static string ChannelName;

        //Device
        public static string DeviceName; //Must contain "prtg" in the name
        public static string DeviceTag;

        //Group
        public static string GroupName;
        public static string GroupTag;

        //Probe
        public static string ProbeName;
        public static string ProbeTag;

        //Sensor Types/States
        public static int UpSensor = -1;
        public static int WarningSensor = -1;
        public static int DownSensor = -1;
        public static int DownAcknowledgedSensor = -1;
        public static int PausedSensor = -1;
        public static int PausedByDependencySensor = -1;
        public static int UnknownSensor = -1; //NetFlow sensors work best
        public static int ChannelSensor = -1;

        //Channel Limits
        public static int ChannelErrorLimit = -1;   //Max: positive of value. Min: negative of value
        public static int ChannelWarningLimit = -1; //Max: positive of value. Min: negative of value
        public static string ChannelErrorMessage;
        public static string ChannelWarningMessage;

        //Notification Actions
        public static int NotificationAction;
        public static string NotificationActionName;
        public static string NotificationActionTag1;
        public static string NotificationActionTag2;

        //Schedules
        public static int Schedule = -1;

        //Object Counts

        public static int ProbesInTestServer = -1;

        public static int GroupsInTestGroup = -1;
        public static int GroupsInTestProbe = -1;
        public static int GroupsInTestServer = -1;

        public static int DevicesInTestGroup = -1;
        public static int DevicesInTestProbe = -1;
        public static int DevicesInTestServer = -1;

        public static int SensorsInTestDevice = -1;
        public static int SensorsInTestGroup = -1;
        public static int SensorsInTestProbe = -1;
        public static int SensorsInTestServer = -1;

        public static int ChannelsInTestSensor = -1;

        public static int NotificationTiggersOnDevice = -1;
        public static int NotificationActionsInTestServer = -1;

        public static int SchedulesInTestServer = -1;

        //Settings

        public static string[] ParentTags;
        public static TimeSpan? CustomInterval;
        public static TimeSpan? CustomUnsupportedInterval;
        public static DateTime? MaintenanceStart;
        public static DateTime? MaintenanceEnd;
        public static string Comment;
        public static int CommentSensor = -1;
        public static int FavoriteDevice = -1;
        public static int FavoriteSensor = -1;
        public static string Location;

        //Sensor Types
        public static int WmiRemotePing = -1;
        public static int ExeXml = -1;
        public static int SNMP = -1;
        public static int SSLSecurityCheck = -1;
        public static int SensorFactory = -1;
        public static int WmiService = -1;
        public static int SqlServerDB = -1;

#pragma warning restore CS0649 //Restore 'field' is never assigned to.
    }
}
