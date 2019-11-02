using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Attributes;
using PrtgAPI.Linq;
using PrtgAPI.Parameters;
using PrtgAPI.Request;
using PrtgAPI.Request.Serialization.FilterHandlers;
using PrtgAPI.Request.Serialization.ValueConverters;
using PrtgAPI.Schedules;
using PrtgAPI.Targets;
using PrtgAPI.Utilities;
using PrtgAPI.Tests.UnitTests.ObjectData.Query;
using PrtgAPI.Tests.UnitTests.Support;
using PrtgAPI.Tests.UnitTests.Support.TestItems;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;
using ReflectionExtensions = PrtgAPI.Reflection.ReflectionExtensions;

namespace PrtgAPI.Tests.UnitTests.Infrastructure
{
    class Parsee
    {
        public object Value { get; }

        public string StringValue { get; }

        public string Serialized { get; }

        public Parsee(object val, string str, string serialized)
        {
            Value = val;
            StringValue = str;
            Serialized = serialized;
        }
    }

    interface IThrows
    {
        object Value { get; }

        string StringValue { get; }

        void Assert(object val, Action<object> action);
    }

    class Throws<T> : IThrows where T : Exception
    {
        public object Value { get; }

        public string StringValue { get; }

        private Func<object, string> Message { get; }

        public Throws(object value, string str, Func<object, string> message)
        {
            Value = value;
            StringValue = str;
            Message = message;
        }

        public void Assert(object val, Action<object> action)
        {
            AssertEx.Throws<T>(() => action(val), Message(val));
        }
    }

    [TestClass]
    public class TypeTests
    {
        #region Sensor Target

        [UnitTest]
        [TestMethod]
        public void SensorTarget_ObjectEquals_SensorTarget()
        {
            TestObjectEquals(
                (ExeFileTarget) "test.ps1",
                (ExeFileTarget) "test.ps1",
                (ExeFileTarget) "test.sql"
            );
        }

        [UnitTest]
        [TestMethod]
        public void SensorTarget_TypeEquals_SensorTarget()
        {
            TestTypeEquals<SqlServerQueryTarget>(
                "test.sql",
                "test.sql",
                "test.ps1"
            );
        }

        [UnitTest]
        [TestMethod]
        public void SensorTarget_HashCodeEquals_SensorTarget()
        {
            TestHashCode<ExeFileTarget>(
                "test.ps1",
                "test.ps1",
                "test.ps2",
                (SqlServerQueryTarget)"test.ps1", "test.ps1"
            );
        }

        [UnitTest]
        [TestMethod]
        public void SensorTarget_Parse()
        {
            TestParse<ExeFileTarget>(
                new Parsee((ExeFileTarget)"test.ps1", "test.ps1", "test.ps1|test.ps1||")
            );
            TestParse<SqlServerQueryTarget>(
                new Parsee((SqlServerQueryTarget) "test.sql", "test.sql", "test.sql|test.sql||")
            );
        }

        #endregion
        #region Scanning Interval

        [UnitTest]
        [TestMethod]
        public void ScanningInterval_Equals()
        {
            TestObjectEquals(
                ScanningInterval.FifteenMinutes,
                new ScanningInterval(new TimeSpan(0, 15, 0)),
                StandardScanningInterval.FiveMinutes
            );

            TestTypeEquals(
                ScanningInterval.FifteenMinutes,
                new ScanningInterval(new TimeSpan(0, 15, 0)),
                ScanningInterval.FiveMinutes
            );

            TestObjectEquals(
                ScanningInterval.FifteenMinutes,
                new TimeSpan(0, 15, 0),
                new TimeSpan(0, 5, 0)
            );
        }

        [UnitTest]
        [TestMethod]
        public void ScanningInterval_Parse()
        {
            TestParse<ScanningInterval>(
                new Parsee(ScanningInterval.FiveMinutes,       "300|5 minutes", "300|5 minutes"),
                new Parsee(new TimeSpan(0, 0, 10),             "00:00:10",      "10|10 seconds"),
                new Parsee(StandardScanningInterval.FourHours, "FourHours",     "14400|4 hours"),
                new Parsee(20,                                 "20",            "20|20 seconds"),
                new Throws<ArgumentException>(21.1, "21.1", v => $"Cannot convert value '21.1' of type '{v.GetType().FullName}'")
            );
        }

        [UnitTest]
        [TestMethod]
        public void ScanningInterval_HasCorrectHashCode()
        {
            TestHashCode(
                ScanningInterval.OneHour,
                new TimeSpan(1, 0, 0),
                new TimeSpan(0, 1, 0)
            );
        }

        #endregion
        #region Trigger Channel

        [UnitTest]
        [TestMethod]
        public void TriggerChannel_Equals()
        {
            TestObjectEquals(
                TriggerChannel.Primary,
                new TriggerChannel(-999),
                TriggerChannel.Total
            );

            TestTypeEquals(
                TriggerChannel.Primary,
                new TriggerChannel(-999),
                TriggerChannel.Total
            );

            TestObjectEquals(
                TriggerChannel.Primary,
                new Channel { Id = -999 },
                new Channel { Id = 0 }
            );
        }

        [UnitTest]
        [TestMethod]
        public void TriggerChannel_Parse()
        {
            TestParse<TriggerChannel>(
                new Parsee(TriggerChannel.Primary,         "-999",    "-999"),
                new Parsee(StandardTriggerChannel.Primary, "Primary", "-999"),
                new Parsee(new Channel { Id = 3 },         "3",       "3"),
                new Parsee(1,                              "1",       "1"),
                new Throws<ArgumentException>(21.1, "21.1", v => $"Cannot convert value '21.1' of type '{v.GetType().FullName}'")
            );
        }

        [UnitTest]
        [TestMethod]
        public void TriggerChannel_HasCorrectHashCode()
        {
            TestHashCode(
                TriggerChannel.Total,
                new TriggerChannel(-1),
                new Channel {Id = 2}
            );
        }

        #endregion
        #region Device Template

        [UnitTest]
        [TestMethod]
        public void DeviceTemplate_ObjectEquals_DeviceTemplate()
        {
            TestObjectEquals(
                new DeviceTemplate("file1.odt|File 1||"),
                new DeviceTemplate("file1.odt|File 1||"),
                new DeviceTemplate("file2.odt|File 2||")
            );
        }

        [UnitTest]
        [TestMethod]
        public void DeviceTemplate_TypeEquals_DeviceTemplate()
        {
            TestTypeEquals(
                new DeviceTemplate("file1.odt|File 1||"),
                new DeviceTemplate("file1.odt|File 1||"),
                new DeviceTemplate("file2.odt|File 2||")
            );
        }

        [UnitTest]
        [TestMethod]
        public void DeviceTemplate_HashCodeEquals_DeviceTemplate()
        {
            TestHashCode(
                new DeviceTemplate("file1.odt|File 1||"),
                new DeviceTemplate("file1.odt|File 1||"),
                new DeviceTemplate("file2.odt|File 2||")
            );
        }

        [UnitTest]
        [TestMethod]
        public void DeviceTemplate_StringEquals_DeviceTemplate()
        {
            DeviceTemplate template1 = new DeviceTemplate("file.odt|File||");
            DeviceTemplate template2 = new DeviceTemplate("file.odt|File||");

            Assert.AreEqual(template1.ToString(), template2.ToString());
        }

        #endregion
        #region Schedule
        
        [UnitTest]
        [TestMethod]
        public void Schedule_ObjectEquals_Schedule()
        {
            TestObjectEquals(
                new Schedule("623|Saturdays [GMT+0800]|"),
                new Schedule("623|Saturdays [GMT+0800]|"),
                new Schedule("624|Sundays [GMT+0800]|")
            );
        }

        [UnitTest]
        [TestMethod]
        public void Schedule_TypeEquals_Schedule()
        {
            TestTypeEquals(
                new Schedule("623|Saturdays [GMT+0800]|"),
                new Schedule("623|Saturdays [GMT+0800]|"),
                new Schedule("624|Sundays [GMT+0800]|")
            );
        }

        [UnitTest]
        [TestMethod]
        public void Schedule_HashCodeEquals_Schedule()
        {
            TestHashCode(
                new Schedule("623|Saturdays [GMT+0800]|"),
                new Schedule("623|Saturdays [GMT+0800]|"),
                new Schedule("624|Sundays [GMT+0800]|")
            );
        }
        
        #endregion
        #region TimeTable

        [UnitTest]
        [TestMethod]
        public void TimeTable_ObjectEquals_TimeTable()
        {
            var thursday = TimeSlot.Default(DayOfWeek.Thursday, i => i != 3);
            var friday = TimeSlot.Default(DayOfWeek.Friday, i => i != 3);


            TestObjectEquals(
                new TimeTable(thursday: thursday),
                new TimeTable(thursday: thursday),
                new TimeTable(friday: friday)
            );
        }

        [UnitTest]
        [TestMethod]
        public void TimeTable_TypeEquals_TimeTable()
        {
            var thursday = TimeSlot.Default(DayOfWeek.Thursday, i => i != 3);
            var friday = TimeSlot.Default(DayOfWeek.Friday, i => i != 3);

            TestTypeEquals(
                new TimeTable(thursday: thursday),
                new TimeTable(thursday: thursday),
                new TimeTable(friday: friday)
            );
        }

        [UnitTest]
        [TestMethod]
        public void TimeTable_HashCodeEquals_TimeTable()
        {
            var thursday = TimeSlot.Default(DayOfWeek.Thursday, i => i != 3);
            var friday = TimeSlot.Default(DayOfWeek.Friday, i => i != 3);

            TestHashCode(
                new TimeTable(thursday: thursday),
                new TimeTable(thursday: thursday),
                new TimeTable(friday: friday)
            );
        }

        [UnitTest]
        [TestMethod]
        public void TimeTable_Equals_DifferentDaysActive()
        {
            var normal = new TimeTable();

            Action<TimeTable> assert = t => Assert.AreNotEqual(normal, t);

            assert(new TimeTable(TimeSlot.Default(DayOfWeek.Monday, i => i != 0)));
            assert(new TimeTable(tuesday: TimeSlot.Default(DayOfWeek.Tuesday, i => i != 0)));
            assert(new TimeTable(wednesday: TimeSlot.Default(DayOfWeek.Wednesday, i => i != 0)));
            assert(new TimeTable(thursday: TimeSlot.Default(DayOfWeek.Thursday, i => i != 0)));
            assert(new TimeTable(friday: TimeSlot.Default(DayOfWeek.Friday, i => i != 0)));
            assert(new TimeTable(saturday: TimeSlot.Default(DayOfWeek.Saturday, i => i != 0)));
            assert(new TimeTable(sunday: TimeSlot.Default(DayOfWeek.Sunday, i => i != 0)));
        }

        [UnitTest]
        [TestMethod]
        public void TimeTable_Requires24Hours()
        {
            AssertEx.Throws<ArgumentException>(() => new TimeTable(new TimeSlot[] { }), "Must specify 24 records however only 0 were specified.");
        }

        #endregion
        #region TimeSlotRow

        [UnitTest]
        [TestMethod]
        public void TimeSlotRow_ObjectEquals_TimeSlotRow()
        {
            TestObjectEquals(
                new TimeSlotRow(3, wednesday: false),
                new TimeSlotRow(3, wednesday: false),
                new TimeSlotRow(3, thursday: false)
            );
        }

        [UnitTest]
        [TestMethod]
        public void TimeSlotRow_TypeEquals_TimeSlotRow()
        {
            TestTypeEquals(
                new TimeSlotRow(3, wednesday: false),
                new TimeSlotRow(3, wednesday: false),
                new TimeSlotRow(3, thursday: false)
            );
        }

        [UnitTest]
        [TestMethod]
        public void TimeSlotRow_Equals_DifferentDaysActive()
        {
            var normal = new TimeSlotRow(1);

            Action<TimeSlotRow> assert = r => Assert.AreNotEqual(normal, r);

            assert(new TimeSlotRow(2));
            assert(new TimeSlotRow(1, false));
            assert(new TimeSlotRow(1, tuesday: false));
            assert(new TimeSlotRow(1, wednesday: false));
            assert(new TimeSlotRow(1, thursday: false));
            assert(new TimeSlotRow(1, friday: false));
            assert(new TimeSlotRow(1, saturday: false));
            assert(new TimeSlotRow(1, sunday: false));
        }

        [UnitTest]
        [TestMethod]
        public void TimeSlotRow_HashCodeEquals_TimeSlotRow()
        {
            TestHashCode(
                new TimeSlotRow(3, wednesday: false),
                new TimeSlotRow(3, wednesday: false),
                new TimeSlotRow(3, thursday: false)
            );
        }

        [UnitTest]
        [TestMethod]
        public void TimeSlotRow_CompareTo()
        {
            var row1 = new TimeSlotRow(1);
            var row2 = new TimeSlotRow(2);

            Assert.AreEqual(1, row2.CompareTo(row1));
            Assert.AreEqual(1, row2.CompareTo((object)row1));
            Assert.AreEqual(-1, row1.CompareTo(row2));
            Assert.AreEqual(1, row2.CompareTo(null));
            Assert.AreEqual(1, row2.CompareTo((object)null));
            AssertEx.Throws<ArgumentException>(() => row2.CompareTo(1), "Cannot compare TimeSlotRow with value of type Int32.");

            var row2_2 = new TimeSlotRow(2);
            Assert.AreEqual(0, row2.CompareTo(row2_2));

            var row2_3 = new TimeSlotRow(2, false);
            Assert.AreEqual(-1, row2_3.CompareTo(row2));
            Assert.AreEqual(1, row2.CompareTo(row2_3));

            var row3_1 = new TimeSlotRow(3, false, false);
            var row3_2 = new TimeSlotRow(3, false);
            Assert.AreEqual(-1, row3_1.CompareTo(row3_2));
            Assert.AreEqual(1, row3_2.CompareTo(row3_1));
        }

        [UnitTest]
        [TestMethod]
        public void TimeSlotRow_CompareTo_DifferentDays()
        {
            var normal = new TimeSlotRow(1);

            Action<TimeSlotRow> assert = r => Assert.AreEqual(1, normal.CompareTo(r));

            Assert.AreEqual(-1, normal.CompareTo(new TimeSlotRow(2)));
            assert(new TimeSlotRow(1, false));
            assert(new TimeSlotRow(1, tuesday: false));
            assert(new TimeSlotRow(1, wednesday: false));
            assert(new TimeSlotRow(1, thursday: false));
            assert(new TimeSlotRow(1, friday: false));
            assert(new TimeSlotRow(1, saturday: false));
            assert(new TimeSlotRow(1, sunday: false));
        }

        #endregion
        #region TimeSlot

        [UnitTest]
        [TestMethod]
        public void TimeSlot_InvalidHour()
        {
            AssertEx.Throws<ArgumentException>(() => new TimeSlot(30, DayOfWeek.Monday), "Hour must be between 0-23.");
        }

        [UnitTest]
        [TestMethod]
        public void TimeSlot_InvalidDay()
        {
            AssertEx.Throws<ArgumentException>(() => new TimeSlot(1, (DayOfWeek) 10), "'10' is not a valid System.DayOfWeek.");
        }

        [UnitTest]
        [TestMethod]
        public void TimeSlot_ObjectEquals_TimeSlot()
        {
            TestObjectEquals(
                new TimeSlot(1, DayOfWeek.Wednesday),
                new TimeSlot(1, DayOfWeek.Wednesday),
                new TimeSlot(1, DayOfWeek.Thursday)
            );
        }

        [UnitTest]
        [TestMethod]
        public void TimeSlot_TypeEquals_Schedule()
        {
            TestTypeEquals(
                new TimeSlot(2, DayOfWeek.Friday),
                new TimeSlot(2, DayOfWeek.Friday),
                new TimeSlot(2, DayOfWeek.Friday, false)
            );
        }

        [UnitTest]
        [TestMethod]
        public void TimeSlot_HashCodeEquals_TimeSlot()
        {
            TestHashCode(
                new TimeSlot(2, DayOfWeek.Friday),
                new TimeSlot(2, DayOfWeek.Friday),
                new TimeSlot(2, DayOfWeek.Friday, false)
            );
        }

        [UnitTest]
        [TestMethod]
        public void TimeSlot_CompareTo_Same()
        {
            var slot1 = new TimeSlot(1, DayOfWeek.Monday);
            var slot2 = new TimeSlot(1, DayOfWeek.Monday);

            Assert.AreEqual(0, slot1.CompareTo(slot2));
            Assert.AreEqual(0, slot1.CompareTo((object)slot2));
        }

        [UnitTest]
        [TestMethod]
        public void TimeSlot_CompareTo_Illegal()
        {
            var slot1 = new TimeSlot(1, DayOfWeek.Tuesday);
            Assert.AreEqual(1, slot1.CompareTo(null));
            Assert.AreEqual(1, slot1.CompareTo((object)null));
            AssertEx.Throws<ArgumentException>(() => slot1.CompareTo(1), "Cannot compare TimeSlot with value of type Int32.");
        }

        [UnitTest]
        [TestMethod]
        public void TimeSlot_CompareTo_SameDay_DifferentHour()
        {
            var slot1 = new TimeSlot(0, DayOfWeek.Monday);
            var slot2 = new TimeSlot(1, DayOfWeek.Monday);

            Assert.AreEqual(1, slot2.CompareTo(slot1));
            Assert.AreEqual(-1, slot1.CompareTo(slot2));
        }

        [UnitTest]
        [TestMethod]
        public void TimeSlot_CompareTo_DifferentDay_SameHour()
        {
            var slot1 = new TimeSlot(1, DayOfWeek.Monday);
            var slot2 = new TimeSlot(1, DayOfWeek.Tuesday);

            Assert.AreEqual(1, slot2.CompareTo(slot1));
            Assert.AreEqual(-1, slot1.CompareTo(slot2));
        }

        [UnitTest]
        [TestMethod]
        public void TimeSlot_CompareTo_SameDay_SameHour_DifferentActive()
        {
            var slot1 = new TimeSlot(0, DayOfWeek.Monday);
            var slot2 = new TimeSlot(0, DayOfWeek.Monday, false);

            Assert.AreEqual(1, slot1.CompareTo(slot2));
            Assert.AreEqual(-1, slot2.CompareTo(slot1));
        }

        [UnitTest]
        [TestMethod]
        public void TimeSlot_CompareTo_OrderOfDays()
        {
            var monday = new TimeSlot(1, DayOfWeek.Monday);
            var sunday = new TimeSlot(1, DayOfWeek.Sunday);

            Assert.AreEqual(-1, monday.CompareTo(sunday));
            Assert.AreEqual(1, sunday.CompareTo(monday));

            Assert.AreEqual(0, monday.CompareTo(monday));
            Assert.AreEqual(0, sunday.CompareTo(sunday));
        }

        #endregion
        #region Notification Types

        [UnitTest]
        [TestMethod]
        public void NotificationTypes_ObjectEquals_NotificationTypes()
        {
            TestObjectEquals(
                new NotificationTypes("Inherited 1 2 3"),
                new NotificationTypes("Inherited 1 2 3"),
                new NotificationTypes("Inherited 1 2 4")
            );
        }

        [UnitTest]
        [TestMethod]
        public void NotificationTypes_TypeEquals_NotificationTypes()
        {
            TestTypeEquals(
                new NotificationTypes("Inherited 1 2 3"),
                new NotificationTypes("Inherited 1 2 3"),
                new NotificationTypes("Inherited 1 2 4")
            );
        }

        [UnitTest]
        [TestMethod]
        public void NotificationTypes_HashCodeEquals_NotificationTypes()
        {
            TestHashCode(
                new NotificationTypes("Inherited 1 2 3"),
                new NotificationTypes("Inherited 1 2 3"),
                new NotificationTypes("Inherited 1 2 4")
            );
        }

        #endregion
        #region IShallowCloneable

        [UnitTest]
        [TestMethod]
        public void IShallowCloneable_PrtgObjectParameters_CloneFully()
        {
            CloneTable<PrtgObject, PrtgObjectParameters>(new PrtgObjectParameters(), i =>
            {
                if (i.Name == "StartOffset")
                    return true;

                return false;
            });
        }

        [UnitTest]
        [TestMethod]
        public void IShallowCloneable_SensorParameters_CloneFully()
        {
            CloneTable<Sensor, SensorParameters>(new SensorParameters
            {
                Status = new[] {Status.Up}
            }, i =>
            {
                if (i.Name == "StartOffset")
                    return true;

                return false;
            });
        }

        [UnitTest]
        [TestMethod]
        public void IShallowCloneable_DeviceParameters_CloneFully()
        {
            CloneTable<Device, DeviceParameters>(new DeviceParameters(), i =>
            {
                if (i.Name == "StartOffset")
                    return true;

                return false;
            });
        }

        [UnitTest]
        [TestMethod]
        public void IShallowCloneable_GroupParameters_CloneFully()
        {
            CloneTable<Group, GroupParameters>(new GroupParameters(), i =>
            {
                if (i.Name == "StartOffset")
                    return true;

                return false;
            });
        }

        [UnitTest]
        [TestMethod]
        public void IShallowCloneable_ProbeParameters_CloneFully()
        {
            CloneTable<Probe, ProbeParameters>(new ProbeParameters(), i =>
            {
                if (i.Name == "StartOffset")
                    return true;

                if (i.Name == "Content")
                    return true;

                return false;
            });
        }

        [UnitTest]
        [TestMethod]
        public void IShallowCloneable_LogParameters_CloneFully()
        {
            CloneTable<Log, LogParameters>(new LogParameters(3000)
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddHours(-1),
                RecordAge = RecordAge.LastMonth
            });
        }

        [UnitTest]
        [TestMethod]
        public void IShallowCloneable_SensorHistoryParameters_CloneFully()
        {
            Clone(new SensorHistoryParameters(1001, 30, DateTime.Now, DateTime.Now.AddHours(-1), 300)
            {
                Page = 2,
                PageSize = 250,
                Cookie = true
            }, i =>
            {
                if (i.Name == "StartOffset")
                    return true;

                return false;
            });
        }

        [UnitTest]
        [TestMethod]
        public void IShallowCloneable_SetChannelPropertyParameters_CloneFully()
        {
            var ids = new[] {1001, 1002, 1003};
            var @params = new[]
            {
                new ChannelParameter(ChannelProperty.UpperErrorLimit, 100),
                new ChannelParameter(ChannelProperty.SpikeFilterEnabled, true)
            };

            var parameters = new SetChannelPropertyParameters(ids, 1, @params)
            {
                Cookie = true
            };

            Clone(parameters);
        }

        private void CloneTable<TObject, TParam>(TParam parameters, Func<MemberInfo, bool> customHandler = null)
            where TObject : ITableObject, IObject
            where TParam : TableParameters<TObject>, IShallowCloneable<TParam>
        {
            if (parameters.SearchFilters != null)
            {
                var old = parameters.SearchFilters.ToList();

                old.Add(new SearchFilter(Property.Id, 3000));

                parameters.SearchFilters = old.ToList();
            }
            else
                parameters.SearchFilters = new List<SearchFilter> {new SearchFilter(Property.Id, 3000)};

            parameters.Count = 15;
            parameters.Page = 2;
            parameters.PageSize = 250;
            parameters.SortBy = Property.Id;
            parameters.SortDirection = SortDirection.Descending;
            parameters.Cookie = true;

            Clone(parameters, customHandler);
        }

        private void Clone<TParam>(TParam parameters, Func<MemberInfo, bool> customHandler = null) where TParam : IShallowCloneable<TParam>
        {
            AssertEx.AllPropertiesAndFieldsAreNotDefault(parameters, i =>
            {
                if (i.Name == "PageSize" && i.GetValue(parameters).Equals(500))
                    Assert.Fail("Property 'PageSize' did not have a value.");

                if (customHandler != null)
                    return customHandler(i);

                return false;
            });

            try
            {
                AssertEx.AllPropertiesAndFieldsAreEqual(parameters, parameters);
            }
            catch (AssertFailedException ex)
            {
                throw new AssertFailedException($"Source object was not self equatable: {ex.Message}", ex);
            }

            var clone = ((IShallowCloneable<TParam>)parameters).ShallowClone();

            AssertEx.AllPropertiesAndFieldsAreEqual(parameters, clone);
        }

        [UnitTest]
        [TestMethod]
        public void All_IShallowCloneable_Types_HaveTests()
        {
            var types = typeof(IShallowCloneable).Assembly.GetTypes().Where(t => typeof(IShallowCloneable).IsAssignableFrom(t) && !t.IsInterface).ToList();

            var expected = types.Select(t => $"IShallowCloneable_{t.Name}_CloneFully").ToList();

            var actual = GetType().GetMethods().Where(m => m.GetCustomAttribute<TestMethodAttribute>() != null).ToList();

            var missing = expected.Where(e => actual.All(m => m.Name != e)).OrderBy(m => m).ToList();

            if (missing.Count > 0)
                Assert.Fail($"{missing.Count} tests are missing: " + string.Join(", ", missing));
        }

        #endregion
        #region Log

        [UnitTest]
        [TestMethod]
        public void Log_EqualityComparer_Equals_DifferentObjectsSameValues()
        {
            var client = BaseTest.Initialize_Client(new MessageResponse(new MessageItem()));

            var log1 = client.GetLogs().Single();
            var log2 = client.GetLogs().Single();

            var comparer = new LogEqualityComparer();

            Assert.IsTrue(!Equals(log1, log2), "Both logs referenced equaled each other");
            Assert.IsTrue(comparer.Equals(log1, log2), "Logs were not equal");
        }

        [UnitTest]
        [TestMethod]
        public void Log_EqualityComparer_Equals_DifferentObjectsNullableValues()
        {
            var item = new MessageItem(null, null, "43042.2573037732", null, null, "604", null, null, null, null, "1", null, null, "2304", null, null, null, null);

            var client = BaseTest.Initialize_Client(new MessageResponse(item));

            var log1 = client.GetLogs().Single();
            var log2 = client.GetLogs().Single();

            var comparer = new LogEqualityComparer();

            Assert.IsTrue(!Equals(log1, log2), "Both logs referenced equaled each other");
            Assert.IsTrue(comparer.Equals(log1, log2), "Logs were not equal");
        }

        [UnitTest]
        [TestMethod]
        public void Log_EqualityComparer_GetHashCode_UniqueValues()
        {
            var random = new Random();

            var response = new MessageResponse(Enumerable.Range(0, 50).Select(i =>
            {
                var r = random.NextDouble();

                return new MessageItem(
                    $"WMI Remote Ping_{r}",
                    datetimeRaw: (43042.2573037732 + r).ToString(),
                    parent: $"Probe Device_{r}",
                    sensor: $"WMI Remote Ping_{r}",
                    device: $"Probe Device_{r}",
                    group: $"Local Probe_{r}"
                );
            }).ToArray());

            var logs = BaseTest.Initialize_Client(response).GetLogs(count: null);

            Assert.AreEqual(50, logs.Count);

            var set = new HashSet<Log>(new LogEqualityComparer());

            foreach (var log in logs)
                set.Add(log);

            Assert.AreEqual(50, set.Count);

            var comparer = new LogEqualityComparer();

            foreach (var log in logs)
                Assert.AreEqual(comparer.GetHashCode(log), comparer.GetHashCode(log));
        }

        [UnitTest]
        [TestMethod]
        public void Log_EqualityComparer_GetHashCode_NullableValues()
        {
            var random = new Random();

            var response = new MessageResponse(Enumerable.Range(0, 50).Select(i =>
            {
                var dateTimeRaw = 43042.2573037732 + random.NextDouble() * 3000;
                var dateTimeRawStr = dateTimeRaw.ToString(CultureInfo.InvariantCulture);

                return new MessageItem(null, null, dateTimeRawStr, null, null, "604", null, null, null, null, "1", null, null, "2304", null, null, null, null);
            }).ToArray());

            var logs = BaseTest.Initialize_Client(response).GetLogs(count: null);

            Assert.AreEqual(50, logs.Count);

            var set = new HashSet<Log>(new LogEqualityComparer());

            foreach (var log in logs)
                set.Add(log);

            Assert.AreEqual(50, set.Count);

            var comparer = new LogEqualityComparer();

            foreach (var log in logs)
                Assert.AreEqual(comparer.GetHashCode(log), comparer.GetHashCode(log));
        }

        #endregion
        #region SensorType

        [UnitTest]
        [TestMethod]
        public void SensorType_AllValues_HaveTypeAttributes()
        {
            //All SensorType values should have type attributes
            //If a value has a null type attribute, further assert that there are no types derived from
            //SensorParametersInternal that ISN'T defined on any SensorType

            var missingParameter = false;

            var knownTypes = new List<Type>();

            foreach (SensorType value in Enum.GetValues(typeof(SensorType)))
            {
                var attribute = value.GetEnumAttribute<TypeAttribute>();

                if (attribute == null)
                    Assert.Fail($"SensorType '{value}' is missing a {nameof(TypeAttribute)}");

                if (attribute.Class == null)
                    missingParameter = true;
                else
                    knownTypes.Add(attribute.Class);

            }

            if (missingParameter)
            {
                var types = typeof(SensorParametersInternal).Assembly.GetTypes();

                var filteredTypes = types.Where(t => typeof(SensorParametersInternal).IsAssignableFrom(t) && typeof(SensorParametersInternal) != t).ToList();

                var missing = filteredTypes.Except(knownTypes).ToList();

                if (missing.Count > 0)
                    Assert.Fail($"Sensor parameter type '{missing[0].Name}' has not been assigned to a SensorType. Do these parameters belong to a SensorType whose parameter type is null?");
            }
        }

        #endregion
        #region ToString

        [UnitTest]
        [TestMethod]
        public void ToStringTests()
        {
            var client = BaseTest.Initialize_Client(new MultiTypeResponse());

            var @event = client.GetModificationHistory(1001).First();
            Assert.AreEqual($"{@event.DateTime}: Created. 17.2.31.2018", @event.ToString());

            var properties = client.GetDeviceProperties(1001);
            Assert.AreEqual("Device", properties.ToString());

            var log = client.GetLogs().First();
            Assert.AreEqual($"{log.DateTime}: WMI Remote Ping0", log.ToString());

            var history = client.GetSensorHistory(1001).First();
            Assert.AreEqual(history.DateTime.ToString(), history.ToString());
            Assert.AreEqual("PercentAvailableMemory: 51 %", history.ChannelRecords.First().ToString());

            var timeSlotRowNever = new TimeSlotRow(1, false, false, false, false, false, false, false);
            Assert.AreEqual("01:00 Never", timeSlotRowNever.ToString());

            var timeSlotRowWeekdays = new TimeSlotRow(2, saturday: false, sunday: false);
            Assert.AreEqual("02:00 Weekdays", timeSlotRowWeekdays.ToString());

            var timeSlotRowWeekends = new TimeSlotRow(3, false, false, false, false, false);
            Assert.AreEqual("03:00 Weekends", timeSlotRowWeekends.ToString());
        }

        #endregion

        private void TestObjectEquals(object value1, object sameValue1, object value2)
        {
            object value1Ref = value1;

            //Value equals object reference
            Assert.IsTrue(value1.Equals(value1Ref));

            //Value equals object null
            Assert.IsFalse(value1.Equals(null));

            //Value equals object value
            Assert.IsFalse(value1.Equals(value2));
            Assert.IsTrue(value1.Equals(sameValue1));

            //Value equals object wrong type
            Assert.IsFalse(value1.Equals(1));
        }

        private void TestTypeEquals<T>(T value1, T sameValue1, T value2) where T : class, IEquatable<T>
        {
            T value1Ref = value1;

            //Value equals type reference
            Assert.IsTrue(value1.Equals(value1Ref));

            //Value equals type null
            Assert.IsFalse(value1.Equals(null));

            //Value equals type value
            Assert.IsFalse(value1.Equals(value2));
            Assert.IsTrue(value1.Equals(sameValue1));
        }

        private void TestHashCode<T>(T value1, T sameValue1, T value2, params object[] wrongType)
        {
            var value1Hash = value1.GetHashCode();
            var sameValue1Hash = sameValue1.GetHashCode();
            var value2Hash = value2.GetHashCode();

            Assert.AreEqual(value1Hash, sameValue1Hash);
            Assert.AreNotEqual(value1Hash, value2Hash);

            foreach (var wrong in wrongType)
            {
                var wrongTypeHash = wrong.GetHashCode();
                Assert.AreNotEqual(value1Hash, wrongTypeHash);
            }
        }

        private void TestParse<T>(params object[] args) where T : ISerializable
        {
             var method = typeof(T).GetMethod("Parse", BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static);

            var parse = (Func<object, T>)Delegate.CreateDelegate(typeof(Func<object, T>), method);

            foreach (var v in args)
            {
                var arg = v as Parsee;

                if (arg != null)
                {
                    var normal = parse(arg.Value);
                    var str = parse(arg.StringValue);

                    Assert.AreEqual(arg.Serialized, normal.GetSerializedFormat());
                    Assert.AreEqual(arg.Serialized, str.GetSerializedFormat());
                    Assert.AreEqual(normal, str);
                }
                else
                {
                    var throws = v as IThrows;

                    if (throws != null)
                    {
                        throws.Assert(throws.Value, a => parse(a));
                        throws.Assert(throws.StringValue, a => parse(a));
                    }
                    else
                        throw new NotImplementedException($"Don't know how to handle argument of type {v.GetType().Name}");
                }
            }

            AssertEx.Throws<ArgumentNullException>(() => parse(null), "Value cannot be null");
            AssertEx.Throws<ArgumentException>(() => parse(string.Empty), "Cannot convert value '' of type 'System.String'");

            if (typeof(IEnumEx).IsAssignableFrom(typeof(T)))
                AssertEx.Throws<ArgumentException>(() => parse("abc123"), "Cannot convert value 'abc123' of type 'System.String'");
        }

        [UnitTest]
        [TestMethod]
        public void All_PrtgObjectProperties_HaveArrayLookupProperties()
        {
            var properties = PrtgObjectFilterTests.GetPrtgObjectProperties(new[] {"NotificationTypes"});
            var propertyTypes = PrtgAPIHelpers.DistinctBy(properties.Select(p => p.PropertyType).Select(ReflectionExtensions.GetUnderlyingType), p => p.Name).ToList();

            var arrayTypes = typeof(DynamicParameterPropertyTypes).GetProperties();

            var missing = propertyTypes.Where(p => arrayTypes.All(a => a.PropertyType.GetElementType() != p) && !p.IsArray && !ReflectionExtensions.IsNullable(p)).ToList();

            if (missing.Count > 0)
                Assert.Fail($"{missing.Count}/{propertyTypes.Count} properties are missing are missing: " + string.Join(", ", missing));
        }

        [UnitTest]
        [TestMethod]
        public void AllBoolean_PrtgObjectProperties_HaveXmlBoolAttribute()
        {
            var values = GetFilterPropertiesForPrtgObjectProperties(p => p.PropertyType == typeof(bool) || p.PropertyType == typeof(bool?));

            foreach (var val in values)
            {
                var attrib = val.GetEnumAttribute<XmlBoolAttribute>();

                if (attrib == null)
                    Assert.Fail($"Boolean property {nameof(Property)}.{val} is missing a {nameof(XmlBoolAttribute)}");
            }
        }

        [UnitTest]
        [TestMethod]
        public void AllString_FilterProperties_HaveStringFilterHandler()
        {
            AllPropertiesOfTypeHaveFilterHandler<StringFilterHandler>(
                p => p.PropertyType == typeof(string) || p.PropertyType == typeof(string[]),
                v => v != Property.Url && v != Property.Condition
            );
        }

        [UnitTest]
        [TestMethod]
        public void AllTimeSpan_FilterProperties_HaveTimeSpanValueConverter()
        {
            var exclusions = new[]
            {
                Property.Interval //Only requires padding to 10 spaces. Covered by ZeroPaddingConverter
            };

            AllPropertiesOfTypeHaveValueConverter<TimeSpanConverter>(p => p.PropertyType == typeof(TimeSpan) || p.PropertyType == typeof(TimeSpan?), exclusions);
        }

        [UnitTest]
        [TestMethod]
        public void AllDateTime_FilterProperties_HaveDateTimeValueConverter()
        {
            AllPropertiesOfTypeHaveValueConverter<DateTimeConverter>(p => p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?));
        }

        private void AllPropertiesOfTypeHaveFilterHandler<THandler>(Func<PropertyInfo, bool> typeFilter, Func<Property, bool> exclusions)
        {
            var values = GetFilterPropertiesForPrtgObjectProperties(typeFilter).
                Where(exclusions).ToList();

            foreach (var value in values)
            {
                var handler = value.GetEnumAttribute<FilterHandlerAttribute>();

                Assert.IsNotNull(handler, $"Property '{value}' does not have a {typeof(THandler).Name}");
                
                Assert.IsTrue(handler.Handler is THandler, $"Filter handler on property {value} was a {handler.Handler.GetType().Name} instead of a {typeof(THandler).Name}");
            }
        }

        private void AllPropertiesOfTypeHaveValueConverter<TConverter>(Func<PropertyInfo, bool> typeFilter, params Property[] exclusions)
        {
            var values = GetFilterPropertiesForPrtgObjectProperties(typeFilter).
                Where(v => v != Property.Url && !exclusions.Contains(v)).ToList();

            foreach (var value in values)
            {
                var converter = value.GetEnumAttribute<ValueConverterAttribute>();

                Assert.IsNotNull(converter, $"Property '{value}' does not have a {typeof(TConverter).Name}");

                Assert.IsTrue(converter.Converter is TConverter, $"Filter handler on property {value} was a {converter.Converter.GetType().Name} instead of a {typeof(TConverter).Name}");
            }
        }

        private List<Property> GetFilterPropertiesForPrtgObjectProperties(Func<PropertyInfo, bool> typeFilter)
        {
            var types = typeof(PrtgObject).Assembly.GetTypes().Where(t => typeof(PrtgObject).IsAssignableFrom(t)).ToList();

            var properties = types.SelectMany(t => t.GetProperties().Where(typeFilter)).ToList();

            var validProperties = properties.Where(p => p.GetCustomAttributes(typeof(PropertyParameterAttribute), false).Length > 0).ToList();

            var values = validProperties
                .Select(p => p.GetCustomAttributes(typeof(PropertyParameterAttribute), false).First())
                .Cast<PropertyParameterAttribute>()
                .Select(a =>
                {
                    if (a.Property.GetType() == typeof(Property))
                        return (Property?)a.Property;

                    return null;
                })
                .Where(e => e != null)
                .Cast<Property>()
                .Distinct()
                .ToList();

            return values;
        }
    }
}
