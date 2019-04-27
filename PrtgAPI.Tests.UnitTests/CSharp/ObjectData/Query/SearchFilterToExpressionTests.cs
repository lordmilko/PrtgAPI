using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Reflection;
using PrtgAPI.Request.Serialization;
using PrtgAPI.Linq.Expressions;
using PrtgAPI.Utilities;
using PrtgAPI.Tests.UnitTests.Support;

namespace PrtgAPI.Tests.UnitTests.ObjectData.Query
{
    [TestClass]
    public class SearchFilterToExpressionTests
    {
        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_NoFilters()
        {
            TestSearchFilterExpression();
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_ArrayOfDifferent()
        {
            AssertEx.Throws<AssertFailedException>(() =>
            {
                TestSearchFilterExpression(new SearchFilter(Property.Name, "firstObj"), new SearchFilter(Property.Id, 4001));
            }, "Elements firstObj, secondObj were missing from second");

            AssertEx.Throws<AssertFailedException>(() =>
            {
                TestSearchFilterExpression(new SearchFilter(Property.Name, "firstObj"), new SearchFilter(Property.Id, 4000));
            }, "Elements secondObj were missing from second");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Single_WithArray()
        {
            TestSearchFilterExpression(new SearchFilter(Property.Id, new[] {4000, 4001}));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_ArrayOfSame()
        {
            TestSearchFilterExpression(new SearchFilter(Property.Id, 4000), new SearchFilter(Property.Id, 4001), new SearchFilter(Property.Id, 9999));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_ArrayOfSame_OneHasArray()
        {
            TestSearchFilterExpression(new SearchFilter(Property.Id, 9999), new SearchFilter(Property.Id, new[] { 4000, 4001 }));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_ArrayOfSame_AllHaveArrays()
        {
            TestSearchFilterExpression(new SearchFilter(Property.Id, new[] {9998, 9999}), new SearchFilter(Property.Id, new[] { 4000, 4001 }));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_InvalidFilterForType()
        {
            AssertEx.Throws<AssertFailedException>(() =>
            {
                TestSearchFilterExpression(new SearchFilter(Property.Collapsed, true));
            }, "Elements firstObj, secondObj were missing from second");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_InvalidFilterForType_WithValidFilter()
        {
            TestSearchFilterExpression(new SearchFilter(Property.Collapsed, true), new SearchFilter(Property.Name, FilterOperator.Contains, "OBJ"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_ImplicitlyCastable()
        {
            AssertEx.Throws<AssertFailedException>(() =>
            {
                TestSearchFilterExpression(new SearchFilter(Property.Id, new IllegalInt { Value = 4000 }));
            }, "Elements secondObj were missing from second");
        }

        private void TestSearchFilterExpression(params SearchFilter[] filters)
        {
            var firstObj = new Sensor
            {
                Name = "firstObj",
                Id = 4000
            };
            var secondObj = new Sensor
            {
                Name = "secondObj",
                Id = 4001
            };

            var objs = new List<Sensor>
            {
                firstObj,
                secondObj
            };

            var func = SearchFilterToExpression.Parse<Sensor>(filters);

            var filtered = objs.Where(func).ToList();

            AssertEx.AreEqualLists(objs, filtered, "Lists were not equal");
        }

        #region PrtgObject

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_PrtgObjectProperties_Name() =>
            Execute(new Sensor { Name = "goodName" }, new Sensor { Name = "badName" },  null, Property.Name, "goodName");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_PrtgObjectProperties_Id() =>
            Execute(new Sensor { Id = 1000 }, new Sensor { Id = 2000 }, null, Property.Id, 1000);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_PrtgObjectProperties_ParentId() =>
            Execute(new Sensor { ParentId = 1000 }, new Sensor { ParentId = 2000 }, null, Property.ParentId, 1000);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_PrtgObjectProperties_Active() =>
            Execute(new Sensor { Active = true }, new Sensor { Active = false }, null, Property.Active, true);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_PrtgObjectProperties_Type() =>
            Execute(new Sensor { Type = "exexml" }, new Sensor { Type = "ping" }, null, Property.Type, "exexml");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_PrtgObjectProperties_Type_FromStringEnum() =>
            Execute(new Sensor { Type = "probenode" }, new Sensor { Type = "device" }, null, Property.Type, new StringEnum<ObjectType>(ObjectType.Probe));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_PrtgObjectProperties_Tags() =>
            Execute(new Sensor { Tags = new[] { "good" } }, new Sensor { Tags = new[] { "bad" } }, null, Property.Tags, "good", FilterOperator.Contains);


        #endregion
        #region SensorOrDeviceOrGroupOrProbe

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_SensorOrDeviceOrGroupOrProbeProperties_Access() =>
            Execute(new Sensor { Access = Access.Full }, new Sensor { Access = Access.Read }, null, Property.Access, Access.Full);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_SensorOrDeviceOrGroupOrProbeProperties_BaseType() =>
            Execute(new Sensor { BaseType = BaseType.Sensor }, new Sensor { BaseType = BaseType.Device }, null, Property.BaseType, BaseType.Sensor);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_SensorOrDeviceOrGroupOrProbeProperties_Comments() =>
            Execute(new Sensor { Comments = "goodComment" }, new Sensor { Comments = "badComment" }, null, Property.Comments, "goodComment");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_SensorOrDeviceOrGroupOrProbeProperties_Dependency() =>
            Execute(new Sensor { Dependency = "goodDependency" }, new Sensor { Dependency = "badDependency" }, null, Property.Dependency, "goodDependency");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_SensorOrDeviceOrGroupOrProbeProperties_Interval() =>
            Execute(new Sensor { Interval = new TimeSpan(1, 0, 0) }, new Sensor { Interval = new TimeSpan(2, 0, 0) }, null, Property.Interval, new TimeSpan(1, 0, 0));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_SensorOrDeviceOrGroupOrProbeProperties_Position() =>
            Execute(new Sensor { Position = 1 }, new Sensor { Position = 2 }, null, Property.Position, 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_SensorOrDeviceOrGroupOrProbeProperties_Schedule() =>
            Execute(new Sensor { Schedule = "Weekends [GMT+0800]" }, new Sensor { Schedule = "Weekends [GMT+0900]" }, null, Property.Schedule, "Weekends [GMT+0800]");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_SensorOrDeviceOrGroupOrProbeProperties_Status() =>
            Execute(new Sensor { Status = Status.Up }, new Sensor { Status = Status.Down }, null, Property.Status, Status.Up);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_SensorOrDeviceOrGroupOrProbeProperties_Url() =>
            Execute(new Sensor { Url = "/sensor.htm?id=1001" }, new Sensor { Url = "/sensor.htm?id=2001" }, null, Property.Url, "/sensor.htm?id=1001");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_SensorOrDeviceOrGroupOrProbeProperties_Url_Raw() =>
            Execute(new Sensor { Url = "/sensor.htm?id=1001" }, new Sensor { Url = "/sensor.htm?id=2001" }, null, Property.Url, 1001);

        #endregion
        #region SensorOrDeviceOrGroupOrProbeOrTicket

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_SensorOrDeviceOrGroupOrProbeOrTicketProperties_Priority() =>
            Execute(new Sensor { Priority = Priority.Four }, new Sensor { Priority = Priority.Three }, null, Property.Priority, Priority.Four);

        #endregion
        #region SensorOrDeviceOrGroupOrProbeOrTicketOrTicketData

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_SensorOrDeviceOrGroupOrProbeOrTicketOrTicketDataProperties_Message() =>
            Execute(new Sensor { Message = "goodMessage" }, new Sensor { Message = "badMessage" }, null, Property.Message, "goodMessage");

        #endregion
        #region DeviceOrGroupOrProbe

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_DeviceOrGroupOrProbeProperties_DownAcknowledgedSensors() =>
            Execute(new Device { DownAcknowledgedSensors = 1 }, new Device { DownAcknowledgedSensors = 2 }, null, Property.DownAcknowledgedSensors, 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_DeviceOrGroupOrProbeProperties_DownSensors() =>
            Execute(new Device { DownSensors = 1 }, new Device { DownSensors = 2 }, null, Property.DownSensors, 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_DeviceOrGroupOrProbeProperties_PartialDownSensors() =>
            Execute(new Device { PartialDownSensors = 1 }, new Device { PartialDownSensors = 2 }, null, Property.PartialDownSensors, 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_DeviceOrGroupOrProbeProperties_PausedSensors() =>
            Execute(new Device { PausedSensors = 1 }, new Device { PausedSensors = 2 }, null, Property.PausedSensors, 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_DeviceOrGroupOrProbeProperties_TotalSensors() =>
            Execute(new Device { TotalSensors = 1 }, new Device { TotalSensors = 2 }, null, Property.TotalSensors, 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_DeviceOrGroupOrProbeProperties_UnknownSensors() =>
            Execute(new Device { UnknownSensors = 1 }, new Device { UnknownSensors = 2 }, null, Property.UnknownSensors, 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_DeviceOrGroupOrProbeProperties_UnusualSensors() =>
            Execute(new Device { UnusualSensors = 1 }, new Device { UnusualSensors = 2 }, null, Property.UnusualSensors, 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_DeviceOrGroupOrProbeProperties_UpSensors() =>
            Execute(new Device { UpSensors = 1 }, new Device { UpSensors = 2 }, null, Property.UpSensors, 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_DeviceOrGroupOrProbeProperties_WarningSensors() =>
            Execute(new Device { WarningSensors = 1 }, new Device { WarningSensors = 2 }, null, Property.WarningSensors, 1);

        #endregion
        #region GroupOrProbe

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_GroupOrProbeProperties_Collapsed() =>
            Execute(new Group { Collapsed = true }, new Group { Collapsed = false }, null, Property.Collapsed, true);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_GroupOrProbeProperties_TotalDevices() =>
            Execute(new Group { TotalDevices = 1 }, new Group { TotalDevices = 2 }, null, Property.TotalDevices, 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_GroupOrProbeProperties_TotalGroups() =>
            Execute(new Group { TotalGroups = 1 }, new Group { TotalGroups = 2 }, null, Property.TotalGroups, 1);

        #endregion
        #region Sensor

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_SensorProperties_DataCollectedSince() =>
            Execute(new Sensor { DataCollectedSince = Time.Today }, new Sensor { DataCollectedSince = Time.Yesterday }, null, Property.DataCollectedSince, Time.Today);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_SensorProperties_Device() =>
            Execute(new Sensor { Device = "dc-1" }, new Sensor { Device = "exch-1" }, null, Property.Device, "dc-1");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_SensorProperties_DownDuration() =>
            Execute(new Sensor { DownDuration = TimeSpan.FromSeconds(50) }, new Sensor { DownDuration = TimeSpan.FromSeconds(40) }, null, Property.DownDuration, TimeSpan.FromSeconds(50));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_SensorProperties_Downtime() =>
            Execute(new Sensor { Downtime = 10 }, new Sensor { Downtime = 5 }, null, Property.Downtime, 10);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_SensorProperties_LastCheck() =>
            Execute(new Sensor { LastCheck = Time.Today }, new Sensor { LastCheck = Time.Yesterday }, null, Property.LastCheck, Time.Today);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_SensorProperties_LastUp() =>
            Execute(new Sensor { LastUp = Time.Today }, new Sensor { LastUp = Time.Yesterday }, null, Property.LastUp, Time.Today);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_SensorProperties_LastDown() =>
            Execute(new Sensor { LastDown = Time.Today }, new Sensor { LastDown = Time.Yesterday }, null, Property.LastDown, Time.Today);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_SensorProperties_LastValue() =>
            Execute(new Sensor { LastValue = 1 }, new Sensor { LastValue = 2 }, null, Property.LastValue, 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_SensorProperties_MiniGraph() =>
            Execute(new Sensor { MiniGraph = "0,0,0,0" }, new Sensor { MiniGraph = "1,1,1,1" }, null, Property.MiniGraph, "0,0,0,0");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_SensorProperties_TotalDowntime() =>
            Execute(new Sensor { TotalDowntime = TimeSpan.FromSeconds(50) }, new Sensor { TotalDowntime = TimeSpan.FromSeconds(40) }, null, Property.TotalDowntime, TimeSpan.FromSeconds(50));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_SensorProperties_TotalMonitorTime() =>
            Execute(new Sensor { TotalMonitorTime = TimeSpan.FromSeconds(50) }, new Sensor { TotalMonitorTime = TimeSpan.FromSeconds(40) }, null, Property.TotalMonitorTime, TimeSpan.FromSeconds(50));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_SensorProperties_TotalUptime() =>
            Execute(new Sensor { TotalUptime = TimeSpan.FromSeconds(50) }, new Sensor { TotalUptime = TimeSpan.FromSeconds(40) }, null, Property.TotalUptime, TimeSpan.FromSeconds(50));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_SensorProperties_UpDuration() =>
            Execute(new Sensor { UpDuration = TimeSpan.FromSeconds(50) }, new Sensor { UpDuration = TimeSpan.FromSeconds(40) }, null, Property.UpDuration, TimeSpan.FromSeconds(50));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_SensorProperties_Uptime() =>
            Execute(new Sensor { Uptime = 10 }, new Sensor { Uptime = 5 }, null, Property.Uptime, 10);

        #endregion
        #region Device

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_DeviceProperties_Condition() =>
            Execute(new Device { Condition = "Auto-Discovery in progress (10%)" }, new Device { Condition = "Auto-Discovery in progress (20%)" }, null, Property.Condition, "Auto-Discovery in progress (10%)");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_DeviceProperties_Favorite() =>
            Execute(new Device { Favorite = true }, new Device { Favorite = false }, null, Property.Favorite, true);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_DeviceProperties_Group() =>
            Execute(new Device { Group = "Servers" }, new Device { Group = "Clients" }, null, Property.Group, "Servers");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_DeviceProperties_Host() =>
            Execute(new Device { Host = "dc-1" }, new Device { Host = "dc-2" }, null, Property.Host, "dc-1");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_DeviceProperties_Location() =>
            Execute(new Device { Location = "Nebraska" }, new Device { Location = "New York" }, null, Property.Location, "Nebraska");
        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_DeviceProperties_Probe() =>
            Execute(new Device { Probe = "Local Probe" }, new Device { Probe = "Remote Probe" }, null, Property.Probe, "Local Probe");

        #endregion
        #region Probe

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_ProbeProperties_ProbeStatus() =>
            Execute(new Probe { ProbeStatus = ProbeStatus.Connected }, new Probe { ProbeStatus = ProbeStatus.Disconnected }, null, Property.ProbeStatus, ProbeStatus.Connected);

        #endregion
        #region Types
        #region Boolean
        #region Equals

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_BooleanProperty_Equals_Boolean() => ExecuteBoolean(FilterOperator.Equals);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_BooleanProperty_Equals_Integer() => ExecuteBoolean(FilterOperator.Equals, -1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_BooleanProperty_Equals_String() => ExecuteBoolean(FilterOperator.Equals, "true", -1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_BooleanProperty_Equals_InvalidString() => ExecuteBoolean(FilterOperator.Equals, "banana", -1);

        #endregion
        #region NotEquals

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_BooleanProperty_NotEquals_Boolean() => ExecuteBoolean(FilterOperator.NotEquals, expectedIndex: 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_BooleanProperty_NotEquals_Integer() => ExecuteBoolean(FilterOperator.NotEquals, -1, 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_BooleanProperty_NotEquals_String() => ExecuteBoolean(FilterOperator.NotEquals, "-1", 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_BooleanProperty_NotEquals_InvalidString() => ExecuteBoolean(FilterOperator.NotEquals, "true", -2);

        #endregion
        #region GreaterThan

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_BooleanProperty_GreaterThan_Boolean() => ExecuteBoolean(FilterOperator.GreaterThan, true, 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_BooleanProperty_GreaterThan_Integer() => ExecuteBoolean(FilterOperator.GreaterThan, -1, 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_BooleanProperty_GreaterThan_String() => ExecuteBoolean(FilterOperator.GreaterThan, "true", -1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_BooleanProperty_GreaterThan_InvalidString() => ExecuteBoolean(FilterOperator.GreaterThan, "banana", -1);

        #endregion
        #region LessThan

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_BooleanProperty_LessThan_Boolean() => ExecuteBoolean(FilterOperator.LessThan, false);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_BooleanProperty_LessThan_Integer() => ExecuteBoolean(FilterOperator.LessThan, 0);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_BooleanProperty_LessThan_String() => ExecuteBoolean(FilterOperator.LessThan, "false", -1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_BooleanProperty_LessThan_InvalidString() => ExecuteBoolean(FilterOperator.LessThan, "banana", -1);

        #endregion
        #region Contains

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_BooleanProperty_Contains_Boolean() => ExecuteBoolean(FilterOperator.Contains, true);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_BooleanProperty_Contains_Integer() => ExecuteBoolean(FilterOperator.Contains, 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_BooleanProperty_Contains_String() => ExecuteBoolean(FilterOperator.Contains, "1");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_BooleanProperty_Contains_InvalidString() => ExecuteBoolean(FilterOperator.Contains, "banana", -1);

        #endregion
        #endregion
        #region Integer
        #region Equals

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_IntegerProperty_Equals_Integer() => ExecuteInteger(FilterOperator.Equals);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_IntegerProperty_Equals_Double() => ExecuteInteger(FilterOperator.Equals, 1.1, -1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_IntegerProperty_Equals_String() => ExecuteInteger(FilterOperator.Equals, "10");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_IntegerProperty_Equals_InvalidString() => ExecuteInteger(FilterOperator.Equals, "banana", -1);

        #endregion
        #region NotEquals

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_IntegerProperty_NotEquals_Integer() => ExecuteInteger(FilterOperator.NotEquals, expectedIndex: 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_IntegerProperty_NotEquals_Double() => ExecuteInteger(FilterOperator.NotEquals, 1.1, -2);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_IntegerProperty_NotEquals_String() => ExecuteInteger(FilterOperator.NotEquals, "10", 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_IntegerProperty_NotEquals_InvalidString() => ExecuteInteger(FilterOperator.NotEquals, "banana", -2);

        #endregion
        #region GreaterThan

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_IntegerProperty_GreaterThan_Integer() => ExecuteInteger(FilterOperator.GreaterThan, expectedIndex: 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_IntegerProperty_GreaterThan_Double() => ExecuteInteger(FilterOperator.GreaterThan, 10.1, 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_IntegerProperty_GreaterThan_String() => ExecuteInteger(FilterOperator.GreaterThan, "10", 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_IntegerProperty_GreaterThan_InvalidString() => ExecuteInteger(FilterOperator.GreaterThan, "banana", -1);

        #endregion
        #region LessThan

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_IntegerProperty_LessThan_Integer() => ExecuteInteger(FilterOperator.LessThan, 20);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_IntegerProperty_LessThan_Double() => ExecuteInteger(FilterOperator.LessThan, 11.1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_IntegerProperty_LessThan_String() => ExecuteInteger(FilterOperator.LessThan, "20");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_IntegerProperty_LessThan_InvalidString() => ExecuteInteger(FilterOperator.LessThan, "banana", -1);

        #endregion
        #region Contains

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_IntegerProperty_Contains_Integer() => ExecuteInteger(FilterOperator.Contains, 0, -2);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_IntegerProperty_Contains_Double() => ExecuteInteger(FilterOperator.Contains, 1.1, -1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_IntegerProperty_Contains_String() => ExecuteInteger(FilterOperator.Contains, "10");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_IntegerProperty_Contains_InvalidString() => ExecuteInteger(FilterOperator.Contains, "banana", -1);

        #endregion
        #endregion
        #region NullableDouble
        #region Equals

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableDoubleProperty_Equals_Double() => ExecuteDouble(FilterOperator.Equals);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableDoubleProperty_Equals_Integer() => ExecuteDouble(FilterOperator.Equals, 10, -1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableDoubleProperty_Equals_String() => ExecuteDouble(FilterOperator.Equals, "10.2");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableDoubleProperty_Equals_InvalidString() => ExecuteDouble(FilterOperator.Equals, "banana", -1);

        #endregion
        #region NotEquals

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableDoubleProperty_NotEquals_Double() => ExecuteDouble(FilterOperator.NotEquals, expectedIndex: 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableDoubleProperty_NotEquals_Integer() => ExecuteDouble(FilterOperator.NotEquals, 10, -2);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableDoubleProperty_NotEquals_String() => ExecuteDouble(FilterOperator.NotEquals, "10.2", 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableDoubleProperty_NotEquals_InvalidString() => ExecuteDouble(FilterOperator.NotEquals, "banana", -2);

        #endregion
        #region GreaterThan

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableDoubleProperty_GreaterThan_Double() => ExecuteDouble(FilterOperator.GreaterThan, expectedIndex: 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableDoubleProperty_GreaterThan_Integer() => ExecuteDouble(FilterOperator.GreaterThan, 11, 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableDoubleProperty_GreaterThan_String() => ExecuteDouble(FilterOperator.GreaterThan, "12.2", 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableDoubleProperty_GreaterThan_InvalidString() => ExecuteDouble(FilterOperator.GreaterThan, "banana", -1);

        #endregion
        #region LessThan

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableDoubleProperty_LessThan_Double() => ExecuteDouble(FilterOperator.LessThan, 11.3);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableDoubleProperty_LessThan_Integer() => ExecuteDouble(FilterOperator.LessThan, 11);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableDoubleProperty_LessThan_String() => ExecuteDouble(FilterOperator.LessThan, "12.5");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableDoubleProperty_LessThan_InvalidString() => ExecuteDouble(FilterOperator.LessThan, "banana", -1);

        #endregion
        #region Contains

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableDoubleProperty_Contains_Double() => ExecuteDouble(FilterOperator.Contains, 10.2);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableDoubleProperty_Contains_Integer() => ExecuteDouble(FilterOperator.Contains, 0, -2);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableDoubleProperty_Contains_String() => ExecuteDouble(FilterOperator.Contains, ".2", -2);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableDoubleProperty_Contains_InvalidString() => ExecuteDouble(FilterOperator.Contains, "banana", -1);

        #endregion
        #endregion
        #region Enum
        #region Equals

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_EnumProperty_Equals_Enum() => ExecuteEnum(FilterOperator.Equals);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_EnumProperty_Equals_InvalidEnum() => ExecuteEnum(FilterOperator.Equals, Priority.One);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_EnumProperty_Equals_String() => ExecuteEnum(FilterOperator.Equals, "1");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_EnumProperty_Equals_InvalidString() => ExecuteEnum(FilterOperator.Equals, "Unknown", -1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_EnumProperty_Equals_FlagsEnum() => ExecuteEnumFlags(FilterOperator.Equals);

        #endregion
        #region NotEquals

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_EnumProperty_NotEquals_Enum() => ExecuteEnum(FilterOperator.NotEquals, expectedIndex: 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_EnumProperty_NotEquals_InvalidEnum() => ExecuteEnum(FilterOperator.NotEquals, Priority.One, 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_EnumProperty_NotEquals_String() => ExecuteEnum(FilterOperator.NotEquals, "1", 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_EnumProperty_NotEquals_InvalidString() => ExecuteEnum(FilterOperator.NotEquals, "Unknown", -2);

        //Logically you want it to have done not AND not, but PRTG doesn't work like that, so doing multiple not conditions effectively does nothing
        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_EnumProperty_NotEquals_FlagsEnum() => ExecuteEnumFlags(FilterOperator.NotEquals, expectedIndex: -2);

        #endregion
        #region GreaterThan

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_EnumProperty_GreaterThan_Enum() => ExecuteEnum(FilterOperator.GreaterThan, expectedIndex: 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_EnumProperty_GreaterThan_InvalidEnum() => ExecuteEnum(FilterOperator.GreaterThan, Priority.One, 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_EnumProperty_GreaterThan_String() => ExecuteEnum(FilterOperator.GreaterThan, "1", 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_EnumProperty_GreaterThan_InvalidString() => ExecuteEnum(FilterOperator.GreaterThan, "Unknown", -1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_EnumProperty_GreaterThan_FlagsEnum() => ExecuteEnumFlags(FilterOperator.GreaterThan, expectedIndex: -2);

        #endregion
        #region LessThan

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_EnumProperty_LessThan_Enum() => ExecuteEnum(FilterOperator.LessThan, Status.Up);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_EnumProperty_LessThan_InvalidEnum() => ExecuteEnum(FilterOperator.LessThan, Priority.Two);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_EnumProperty_LessThan_String() => ExecuteEnum(FilterOperator.LessThan, "2");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_EnumProperty_LessThan_InvalidString() => ExecuteEnum(FilterOperator.LessThan, "Unknown", -1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_EnumProperty_LessThan_FlagsEnum() => ExecuteEnumFlags(FilterOperator.LessThan);

        #endregion
        #region Contains

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_EnumProperty_Contains_Enum() => ExecuteEnum(FilterOperator.Contains);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_EnumProperty_Contains_InvalidEnum() => ExecuteEnum(FilterOperator.Contains, Priority.One);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_EnumProperty_Contains_String() => ExecuteEnum(FilterOperator.Contains, "1");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_EnumProperty_Contains_InvalidString() => ExecuteEnum(FilterOperator.Contains, "Unknown", -1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_EnumProperty_Contains_FlagsEnum() => ExecuteEnumFlags(FilterOperator.Contains);

        #endregion
        #endregion
        #region TimeSpan
        #region Equals

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_TimeSpanProperty_Equals_TimeSpan() => ExecuteTimeSpan(FilterOperator.Equals);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_TimeSpanProperty_Equals_Integer() => ExecuteTimeSpan(FilterOperator.Equals, 60);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_TimeSpanProperty_Equals_String() => ExecuteTimeSpan(FilterOperator.Equals, "60");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_TimeSpanProperty_Equals_InvalidString() => ExecuteTimeSpan(FilterOperator.Equals, "00:01:00", -1);

        #endregion
        #region NotEquals

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_TimeSpanProperty_NotEquals_TimeSpan() => ExecuteTimeSpan(FilterOperator.NotEquals, expectedIndex: 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_TimeSpanProperty_NotEquals_Integer() => ExecuteTimeSpan(FilterOperator.NotEquals, 60, 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_TimeSpanProperty_NotEquals_String() => ExecuteTimeSpan(FilterOperator.NotEquals, "60", 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_TimeSpanProperty_NotEquals_InvalidString() => ExecuteTimeSpan(FilterOperator.NotEquals, "00:01:00", -2);

        #endregion
        #region GreaterThan

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_TimeSpanProperty_GreaterThan_TimeSpan() => ExecuteTimeSpan(FilterOperator.GreaterThan, TimeSpan.FromSeconds(40));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_TimeSpanProperty_GreaterThan_Integer() => ExecuteTimeSpan(FilterOperator.GreaterThan, 40);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_TimeSpanProperty_GreaterThan_String() => ExecuteTimeSpan(FilterOperator.GreaterThan, "40");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_TimeSpanProperty_GreaterThan_InvalidString() => ExecuteTimeSpan(FilterOperator.GreaterThan, "00:01:00", -1);

        #endregion
        #region LessThan

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_TimeSpanProperty_LessThan_TimeSpan() => ExecuteTimeSpan(FilterOperator.LessThan, expectedIndex: 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_TimeSpanProperty_LessThan_Integer() => ExecuteTimeSpan(FilterOperator.LessThan, 60, 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_TimeSpanProperty_LessThan_String() => ExecuteTimeSpan(FilterOperator.LessThan, "60", 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_TimeSpanProperty_LessThan_InvalidString() => ExecuteTimeSpan(FilterOperator.LessThan, "00:01:00", -1);

        #endregion
        #region Contains

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_TimeSpanProperty_Contains_TimeSpan() => ExecuteTimeSpan(FilterOperator.Contains);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_TimeSpanProperty_Contains_Integer() => ExecuteTimeSpan(FilterOperator.Contains, 0, -2);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_TimeSpanProperty_Contains_String() => ExecuteTimeSpan(FilterOperator.Contains, "0", -2);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_TimeSpanProperty_Contains_InvalidString() => ExecuteTimeSpan(FilterOperator.Contains, "00:01:00", -1);

        #endregion
        #endregion
        #region NullableTimeSpan
        #region Equals

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableTimeSpanProperty_Equals_TimeSpan() => ExecuteNullableTimeSpan(FilterOperator.Equals);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableTimeSpanProperty_Equals_Integer() => ExecuteNullableTimeSpan(FilterOperator.Equals, "60");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableTimeSpanProperty_Equals_String() => ExecuteNullableTimeSpan(FilterOperator.Equals, "60");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableTimeSpanProperty_Equals_InvalidString() => ExecuteNullableTimeSpan(FilterOperator.Equals, "00:01:00", -1);

        #endregion
        #region NotEquals

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableTimeSpanProperty_NotEquals_TimeSpan() => ExecuteNullableTimeSpan(FilterOperator.NotEquals, expectedIndex: 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableTimeSpanProperty_NotEquals_Integer() => ExecuteNullableTimeSpan(FilterOperator.NotEquals, "60", 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableTimeSpanProperty_NotEquals_String() => ExecuteNullableTimeSpan(FilterOperator.NotEquals, "60", 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableTimeSpanProperty_NotEquals_InvalidString() => ExecuteNullableTimeSpan(FilterOperator.NotEquals, "00:01:00", -2);

        #endregion
        #region GreaterThan

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableTimeSpanProperty_GreaterThan_TimeSpan() => ExecuteNullableTimeSpan(FilterOperator.GreaterThan, TimeSpan.FromSeconds(40));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableTimeSpanProperty_GreaterThan_Integer() => ExecuteNullableTimeSpan(FilterOperator.GreaterThan, "40");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableTimeSpanProperty_GreaterThan_String() => ExecuteNullableTimeSpan(FilterOperator.GreaterThan, "40");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableTimeSpanProperty_GreaterThan_InvalidString() => ExecuteNullableTimeSpan(FilterOperator.GreaterThan, "00:01:00", -1);

        #endregion
        #region LessThan

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableTimeSpanProperty_LessThan_TimeSpan() => ExecuteNullableTimeSpan(FilterOperator.LessThan, expectedIndex: 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableTimeSpanProperty_LessThan_Integer() => ExecuteNullableTimeSpan(FilterOperator.LessThan, "60", 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableTimeSpanProperty_LessThan_String() => ExecuteNullableTimeSpan(FilterOperator.LessThan, "60", 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableTimeSpanProperty_LessThan_InvalidString() => ExecuteNullableTimeSpan(FilterOperator.LessThan, "00:01:00", -1);

        #endregion
        #region Contains

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableTimeSpanProperty_Contains_TimeSpan() => ExecuteNullableTimeSpan(FilterOperator.Contains);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableTimeSpanProperty_Contains_Integer() => ExecuteNullableTimeSpan(FilterOperator.Contains, 0, -2);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableTimeSpanProperty_Contains_String() => ExecuteNullableTimeSpan(FilterOperator.Contains, "0", -2);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableTimeSpanProperty_Contains_InvalidString() => ExecuteNullableTimeSpan(FilterOperator.Contains, "00:01:00", -1);

        #endregion
        #endregion
        #region String

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_StringProperty_Equals_String() => ExecuteString(FilterOperator.Equals);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_StringProperty_Equals_StringIgnoreCase() => ExecuteString(FilterOperator.Equals, "goodname");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_StringProperty_NotEquals_String() => ExecuteString(FilterOperator.NotEquals, expectedIndex: 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_StringProperty_GreaterThan_String() => ExecuteString(FilterOperator.GreaterThan, expectedIndex: -1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_StringProperty_LessThan_String() => ExecuteString(FilterOperator.LessThan, expectedIndex: -1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_StringProperty_Contains_String() => ExecuteString(FilterOperator.Contains);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_StringProperty_Contains_StringIgnoreCase() => ExecuteString(FilterOperator.Contains, "goodname");

        #endregion
        #region StringArray
        #region Equals

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_StringArrayProperty_Equals_StringArray() => ExecuteStringArray(FilterOperator.Equals);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_StringArrayProperty_Equals_String() => ExecuteStringArray(FilterOperator.Equals, "good1");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_StringArrayProperty_Equals_InvalidString() => ExecuteStringArray(FilterOperator.Equals, "banana", -1);

        #endregion
        #region NotEquals

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_StringArrayProperty_NotEquals_StringArray() => ExecuteStringArray(FilterOperator.NotEquals, expectedIndex: -1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_StringArrayProperty_NotEquals_String() => ExecuteStringArray(FilterOperator.NotEquals, "good1", -1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_StringArrayProperty_NotEquals_InvalidString() => ExecuteStringArray(FilterOperator.NotEquals, "banana", -1);

        #endregion
        #region GreaterThan

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_StringArrayProperty_GreaterThan_StringArray() => ExecuteStringArray(FilterOperator.GreaterThan, expectedIndex: -1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_StringArrayProperty_GreaterThan_String() => ExecuteStringArray(FilterOperator.GreaterThan, "good1", -1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_StringArrayProperty_GreaterThan_InvalidString() => ExecuteStringArray(FilterOperator.GreaterThan, "banana", -1);

        #endregion
        #region LessThan

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_StringArrayProperty_LessThan_StringArray() => ExecuteStringArray(FilterOperator.LessThan, expectedIndex: -1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_StringArrayProperty_LessThan_String() => ExecuteStringArray(FilterOperator.LessThan, "good1", -1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_StringArrayProperty_LessThan_InvalidString() => ExecuteStringArray(FilterOperator.LessThan, "banana", -1);

        #endregion
        #region Contains

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_StringArrayProperty_Contains_StringArray() => ExecuteStringArray(FilterOperator.Contains);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_StringArrayProperty_Contains_String() => ExecuteStringArray(FilterOperator.Contains, "good1");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_StringArrayProperty_Contains_InvalidString() => ExecuteStringArray(FilterOperator.Contains, "banana", -1);

        #endregion
        #endregion
        #region DateTime
        #region Equals

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_DateTimeProperty_Equals_DateTime() => ExecuteDateTime(FilterOperator.Equals);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_DateTimeProperty_Equals_Double() => ExecuteDateTime(FilterOperator.Equals, 43236.4726273148);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_DateTimeProperty_Equals_String() => ExecuteDateTime(FilterOperator.Equals, "43236.4726273148");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_DateTimeProperty_Equals_InvalidString() => ExecuteDateTime(FilterOperator.Equals, "banana", -1);

        #endregion
        #region NotEquals

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_DateTimeProperty_NotEquals_DateTime() => ExecuteDateTime(FilterOperator.NotEquals, expectedIndex: 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_DateTimeProperty_NotEquals_Double() => ExecuteDateTime(FilterOperator.NotEquals, 43236.4726273148, 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_DateTimeProperty_NotEquals_String() => ExecuteDateTime(FilterOperator.NotEquals, "43236.4726273148", 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_DateTimeProperty_NotEquals_InvalidString() => ExecuteDateTime(FilterOperator.NotEquals, "banana", -2);

        #endregion
        #region GreaterThan

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_DateTimeProperty_GreaterThan_DateTime() => ExecuteDateTime(FilterOperator.GreaterThan, Time.Yesterday);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_DateTimeProperty_GreaterThan_Double() => ExecuteDateTime(FilterOperator.GreaterThan, 43235.4726273148);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_DateTimeProperty_GreaterThan_String() => ExecuteDateTime(FilterOperator.GreaterThan, "43235.4726273148");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_DateTimeProperty_GreaterThan_InvalidString() => ExecuteDateTime(FilterOperator.GreaterThan, "banana", -1);

        #endregion
        #region LessThan

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_DateTimeProperty_LessThan_DateTime() => ExecuteDateTime(FilterOperator.LessThan, expectedIndex: 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_DateTimeProperty_LessThan_Double() => ExecuteDateTime(FilterOperator.LessThan, 43236.4726273148, 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_DateTimeProperty_LessThan_String() => ExecuteDateTime(FilterOperator.LessThan, "43236.4726273148", 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_DateTimeProperty_LessThan_InvalidString() => ExecuteDateTime(FilterOperator.LessThan, "banana", -1);

        #endregion
        #region Contains

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_DateTimeProperty_Contains_DateTime() => ExecuteDateTime(FilterOperator.Contains);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_DateTimeProperty_Contains_Double() => ExecuteDateTime(FilterOperator.Contains, 43236);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_DateTimeProperty_Contains_String() => ExecuteDateTime(FilterOperator.Contains, "43236");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_DateTimeProperty_Contains_InvalidString() => ExecuteDateTime(FilterOperator.Contains, "banana", -1);

        #endregion
        #endregion
        #region NullableDateTime
        #region Equals

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableDateTimeProperty_Equals_DateTime() => ExecuteNullableDateTime(FilterOperator.Equals);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableDateTimeProperty_Equals_Double() => ExecuteNullableDateTime(FilterOperator.Equals, 43236.4726273148);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableDateTimeProperty_Equals_String() => ExecuteNullableDateTime(FilterOperator.Equals, "43236.4726273148");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableDateTimeProperty_Equals_InvalidString() => ExecuteNullableDateTime(FilterOperator.Equals, "banana", -1);

        #endregion
        #region NotEquals

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableDateTimeProperty_NotEquals_DateTime() => ExecuteNullableDateTime(FilterOperator.NotEquals, expectedIndex: 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableDateTimeProperty_NotEquals_Double() => ExecuteNullableDateTime(FilterOperator.NotEquals, 43236.4726273148, 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableDateTimeProperty_NotEquals_String() => ExecuteNullableDateTime(FilterOperator.NotEquals, TypeHelpers.ConvertToPrtgDateTime(Time.Today), 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableDateTimeProperty_NotEquals_InvalidString() => ExecuteNullableDateTime(FilterOperator.NotEquals, "banana", -2);

        #endregion
        #region GreaterThan

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableDateTimeProperty_GreaterThan_DateTime() => ExecuteNullableDateTime(FilterOperator.GreaterThan, Time.Yesterday);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableDateTimeProperty_GreaterThan_Double() => ExecuteNullableDateTime(FilterOperator.GreaterThan, 43235.4726273148);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableDateTimeProperty_GreaterThan_String() => ExecuteNullableDateTime(FilterOperator.GreaterThan, "43235.4726273148");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableDateTimeProperty_GreaterThan_InvalidString() => ExecuteNullableDateTime(FilterOperator.GreaterThan, "banana", -1);

        #endregion
        #region LessThan

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableDateTimeProperty_LessThan_DateTime() => ExecuteNullableDateTime(FilterOperator.LessThan, expectedIndex: 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableDateTimeProperty_LessThan_Double() => ExecuteNullableDateTime(FilterOperator.LessThan, 43236.4726273148, 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableDateTimeProperty_LessThan_String() => ExecuteNullableDateTime(FilterOperator.LessThan, "43236.4726273148", 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableDateTimeProperty_LessThan_InvalidString() => ExecuteNullableDateTime(FilterOperator.LessThan, "banana", -1);

        #endregion
        #region Contains

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableDateTimeProperty_Contains_DateTime() => ExecuteNullableDateTime(FilterOperator.Contains);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableDateTimeProperty_Contains_Double() => ExecuteNullableDateTime(FilterOperator.Contains, 43236);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableDateTimeProperty_Contains_String() => ExecuteNullableDateTime(FilterOperator.Contains, "43236");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_NullableDateTimeProperty_Contains_InvalidString() => ExecuteNullableDateTime(FilterOperator.Contains, "banana", -1);

        #endregion
        #endregion
        #region StringEnum
        #region Equals

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_StringEnumProperty_Equals_StringEnum() => ExecuteStringEnum(FilterOperator.Equals);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_StringEnumProperty_Equals_String() => ExecuteStringEnum(FilterOperator.Equals, "device");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_StringEnumProperty_Equals_InvalidString() => ExecuteStringEnum(FilterOperator.Equals, "banana", -1);

        #endregion
        #region NotEquals

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_StringEnumProperty_NotEquals_StringEnum() => ExecuteStringEnum(FilterOperator.NotEquals, expectedIndex: 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_StringEnumProperty_NotEquals_String() => ExecuteStringEnum(FilterOperator.NotEquals, "device", 1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_StringEnumProperty_NotEquals_InvalidString() => ExecuteStringEnum(FilterOperator.NotEquals, "banana", -2);

        #endregion
        #region GreaterThan

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_StringEnumProperty_GreaterThan_StringEnum() => ExecuteStringEnum(FilterOperator.GreaterThan, new StringEnum<ObjectType>(ObjectType.Device), -1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_StringEnumProperty_GreaterThan_String() => ExecuteStringEnum(FilterOperator.GreaterThan, "device", -1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_StringEnumProperty_GreaterThan_InvalidString() => ExecuteStringEnum(FilterOperator.GreaterThan, "banana", -1);

        #endregion
        #region LessThan

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_StringEnumProperty_LessThan_StringEnum() => ExecuteStringEnum(FilterOperator.LessThan, expectedIndex: -1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_StringEnumProperty_LessThan_String() => ExecuteStringEnum(FilterOperator.LessThan, "device", -1);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_StringEnumProperty_LessThan_InvalidString() => ExecuteStringEnum(FilterOperator.LessThan, "banana", -1);

        #endregion
        #region Contains

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_StringEnumProperty_Contains_StringEnum() => ExecuteStringEnum(FilterOperator.Contains);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_StringEnumProperty_Contains_String() => ExecuteStringEnum(FilterOperator.Contains, "dev");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SearchFilterExpression_Types_StringEnumProperty_Contains_InvalidString() => ExecuteStringEnum(FilterOperator.Contains, "banana", -1);

        #endregion
        #endregion

        #endregion

        private void Execute<T>(T good, T bad, T nullObject, Property prop, object value, FilterOperator op = FilterOperator.Equals, int expectedIndex = 0, int expectedCount = 1)
        {
            T[] items;

            if (nullObject == null)
            {
                items = new[] {good, bad};
            }
            else
                items = new[] {good, bad, nullObject};

            var func = SearchFilterToExpression.Parse<T>(new SearchFilter(prop, op, value, FilterMode.Illegal));

            var result = items.Where(func).Where(r => r.Equals(nullObject) == false).ToList();

            if (expectedIndex == -1)
            {
                Assert.AreEqual(0, result.Count);
            }
            else if (expectedIndex == -2)
            {
                Assert.AreEqual(2, result.Count);
            }
            else
            {
                Assert.AreEqual(expectedCount, result.Count, "Count was incorrect");
                Assert.AreEqual(items[expectedIndex], result.Single(), "Expected value was incorrect");
            }
        }

        private void ExecuteBoolean(FilterOperator op, object expectedValue = null, int expectedIndex = 0)
        {
            if (expectedValue == null)
                expectedValue = true;

            var goodObj = new Sensor { Active = true };
            var badObj =  new Sensor { Active = false };

            Execute(goodObj, badObj, null, Property.Active, expectedValue, op, expectedIndex);
        }

        private void ExecuteString(FilterOperator op, string expectedValue = "goodName", int expectedIndex = 0)
        {
            var goodObj = new Sensor { Name = "goodName" };
            var badObj =  new Sensor { Name = "badName"  };
            var nullObj = new Sensor { Name = null };

            Execute(goodObj, badObj, nullObj, Property.Name, expectedValue, op, expectedIndex);
        }

        private void ExecuteInteger(FilterOperator op, object expectedValue = null, int expectedIndex = 0)
        {
            if (expectedValue == null)
                expectedValue = 10;

            var goodObj = new Sensor { Id = 10 };
            var badObj =  new Sensor { Id = 20 };

            Execute(goodObj, badObj, null, Property.Id, expectedValue, op, expectedIndex);
        }

        private void ExecuteDouble(FilterOperator op, object expectedValue = null, int expectedIndex = 0)
        {
            if (expectedValue == null)
                expectedValue = 10.2;

            var goodObj = new Sensor { LastValue = 10.2 };
            var badObj =  new Sensor { LastValue = 20.2 };

            Execute(goodObj, badObj, null, Property.LastValue, expectedValue, op, expectedIndex);
        }

        private void ExecuteEnum(FilterOperator op, object expectedValue = null, int expectedIndex = 0)
        {
            if (expectedValue == null)
                expectedValue = Status.Unknown;

            var goodObj = new Sensor { Status = Status.Unknown };
            var badObj =  new Sensor { Status = Status.Down };

            Execute(goodObj, badObj, null, Property.Status, expectedValue, op, expectedIndex);
        }

        private void ExecuteStringEnum(FilterOperator op, object expectedValue = null, int expectedIndex = 0)
        {
            if (expectedValue == null)
                expectedValue = ObjectType.Device;

            var goodObj = new Device {Type = ObjectType.Device};
            var badObj = new Group {Type = ObjectType.Group};
            var nullObj = new Sensor();

            Execute<PrtgObject>(goodObj, badObj, nullObj, Property.Type, expectedValue, op, expectedIndex);
        }

        private void ExecuteEnumFlags(FilterOperator op, object expectedValue = null, int expectedIndex = 0)
        {
            if (expectedValue == null)
                expectedValue = Status.Paused;

            var goodObj = new Sensor { Status = Status.PausedByLicense };
            var badObj = new Sensor { Status = Status.DownPartial };

            Execute(goodObj, badObj, null, Property.Status, expectedValue, op, expectedIndex);
        }

        private void ExecuteTimeSpan(FilterOperator op, object expectedValue = null, int expectedIndex = 0)
        {
            if (expectedValue == null)
                expectedValue = TimeSpan.FromSeconds(60);

            var goodObj = new Sensor { TotalMonitorTime = TimeSpan.FromSeconds(60) };
            var badObj =  new Sensor { TotalMonitorTime = TimeSpan.FromSeconds(30) };

            Execute(goodObj, badObj, null, Property.TotalMonitorTime, expectedValue, op, expectedIndex);
        }

        private void ExecuteNullableTimeSpan(FilterOperator op, object expectedValue = null, int expectedIndex = 0)
        {
            if (expectedValue == null)
                expectedValue = TimeSpan.FromSeconds(60);

            var goodObj = new Sensor { UpDuration = TimeSpan.FromSeconds(60) };
            var badObj =  new Sensor { UpDuration = TimeSpan.FromSeconds(30) };
            var nullObj = new Sensor { UpDuration = null };

            Execute(goodObj, badObj, nullObj, Property.UpDuration, expectedValue, op, expectedIndex);
        }

        private void ExecuteDateTime(FilterOperator op, object expectedValue = null, int expectedIndex = 0)
        {
            if (expectedValue == null)
                expectedValue = Time.Today;

            var goodObj = new Log { DateTime = Time.Today };
            var badObj =  new Log { DateTime = Time.Yesterday };

            Execute(goodObj, badObj, null, Property.DateTime, expectedValue, op, expectedIndex);
        }

        private void ExecuteNullableDateTime(FilterOperator op, object expectedValue = null, int expectedIndex = 0)
        {
            if (expectedValue == null)
                expectedValue = Time.Today;

            var goodObj = new Sensor { LastUp = Time.Today };
            var badObj =  new Sensor { LastUp = Time.Yesterday };
            var nullObj = new Sensor { LastUp = null };

            Execute(goodObj, badObj, nullObj, Property.LastUp, expectedValue, op, expectedIndex);
        }

        private void ExecuteStringArray(FilterOperator op, object expectedValue = null, int expectedIndex = 0)
        {
            if (expectedValue == null)
                expectedValue = new[] { "good1", "good2" };

            var goodObj = new Sensor { Tags = new[] {"good1", "good2"} };
            var badObj =  new Sensor { Tags = new[] {"bad1", "bad2"} };
            var nullObj = new Sensor { LastUp = null };

            Execute(goodObj, badObj, nullObj, Property.Tags, expectedValue, op, expectedIndex);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void AllPrtgObjectProperties_HaveSearchFilterToExpressionTests()
        {
            PrtgObjectFilterTests.AllPrtgObjectProperties_HaveTests(GetType(), "SearchFilterExpression", null, new[] {"InheritInterval", "NotificationTypes", "DisplayType"} ,new[] {typeof(Log)});
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void AllPrtgObjectPropertyTypes_HaveSearchFilterToExpressionTests()
        {
            var properties = PrtgObjectFilterTests.GetPrtgObjectProperties(new[] {"NotificationTypes","NullableBaseType"});

            var types = properties.Select(p =>
            {
                if (p.PropertyType.IsEnum)
                    return typeof(Enum);

                return p.PropertyType;
            });

            var filteredTypes = PrtgAPIHelpers.DistinctBy(types, t =>
            {
                if (!t.IsNullable())
                    return t.Name;

                return $"Nullable{Nullable.GetUnderlyingType(t).Name}";
            }).ToList();

            var expectedMethods = filteredTypes.SelectMany(GetExpectedTypeMethods).ToList();

            TestHelpers.Assert_TestClassHasMethods(GetType(), expectedMethods);
        }

        private List<string> GetExpectedTypeMethods(Type type)
        {
            var typeName = type.Name.Replace("`1", "");

            var underlying = Nullable.GetUnderlyingType(type);

            if (underlying != null)
                typeName = "Nullable" + underlying.Name;

            if (type.IsEnum)
                typeName = "Enum";

            if (type.Name == "Int32")
                typeName = "Integer";

            if (type.IsArray)
                typeName = type.GetElementType().Name + "Array";

            var operators = Enum.GetValues(typeof(FilterOperator)).Cast<FilterOperator>().Select(f => f.ToString()).ToList();

            var list = new List<string>();

            var prefix = $"SearchFilterExpression_Types_{typeName}Property_";

            foreach (var op in operators)
            {
                var name = $"{prefix}{op}_";

                if (typeName.StartsWith("Nullable"))
                {
                    list.Add(name + typeName.Substring("Nullable".Length));
                }
                else
                    list.Add(name + typeName);

                if (type.Name != "String")
                {
                    list.Add(name + "String");
                    list.Add(name + "InvalidString");
                }

                if (type.Name == "Enum")
                {
                    list.Add(name + "InvalidEnum");
                }

                var u = type.GetUnderlyingType();

                if (type.IsNullable() && u.Name == "Int32" || u.Name == "TimeSpan" || u.Name == "Double" || u.Name == "Boolean" || u.IsEnum) //underlying to enum needs to get the xml/description attribute
                    list.Add(name + "Integer");

                if (u.Name == "Int32" || u.Name == "DateTime")
                    list.Add(name + "Double");
            }

            return list;
        }
    }
}
