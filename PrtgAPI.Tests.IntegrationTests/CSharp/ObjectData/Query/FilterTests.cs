using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Parameters;
using PrtgAPI.Tests.UnitTests.ObjectData.Query;
using PrtgAPI.Tests.UnitTests.Support;

namespace PrtgAPI.Tests.IntegrationTests.ObjectData.Query
{
    static class Time
    {
        private static DateTime baseDate = DateTime.Now;

        public static DateTime Today => baseDate;

        public static DateTime Yesterday => baseDate.AddDays(-1);

        public static DateTime TwoDaysAgo => baseDate.AddDays(-2);

        public static DateTime LastWeek => baseDate.AddDays(-7);

        public static DateTime TwoWeeksAgo => baseDate.AddDays(-14);
    }

    [TestClass]
    public class FilterTests : BasePrtgClientTest
    {
        #region PrtgObject
        #region Name

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_PrtgObjectProperties_Name_Equals() =>
            ExecuteSensor(s => s.Name == "Ping", Property.Name, "Ping");

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_PrtgObjectProperties_Name_NotEquals() =>
            ExecuteSensor(s => s.Name != "Ping", Property.Name, "Ping", FilterOperator.NotEquals, filterThrows: true);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_PrtgObjectProperties_Name_GreaterThan() =>
            ExecuteUnsupported(Property.Name, FilterOperator.GreaterThan, "Ping");

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_PrtgObjectProperties_Name_LessThan() =>
            ExecuteUnsupported(Property.Name, FilterOperator.LessThan, "Ping");

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_PrtgObjectProperties_Name_Contains() =>
            ExecuteSensor(s => s.Name.Contains("Ping"), Property.Name, "Ping", FilterOperator.Contains);

        #endregion
        #region Id

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_PrtgObjectProperties_Id_Equals() =>
            ExecuteSensor(s => s.Id == Settings.UpSensor, Property.Id, Settings.UpSensor);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_PrtgObjectProperties_Id_NotEquals() =>
            ExecuteSensor(s => s.Id != Settings.UpSensor, Property.Id, Settings.UpSensor, FilterOperator.NotEquals);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_PrtgObjectProperties_Id_GreaterThan() =>
            ExecuteSensor(s => s.Id > Settings.UpSensor, Property.Id, Settings.UpSensor, FilterOperator.GreaterThan);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_PrtgObjectProperties_Id_LessThan() =>
            ExecuteSensor(s => s.Id < Settings.UpSensor, Property.Id, Settings.UpSensor, FilterOperator.LessThan);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_PrtgObjectProperties_Id_Contains() =>
            ExecuteSensor(s => s.Id.ToString().Contains(Settings.UpSensor.ToString()), Property.Id, Settings.UpSensor, FilterOperator.Contains);

        #endregion
        #region ParentId

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_PrtgObjectProperties_ParentId_Equals() =>
            ExecuteSensor(s => s.ParentId == Settings.Device, Property.ParentId, Settings.Device);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_PrtgObjectProperties_ParentId_NotEquals() =>
            ExecuteSensor(s => s.ParentId != Settings.Device, Property.ParentId, Settings.Device, FilterOperator.NotEquals);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_PrtgObjectProperties_ParentId_GreaterThan() =>
            ExecuteSensor(s => s.ParentId > Settings.Device, Property.ParentId, Settings.Device, FilterOperator.GreaterThan);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_PrtgObjectProperties_ParentId_LessThan() =>
            ExecuteSensor(s => s.ParentId < Settings.Device, Property.ParentId, Settings.Device, FilterOperator.LessThan);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_PrtgObjectProperties_ParentId_Contains() =>
            ExecuteSensor(s => s.ParentId.ToString().Contains(Settings.Device.ToString()), Property.ParentId, Settings.Device, FilterOperator.Contains);

        #endregion
        #region Active

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_PrtgObjectProperties_Active_Equals() =>
            ExecuteSensor(s => s.Active, Property.Active, true);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_PrtgObjectProperties_Active_NotEquals() =>
            ExecuteSensor(s => !s.Active, Property.Active, true, FilterOperator.NotEquals);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_PrtgObjectProperties_Active_GreaterThan() =>
            ExecuteSerialized(Property.Active, FilterOperator.GreaterThan, true); //Active True is -1

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_PrtgObjectProperties_Active_LessThan() =>
            ExecuteSerialized(Property.Active, FilterOperator.LessThan, false); //Active True is -1

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_PrtgObjectProperties_Active_Contains() =>
            ExecuteSensor(s => s.Active.ToString().Contains(true.ToString()), Property.Active, true, FilterOperator.Contains);

        #endregion
        #region DisplayType

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_PrtgObjectProperties_DisplayType_Equals()
        {
            var querySensors = client.QuerySensors(s => s.DisplayType == "Ping").ToList();
            var manualSensors = client.GetSensors().Where(s => s.DisplayType == "Ping").ToList();

            AssertEx.AreEqualLists(querySensors, manualSensors, new PrtgObjectComparer(), "Lists were not equal");
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_PrtgObjectProperties_DisplayType_NotEquals()
        {
            var querySensors = client.QuerySensors(s => s.DisplayType != "Ping").ToList();
            var manualSensors = client.GetSensors().Where(s => s.DisplayType != "Ping").ToList();

            AssertEx.AreEqualLists(querySensors, manualSensors, new PrtgObjectComparer(), "Lists were not equal");
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_PrtgObjectProperties_DisplayType_Contains()
        {
            var querySensors = client.QuerySensors(s => s.DisplayType.Contains("Ping")).ToList();
            var manualSensors = client.GetSensors().Where(s => s.DisplayType.Contains("Ping")).ToList();

            AssertEx.AreEqualLists(querySensors, manualSensors, new PrtgObjectComparer(), "Lists were not equal");
        }

        #endregion
        #region Type

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_PrtgObjectProperties_Type_Equals() =>
            ExecuteSensor(s => s.Type == "exe", Property.Type, "exe");

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_PrtgObjectProperties_Type_NotEquals() =>
            ExecuteSensor(s => s.Type != "exe", Property.Type, "exe", FilterOperator.NotEquals, filterThrows: true);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_PrtgObjectProperties_Type_GreaterThan() =>
            ExecuteUnsupported(Property.Type, FilterOperator.GreaterThan, "exe");

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_PrtgObjectProperties_Type_LessThan() =>
            ExecuteUnsupported(Property.Type, FilterOperator.LessThan, "exe");

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_PrtgObjectProperties_Type_Contains() =>
            ExecuteSensor(s => s.Type.ToString().Contains("exe"), Property.Type, "exe", FilterOperator.Contains);

        #endregion
        #region Tags

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_PrtgObjectProperties_Tags_Equals() =>
            Data_QueryFilter_PrtgObjectProperties_Tags_Index_Equals();

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_PrtgObjectProperties_Tags_Index_Equals() =>
            ExecuteSensor(s => s.Tags[0] == "pingsensor", Property.Tags, "pingsensor");

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_PrtgObjectProperties_Tags_Contains_Equals() =>
            ExecuteSensor(s => s.Tags.Contains(Settings.DeviceTag), Property.Tags, Settings.DeviceTag);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_PrtgObjectProperties_Tags_NotEquals() =>
            ExecuteUnsupported(Property.Tags, FilterOperator.NotEquals, "pingsensor");

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_PrtgObjectProperties_Tags_GreaterThan() =>
            ExecuteUnsupported(Property.Tags, FilterOperator.GreaterThan, "pingsensor");

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_PrtgObjectProperties_Tags_LessThan() =>
            ExecuteUnsupported(Property.Tags, FilterOperator.LessThan, "pingsensor");

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_PrtgObjectProperties_Tags_Contains() =>
            ExecuteSensor(s => s.Tags.Contains("pingsensor"), Property.Tags, "pingsensor", FilterOperator.Contains);

        #endregion
        #endregion
        #region SensorOrDeviceOrGroupOrProbe
        #region Access

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Access_Equals() =>
            ExecuteSensor(s => s.Access == Access.Full, Property.Access, Access.Full);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Access_NotEquals() =>
            ExecuteSensor(s => s.Access != Access.Read, Property.Access, Access.Read, FilterOperator.NotEquals);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Access_GreaterThan() =>
            ExecuteSensor(s => s.Access > Access.Read, Property.Access, Access.Read, FilterOperator.GreaterThan);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Access_LessThan() =>
            ExecuteSerialized(Property.Access, FilterOperator.LessThan, 800);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Access_Contains() =>
            ExecuteSensor(s => s.Access.ToString().Contains(Access.Full.ToString()), Property.Access, Access.Full, FilterOperator.Contains);

        #endregion
        #region BaseType

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_BaseType_Equals() =>
            ExecuteSensor(s => s.BaseType == BaseType.Sensor, Property.BaseType, BaseType.Sensor);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_BaseType_NotEquals() =>
            ExecuteSensor(s => s.BaseType != BaseType.Device, Property.BaseType, BaseType.Device, FilterOperator.NotEquals, filterThrows: true);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_BaseType_GreaterThan() =>
            ExecuteUnsupported(Property.BaseType, FilterOperator.GreaterThan, BaseType.Device);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_BaseType_LessThan() =>
            ExecuteUnsupported(Property.BaseType, FilterOperator.LessThan, BaseType.Device);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_BaseType_Contains() =>
            ExecuteSensor(s => s.BaseType.ToString().Contains(BaseType.Sensor.ToString()), Property.BaseType, BaseType.Sensor, FilterOperator.Contains);

        #endregion
        #region Comments

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Comments_Equals() =>
            ExecuteSensor(s => s.Comments == Settings.Comment, Property.Comments, Settings.Comment);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Comments_NotEquals() =>
            ExecuteSensor(s => s.Comments != Settings.Comment, Property.Comments, Settings.Comment, FilterOperator.NotEquals, filterThrows: true);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Comments_GreaterThan() =>
            ExecuteUnsupported(Property.Comments, FilterOperator.GreaterThan, Settings.Comment);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Comments_LessThan() =>
            ExecuteUnsupported(Property.Comments, FilterOperator.LessThan, Settings.Comment);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Comments_Contains() =>
            ExecuteSensor(s => s.Comments.Contains(Settings.Comment), Property.Comments, Settings.Comment, FilterOperator.Contains);

        #endregion
        #region Dependency

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Dependency_Equals() =>
            ExecuteSensor(s => s.Dependency == Settings.DeviceName, Property.Dependency, Settings.DeviceName);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Dependency_NotEquals() =>
            ExecuteSensor(s => s.Dependency != Settings.DeviceName, Property.Dependency, Settings.DeviceName, FilterOperator.NotEquals, filterThrows: true);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Dependency_GreaterThan() =>
            ExecuteUnsupported(Property.Dependency, FilterOperator.GreaterThan, Settings.DeviceName);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Dependency_LessThan() =>
            ExecuteUnsupported(Property.Dependency, FilterOperator.LessThan, Settings.DeviceName);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Dependency_Contains() =>
            ExecuteSensor(s => s.Dependency.Contains(Settings.DeviceName), Property.Dependency, Settings.DeviceName, FilterOperator.Contains);

        #endregion
        #region Interval

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Interval_Equals() =>
            ExecuteSensor(s => s.Interval == TimeSpan.FromSeconds(60), Property.Interval, TimeSpan.FromSeconds(60));

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Interval_Integer_Equals() =>
            ExecuteSensor(s => s.Interval.TotalSeconds == 60, Property.Interval, 60);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Interval_Device_Equals() =>
            ExecuteDevice(d => d.Interval.TotalSeconds == 60, Property.Interval, 60);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Interval_NotEquals() =>
            ExecuteSensor(s => s.Interval != TimeSpan.FromSeconds(60), Property.Interval, TimeSpan.FromSeconds(60), FilterOperator.NotEquals);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Interval_GreaterThan() =>
            ExecuteSensor(s => s.Interval > TimeSpan.FromSeconds(60), Property.Interval, TimeSpan.FromSeconds(60), FilterOperator.GreaterThan);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Interval_LessThan() =>
            ExecuteSensor(s => s.Interval < TimeSpan.FromSeconds(60), Property.Interval, TimeSpan.FromSeconds(60), FilterOperator.LessThan);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Interval_Contains() =>
            ExecuteSensor(s => s.Interval.TotalSeconds.ToString().Contains(60.ToString()), Property.Interval, 60, FilterOperator.Contains);

        #endregion
        #region NotificationTypes

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_NotificationTypes_Equals() =>
            ExecuteSensor(s => s.NotificationTypes == new NotificationTypes(string.Empty), Property.NotificationTypes, "State", filterThrows: true, filterUnsupported: true);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_NotificationTypes_NotEquals() =>
            ExecuteSensor(s => s.NotificationTypes != new NotificationTypes(string.Empty), Property.NotificationTypes, "State", FilterOperator.NotEquals, filterThrows: true, filterUnsupported: true);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_NotificationTypes_GreaterThan() =>
            ExecuteUnsupported(Property.NotificationTypes, FilterOperator.GreaterThan, "State", unsupported: true);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_NotificationTypes_LessThan() =>
            ExecuteUnsupported(Property.NotificationTypes, FilterOperator.LessThan, "State", unsupported: true);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_NotificationTypes_Contains() =>
            ExecuteUnsupported(Property.NotificationTypes, FilterOperator.Contains, "State", unsupported: true);

        #endregion
        #region Position

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Position_Equals() =>
            ExecuteSensor(s => s.Position == 4, Property.Position, 4);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Position_NotEquals() =>
            ExecuteSensor(s => s.Position != 4, Property.Position, 4, FilterOperator.NotEquals);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Position_GreaterThan() =>
            ExecuteSensor(s => s.Position > 4, Property.Position, 4, FilterOperator.GreaterThan);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Position_LessThan() =>
            ExecuteSensor(s => s.Position < 4, Property.Position, 4, FilterOperator.LessThan);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Position_Contains() =>
            ExecuteSensor(s => s.Position.ToString().Contains(10.ToString()), Property.Position, 10, FilterOperator.Contains);

        #endregion
        #region Schedule

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Schedule_Equals()
        {
            PrepareSchedule(schedule =>
            {
                ExecuteSensor(s => s.Schedule == schedule.Name, Property.Schedule, schedule.Name);
            });
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Schedule_NotEquals()
        {
            PrepareSchedule(schedule =>
            {
                ExecuteSensor(s => s.Schedule != schedule.Name, Property.Schedule, schedule.Name, FilterOperator.NotEquals, filterThrows: true);
            });
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Schedule_GreaterThan()
        {
            PrepareSchedule(schedule =>
            {
                ExecuteUnsupported(Property.Schedule, FilterOperator.GreaterThan, schedule.Name);
            });
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Schedule_LessThan()
        {
            PrepareSchedule(schedule =>
            {
                ExecuteUnsupported(Property.Schedule, FilterOperator.LessThan, schedule.Name);
            });
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Schedule_Contains()
        {
            PrepareSchedule(schedule =>
            {
                ExecuteSensor(s => s.Schedule.Contains(schedule.Name), Property.Schedule, schedule.Name, FilterOperator.Contains);
            });
        }

        private void PrepareSchedule(Action<Schedule> action)
        {
            try
            {
                var schedule = client.GetSchedule(Settings.Schedule);

                var initialSensor = client.GetSensor(Settings.UpSensor);
                AssertEx.IsTrue(initialSensor.Schedule == null, "Sensor had an initial schedule, however this should not have been the case");

                var initialSensorsWithSchedules = client.GetSensors().Where(s => s.Schedule != null).ToList();
                AssertEx.AreEqual(0, initialSensorsWithSchedules.Count, "No sensors should have existing schedules");

                client.SetObjectPropertyRaw(
                    Settings.UpSensor,
                    new CustomParameter("scheduledependency", 0),
                    new CustomParameter("schedule_", schedule)
                );

                var sensor = client.GetSensor(Settings.UpSensor);
                AssertEx.AreEqual(schedule.Name, sensor.Schedule, "Schedule did not apply properly");

                var newSensorsWithSchedules = client.GetSensors(Property.Schedule, schedule.Name);

                AssertEx.AreEqual(1, newSensorsWithSchedules.Count, "Schedule was not applied correctly");

                AssertEx.AreEqual(Settings.UpSensor, newSensorsWithSchedules.Single().Id, "Sensor was applied to wrong sensor");

                action(schedule);
            }
            finally
            {
                client.SetObjectPropertyRaw(Settings.UpSensor, "scheduledependency", "1");

                ServerManager.WaitForSensor(Settings.UpSensor, Status.Up);
            }
        }

        #endregion
        #region Status

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Status_Equals() =>
            ExecuteSensor(s => s.Status == Status.Down, Property.Status, Status.Down);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Status_NotEquals() =>
            ExecuteSensor(s => s.Status != Status.Down, Property.Status, Status.Down, FilterOperator.NotEquals);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Status_GreaterThan() =>
            ExecuteSensor(s => s.Status > Status.Down, Property.Status, Status.Down, FilterOperator.GreaterThan);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Status_LessThan() =>
            ExecuteSensor(s => s.Status < Status.Down, Property.Status, Status.Down, FilterOperator.LessThan);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Status_Contains() =>
            ExecuteSensor(s => s.Status.ToString().Contains(Status.Warning.ToString()), Property.Status, Status.Warning, FilterOperator.Contains);

        #endregion
        #region Url

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Url_Equals() =>
            ExecuteSensor(s => s.Url == $"/sensor.htm?id={Settings.UpSensor}", Property.Url, $"/sensor.htm?id={Settings.UpSensor}");

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Url_Equals_Id()
        {
            var full = client.GetSensors(Property.Url, $"/sensor.htm?id={Settings.UpSensor}");
            var partial = client.GetSensors(Property.Url, Settings.UpSensor);

            Assert.AreEqual(1, full.Count);
            Assert.AreEqual(1, partial.Count);
            Assert.AreEqual(full.Single().Id, partial.Single().Id);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Url_NotEquals() =>
            ExecuteSensor(s => s.Url != $"/sensor.htm?id={Settings.UpSensor}", Property.Url, $"/sensor.htm?id={Settings.UpSensor}", FilterOperator.NotEquals);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Url_GreaterThan() =>
            ExecuteSerialized(Property.Url, FilterOperator.GreaterThan, $"/sensor.htm?id={Settings.UpSensor}");

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Url_LessThan() =>
            ExecuteSerialized(Property.Url, FilterOperator.LessThan, $"/sensor.htm?id={Settings.UpSensor}");

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeProperties_Url_Contains() =>
            ExecuteSensor(s => s.Url.Contains($"/sensor.htm?id={Settings.UpSensor}"), Property.Url, $"/sensor.htm?id={Settings.UpSensor}", FilterOperator.Contains);

        #endregion
        #endregion
        #region SensorOrDeviceOrGroupOrProbeOrTicket
        #region Priority

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeOrTicketProperties_Priority_Equals() =>
            ExecuteSensor(s => s.Priority == Priority.Five, Property.Priority, Priority.Five);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeOrTicketProperties_Priority_NotEquals() =>
            ExecuteSensor(s => s.Priority != Priority.Five, Property.Priority, Priority.Five, FilterOperator.NotEquals);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeOrTicketProperties_Priority_GreaterThan() =>
            ExecuteSensor(s => s.Priority > Priority.Four, Property.Priority, Priority.Four, FilterOperator.GreaterThan);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeOrTicketProperties_Priority_LessThan() =>
            ExecuteSensor(s => s.Priority < Priority.Five, Property.Priority, Priority.Five, FilterOperator.LessThan);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeOrTicketProperties_Priority_Contains() =>
            ExecuteSensor(s => s.Priority.ToString().Contains(Priority.Five.ToString()), Property.Priority, Priority.Five, FilterOperator.Contains);

        #endregion
        #endregion
        #region SensorOrDeviceOrGroupOrProbeOrTicketOrTicketData
        #region Message

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeOrTicketOrTicketDataProperties_Message_Equals() =>
            ExecuteSensor(s => s.Message == "OK", Property.Message, "OK");

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeOrTicketOrTicketDataProperties_Message_NotEquals() =>
            ExecuteSensor(s => s.Message != "OK", Property.Message, "OK", FilterOperator.NotEquals, filterThrows: true);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeOrTicketOrTicketDataProperties_Message_GreaterThan() =>
            ExecuteUnsupported(Property.Message, FilterOperator.GreaterThan, "OK");

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeOrTicketOrTicketDataProperties_Message_LessThan() =>
            ExecuteUnsupported(Property.Message, FilterOperator.LessThan, "OK");

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorOrDeviceOrGroupOrProbeOrTicketOrTicketDataProperties_Message_Contains() =>
            ExecuteSensor(s => s.Message.Contains("OK"), Property.Message, "OK", FilterOperator.Contains, s => s.Message == "ok");

        #endregion
        #endregion
        #region DeviceOrGroupOrProbe
        #region DownAcknowledgedSensors

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceOrGroupOrProbeProperties_DownAcknowledgedSensors_Equals() =>
            ExecuteDevice(d => d.DownAcknowledgedSensors == 1, Property.DownAcknowledgedSensors, 1);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceOrGroupOrProbeProperties_DownAcknowledgedSensors_NotEquals() =>
            ExecuteDevice(d => d.DownAcknowledgedSensors != 1, Property.DownAcknowledgedSensors, 1, FilterOperator.NotEquals);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceOrGroupOrProbeProperties_DownAcknowledgedSensors_GreaterThan() =>
            ExecuteDevice(d => d.DownAcknowledgedSensors > 0, Property.DownAcknowledgedSensors, 0, FilterOperator.GreaterThan);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceOrGroupOrProbeProperties_DownAcknowledgedSensors_LessThan() =>
            ExecuteDevice(d => d.DownAcknowledgedSensors < 1, Property.DownAcknowledgedSensors, 1, FilterOperator.LessThan);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceOrGroupOrProbeProperties_DownAcknowledgedSensors_Contains() =>
            ExecuteDevice(d => d.DownAcknowledgedSensors.ToString().Contains(1.ToString()), Property.DownAcknowledgedSensors, 1, FilterOperator.Contains);

        #endregion
        #region DownSensors

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceOrGroupOrProbeProperties_DownSensors_Equals() =>
            ExecuteDevice(d => d.DownSensors == 1, Property.DownSensors, 1);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceOrGroupOrProbeProperties_DownSensors_NotEquals() =>
            ExecuteDevice(d => d.DownSensors != 1, Property.DownSensors, 1, FilterOperator.NotEquals);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceOrGroupOrProbeProperties_DownSensors_GreaterThan() =>
            ExecuteDevice(d => d.DownSensors > 0, Property.DownSensors, 0, FilterOperator.GreaterThan);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceOrGroupOrProbeProperties_DownSensors_LessThan() =>
            ExecuteDevice(d => d.DownSensors < 1, Property.DownSensors, 1, FilterOperator.LessThan);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceOrGroupOrProbeProperties_DownSensors_Contains() =>
            ExecuteDevice(d => d.DownSensors.ToString().Contains(1.ToString()), Property.DownSensors, 1, FilterOperator.Contains);

        #endregion
        #region PausedSensors

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceOrGroupOrProbeProperties_PausedSensors_Equals() =>
            ExecuteDevice(d => d.PausedSensors == 2, Property.PausedSensors, 2);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceOrGroupOrProbeProperties_PausedSensors_NotEquals() =>
            ExecuteDevice(d => d.PausedSensors != 2, Property.PausedSensors, 2, FilterOperator.NotEquals);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceOrGroupOrProbeProperties_PausedSensors_GreaterThan() =>
            ExecuteDevice(d => d.PausedSensors > 1, Property.PausedSensors, 1, FilterOperator.GreaterThan);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceOrGroupOrProbeProperties_PausedSensors_LessThan() =>
            ExecuteDevice(d => d.PausedSensors < 2, Property.PausedSensors, 2, FilterOperator.LessThan);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceOrGroupOrProbeProperties_PausedSensors_Contains() =>
            ExecuteDevice(d => d.PausedSensors.ToString().Contains(2.ToString()), Property.PausedSensors, 2, FilterOperator.Contains);

        #endregion
        #region TotalSensors

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceOrGroupOrProbeProperties_TotalSensors_Equals() =>
            ExecuteDevice(d => d.TotalSensors == Settings.SensorsInTestDevice, Property.TotalSensors, Settings.SensorsInTestDevice);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceOrGroupOrProbeProperties_TotalSensors_NotEquals() =>
            ExecuteDevice(d => d.TotalSensors != Settings.SensorsInTestDevice, Property.TotalSensors, Settings.SensorsInTestDevice, FilterOperator.NotEquals);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceOrGroupOrProbeProperties_TotalSensors_GreaterThan() =>
            ExecuteDevice(d => d.TotalSensors > Settings.SensorsInTestDevice - 1, Property.TotalSensors, Settings.SensorsInTestDevice - 1, FilterOperator.GreaterThan);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceOrGroupOrProbeProperties_TotalSensors_LessThan() =>
            ExecuteDevice(d => d.TotalSensors < Settings.SensorsInTestDevice, Property.TotalSensors, Settings.SensorsInTestDevice, FilterOperator.LessThan);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceOrGroupOrProbeProperties_TotalSensors_Contains() =>
            ExecuteDevice(d => d.TotalSensors.ToString().Contains(Settings.SensorsInTestDevice.ToString()), Property.TotalSensors, Settings.SensorsInTestDevice, FilterOperator.Contains);

        #endregion
        #region UnknownSensors

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceOrGroupOrProbeProperties_UnknownSensors_Equals() =>
            ExecuteDevice(d => d.UnknownSensors == 1, Property.UnknownSensors, 1);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceOrGroupOrProbeProperties_UnknownSensors_NotEquals() =>
            ExecuteDevice(d => d.UnknownSensors != 1, Property.UnknownSensors, 1, FilterOperator.NotEquals);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceOrGroupOrProbeProperties_UnknownSensors_GreaterThan() =>
            ExecuteDevice(d => d.UnknownSensors > 0, Property.UnknownSensors, 0, FilterOperator.GreaterThan);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceOrGroupOrProbeProperties_UnknownSensors_LessThan() =>
            ExecuteDevice(d => d.UnknownSensors < 1, Property.UnknownSensors, 1, FilterOperator.LessThan);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceOrGroupOrProbeProperties_UnknownSensors_Contains() =>
            ExecuteDevice(d => d.UnknownSensors.ToString().Contains(1.ToString()), Property.UnknownSensors, 1, FilterOperator.Contains);

        #endregion
        #region UpSensors

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceOrGroupOrProbeProperties_UpSensors_Equals() =>
            ExecuteDevice(d => d.UpSensors == 5, Property.UpSensors, 5);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceOrGroupOrProbeProperties_UpSensors_NotEquals() =>
            ExecuteDevice(d => d.UpSensors != 5, Property.UpSensors, 5, FilterOperator.NotEquals);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceOrGroupOrProbeProperties_UpSensors_GreaterThan() =>
            ExecuteDevice(d => d.UpSensors > 5, Property.UpSensors, 5, FilterOperator.GreaterThan);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceOrGroupOrProbeProperties_UpSensors_LessThan() =>
            ExecuteDevice(d => d.UpSensors < 5 + 1, Property.UpSensors, 5 + 1, FilterOperator.LessThan);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceOrGroupOrProbeProperties_UpSensors_Contains() =>
            ExecuteDevice(d => d.UpSensors.ToString().Contains(5.ToString()), Property.UpSensors, 5, FilterOperator.Contains);

        #endregion
        #region WarningSensors

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceOrGroupOrProbeProperties_WarningSensors_Equals() =>
            ExecuteDevice(d => d.WarningSensors == 1, Property.WarningSensors, 1);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceOrGroupOrProbeProperties_WarningSensors_NotEquals() =>
            ExecuteDevice(d => d.WarningSensors != 1, Property.WarningSensors, 1, FilterOperator.NotEquals);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceOrGroupOrProbeProperties_WarningSensors_GreaterThan() =>
            ExecuteDevice(d => d.WarningSensors > 0, Property.WarningSensors, 0, FilterOperator.GreaterThan);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceOrGroupOrProbeProperties_WarningSensors_LessThan() =>
            ExecuteDevice(d => d.WarningSensors < 1, Property.WarningSensors, 1, FilterOperator.LessThan);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceOrGroupOrProbeProperties_WarningSensors_Contains() =>
            ExecuteDevice(d => d.WarningSensors.ToString().Contains(1.ToString()), Property.WarningSensors, 1, FilterOperator.Contains);

        #endregion
        #endregion
        #region GroupOrProbe
        #region Collapsed

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_GroupOrProbeProperties_Collapsed_Equals() =>
            ExecuteGroup(g => !g.Collapsed, Property.Collapsed, false);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_GroupOrProbeProperties_Collapsed_NotEquals() =>
            ExecuteGroup(g => !g.Collapsed, Property.Collapsed, true, FilterOperator.NotEquals);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_GroupOrProbeProperties_Collapsed_GreaterThan() =>
            ExecuteGroupSerialized(Property.Collapsed, FilterOperator.GreaterThan, true);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_GroupOrProbeProperties_Collapsed_LessThan()
        {
            PrepareCollapse(group =>
            {
                ExecuteGroupSerialized(Property.Collapsed, FilterOperator.LessThan, false);
            });
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_GroupOrProbeProperties_Collapsed_Contains()
        {
            PrepareCollapse(group =>
            {
                ExecuteGroup(g => g.Collapsed.ToString().Contains(true.ToString()), Property.Collapsed, true, FilterOperator.Contains);
            });
        }

        private void PrepareCollapse(Action<Group> action, bool fold = true)
        {
            Group original = client.GetGroup(Settings.Group);

            PrtgAPIHelpers.FoldObject(client, Settings.Group, fold);

            Group group = null;

            for(var i = 0; i < 10; i++)
            {
                group = client.GetGroup(Settings.Group);

                if (group.Collapsed == fold)
                    break;
            }

            if (group.Collapsed != fold)
                throw new Exception($"Group fold didn't change to {fold}");

            try
            {
                action(group);
            }
            finally
            {
                PrtgAPIHelpers.FoldObject(client, Settings.Group, original.Collapsed);
            }
        }

        #endregion
        #region TotalDevices

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_GroupOrProbeProperties_TotalDevices_Equals() =>
            ExecuteGroup(g => g.TotalDevices == 1, Property.TotalDevices, 1);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_GroupOrProbeProperties_TotalDevices_NotEquals() =>
            ExecuteGroup(g => g.TotalDevices != 1, Property.TotalDevices, 1, FilterOperator.NotEquals);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_GroupOrProbeProperties_TotalDevices_GreaterThan() =>
            ExecuteGroup(g => g.TotalDevices > 1, Property.TotalDevices, 1, FilterOperator.GreaterThan);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_GroupOrProbeProperties_TotalDevices_LessThan() =>
            ExecuteGroup(g => g.TotalDevices < 1, Property.TotalDevices, 1, FilterOperator.LessThan);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_GroupOrProbeProperties_TotalDevices_Contains() =>
            ExecuteGroup(g => g.TotalDevices.ToString().Contains(1.ToString()), Property.TotalDevices, 1, FilterOperator.Contains);

        #endregion
        #region TotalGroups

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_GroupOrProbeProperties_TotalGroups_Equals() =>
            ExecuteGroup(g => g.TotalGroups == 1, Property.TotalGroups, 1);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_GroupOrProbeProperties_TotalGroups_NotEquals() =>
            ExecuteGroup(g => g.TotalGroups != 1, Property.TotalGroups, 1, FilterOperator.NotEquals);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_GroupOrProbeProperties_TotalGroups_GreaterThan() =>
            ExecuteGroup(g => g.TotalGroups > 1, Property.TotalGroups, 1, FilterOperator.GreaterThan);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_GroupOrProbeProperties_TotalGroups_LessThan() =>
            ExecuteGroup(g => g.TotalGroups < 1, Property.TotalGroups, 1, FilterOperator.LessThan);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_GroupOrProbeProperties_TotalGroups_Contains() =>
            ExecuteGroup(g => g.TotalGroups.ToString().Contains(1.ToString()), Property.TotalGroups, 1, FilterOperator.Contains);

        #endregion
        #endregion
        #region Sensor
        #region DataCollectedSince

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_DataCollectedSince_Equals()
        {
            var upSensor = client.GetSensor(Settings.UpSensor);
            ExecuteSensor(s => s.DataCollectedSince == upSensor.DataCollectedSince, Property.DataCollectedSince, upSensor.DataCollectedSince);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_DataCollectedSince_NotEquals()
        {
            var upSensor = client.GetSensor(Settings.UpSensor);
            ExecuteSensor(s => s.DataCollectedSince != upSensor.DataCollectedSince, Property.DataCollectedSince, upSensor.DataCollectedSince, FilterOperator.NotEquals);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_DataCollectedSince_GreaterThan()
        {
            var upSensor = client.GetSensor(Settings.UpSensor);
            ExecuteSensor(s => s.DataCollectedSince > upSensor.DataCollectedSince, Property.DataCollectedSince, upSensor.DataCollectedSince, FilterOperator.GreaterThan);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_DataCollectedSince_LessThan()
        {
            var sensor = client.GetSensors().OrderByDescending(s => s.DataCollectedSince).First();
            ExecuteSensor(s => s.DataCollectedSince < sensor.DataCollectedSince, Property.DataCollectedSince, sensor.DataCollectedSince, FilterOperator.LessThan, s => s.DataCollectedSince == null);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_DataCollectedSince_Contains()
        {
            var upSensor = client.GetSensors(Property.Name, "System Health").First();
            ExecuteSensor(s => (s.DataCollectedSince ?? new DateTime()).Ticks.ToString().Contains(upSensor.DataCollectedSince.Value.Ticks.ToString()), Property.DataCollectedSince, upSensor.DataCollectedSince, FilterOperator.Contains);
        }

        #endregion
        #region Device

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_Device_Equals() =>
            ExecuteSensor(s => s.Device == Settings.DeviceName, Property.Device, Settings.DeviceName);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_Device_NotEquals() =>
            ExecuteSensor(s => s.Device != Settings.DeviceName, Property.Device, Settings.DeviceName, FilterOperator.NotEquals, filterThrows: true);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_Device_GreaterThan() =>
            ExecuteUnsupported(Property.Device, FilterOperator.GreaterThan, Settings.DeviceName);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_Device_LessThan() =>
            ExecuteUnsupported(Property.Device, FilterOperator.LessThan, Settings.DeviceName);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_Device_Contains() =>
            ExecuteSensor(s => s.Device.Contains(Settings.DeviceName), Property.Device, Settings.DeviceName, FilterOperator.Contains);

        #endregion
        #region DownDuration

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_DownDuration_Equals()
        {
            Retry(retry =>
            {
                var downDuration = client.GetSensor(Settings.DownSensor).DownDuration.Value;
                ExecuteSensor(s => s.DownDuration == downDuration, Property.DownDuration, downDuration, retry: retry);
            });
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_DownDuration_NotEquals()
        {
            Retry(retry =>
            {
                var downDuration = client.GetSensor(Settings.DownSensor).DownDuration.Value;
                ExecuteSensor(s => s.DownDuration != downDuration, Property.DownDuration, downDuration, FilterOperator.NotEquals, retry: retry);
            });
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_DownDuration_GreaterThan()
        {
            Retry(retry =>
            {
                var downDuration = client.GetSensor(Settings.DownSensor).DownDuration.Value - TimeSpan.FromSeconds(5);
                ExecuteSensor(s => s.DownDuration > downDuration, Property.DownDuration, downDuration, FilterOperator.GreaterThan, retry: retry);
            });
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_DownDuration_LessThan()
        {
            Retry(retry =>
            {
                var downDuration = client.GetSensor(Settings.DownSensor).DownDuration.Value + new TimeSpan(0, 1, 0);
                ExecuteSensor(s => s.DownDuration < downDuration, Property.DownDuration, downDuration, FilterOperator.LessThan, s => s.DownDuration == null);
            });
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_DownDuration_Contains()
        {
            Retry(retry =>
            {
                var downDuration = client.GetSensor(Settings.DownSensor).DownDuration.Value;
                ExecuteSensor(s => s.DownDuration.ToString().Contains(downDuration.ToString()), Property.DownDuration, downDuration, FilterOperator.Contains, retry: retry);
            });
        }

        #endregion
        #region Downtime

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_Downtime_Equals()
        {
            var downSensor = client.GetSensor(Settings.DownSensor);

            ExecuteSensor(s => s.Downtime == downSensor.Downtime, Property.Downtime, downSensor.Downtime);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_Downtime_NotEquals()
        {
            var downSensor = client.GetSensor(Settings.DownSensor);

            ExecuteSensor(s => s.Downtime != downSensor.Downtime, Property.Downtime, downSensor.Downtime, FilterOperator.NotEquals);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_Downtime_GreaterThan()
        {
            var downSensor = client.GetSensor(Settings.DownSensor);

            ExecuteSensor(s => s.Downtime > downSensor.Downtime, Property.Downtime, downSensor.Downtime, FilterOperator.GreaterThan);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_Downtime_LessThan()
        {
            var downtime = client.GetSensor(Settings.DownSensor).Downtime.Value + 10;

            ExecuteSensor(s => s.Downtime < downtime, Property.Downtime, downtime, FilterOperator.LessThan);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_Downtime_Contains()
        {
            var upSensor = client.GetSensor(Settings.UpSensor);

            Assert.AreNotEqual(0, upSensor.Downtime);

            ExecuteSensor(s => s.Downtime.ToString().Contains(upSensor.Downtime.ToString()), Property.Downtime, upSensor.Downtime, FilterOperator.Contains, s => s.Downtime == null);
        }

        #endregion
        #region LastCheck

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_LastCheck_Equals()
        {
            Retry(retry =>
            {
                var upSensor = client.GetSensor(Settings.UpSensor);
                ExecuteSensor(s => s.LastCheck == upSensor.LastCheck, Property.LastCheck, upSensor.LastCheck, retry: retry);
            });

        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_LastCheck_NotEquals()
        {
            Retry(retry =>
            {
                var upSensor = client.GetSensor(Settings.UpSensor);
                ExecuteSensor(s => s.LastCheck != upSensor.LastCheck, Property.LastCheck, upSensor.LastCheck, FilterOperator.NotEquals);
            });
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_LastCheck_GreaterThan()
        {
            Retry(retry =>
            {
                var upSensor = client.GetSensor(Settings.UpSensor);
                ExecuteSensor(s => s.LastCheck > upSensor.LastCheck, Property.LastCheck, upSensor.LastCheck, FilterOperator.GreaterThan);
            });
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_LastCheck_LessThan()
        {
            Retry(retry =>
            {
                var upSensor = client.GetSensor(Settings.UpSensor);
                ExecuteSensor(s => s.LastCheck < upSensor.LastCheck.Value.AddSeconds(20), Property.LastCheck, upSensor.LastCheck.Value.AddSeconds(20), FilterOperator.LessThan, s => s.LastCheck == null);
            });
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_LastCheck_Contains()
        {
            Retry(retry =>
            {
                var sensor = client.GetSensors(Property.Name, "System Health").Single();
                ExecuteSensor(s => s.LastCheck.ToString().Contains(sensor.LastCheck.ToString()), Property.LastCheck, sensor.LastCheck, FilterOperator.Contains, retry: retry);
            });
        }

        #endregion
        #region LastUp

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_LastUp_Equals()
        {
            Retry(retry =>
            {
                var upSensor = client.GetSensor(Settings.UpSensor);
                ExecuteSensor(s => s.LastUp == upSensor.LastUp, Property.LastUp, upSensor.LastUp, retry: retry);
            });
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_LastUp_NotEquals()
        {
            Retry(retry =>
            {
                var upSensor = client.GetSensor(Settings.UpSensor);
                ExecuteSensor(s => s.LastUp != upSensor.LastUp, Property.LastUp, upSensor.LastUp, FilterOperator.NotEquals, retry: retry);
            });
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_LastUp_GreaterThan()
        {
            Retry(retry =>
            {
                var upSensor = client.GetSensor(Settings.UpSensor);
                ExecuteSensor(s => s.LastUp > upSensor.LastUp, Property.LastUp, upSensor.LastUp, FilterOperator.GreaterThan, retry: retry);
            });
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_LastUp_LessThan()
        {
            Retry(retry =>
            {
                var upSensor = client.GetSensor(Settings.UpSensor);
                ExecuteSensor(s => s.LastUp < upSensor.LastUp, Property.LastUp, upSensor.LastUp, FilterOperator.LessThan, s => s.LastUp == null, retry: retry);
            });
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_LastUp_Contains()
        {
            Retry(retry =>
            {
                var sensor = client.GetSensors(Property.Name, "System Health").Single();
                ExecuteSensor(s => s.LastUp.ToString().Contains(sensor.LastUp.ToString()), Property.LastUp, sensor.LastUp, FilterOperator.Contains);
            });
        }

        #endregion
        #region LastDown

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_LastDown_Equals()
        {
            Retry(retry =>
            {
                var downSensor = client.GetSensor(Settings.DownSensor);
                ExecuteSensor(s => s.LastDown == downSensor.LastDown, Property.LastDown, downSensor.LastDown, retry: retry);
            });
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_LastDown_NotEquals()
        {
            var downSensor = client.GetSensor(Settings.DownSensor);
            ExecuteSensor(s => s.LastDown != downSensor.LastDown, Property.LastDown, downSensor.LastDown, FilterOperator.NotEquals);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_LastDown_GreaterThan()
        {
            var lastDown = client.GetSensor(Settings.DownSensor).LastDown.Value.AddDays(-1);
            ExecuteSensor(s => s.LastDown > lastDown, Property.LastDown, lastDown, FilterOperator.GreaterThan);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_LastDown_LessThan()
        {
            var lastDown = client.GetSensor(Settings.DownSensor).LastDown.Value.AddDays(1);
            ExecuteSensor(s => s.LastDown < lastDown, Property.LastDown, lastDown, FilterOperator.LessThan, s => s.LastDown == null);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_LastDown_Contains()
        {
            var downSensor = client.GetSensor(Settings.DownSensor);
            ExecuteSensor(s => s.LastDown.ToString().Contains(downSensor.LastDown.ToString()), Property.LastDown, downSensor.LastDown, FilterOperator.Contains);
        }

        #endregion
        #region LastValue

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_LastValue_Equals()
        {
            var sensor = GetLastValueSensor(Property.Id, Settings.UpSensor);

            AssertEx.IsTrue(sensor.LastValue % 1 == 0, $"Expected LastValue to be an integer value however instead value was '{sensor.LastValue}'");

            ExecuteSensor(s => s.LastValue == sensor.LastValue, Property.LastValue, sensor.LastValue);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_LastValue_Equals_WithDecimalPlaces()
        {
            var sensor = GetLastValueSensor(Property.Name, "Uptime");

            AssertEx.IsTrue(sensor.LastValue % 1 != 0, $"Expected LastValue to be a decimal value however instead value was '{sensor.LastValue}'");

            ExecuteSensor(s => s.LastValue == sensor.LastValue, Property.LastValue, sensor.LastValue);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_LastValue_NotEquals()
        {
            var sensor = GetLastValueSensor(Property.Id, Settings.UpSensor);

            ExecuteSensor(s => s.LastValue != sensor.LastValue, Property.LastValue, sensor.LastValue, FilterOperator.NotEquals);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_LastValue_GreaterThan()
        {
            var sensor = GetLastValueSensor(Property.Name, "Uptime");

            var value = sensor.LastValue - 1;

            ExecuteSensor(s => s.LastValue > value, Property.LastValue, value, FilterOperator.GreaterThan);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_LastValue_LessThan()
        {
            var sensor = GetLastValueSensor(Property.Name, "Uptime");

            var value = sensor.LastValue + 10;

            ExecuteSensor(s => s.LastValue < value, Property.LastValue, value, FilterOperator.LessThan, s => s.LastValue == null);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_LastValue_Contains()
        {
            var sensor = GetLastValueSensor(Property.Name, "Uptime");

            ExecuteSensor(s => s.LastValue.ToString().Contains(sensor.LastValue.ToString()), Property.LastValue, sensor.LastValue, FilterOperator.Contains);
        }

        internal Sensor GetLastValueSensor(Property property, object value)
        {
            var sensor = client.GetSensors(property, value).Single();

            var count = 0;

            while (sensor.LastValue == null)
            {
                count++;
                CheckAndSleep(sensor.Id);
                sensor = client.GetSensor(sensor.Id);

                if (count == 30)
                    throw new Exception($"LastValue never initialized. Sensor status was {sensor.Status}. Something is not right here");
            }

            return sensor;
        }

        #endregion
        #region MiniGraph

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_MiniGraph_Equals()
        {
            var sensor = client.GetSensors().First(s => s.MiniGraph != null);

            ExecuteSensor(s => s.MiniGraph == sensor.MiniGraph, Property.MiniGraph, sensor.MiniGraph);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_MiniGraph_NotEquals()
        {
            var sensor = client.GetSensors().First(s => s.MiniGraph != null);

            ExecuteSensor(s => s.MiniGraph != sensor.MiniGraph, Property.MiniGraph, sensor.MiniGraph, FilterOperator.NotEquals, filterThrows: true);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_MiniGraph_GreaterThan()
        {
            var sensor = client.GetSensors().First(s => s.MiniGraph != null);

            ExecuteUnsupported(Property.MiniGraph, FilterOperator.GreaterThan, sensor.MiniGraph);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_MiniGraph_LessThan()
        {
            var sensor = client.GetSensors().First(s => s.MiniGraph != null);

            ExecuteUnsupported(Property.MiniGraph, FilterOperator.LessThan, sensor.MiniGraph);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_MiniGraph_Contains()
        {
            var sensor = client.GetSensors().First(s => s.MiniGraph != null);

            ExecuteSensor(s => s.MiniGraph.Contains(sensor.MiniGraph), Property.MiniGraph, sensor.MiniGraph, FilterOperator.Contains, s => s.MiniGraph != null);
        }

        #endregion
        #region TotalDowntime

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_TotalDowntime_Equals()
        {
            var downtime = client.GetSensor(Settings.DownSensor).TotalDowntime;

            Assert.IsNotNull(downtime);

            ExecuteSensor(s => s.TotalDowntime == downtime, Property.TotalDowntime, downtime);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_TotalDowntime_NotEquals()
        {
            var downtime = client.GetSensor(Settings.DownSensor).TotalDowntime;

            Assert.IsNotNull(downtime);

            ExecuteSensor(s => s.TotalDowntime != downtime, Property.TotalDowntime, downtime, FilterOperator.NotEquals);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_TotalDowntime_GreaterThan()
        {
            var downtime = client.GetSensor(Settings.DownSensor).TotalDowntime - TimeSpan.FromMinutes(1);

            Assert.IsNotNull(downtime);

            ExecuteSensor(s => s.TotalDowntime > downtime, Property.TotalDowntime, downtime, FilterOperator.GreaterThan);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_TotalDowntime_LessThan()
        {
            var downtime = client.GetSensor(Settings.DownSensor).TotalDowntime;

            Assert.IsNotNull(downtime);

            ExecuteSensor(s => s.TotalDowntime < downtime, Property.TotalDowntime, downtime, FilterOperator.LessThan);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_TotalDowntime_Contains()
        {
            var downtime = client.GetSensor(Settings.DownSensor).TotalDowntime;

            Assert.IsNotNull(downtime);

            ExecuteSensor(s => s.TotalDowntime.ToString().Contains(downtime.ToString()), Property.TotalDowntime, downtime);
        }

        #endregion
        #region TotalMonitorTime

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_TotalMonitorTime_Equals()
        {
            var monitorTime = client.GetSensor(Settings.UpSensor).TotalMonitorTime;

            Assert.IsNotNull(monitorTime);

            ExecuteSensor(s => s.TotalMonitorTime == monitorTime, Property.TotalMonitorTime, monitorTime);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_TotalMonitorTime_NotEquals()
        {
            var monitorTime = client.GetSensor(Settings.UpSensor).TotalMonitorTime;

            Assert.IsNotNull(monitorTime);

            ExecuteSensor(s => s.TotalMonitorTime != monitorTime, Property.TotalMonitorTime, monitorTime, FilterOperator.NotEquals);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_TotalMonitorTime_GreaterThan()
        {
            var monitorTime = client.GetSensor(Settings.UpSensor).TotalMonitorTime;

            Assert.IsNotNull(monitorTime);

            ExecuteSensor(s => s.TotalMonitorTime > monitorTime, Property.TotalMonitorTime, monitorTime, FilterOperator.GreaterThan);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_TotalMonitorTime_LessThan()
        {
            var monitorTime = client.GetSensor(Settings.UpSensor).TotalMonitorTime;

            Assert.IsNotNull(monitorTime);

            ExecuteSensor(s => s.TotalMonitorTime < monitorTime, Property.TotalMonitorTime, monitorTime, FilterOperator.LessThan);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_TotalMonitorTime_Contains()
        {
            var monitorTime = client.GetSensor(Settings.UpSensor).TotalMonitorTime;

            Assert.IsNotNull(monitorTime);

            ExecuteSensor(s => s.TotalMonitorTime.ToString().Contains(monitorTime.ToString()), Property.TotalMonitorTime, monitorTime);
        }

        #endregion
        #region TotalUptime

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_TotalUptime_Equals()
        {
            var uptime = client.GetSensor(Settings.UpSensor).TotalUptime;

            Assert.IsNotNull(uptime);

            ExecuteSensor(s => s.TotalUptime == uptime, Property.TotalUptime, uptime);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_TotalUptime_NotEquals()
        {
            var uptime = client.GetSensor(Settings.UpSensor).TotalUptime;

            Assert.IsNotNull(uptime);

            ExecuteSensor(s => s.TotalUptime != uptime, Property.TotalUptime, uptime, FilterOperator.NotEquals);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_TotalUptime_GreaterThan()
        {
            var uptime = client.GetSensor(Settings.UpSensor).TotalUptime;

            Assert.IsNotNull(uptime);

            ExecuteSensor(s => s.TotalUptime > uptime, Property.TotalUptime, uptime, FilterOperator.GreaterThan);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_TotalUptime_LessThan()
        {
            var uptime = client.GetSensor(Settings.UpSensor).TotalUptime;

            Assert.IsNotNull(uptime);

            ExecuteSensor(s => s.TotalUptime < uptime, Property.TotalUptime, uptime, FilterOperator.LessThan);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_TotalUptime_Contains()
        {
            var uptime = client.GetSensor(Settings.UpSensor).TotalUptime;

            Assert.IsNotNull(uptime);

            ExecuteSensor(s => s.TotalUptime.ToString().Contains(uptime.ToString()), Property.TotalUptime, uptime, FilterOperator.Contains);
        }

        #endregion
        #region UpDuration

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_UpDuration_Equals()
        {
            Retry(retry =>
            {
                var duration = client.GetSensor(Settings.UpSensor).UpDuration;

                Assert.IsNotNull(duration);

                ExecuteSensor(s => s.UpDuration == duration, Property.UpDuration, duration, retry: retry);
            });
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_UpDuration_NotEquals()
        {
            Retry(retry =>
            {
                var duration = client.GetSensor(Settings.UpSensor).UpDuration;

                Assert.IsNotNull(duration);

                ExecuteSensor(s => s.UpDuration != duration, Property.UpDuration, duration, FilterOperator.NotEquals, retry: retry);
            });
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_UpDuration_GreaterThan()
        {
            Retry(retry =>
            {
                var duration = client.GetSensor(Settings.UpSensor).UpDuration;

                Assert.IsNotNull(duration);

                ExecuteSensor(s => s.UpDuration > duration, Property.UpDuration, duration, FilterOperator.GreaterThan, retry: retry);
            });
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_UpDuration_LessThan()
        {
            Retry(retry =>
            {
                var duration = client.GetSensor(Settings.UpSensor).UpDuration;

                if (duration == null)
                    CheckAndSleep(Settings.UpSensor);

                duration = client.GetSensor(Settings.UpSensor).UpDuration;

                Assert.IsNotNull(duration);

                ExecuteSensor(s => s.UpDuration < duration, Property.UpDuration, duration, FilterOperator.LessThan, s => s.UpDuration == null, retry: retry);
            });
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_UpDuration_Contains()
        {
            Retry(retry =>
            {
                var duration = client.GetSensor(Settings.UpSensor).UpDuration;

                Assert.IsNotNull(duration);

                ExecuteSensor(s => s.UpDuration.ToString().Contains(duration.ToString()), Property.UpDuration, duration, retry: retry);
            });
        }

        #endregion
        #region Uptime

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_Uptime_Equals()
        {
            var upSensor = client.GetSensor(Settings.UpSensor);

            ExecuteSensor(s => s.Uptime == upSensor.Uptime, Property.Uptime, upSensor.Uptime);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_Uptime_NotEquals()
        {
            var upSensor = client.GetSensor(Settings.UpSensor);

            ExecuteSensor(s => s.Uptime != upSensor.Uptime, Property.Uptime, upSensor.Uptime, FilterOperator.NotEquals);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_Uptime_GreaterThan()
        {
            var upSensor = client.GetSensor(Settings.UpSensor);

            ExecuteSensor(s => s.Uptime > upSensor.Uptime, Property.Uptime, upSensor.Uptime, FilterOperator.GreaterThan);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_Uptime_LessThan()
        {
            var upSensor = client.GetSensor(Settings.UpSensor);

            ExecuteSensor(s => s.Uptime < upSensor.Uptime, Property.Uptime, upSensor.Uptime, FilterOperator.LessThan);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_SensorProperties_Uptime_Contains()
        {
            var upSensor = client.GetSensor(Settings.UpSensor);

            ExecuteSensor(s => s.Uptime.ToString().Contains(upSensor.Uptime.ToString()), Property.Uptime, upSensor.Uptime, FilterOperator.Contains);
        }

        #endregion
        #endregion
        #region Device
        #region Condition

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceProperties_Condition_Equals()
        {
            PrepareCondition(device =>
            {
                ExecuteDevice(d => d.Condition == device.Condition, Property.Condition, device.Condition);
            });
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceProperties_Condition_NotEquals()
        {
            PrepareCondition(device =>
            {
                ExecuteDevice(d => d.Condition != device.Condition, Property.Condition, device.Condition, FilterOperator.NotEquals, filterThrows: true);
            });
        }

        [TestMethod]
        [TestCategory("Unreliable")]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceProperties_Condition_GreaterThan()
        {
            PrepareCondition(device =>
            {
                ExecuteDeviceUnsupported(Property.Condition, FilterOperator.GreaterThan, device.Condition);
            });
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceProperties_Condition_LessThan()
        {
            PrepareCondition(device =>
            {
                ExecuteDeviceUnsupported(Property.Condition, FilterOperator.LessThan, device.Condition);
            });
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceProperties_Condition_Contains()
        {
            PrepareCondition(device =>
            {
                ExecuteDevice(d => d.Condition.Contains(device.Condition), Property.Condition, device.Condition, FilterOperator.Contains);
            });
        }

        private void PrepareCondition(Action<Device> action, bool inner = false)
        {
            try
            {
                var device = client.GetDevice(Settings.Device);

                if (device.Condition != null && device.Condition.Contains("recommendation in progress"))
                {
                    Logger.LogTestDetail("Sensor recommendation in progress. Pausing device");
                    client.PauseObject(device.Id);
                    client.RefreshObject(device.Id);
                    Thread.Sleep(5000);
                    client.ResumeObject(device.Id);
                }
                else
                {
                    AssertEx.AreEqual(null, device.Condition, "Expected device condition to be null");
                }

                client.AutoDiscover(Settings.Device);

                device = client.GetDevice(Settings.Device);

                for (var i = 0; i < 10; i++)
                {
                    if (device.Condition == null)
                    {
                        client.RefreshObject(Settings.Device);
                        Thread.Sleep(5000);
                        device = client.GetDevice(Settings.Device);
                    }
                }

                if (device.Condition.Contains("Sensor recommendation in progress"))
                    PrepareCondition(action, true);
                else
                {
                    Assert.IsTrue(device.Condition?.Contains("Auto-Discovery") == true, $"Expected condition to contain the words 'Auto-Discovery', however instead contained '{device.Condition}'");

                    action(device);
                }
            }
            finally
            {
                if (!inner)
                {
                    ServerManager.RepairConfig();
                    ServerManager.WaitForObjects();
                }
            }
        }

        #endregion
        #region Favorite

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceProperties_Favorite_Equals() =>
            ExecuteDevice(d => d.Favorite, Property.Favorite, true);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceProperties_Favorite_NotEquals() =>
            ExecuteDevice(d => d.Favorite, Property.Favorite, false, FilterOperator.NotEquals);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceProperties_Favorite_GreaterThan() =>
            ExecuteUnsupported(Property.Favorite, FilterOperator.GreaterThan, true);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceProperties_Favorite_LessThan() =>
            ExecuteUnsupported(Property.Favorite, FilterOperator.LessThan, false);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceProperties_Favorite_Contains() =>
            ExecuteDevice(d => d.Favorite.ToString().Contains(true.ToString()), Property.Favorite, true, FilterOperator.Contains);

        #endregion
        #region Host

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceProperties_Host_Equals() =>
            ExecuteDevice(d => d.Host == Settings.DeviceName, Property.Host, Settings.DeviceName);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceProperties_Host_NotEquals() =>
            ExecuteDevice(d => d.Host != Settings.DeviceName, Property.Host, Settings.DeviceName, FilterOperator.NotEquals, filterThrows: true);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceProperties_Host_GreaterThan() =>
            ExecuteDeviceUnsupported(Property.Host, FilterOperator.GreaterThan, Settings.DeviceName);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceProperties_Host_LessThan() =>
            ExecuteDeviceUnsupported(Property.Host, FilterOperator.LessThan, Settings.DeviceName);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceProperties_Host_Contains() =>
            ExecuteDevice(d => d.Host.Contains(Settings.DeviceName), Property.Host, Settings.DeviceName, FilterOperator.Contains);

        #endregion
        #region Group

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceProperties_Group_Equals() =>
            ExecuteDevice(d => d.Group == Settings.GroupName, Property.Group, Settings.GroupName);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceProperties_Group_NotEquals() =>
            ExecuteDevice(d => d.Group != Settings.GroupName, Property.Group, Settings.GroupName, FilterOperator.NotEquals, filterThrows: true);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceProperties_Group_GreaterThan() =>
            ExecuteDeviceUnsupported(Property.Group, FilterOperator.GreaterThan, Settings.GroupName);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceProperties_Group_LessThan() =>
            ExecuteDeviceUnsupported(Property.Group, FilterOperator.LessThan, Settings.GroupName);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceProperties_Group_Contains() =>
            ExecuteDevice(d => d.Group.Contains(Settings.GroupName), Property.Group, Settings.GroupName, FilterOperator.Contains);

        #endregion
        #region Location

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceProperties_Location_Equals() =>
            ExecuteDevice(d => d.Location == Settings.Location, Property.Location, Settings.Location);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceProperties_Location_NotEquals() =>
            ExecuteDevice(d => d.Location != Settings.Location, Property.Location, Settings.Location, FilterOperator.NotEquals, filterThrows: true);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceProperties_Location_GreaterThan() =>
            ExecuteDeviceUnsupported(Property.Location, FilterOperator.GreaterThan, Settings.Location);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceProperties_Location_LessThan() =>
            ExecuteDeviceUnsupported(Property.Location, FilterOperator.LessThan, Settings.Location);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceProperties_Location_Contains() =>
            ExecuteDevice(d => d.Location.Contains(Settings.Location), Property.Location, Settings.Location, FilterOperator.Contains);

        #endregion
        #region Probe

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceProperties_Probe_Equals() =>
            ExecuteDevice(d => d.Probe == Settings.ProbeName, Property.Probe, Settings.ProbeName);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceProperties_Probe_NotEquals() =>
            ExecuteDevice(d => d.Probe != Settings.ProbeName, Property.Probe, Settings.ProbeName, FilterOperator.NotEquals, filterThrows: true);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceProperties_Probe_GreaterThan() =>
            ExecuteDeviceUnsupported(Property.Probe, FilterOperator.GreaterThan, Settings.ProbeName);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceProperties_Probe_LessThan() =>
            ExecuteDeviceUnsupported(Property.Probe, FilterOperator.LessThan, Settings.ProbeName);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_DeviceProperties_Probe_Contains() =>
            ExecuteDevice(d => d.Probe.Contains(Settings.ProbeName), Property.Probe, Settings.ProbeName, FilterOperator.Contains);

        #endregion
        #endregion
        #region Probe
        #region ProbeStatus

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_ProbeProperties_ProbeStatus_Equals() =>
            ExecuteProbe(p => p.ProbeStatus == ProbeStatus.Connected, Property.ProbeStatus, ProbeStatus.Connected);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_ProbeProperties_ProbeStatus_NotEquals() =>
            ExecuteProbe(p => p.ProbeStatus != ProbeStatus.Disconnected, Property.ProbeStatus, ProbeStatus.Disconnected, FilterOperator.NotEquals);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_ProbeProperties_ProbeStatus_GreaterThan() =>
            ExecuteProbe(p => p.ProbeStatus > ProbeStatus.Disconnected, Property.ProbeStatus, ProbeStatus.Disconnected, FilterOperator.GreaterThan);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_ProbeProperties_ProbeStatus_LessThan() =>
            ExecuteSerialized(Property.ProbeStatus, FilterOperator.LessThan, 2);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_ProbeProperties_ProbeStatus_Contains() =>
            ExecuteProbe(p => p.ProbeStatus.ToString().Contains(ProbeStatus.Connected.ToString()), Property.ProbeStatus, ProbeStatus.Connected, FilterOperator.Contains);

        #endregion
        #endregion

        #region Log
        #region Date Ranges

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_LogProperties_DateTime_Equals() => ExecuteLog(
            l => l.DateTime == Time.Yesterday,
            c => c.GetLogs(Time.Yesterday, Time.Yesterday, null),
            true
        );

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_LogProperties_DateTime_DoubleEquals() => ExecuteIllegalLog<InvalidOperationException>(
            l => l.DateTime == Time.Yesterday || l.DateTime == Time.Today,
            "At least one end of a valid date range must be specified"
        );

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_LogProperties_DateTime_NotEquals() => ExecuteIllegalLog<InvalidOperationException>(
            l => l.DateTime != Time.Yesterday,
            "At least one end of a valid date range must be specified"
        );

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_LogProperties_DateTime_BackwardsAndForwards()
        {
            var forwards = LogTests.OrderLogs(client.QueryLogs(l => l.DateTime < DateTime.Now).Take(200).AsEnumerable()).ToList();
            var backwards = LogTests.OrderLogs(client.QueryLogs(l => DateTime.Now > l.DateTime).Take(200).AsEnumerable()).ToList();

            AssertEx.AreEqualLists(forwards, backwards, PrtgAPIHelpers.LogEqualityComparer(), "Lists were not equal");
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_LogProperties_DateTime_Illegal_WithRange() => ExecuteLog(
            l => l.DateTime > Time.LastWeek && l.DateTime != Time.TwoDaysAgo && l.DateTime < Time.Yesterday,
            c => c.StreamLogs(Time.Yesterday, Time.LastWeek).Where(l => l.DateTime != Time.TwoDaysAgo).ToList()
        );

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_LogProperties_DateTime_GreaterThan() => ExecuteLog(
            l => l.DateTime > Time.Yesterday && l.Id == Settings.UpSensor,
            c => c.GetLogs(Settings.UpSensor, null, Time.Yesterday, null)
        );

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_LogProperties_DateTime_LessThan() => ExecuteLog(
            l => l.DateTime < Time.Yesterday,
            l => l.StreamLogs(Time.Yesterday)
        );

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_LogProperties_DateTime_DoubleGreaterThan() => ExecuteLog(
            l => l.DateTime > Time.TwoDaysAgo && l.DateTime > Time.Yesterday,
            c => c.StreamLogs(null, Time.TwoDaysAgo),
            applyPredicate: true
        );

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_LogProperties_DateTime_DoubleGreaterThan_LessThan() => ExecuteLog(
            l => l.DateTime > Time.LastWeek && l.DateTime > Time.TwoDaysAgo && l.DateTime < Time.Yesterday,
            c => c.StreamLogs(Time.Yesterday, Time.LastWeek),
            applyPredicate: true
        );

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_LogProperties_DateTime_DoubleLessThan() => ExecuteLog(
            l => l.DateTime < Time.Yesterday && l.DateTime < Time.TwoDaysAgo,
            c => c.StreamLogs(Time.Yesterday),
            applyPredicate: true
        );

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_LogProperties_DateTime_DoubleLessThan_GreaterThan() => ExecuteLog(
            l => l.DateTime < Time.Yesterday && l.DateTime < Time.TwoDaysAgo && l.DateTime > Time.LastWeek,
            c => c.StreamLogs(Time.Yesterday, Time.LastWeek),
            applyPredicate: true
        );

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_LogProperties_DateTime_Range() => ExecuteLog(
            l => l.DateTime > Time.Yesterday && l.DateTime < Time.Today,
            c => c.StreamLogs(Time.Today, Time.Yesterday)
        );

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_LogProperties_DateTime_DoubleRange() => ExecuteLog(
            l => l.DateTime > Time.Yesterday && l.DateTime < Time.Today || l.DateTime > Time.TwoWeeksAgo && l.DateTime < Time.LastWeek,
            c => c.StreamLogs(Time.Today, Time.Yesterday, serial: true).Union(c.StreamLogs(Time.LastWeek, Time.TwoWeeksAgo, serial: true)).ToList(),
            distinct: true
        );

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_LogProperties_DateTime_Range_Or_PartialRangeStart() => ExecuteLog(
            l => l.DateTime > Time.Yesterday && l.DateTime < Time.Today || l.DateTime > Time.TwoDaysAgo,
            c => c.StreamLogs(Time.Today, Time.Yesterday).Union(c.StreamLogs(null, Time.TwoDaysAgo)).ToList(),
            distinct: true
        );

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_LogProperties_DateTime_Range_Or_PartialRangeEnd() => ExecuteLog(
            l => l.DateTime > Time.Yesterday && l.DateTime < Time.Today || l.DateTime < Time.TwoDaysAgo,
            c => c.StreamLogs(Time.Today, Time.Yesterday, serial: true).Union(c.StreamLogs(Time.TwoDaysAgo, serial: true)).ToList(),
            distinct: true
        );

        [TestMethod]
        [TestCategory("Unreliable")]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_LogProperties_DateTime_PartialRange_WithId()
        {
            var sensor = client.GetSensors(Property.Tags, "wmimemorysensor").Single();

            ExecuteLog(
                l => l.DateTime > Time.TwoDaysAgo && l.Id == sensor.Id,
                c => c.StreamLogs(sensor.Id, endDate: Time.TwoDaysAgo, serial: true)
            );
        } 

            #endregion
        
        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_LogProperties_Name() => ExecuteLog(
            l => l.Name == Settings.DeviceName && l.DateTime > DateTime.Now.AddDays(-1),
            c => c.GetLogs(DateTime.Now, DateTime.Now.AddDays(-1), null),
            applyPredicate: true
        );

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_LogProperties_Device() => ExecuteLog(
            l => l.Device == "Probe Device" && l.DateTime > Time.Yesterday && l.DateTime < Time.Today,
            c => c.GetLogs(Time.Today, Time.Yesterday, null),
            applyPredicate: true
        );

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_LogProperties_Group() => ExecuteLog(
            l => l.Group == Settings.GroupName && l.DateTime > Time.Yesterday && l.DateTime < Time.Today,
            c => c.GetLogs(Time.Today, Time.Yesterday, null),
            applyPredicate: true
        );

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_LogProperties_Parent() => ExecuteLog(
            l => l.Parent == "Probe Device" && l.DateTime > Time.Yesterday && l.DateTime < Time.Today,
            c => c.GetLogs(Time.Today, Time.Yesterday, null),
            applyPredicate: true
        );

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_LogProperties_Probe() => ExecuteLog(
            l => l.Probe == Settings.ProbeName && l.DateTime > Time.Yesterday && l.DateTime < Time.Today,
            c => c.GetLogs(Time.Today, Time.Yesterday, null),
            applyPredicate: true
        );

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_LogProperties_Message() => ExecuteLog(
            l => l.Message.Contains("Performance") && l.DateTime > Time.Yesterday && l.DateTime < Time.Today,
            c => c.GetLogs(Time.Today, Time.Yesterday, null),
            applyPredicate: true
        );

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_LogProperties_Sensor()
        {
            var sensors = client.GetSensors();
            var logs = client.GetLogs(count: 2000);

            var log = logs.FirstOrDefault(l => sensors.Any(s => s.Id == l.Id));

            if (log == null)
                Assert.Fail("Couldn't find an event that applied to a sensor");

            Logger.LogTestDetail($"Searching for logs for sensor '{log.Name}'");

            Retry(retry =>
            {
                var sensor = client.GetSensors(Property.Name, log.Name).First().Name;

                ExecuteLog(
                    l => l.Sensor == sensor && l.DateTime > Time.Yesterday && l.DateTime < Time.Today,
                    c => c.GetLogs(Time.Today, Time.Yesterday, null),
                    applyPredicate: true,
                    retry: retry
                );
            });
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_LogProperties_Status() => ExecuteLog(
            l => l.Status == LogStatus.Connected || l.Status == LogStatus.Disconnected,
            l => l.GetLogs(RecordAge.All, 3000, LogStatus.Connected, LogStatus.Disconnected)
        );

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void QueryFilter_LogProperties_IdOrId() => ExecuteLog(
            l => l.Id == Settings.UpSensor || l.Id == Settings.DownSensor,
            c => c.StreamLogs(Settings.UpSensor).Union(c.StreamLogs(Settings.DownSensor)).ToList(),
            distinct: true
        );

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void QueryFilter_LogProperties_IdAndId() => ExecuteLog(
            l => l.Id == Settings.UpSensor && l.Id == Settings.DownSensor,
            c => c.StreamLogs(Settings.UpSensor),
            true,
            true
        );

        #endregion
        #region Boolean

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_BooleanProperties_Active_True() => ExecuteSensor(s => s.Active, Property.Active, true);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_BooleanProperties_Active_False() => ExecuteSensor(s => !s.Active, Property.Active, false);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_BooleanProperties_Collapsed_True()
        {
            var hasCollapsed = client.GetGroups().Any(g => g.Collapsed);

            if (!hasCollapsed)
                PrtgAPIHelpers.FoldObject(client, Settings.Group, true);

            ExecuteGroup(g => g.Collapsed, Property.Collapsed, true);

            if (!hasCollapsed)
                PrtgAPIHelpers.FoldObject(client, Settings.Group, false);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_BooleanProperties_Collapsed_False() => ExecuteGroup(g => !g.Collapsed, Property.Collapsed, false);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_BooleanProperties_Favorite_True() => ExecuteSensor(s => s.Favorite, Property.Favorite, true);

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_QueryFilter_BooleanProperties_Favorite_False()
        {
            AssertEx.Throws<NotSupportedException>(() => ExecuteSensor(s => !s.Favorite, Property.Favorite, false), "Cannot filter where property 'Favorite' equals '0'");
        }

        #endregion
        
        private void ExecuteSensor(
            Expression<Func<Sensor, bool>> predicate, Property property, object value, FilterOperator op = FilterOperator.Equals,
            Func<Sensor, bool> ignoreFilterPredicate = null, bool filterThrows = false, bool filterUnsupported = false, bool retry = false)
        {
            ExecuteObject(predicate,
                () => client.GetSensors(),
                () => client.GetSensors(property, op, value),
                strict => client.QuerySensors(predicate, strict),
                new SearchFilter(property, op, value, FilterMode.Illegal),
                ignoreFilterPredicate,
                filterThrows: filterThrows,
                filterUnsupported: filterUnsupported,
                retry: retry
            );
        }

        private void ExecuteDevice(Expression<Func<Device, bool>> predicate, Property property, object value, FilterOperator op = FilterOperator.Equals,
            Func<Device, bool> ignoreFilterPredicate = null, bool filterThrows = false)
        {
            ExecuteObject(predicate,
                () => client.GetDevices(),
                () => client.GetDevices(property, op, value),
                strict => client.QueryDevices(predicate, strict),
                new SearchFilter(property, op, value, FilterMode.Illegal),
                ignoreFilterPredicate,
                filterThrows: filterThrows
            );
        }

        private void ExecuteGroup(Expression<Func<Group, bool>> predicate, Property property, object value, FilterOperator op = FilterOperator.Equals,
            Func<Group, bool> ignoreFilterPredicate = null)
        {
            ExecuteObject(predicate,
                () => client.GetGroups(),
                () => client.GetGroups(property, op, value),
                strict => client.QueryGroups(predicate, strict),
                new SearchFilter(property, op, value, FilterMode.Illegal),
                ignoreFilterPredicate
            );
        }

        private void ExecuteProbe(Expression<Func<Probe, bool>> predicate, Property property, object value, FilterOperator op = FilterOperator.Equals,
            Func<Probe, bool> ignoreFilterPredicate = null)
        {
            ExecuteObject(predicate,
                () => client.GetProbes(),
                () => client.GetProbes(property, op, value),
                strict => client.QueryProbes(predicate, strict),
                new SearchFilter(property, op, value, FilterMode.Illegal),
                ignoreFilterPredicate
            );
        }

        private void ExecuteLog(Expression<Func<Log, bool>> predicate, Func<PrtgClient, IEnumerable<Log>> getFilterObjects,
            bool allowEmpty = false, bool applyPredicate = false, bool distinct = false, bool retry = false)
        {
            var count = 20000;

            ExecuteObject(predicate,
                () => getFilterObjects(client).Take(count).ToList(),
                strict => client.QueryLogs(predicate, strict).Take(count),
                null,
                PrtgAPIHelpers.LogEqualityComparer(),
                allowEmpty,
                applyPredicate,
                distinct,
                retry
            );
        }

        private void ExecuteSerialized(Property property, FilterOperator op, object value)
        {
            var filterObjects = client.GetSensors(property, op, value);

            AssertEx.IsTrue(filterObjects.Count > 0, "Filter objects did not return any results");
        }

        private void ExecuteDeviceSerialized(Property property, FilterOperator op, object value)
        {
            var filterObjects = client.GetDevices(property, op, value);

            AssertEx.IsTrue(filterObjects.Count > 0, "Filter objects did not return any results");
        }

        private void ExecuteGroupSerialized(Property property, FilterOperator op, object value)
        {
            var filterObjects = client.GetGroups(property, op, value);

            AssertEx.IsTrue(filterObjects.Count > 0, "Filter objects did not return any results");
        }

        private void ExecuteUnsupported(Property property, FilterOperator op, object value, bool @throw = true, bool unsupported = false)
        {
            if (@throw)
            {
                var message = unsupported ? "Cannot filter by property" : "Cannot filter where property";

                AssertEx.Throws<NotSupportedException>(() => client.GetSensors(property, op, value), message);

                var results = client.GetSensors(new SearchFilter(property, op, value, FilterMode.Illegal));

                Assert.AreEqual(0, results.Count);
            }
            else
            {
                var results = client.GetSensors(new SearchFilter(property, op, value));

                Assert.AreEqual(0, results.Count);
            }
        }

        private void ExecuteDeviceUnsupported(Property property, FilterOperator op, object value, bool @throw = true)
        {
            if (@throw)
            {
                AssertEx.Throws<NotSupportedException>(() => client.GetDevices(property, op, value), "Cannot filter where property");

                var results = client.GetDevices(new SearchFilter(property, op, value, FilterMode.Illegal));

                Assert.AreEqual(0, results.Count);
            }
            else
            {
                var results = client.GetDevices(new SearchFilter(property, op, value));

                Assert.AreEqual(0, results.Count);
            }
        }

        private void ExecuteIllegalLog<TException>(Expression<Func<Log, bool>> predicate, string message) where TException : Exception
        {
            AssertEx.Throws<TException>(() => client.QueryLogs(predicate).ToList(), message);
        }

        public static void Retry(Action<bool> action)
        {
            for(var i = 5; i >= 0; i--)
            {
                try
                {
                    action(i != 0);
                    break;
                }
                catch(AssertFailedException)
                {
                    if (i == 0)
                        throw;
                    else
                    {
                        Logger.LogTestDetail("Attempt failed. Waiting 30 seconds to retry");

                        Thread.Sleep(30000);
                    }
                }
            }
        }

        private void ExecuteObject<T>(
            Expression<Func<T, bool>> predicate,
            Func<List<T>> getLinqObjects,
            Func<List<T>> getFilterObjects,
            Func<bool, IQueryable<T>> getQueryObjects,
            SearchFilter filter,
            Func<T, bool> ignoreFilterPredicate,
            bool allowEmpty = false,
            bool filterThrows = false,
            bool filterUnsupported = false,
            bool retry = false
        ) where T : PrtgObject
        {
            var lambda = (LambdaExpression) predicate.ToClientExpression();

            var compiled = (Func<T, bool>)lambda.Compile();

            var unfilteredLinqObjects = getLinqObjects();
            var linqObjects = unfilteredLinqObjects.Where(compiled).ToList();

            if (filterThrows)
            {
                var message = filterUnsupported ? "Cannot filter by property" : "Cannot filter where property";

                AssertEx.Throws<NotSupportedException>(() => getQueryObjects(true).ToList(), message);
                AssertEx.Throws<NotSupportedException>(() => getFilterObjects().ToList(), message);

                return;
            }

            var filterObjects = getFilterObjects();
            var queryObjects = getQueryObjects(false).ToList();

            if (!allowEmpty)
            {
                if (retry)
                {
                    Assert.IsTrue(linqObjects.Count > 0);
                    Assert.IsTrue(filterObjects.Count > 0);
                    Assert.IsTrue(queryObjects.Count > 0);
                }

                AssertEx.IsTrue(linqObjects.Count > 0, "LINQ objects did not return any results");
                AssertEx.IsTrue(filterObjects.Count > 0, "Filter objects did not return any results");
                AssertEx.IsTrue(queryObjects.Count > 0, "Query objects did not return any results");
            }

            var missingFiltered = linqObjects.ExceptBy(filterObjects, s => s.Id).ToList();
            var missingQuery = linqObjects.ExceptBy(queryObjects, s => s.Id).ToList();

            var extraFiltered = filterObjects.ExceptBy(linqObjects, s => s.Id).ToList();
            var extraQuery = queryObjects.ExceptBy(linqObjects, s => s.Id).ToList();

            if (ignoreFilterPredicate != null)
                extraFiltered = extraFiltered.Where(e => !ignoreFilterPredicate(e)).ToList();

            var getError = FormatError(missingFiltered, missingQuery, extraFiltered, extraQuery, filter);

            if (getError != null)
            {
                if (retry)
                    Assert.Fail(getError);

                AssertEx.Fail(getError);
            }
        }

        private void ExecuteObject<T>(
            Expression<Func<T, bool>> predicate,
            Func<List<T>> getFilterObjects,
            Func<bool, IQueryable<T>> getQueryObjects,
            Func<T, bool> ignoreFilterPredicate,
            IEqualityComparer<T> comparer,
            bool allowEmpty = false,
            bool applyPredicate = false,
            bool distinct = false,
            bool retry = false
        ) where T : IObject
        {
            var lambda = (LambdaExpression)predicate.ToClientExpression();

            var compiled = (Func<T, bool>)lambda.Compile();

            var filterObjects = getFilterObjects();
            var queryObjects = getQueryObjects(false).ToList();

            var originalFiltered = filterObjects;

            if (applyPredicate)
            {
                var predicatedFilterObjects = filterObjects.Where(compiled).ToList();

                AssertEx.AreNotEqual(filterObjects.Count, predicatedFilterObjects.Count, "Predicated filter objects had the same number of items as non predicated");

                filterObjects = predicatedFilterObjects;
            }

            if (distinct)
                filterObjects = filterObjects.DistinctBy(o => o, comparer).ToList();

            var removedFilterObjects = originalFiltered.Except(filterObjects).ToList();

            if (!allowEmpty)
            {
                AssertEx.IsTrue(filterObjects.Count > 0, "Filter objects did not return any results");
                AssertEx.IsTrue(queryObjects.Count > 0, "Query objects did not return any results");
            }

            var extraFiltered = filterObjects.ExceptBy(queryObjects, o => o, comparer).ToList();
            var extraQuery = queryObjects.ExceptBy(filterObjects, o => o, comparer).ToList();

            if (removedFilterObjects.Count > 0)
            {
                //We got up to removedFilterObjects.Count extra objects. Remove this many
                //from extraQuery

                extraQuery = extraQuery.Take(extraQuery.Count - removedFilterObjects.Count).ToList();
            }

            if (ignoreFilterPredicate != null)
                extraFiltered = extraFiltered.Where(e => !ignoreFilterPredicate(e)).ToList();

            var getError = FormatError(new List<T>(), new List<T>(), extraFiltered, extraQuery);

            if (getError != null)
            {
                if (retry)
                    Assert.Fail(getError);

                AssertEx.Fail(getError);
            }
        }

        private string FormatError<T>(List<T> missingFiltered, List<T> missingQuery, List<T> extraFiltered,
            List<T> extraQuery, SearchFilter filter = null) where T : IObject
        {
            var lambda = filter == null ? null : PrtgAPIHelpers.ToMemberAccessLambda<T>(filter.Property);

            var missingFilteredStr = Join("Missing", missingFiltered);
            var extraFilteredStr = Join("Extra:", extraFiltered, lambda, filter);
            var missingQueryStr = Join("Missing:", missingQuery);
            var extraQueryStr = Join("Extra:", extraQuery, lambda, filter);

            var filteredStr = Join("Filtered:", missingFilteredStr, extraFilteredStr);
            var queryStr = Join("Query:", missingQueryStr, extraQueryStr);

            return Join(null, filteredStr, queryStr);
        }

        private string Join<T>(string prefix, List<T> str, Func<T, object> lambda = null, SearchFilter filter = null) where T : IObject
        {
            var builder = new StringBuilder();

            builder.Append(Join(prefix, str.Select(s => s.ToString()).ToArray()));

            if (lambda != null && str.Count > 0)
            {
                builder.Append($". Expected: {filter.Value}{Environment.NewLine}{Environment.NewLine}Values of each object:{Environment.NewLine}");
                
                for(var i = 0; i < str.Count; i++)
                {
                    builder.Append($"{str[i].Name}: '{lambda(str[i])}'");

                    if (i < str.Count - 1)
                        builder.Append(Environment.NewLine);
                }
            }

            return builder.ToString();
        }

        private string Join(string prefix, params string[] str)
        {
            var newStr = string.Join(", ", str.Where(e => !string.IsNullOrEmpty(e)));

            if (!string.IsNullOrEmpty(newStr))
            {
                if (prefix != null)
                    return prefix + " " + newStr;

                return newStr;
            }

            return null;
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_AllFilterOperators_HaveTests_ForAllProperties()
        {
            var operators = Enum.GetNames(typeof(FilterOperator));

            foreach (var op in operators)
            {
                PrtgObjectFilterTests.AllPrtgObjectProperties_HaveTests(GetType(), "Data_QueryFilter", op, new[] { "PartialDownSensors", "UnusualSensors", "InheritInterval", "DisplayType", "NotificationTypes" });
            }
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_AllLogDateTimeScenarios_HaveTests()
        {
            var sourceMethods = typeof(PrtgObjectFilterTests).GetMethods().Where(
                m => m.GetCustomAttribute<TestMethodAttribute>() != null && m.Name.Contains("LogProperties") && m.Name.Contains("DateTime")
            ).ToList();

            var expectedMethods = sourceMethods.Select(m => $"Data_{m.Name}").ToList();

            var actualMethods = GetType().GetMethods();

            var missing = expectedMethods.Where(e => actualMethods.All(m => m.Name != e)).OrderBy(m => m).ToList();

            if (missing.Count > 0)
                AssertEx.Fail($"{missing.Count} tests are missing: " + string.Join(", ", missing));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_AllBooleanProperties_HaveTrueFalseTests()
        {
            var properties = PrtgObjectFilterTests.GetPrtgObjectProperties().Where(
                p => (p.PropertyType == typeof(bool) || p.PropertyType == typeof(bool?)) && p.Name != "InheritInterval");

            var expectedMethods = properties.SelectMany(p =>
            {
                var name = $"Data_QueryFilter_BooleanProperties_{p.Name}";

                return new[]
                {
                    $"{name}_True",
                    $"{name}_False"
                };
            }).ToList();

            TestHelpers.Assert_TestClassHasMethods(GetType(), expectedMethods);
        }
    }
}
