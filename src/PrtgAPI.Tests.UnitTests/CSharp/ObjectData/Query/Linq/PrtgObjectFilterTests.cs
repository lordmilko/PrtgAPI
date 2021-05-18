using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Attributes;
using PrtgAPI.Request.Serialization;
using PrtgAPI.Request.Serialization.ValueConverters;
using PrtgAPI.Utilities;
using PrtgAPI.Tests.UnitTests.Support;

namespace PrtgAPI.Tests.UnitTests.ObjectData.Query
{
    static class Time
    {
        private static DateTime baseDate = new DateTime(2018, 5, 16, 11, 20, 35, DateTimeKind.Utc);

        public static DateTime Today => baseDate;
        public static string TodayStr => "2018-05-16-11-20-35";

        public static DateTime Yesterday => baseDate.AddDays(-1);
        public static string YesterdayStr => "2018-05-15-11-20-35";

        public static DateTime TwoDaysAgo => baseDate.AddDays(-2);
        public static string TwoDaysAgoStr => "2018-05-14-11-20-35";

        public static DateTime LastWeek => baseDate.AddDays(-7);
        public static string LastWeekStr => "2018-05-09-11-20-35";

        public static DateTime TwoWeeksAgo => baseDate.AddDays(-14);
        public static string TwoWeeksAgoStr => "2018-05-02-11-20-35";

        private static string GetTime(DateTime dateTime)
        {
            return TypeHelpers.DateToString(dateTime);
        }
    }

    [TestClass]
    public class PrtgObjectFilterTests : BaseQueryTests
    {
        #region PrtgObject

        [UnitTest]
        [TestMethod]
        public void QueryFilter_PrtgObjectProperties_Name() => QuerySensor(s => s.Name == "ping", "filter_name=ping");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_PrtgObjectProperties_Id() => QuerySensor(s => s.Id == 1001, "filter_objid=1001");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_PrtgObjectProperties_ParentId() => QuerySensor(s => s.ParentId == 3, "filter_parentid=3");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_PrtgObjectProperties_Active() => QuerySensor(s => s.Active, "filter_active=-1");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_PrtgObjectProperties_Type() => QuerySensor(s => s.Type == "exexml", "filter_type=exexml");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_PrtgObjectProperties_Type_WithStringEnum() => QuerySensor(s => s.Type == new StringEnum<ObjectType>(ObjectType.Probe), "filter_type=probenode");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_PrtgObjectProperties_Type_WithSensor() => QuerySensor(s => s.Type == ObjectType.Sensor, string.Empty);

        [UnitTest]
        [TestMethod]
        public void QueryFilter_PrtgObjectProperties_Tags_Index() => QuerySensor(s => s.Tags[0] == "wmivolumesensor", "filter_tags=wmivolumesensor");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_PrtgObjectProperties_Tags_Contains() => QuerySensor(s => s.Tags.Contains("test"), "filter_tags=@sub(test)");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_PrtgObjectProperties_DisplayType() => QuerySensor(s => s.DisplayType == "EXE/Script Advanced", string.Empty); //to filter or not to filter

        #endregion
        #region SensorOrDeviceOrGroupOrProbe

        [UnitTest]
        [TestMethod]
        public void QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Access() => QuerySensor(s => s.Access == Access.Full, "filter_access=0000000400");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_BaseType() => QuerySensor(s => s.BaseType == BaseType.Sensor, "filter_basetype=sensor");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Comments() => QuerySensor(s => s.Comments == "Windows Server", "filter_comments=Windows+Server");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Dependency() => QuerySensor(s => s.Dependency == "dc-1", "filter_dependency=dc-1");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_InheritInterval() => QuerySensor(s => s.InheritInterval, string.Empty);

        [UnitTest]
        [TestMethod]
        public void QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_InheritInterval_False() => QuerySensor(s => !s.InheritInterval, string.Empty);

        [UnitTest]
        [TestMethod]
        public void QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Interval() => QuerySensor(s => s.Interval.TotalSeconds == 60, "filter_intervalx=0000000060");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_NotificationTypes() => QuerySensor(s => s.NotificationTypes.ToString() == "test", string.Empty);

        [UnitTest]
        [TestMethod]
        public void QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Position() => QuerySensor(s => s.Position == 4, "filter_position=0000000040");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Schedule() => QuerySensor(s => s.Schedule == "Weekends [GMT+0800]", "filter_schedule=Weekends+%5BGMT%2B0800%5D");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Status() => QuerySensor(s => s.Status == Status.Down, "filter_status=5");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Url() => QuerySensor(s => s.Url == "/sensor.htm?id=1001", "filter_baselink=1001");


        #endregion
        #region SensorOrDeviceOrGroupOrProbeOrTicket

        [UnitTest]
        [TestMethod]
        public void QueryFilter_SensorOrDeviceOrGroupOrProbeOrTicketProperties_Priority() => QuerySensor(s => s.Priority == Priority.Four, "filter_priority=4");

        #endregion
        #region SensorOrDeviceOrGroupOrProbeOrTicketOrTicketData

        [UnitTest]
        [TestMethod]
        public void QueryFilter_SensorOrDeviceOrGroupOrProbeOrTicketOrTicketDataProperties_Message() => QuerySensor(s => s.Message == "test", "filter_message=test");

        #endregion
        #region DeviceOrGroupOrProbe

        [UnitTest]
        [TestMethod]
        public void QueryFilter_DeviceOrGroupOrProbeProperties_DownAcknowledgedSensors() => QueryDevice(d => d.DownAcknowledgedSensors == 2, "filter_downacksens=0000000002");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_DeviceOrGroupOrProbeProperties_DownSensors() => QueryDevice(d => d.DownSensors == 3, "filter_downsens=0000000003");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_DeviceOrGroupOrProbeProperties_PartialDownSensors() => QueryDevice(d => d.PartialDownSensors == 4, "filter_partialdownsens=0000000004");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_DeviceOrGroupOrProbeProperties_PausedSensors() => QueryDevice(d => d.PausedSensors == 5, "filter_pausedsens=0000000005");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_DeviceOrGroupOrProbeProperties_TotalSensors() => QueryDevice(d => d.TotalSensors == 6, "filter_totalsens=0000000006");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_DeviceOrGroupOrProbeProperties_UnknownSensors() => QueryDevice(d => d.UnknownSensors == 7, "filter_undefinedsens=0000000007");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_DeviceOrGroupOrProbeProperties_UnusualSensors() => QueryDevice(d => d.UnusualSensors == 8, "filter_unusualsens=0000000008");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_DeviceOrGroupOrProbeProperties_UpSensors() => QueryDevice(d => d.UpSensors == 9, "filter_upsens=0000000009");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_DeviceOrGroupOrProbeProperties_WarningSensors() => QueryDevice(d => d.WarningSensors == 10, "filter_warnsens=0000000010");

        #endregion
        #region GroupOrProbe

        [UnitTest]
        [TestMethod]
        public void QueryFilter_GroupOrProbeProperties_Collapsed() => QueryGroup(g => g.Collapsed, "filter_fold=-1");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_GroupOrProbeProperties_TotalDevices() => QueryGroup(g => g.TotalDevices == 2, "filter_devicenum=0000000002");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_GroupOrProbeProperties_TotalGroups() => QueryGroup(g => g.TotalGroups == 3, "filter_groupnum=0000000003");

        #endregion
        #region Sensor

        [UnitTest]
        [TestMethod]
        public void QueryFilter_SensorProperties_DataCollectedSince() => QuerySensor(s => s.DataCollectedSince == new DateTime(2000, 10, 2, 12, 10, 5, DateTimeKind.Utc), "filter_cumsince=36801.5070023148");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_SensorProperties_Device() => QuerySensor(s => s.Device == "dc-1", "filter_device=dc-1");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_SensorProperties_DownDuration() => QuerySensor(s => s.DownDuration == TimeSpan.FromSeconds(50), "filter_downtimesince=000000000000050");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_SensorProperties_Downtime() => QuerySensor(s => s.Downtime == 10.1, "filter_downtime=000000000101000");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_SensorProperties_LastCheck() => QuerySensor(s => s.LastCheck == new DateTime(2000, 10, 2, 12, 10, 5, DateTimeKind.Utc), "filter_lastcheck=36801.5070023148");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_SensorProperties_LastUp() => QuerySensor(s => s.LastUp == new DateTime(2000, 10, 2, 12, 10, 5, DateTimeKind.Utc), "filter_lastup=36801.5070023148");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_SensorProperties_LastDown() => QuerySensor(s => s.LastDown == new DateTime(2000, 10, 2, 12, 10, 5, DateTimeKind.Utc), "filter_lastdown=36801.5070023148");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_SensorProperties_LastValue() => QuerySensor(s => s.LastValue > 3, "filter_lastvalue=@above(0000000000000030.0000)");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_SensorProperties_MiniGraph() => QuerySensor(s => s.MiniGraph == "0,0,0,0", "filter_minigraph=0%2C0%2C0%2C0");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_SensorProperties_TotalDowntime() => QuerySensor(s => s.TotalDowntime == TimeSpan.FromMinutes(2), "filter_downtimetime=000000000000120");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_SensorProperties_TotalMonitorTime() => QuerySensor(s => s.TotalMonitorTime == TimeSpan.FromSeconds(30), "filter_knowntime=000000000000030");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_SensorProperties_TotalUptime() => QuerySensor(s => s.TotalUptime == new TimeSpan(0, 2, 3), "filter_uptimetime=000000000000123");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_SensorProperties_UpDuration() => QuerySensor(s => s.UpDuration == new TimeSpan(0, 2, 1), "filter_uptimesince=000000000000121");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_SensorProperties_Uptime() => QuerySensor(s => s.Uptime == 90, "filter_uptime=000000000900000");

        #endregion
        #region Device

        [UnitTest]
        [TestMethod]
        public void QueryFilter_DeviceProperties_Condition() => QueryDevice(d => d.Condition == "Auto-Discovery (10%)", "filter_condition=Auto-Discovery+(10%25)");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_DeviceProperties_Favorite() => QueryDevice(d => d.Favorite, "filter_favorite=1");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_DeviceProperties_Favorite_False() => QueryDevice(d => !d.Favorite, string.Empty);

        [UnitTest]
        [TestMethod]
        public void QueryFilter_DeviceProperties_Host() => QueryDevice(d => d.Host == "dc-1", "filter_host=dc-1");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_DeviceProperties_Group() => QueryDevice(d => d.Group == "Servers", "filter_group=Servers");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_DeviceProperties_Location() => QueryDevice(d => d.Location.Contains("Nebraska"), "filter_location=@sub(Nebraska)");

        [UnitTest]
        [TestMethod]
        public void QueryFilter_DeviceProperties_Probe() => QueryDevice(d => d.Probe == "Local Probe", "filter_probe=Local+Probe");

        #endregion
        #region Probe

        [UnitTest]
        [TestMethod]
        public void QueryFilter_ProbeProperties_ProbeStatus() => QueryProbe(p => p.ProbeStatus == ProbeStatus.Connected, "filter_condition=2");

        #endregion
        #region Log
            #region Date Ranges

        [UnitTest]
        [TestMethod]
        public void QueryFilter_LogProperties_DateTime_Equals() => QueryLog(
            l => l.DateTime == Time.Yesterday,
            c => c.StreamLogs(Time.Yesterday, Time.Yesterday, serial: true),
            new[] {$"start=1&filter_dstart={Time.YesterdayStr}&filter_dend={Time.YesterdayStr}"}
        );

        [UnitTest]
        [TestMethod]
        public void QueryFilter_LogProperties_DateTime_DoubleEquals() => QueryLogIllegal<InvalidOperationException>(
            l => l.DateTime == Time.Yesterday || l.DateTime == Time.Today
        );

        [UnitTest]
        [TestMethod]
        public void QueryFilter_LogProperties_DateTime_NotEquals() => QueryLogIllegal<InvalidOperationException>(
            l => l.DateTime != Time.Yesterday
        );

        [UnitTest]
        [TestMethod]
        public void QueryFilter_LogProperties_DateTime_BackwardsAndForwards()
        {
            var url = new[] { UnitRequest.Logs($"count=500&start=1&filter_dend={Time.TodayStr}", UrlFlag.Columns)};

            ExecuteClient(c => c.QueryLogs().Where(l => l.DateTime < Time.Today), url, l => l.ToList());
            ExecuteClient(c => c.QueryLogs().Where(l => Time.Today > l.DateTime), url, l => l.ToList());
        }

        [UnitTest]
        [TestMethod]
        public void QueryFilter_LogProperties_DateTime_Illegal_WithRange() => QueryLog(
            l => l.DateTime > Time.LastWeek && l.DateTime != Time.TwoDaysAgo && l.DateTime < Time.Yesterday,
            c => c.StreamLogs(Time.Yesterday, Time.LastWeek, serial: true),
            new[] {$"start=1&filter_dstart={Time.LastWeekStr}&filter_dend={Time.YesterdayStr}"}
        );

        [UnitTest]
        [TestMethod]
        public void QueryFilter_LogProperties_DateTime_GreaterThan() => QueryLog(
            l => l.DateTime > Time.Yesterday,
            c => c.StreamLogs(null, Time.Yesterday, serial: true),
            new[] {$"start=1&filter_dstart={Time.YesterdayStr}"}
        );

        [UnitTest]
        [TestMethod]
        public void QueryFilter_LogProperties_DateTime_LessThan() => QueryLog(
            l => l.DateTime < Time.Yesterday,
            c => c.StreamLogs(Time.Yesterday, serial: true),
            new[] {$"start=1&filter_dend={Time.YesterdayStr}"}
        );

        [UnitTest]
        [TestMethod]
        public void QueryFilter_LogProperties_DateTime_DoubleGreaterThan() => QueryLog(
            l => l.DateTime > Time.Yesterday && l.DateTime > Time.Today,
            c => c.StreamLogs(null, Time.Yesterday, serial: true),
            new[] {$"start=1&filter_dstart={Time.YesterdayStr}"}
        );

        [UnitTest]
        [TestMethod]
        public void QueryFilter_LogProperties_DateTime_DoubleGreaterThan_LessThan() => QueryLog(
            l => l.DateTime > Time.LastWeek && l.DateTime > Time.TwoDaysAgo && l.DateTime < Time.Yesterday,
            c => c.StreamLogs(Time.Yesterday, Time.LastWeek, serial: true),
            new[] {$"start=1&filter_dstart={Time.LastWeekStr}&filter_dend={Time.YesterdayStr}"}
        );

        [UnitTest]
        [TestMethod]
        public void QueryFilter_LogProperties_DateTime_DoubleLessThan() => QueryLog(
            l => l.DateTime < Time.Yesterday && l.DateTime < Time.TwoDaysAgo,
            c => c.StreamLogs(Time.Yesterday, serial: true),
            new[] {$"start=1&filter_dend={Time.YesterdayStr}"}
        );

        [UnitTest]
        [TestMethod]
        public void QueryFilter_LogProperties_DateTime_DoubleLessThan_GreaterThan() => QueryLog(
            l => l.DateTime < Time.Yesterday && l.DateTime < Time.TwoDaysAgo && l.DateTime > Time.LastWeek,
            c => c.StreamLogs(Time.Yesterday, Time.LastWeek, serial: true),
            new[] {$"start=1&filter_dend={Time.YesterdayStr}&filter_dstart={Time.LastWeekStr}"},
            new Enum[] {Property.StartDate, Property.EndDate}
        );

        [UnitTest]
        [TestMethod]
        public void QueryFilter_LogProperties_DateTime_Range() => QueryLog(
            l => l.DateTime > Time.Yesterday && l.DateTime < Time.Today,
            c => c.StreamLogs(Time.Today, Time.Yesterday, serial: true),
            new[] {$"start=1&filter_dstart={Time.YesterdayStr}&filter_dend={Time.TodayStr}"}
        );

        [UnitTest]
        [TestMethod]
        public void QueryFilter_LogProperties_DateTime_DoubleRange() => QueryLog(
            l => l.DateTime > Time.Yesterday && l.DateTime < Time.Today || l.DateTime > Time.TwoWeeksAgo && l.DateTime < Time.LastWeek,
            c => c.StreamLogs(Time.Today, Time.Yesterday, serial: true).ToList(),
            c => c.StreamLogs(Time.LastWeek, Time.TwoWeeksAgo, serial: true).ToList(),
            new[] {
                $"start=1&filter_dstart={Time.YesterdayStr}&filter_dend={Time.TodayStr}",
                $"start=1&filter_dstart={Time.TwoWeeksAgoStr}&filter_dend={Time.LastWeekStr}"
            }
        );

        [UnitTest]
        [TestMethod]
        public void QueryFilter_LogProperties_DateTime_Range_Or_PartialRangeStart() => QueryLog(
            l => l.DateTime > Time.Yesterday && l.DateTime < Time.Today || l.DateTime > Time.TwoDaysAgo,
            c => c.StreamLogs(Time.Today, Time.Yesterday, serial: true).ToList(),
            c => c.StreamLogs(null, Time.TwoDaysAgo, serial: true).ToList(),
            new[] {
                $"start=1&filter_dstart={Time.YesterdayStr}&filter_dend={Time.TodayStr}",
                $"start=1&filter_dstart={Time.TwoDaysAgoStr}"
            } 
        );

        [UnitTest]
        [TestMethod]
        public void QueryFilter_LogProperties_DateTime_Range_Or_PartialRangeEnd() => QueryLog(
            l => l.DateTime > Time.Yesterday && l.DateTime < Time.Today || l.DateTime < Time.TwoDaysAgo,
            c => c.StreamLogs(Time.Today, Time.Yesterday, serial: true).ToList(),
            c => c.StreamLogs(Time.TwoDaysAgo, serial: true).ToList(),
            new[] {
                $"start=1&filter_dstart={Time.YesterdayStr}&filter_dend={Time.TodayStr}",
                $"start=1&filter_dend={Time.TwoDaysAgoStr}"
            } 
        );

        [UnitTest]
        [TestMethod]
        public void QueryFilter_LogProperties_DateTime_PartialRange_WithId() => QueryLog(
            l => l.DateTime > Time.TwoDaysAgo && l.Id == 4004,
            c => c.StreamLogs(4004, endDate: Time.TwoDaysAgo, serial: true),
            new[] {$"start=1&filter_dstart={Time.TwoDaysAgoStr}&id=4004" },
            new Enum[] {Parameter.Start, Parameter.Id, Property.StartDate}
        ); //always go an hour regardless of whether or not we have an ID

            #endregion
            #region Id

        [UnitTest]
        [TestMethod]
        public void QueryFilter_LogProperties_Id_IllegalOperator() => QueryLog(
            l => l.Id.ToString().Contains("2"),
            c => c.StreamLogs(RecordAge.All, serial: true),
            new[] {"start=1"}
        );

            #endregion

        [UnitTest]
        [TestMethod]
        public void QueryFilter_LogProperties_Device() => QueryLogUnsupported(
            l => l.Device == "Probe Device",
            c => c.StreamLogs(RecordAge.All, serial: true),
            "start=1&filter_device=Probe+Device"
        );

        [UnitTest]
        [TestMethod]
        public void QueryFilter_LogProperties_Group() => QueryLogUnsupported(
            l => l.Group == "Servers",
            c => c.StreamLogs(RecordAge.All, serial: true),
            "start=1&filter_group=Servers"
        );

        [UnitTest]
        [TestMethod]
        public void QueryFilter_LogProperties_Parent() => QueryLogUnsupported(
            l => l.Parent == "dc-1",
            c => c.StreamLogs(RecordAge.All, serial: true),
            "start=1&filter_parent=dc-1"
        );

        [UnitTest]
        [TestMethod]
        public void QueryFilter_LogProperties_Probe() => QueryLogUnsupported(
            l => l.Probe == "Local Probe",
            c => c.StreamLogs(RecordAge.All, serial: true),
            "start=1&filter_probe=Local+Probe"
        );

        [UnitTest]
        [TestMethod]
        public void QueryFilter_LogProperties_Sensor() => QueryLogUnsupported(
            l => l.Sensor == "Ping",
            c => c.StreamLogs(RecordAge.All, serial: true),
            "start=1&filter_sensor=Ping"
        );

        [UnitTest]
        [TestMethod]
        public void QueryFilter_LogProperties_Name() => QueryLogUnsupported(
            l => l.Name == "Ping",
            c => c.StreamLogs(RecordAge.All, serial: true),
            "start=1&filter_name=Ping"
        );

        [UnitTest]
        [TestMethod]
        public void QueryFilter_LogProperties_Status() => QueryLog(
            l => l.Status == LogStatus.Active,
            c => c.StreamLogs(RecordAge.All, status: LogStatus.Active, serial: true),
            new[] {"start=1&filter_status=618"}
        );

        [UnitTest]
        [TestMethod]
        public void QueryFilter_LogProperties_Id() => QueryLog(
            l => l.Id == 1001,
            c => c.StreamLogs(1001, serial: true),
            new[] { "start=1&id=1001" }
        );

        [UnitTest]
        [TestMethod]
        public void QueryFilter_LogProperties_IdOrId() => QueryLog(
            l => l.Id == 1001 || l.Id == 1002,
            c => c.StreamLogs(1001, serial: true).ToList(),
            c => c.StreamLogs(1002, serial: true).ToList(),
            new[] {
                "start=1&id=1001",
                "start=1&id=1002"
            }
        );

        [UnitTest]
        [TestMethod]
        public void QueryFilter_LogProperties_IdAndId() => QueryLog(
            l => l.Id == 1001 && l.Id == 1002,
            c => c.StreamLogs(1001, serial: true),
            new[] { "start=1&id=1001" }
        );

        [UnitTest]
        [TestMethod]
        public void QueryFilter_LogProperties_Message() => QueryLogUnsupported(
            l => l.Message == "test",
            c => c.StreamLogs(RecordAge.All, serial: true),
            "start=1"
        );

        [UnitTest]
        [TestMethod]
        public void QueryFilter_Log_WithTake()
        {
            ExecuteClient(c => c.QueryLogs(), new[] { UnitRequest.Logs("count=500&start=1", UrlFlag.Columns)}, s => s.ToList());
            ExecuteClient(c => c.QueryLogs().Take(3), new[] { UnitRequest.Logs("count=3&start=1", UrlFlag.Columns)}, s => s.ToList());
        }

        #endregion

        private void QuerySensor(System.Linq.Expressions.Expression<Func<Sensor, bool>> predicate, string url)
        {
            ExecuteFilter(predicate, url, s => s.ToList());
        }

        private void QueryDevice(System.Linq.Expressions.Expression<Func<Device, bool>> predicate, string url)
        {
            ExecuteClient(c => c.QueryDevices(predicate), new[]
            {
                UnitRequest.Devices($"count=500" + (string.IsNullOrEmpty(url) ? url : $"&{url}"), UrlFlag.Columns)
            }, s => s.ToList());
        }

        private void QueryGroup(System.Linq.Expressions.Expression<Func<Group, bool>> predicate, string url)
        {
            ExecuteClient(c => c.QueryGroups(predicate), new[]
            {
                UnitRequest.Groups($"count=500" + (string.IsNullOrEmpty(url) ? url : $"&{url}"), UrlFlag.Columns)
            }, s => s.ToList());
        }

        private void QueryProbe(System.Linq.Expressions.Expression<Func<Probe, bool>> predicate, string url)
        {
            ExecuteClient(c => c.QueryProbes(predicate), new[]
            {
                UnitRequest.Probes($"count=500" + (string.IsNullOrEmpty(url) ? url : $"&{url}") + "&filter_parentid=0", UrlFlag.Columns)
            }, s => s.ToList());
        }

        #region Log Helpers

        //Two streams
        private void QueryLog(
            System.Linq.Expressions.Expression<Func<Log, bool>> predicate,
            Func<PrtgClient, IEnumerable<Log>> stream1,
            Func<PrtgClient, IEnumerable<Log>> stream2,
            string[] url,
            Enum[] streamOrder = null)
        {
            QueryLog(predicate, c => stream1(c).Union(stream2(c)), url, streamOrder);
        }

        //One stream
        private void QueryLog(
            System.Linq.Expressions.Expression<Func<Log, bool>> predicate,
            Func<PrtgClient, IEnumerable<Log>> stream,
            string[] url,
            Enum[] streamOrder = null)
        {
            var urls = url.SelectMany(s =>
            {
                if (!s.Contains("count"))
                    s = $"count=500&{s}";

                return new[]
                {
                    UnitRequest.Logs(s, UrlFlag.Columns)
                };
            }).ToArray();

            ExecuteClient(c => c.QueryLogs(predicate), urls, s => s.ToList());

            if (streamOrder != null)
            {
                urls = GetStreamLogUrls(urls, streamOrder);
            }

            ExecuteClient(stream, urls, s => s.ToList());
        }

        private string[] GetStreamLogUrls(string[] urls, Enum[] streamOrder)
        {
            var newUrls = urls.Select((u, i) => GetUrlInternal(u, i, streamOrder)).ToArray();

            return newUrls;
        }

        private string GetUrlInternal(string u, int i, Enum[] streamOrder)
        {
            var parts = UrlUtilities.CrackUrl(u);
            var keys = GetKeys(parts);
            var firstIndexCandidates = GetFirstIndexCandidates(keys, streamOrder);

            var firstIndex = firstIndexCandidates.Cast<int?>().FirstOrDefault(ix => ix > -1) ?? -1;
            
            var newKeys = keys.Select((k, ix) => SwapUrl(keys, firstIndex, parts, k, ix, streamOrder)).Where(p => p != null).ToList();

            var joined = string.Join("&", newKeys);

            return $"https://prtg.example.com/api/table.xml?{joined}";
        }

        private Dictionary<Enum, string> GetKeys(NameValueCollection parts)
        {
            var keys = parts.AllKeys.Select(k =>
            {
                if (k.StartsWith("filter_"))
                    return Tuple.Create((Enum)k.Substring("filter_".Length).DescriptionToEnum<Property>(), k);

                return Tuple.Create((Enum)k.DescriptionToEnum<Parameter>(), k);
            }).ToDictionary(k => k.Item1, k => k.Item2);

            return keys;
        }

        private List<int> GetFirstIndexCandidates(Dictionary<Enum, string> keys, Enum[] streamOrder)
        {
            var firstIndexCandidates = keys.Select((k, ix) =>
            {
                if (k.Key is Property)
                {
                    if (streamOrder.ToList().Contains((Property)k.Key))
                        return ix;
                }
                else if (k.Key is Parameter)
                {
                    if (streamOrder.ToList().Contains((Parameter)k.Key))
                        return ix;
                }

                return -1;
            }).ToList();

            return firstIndexCandidates;
        }

        string SwapUrl(
            Dictionary<Enum, string> keys,
            int firstIndex,
            NameValueCollection parts,
            KeyValuePair<Enum, string> k, int ix, Enum[] streamOrder)
        {
            if (k.Key is Property && !streamOrder.Contains((Property)k.Key))
                return null;

            if (k.Key is Parameter && !streamOrder.Contains((Parameter)k.Key))
                return parts.AllKeys[ix] + "=" + parts[ix];

            if (ix >= firstIndex && ix - firstIndex < streamOrder.Length)
            {
                var offset = ix - firstIndex;

                var prop = streamOrder[offset];

                var key = keys[prop];

                return key + "=" + parts[key];
            }

            return parts.AllKeys[ix] + "=" + parts[ix];
        }

        private void QueryLogUnsupported(
            System.Linq.Expressions.Expression<Func<Log, bool>> predicate,
            Func<PrtgClient, IEnumerable<Log>> stream,
            string url
            )
        {
            QueryLog(predicate, stream, new[] {url}, new Enum[] { Parameter.Start });
        }

        #endregion
        #region IZeroPaddingConverter

        [UnitTest]
        [TestMethod]
        public void ZeroPadding_LastValueConverter_Pads_True()
        {
            TestZeroPadding<LastValueConverter>(Property.LastValue, 3, "0000000000000030.0000", true);
            TestZeroPadding<LastValueConverter>(Property.LastValue, 4.56789, "0000000000000040.5679", true);
        }

        [UnitTest]
        [TestMethod]
        public void ZeroPadding_LastValueConverter_Pads_False()
        {
            TestZeroPadding<LastValueConverter>(Property.LastValue, 3, "3", false);
            TestZeroPadding<LastValueConverter>(Property.LastValue, 4.56789, "40.5679", false);
        }

        [UnitTest]
        [TestMethod]
        public void ZeroPadding_PositionConverter_Pads_True() =>
            TestZeroPadding<PositionConverter>(Property.Position, 3, "0000000030", true);

        [UnitTest]
        [TestMethod]
        public void ZeroPadding_PositionConverter_Pads_False() =>
            TestZeroPadding<PositionConverter>(Property.Position, 3, "30", false);

        [UnitTest]
        [TestMethod]
        public void ZeroPadding_TimeSpanConverter_Pads_True() =>
            TestZeroPadding<TimeSpanConverter>(Property.UpDuration, 120, "000000000000120", true);

        [UnitTest]
        [TestMethod]
        public void ZeroPadding_TimeSpanConverter_Pads_False() =>
            TestZeroPadding<TimeSpanConverter>(Property.UpDuration, 120, "120", false);

        [UnitTest]
        [TestMethod]
        public void ZeroPadding_DateTimeConverter_Pads_True() =>
            TestZeroPadding<DateTimeConverter>(Property.LastUp, "42972.6522125", "42972.6522125000", true);

        [UnitTest]
        [TestMethod]
        public void ZeroPadding_DateTimeConverter_Pads_False() =>
            TestZeroPadding<DateTimeConverter>(Property.LastUp, "42972.6522125", "42972.6522125", false);

        [UnitTest]
        [TestMethod]
        public void ZeroPadding_UpDownTimeConverter_Pads_True() =>
            TestZeroPadding<UpDownTimeConverter>(Property.Uptime, 90.9, "000000000909000", true);

        [UnitTest]
        [TestMethod]
        public void ZeroPadding_UpDownTimeConverter_Pads_False() =>
            TestZeroPadding<UpDownTimeConverter>(Property.Uptime, 90.9, "909000", false);

        [UnitTest]
        [TestMethod]
        public void ZeroPadding_ZeroPaddingConverter_Pads_True() =>
            TestZeroPadding<ZeroPaddingConverter>(Property.Interval, 120, "0000000120", true);

        [UnitTest]
        [TestMethod]
        public void ZeroPadding_ZeroPaddingConverter_Pads_False() =>
            TestZeroPadding<ZeroPaddingConverter>(Property.Interval, 120, "120", false);

        #endregion

        private void QueryLogIllegal<TException>(
            System.Linq.Expressions.Expression<Func<Log, bool>> predicate,
            string exceptionMessage = "At least one end of a valid date range must be specified") where TException : Exception
        {
            AssertEx.Throws<TException>(() => QueryLog(predicate, null, new[]{string.Empty}, null), exceptionMessage);
        }

        [UnitTest]
        [TestMethod]
        public void AllPrtgObjectProperties_HaveTests()
        {
            AllPrtgObjectProperties_HaveTests(GetType());
        }

        [UnitTest]
        [TestMethod]
        public void AllIZeroPaddingConverters_HavePaddingTrueFalseTests()
        {
            var types = typeof(IZeroPaddingConverter).Assembly.GetTypes()
                .Where(t => typeof(IZeroPaddingConverter).IsAssignableFrom(t) && t != typeof(IZeroPaddingConverter))
                .ToList();

            var expected = types.SelectMany(t =>
            {
                var prefix = $"ZeroPadding_{t.Name}_Pads_";

                return new[]
                {
                    prefix + "True",
                    prefix + "False"
                };
            }).ToList();

            TestHelpers.Assert_TestClassHasMethods(GetType(), expected);
        }

        private void TestZeroPadding<TConverter>(Property property, object value, string expected, bool pad)
            where TConverter : IZeroPaddingConverter
        {
            IZeroPaddingConverter converter = (IZeroPaddingConverter)property.GetEnumAttribute<ValueConverterAttribute>().Converter;

            Assert.AreEqual(typeof(TConverter), converter.GetType(), "Converter was not of the specified type");

            var val = converter.SerializeWithPadding(value, pad);

            Assert.AreEqual(expected, val, "Converted value was incorrect");
        }

        public static List<PropertyInfo> GetPrtgObjectProperties(string[] propertyExclusions = null, Type[] typeExclusions = null)
        {
            if (propertyExclusions == null)
                propertyExclusions = new string[] { };

            var types = typeof(PrtgObject).Assembly.GetTypes().Where(t => typeof(PrtgObject).IsAssignableFrom(t)).Where(
                t => t != typeof(NotificationAction) && t != typeof(Schedule) && t != typeof(Ticket)
            ).ToList();

            if (typeExclusions != null)
                types = types.Where(t => typeExclusions.All(e => e != t)).ToList();

            var properties = types.SelectMany(t => t.GetProperties()).GroupBy(p => p.Name).ToList();

            var final = new List<PropertyInfo>();

            foreach (var propGroup in properties)
            {
                var t = propGroup.GroupBy(g => g.DeclaringType).ToList();

                foreach (var uniqueType in t)
                {
                    var candidate = uniqueType.First();

                    if (!final.Any(f => f.Name == candidate.Name && f.PropertyType == candidate.PropertyType) && propertyExclusions.All(e => e != candidate.Name))
                        final.Add(candidate);
                }
            }

            final = final.Where(f => f.Name != nameof(Sensor.DisplayLastValue)).ToList();

            return final;
        }

        public static void AllPrtgObjectProperties_HaveTests(Type testClass, string prefix = "QueryFilter", string suffix = null, string[] propertyExclusions = null, Type[] typeExclusions = null)
        {
            var final = GetPrtgObjectProperties(propertyExclusions, typeExclusions);

            var expectedMethods = final.Select(p =>
            {
                var name = $"{prefix}_{p.DeclaringType.Name}Properties_{p.Name}";

                if (suffix != null)
                    name += $"_{suffix}";

                return name;
            }).ToList();

            TestHelpers.Assert_TestClassHasMethods(testClass, expectedMethods);
        }
    }
}
