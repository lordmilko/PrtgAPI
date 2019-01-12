using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Schedules;
using PrtgAPI.Tests.UnitTests.Support.TestItems;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectData
{
    [TestClass]
    public class ScheduleTests : StandardObjectTests<Schedule, ScheduleItem, ScheduleResponse>
    {
        [TestMethod]
        [TestCategory("UnitTest")]
        public void Schedule_CanDeserialize() => Object_CanDeserialize();

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Schedule_CanDeserializeAsync() => await Object_CanDeserializeAsync();

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Schedule_GetObjectsOverloads_CanExecute()
        {
            var client = Initialize_Client_WithItems(GetItem());

            Assert.IsTrue(client.GetSchedules().Any());
            Assert.IsTrue(client.GetSchedulesAsync().Result.Any());

            Assert.IsTrue(client.GetSchedules(Property.Id, 620).Any());
            Assert.IsTrue(client.GetSchedulesAsync(Property.Id, 620).Result.Any());

            Assert.IsTrue(client.GetSchedules(new SearchFilter(Property.Id, 620)).Any());
            Assert.IsTrue(client.GetSchedulesAsync(new SearchFilter(Property.Id, 620)).Result.Any());

            Assert.IsTrue(client.GetSchedule(620) != null);
            Assert.IsTrue(client.GetScheduleAsync(620).Result != null);

            Assert.IsTrue(client.GetSchedule("test") != null);
            Assert.IsTrue(client.GetScheduleAsync("test").Result != null);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Schedule_AllFields_HaveValues() => Object_AllFields_HaveValues(p =>
        {
            if (p.Name == "Tags")
                return true;

            return false;
        });

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Schedule_ReadOnly()
        {
            var client = Initialize_ReadOnlyClient(GetResponse(new[] { GetItem() }));

            var schedule = client.GetSchedule(1001);

            Assert.IsNull(schedule.TimeTable);

            AssertEx.AllPropertiesRetrieveValues(schedule);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Schedule_ReadOnlyAsync()
        {
            var client = Initialize_ReadOnlyClient(GetResponse(new[] { GetItem() }));

            var schedule = await client.GetScheduleAsync(1001);

            Assert.IsNull(schedule.TimeTable);

            AssertEx.AllPropertiesRetrieveValues(schedule);
        }

        protected override List<Schedule> GetObjects(PrtgClient client) => client.GetSchedules();

        protected override async Task<List<Schedule>> GetObjectsAsync(PrtgClient client) => await client.GetSchedulesAsync();

        public override ScheduleItem GetItem() => new ScheduleItem();

        protected override ScheduleResponse GetResponse(ScheduleItem[] items) => new ScheduleResponse(items);

        #region TimeTable

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Schedule_TimeTable_UsesIndexers()
        {
            var timetable = GetTimeTable();

            var tuesday1am = timetable[1, DayOfWeek.Tuesday];
            Assert.IsTrue(ReferenceEquals(tuesday1am, timetable.Grid[DayOfWeek.Tuesday][1]));
            Assert.IsTrue(ReferenceEquals(tuesday1am, timetable[1][1]));
            Assert.IsTrue(ReferenceEquals(tuesday1am, timetable[DayOfWeek.Tuesday][1]));

            Assert.AreEqual(timetable.Rows[1].Tuesday, timetable[1, DayOfWeek.Tuesday].Active);
            Assert.AreEqual(timetable[1, DayOfWeek.Tuesday].Active, timetable[new TimeSpan(1, 0, 0), DayOfWeek.Tuesday].Active);

            Assert.AreEqual(24, timetable[DayOfWeek.Wednesday].Length);
            Assert.AreEqual(7, timetable[3].Length);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Schedule_TimeTable_Rows_MatchSlots()
        {
            var timetable = GetTimeTable();

            foreach (var col in timetable.Grid)
            {
                foreach (var slot in col.Value)
                {
                    var row = timetable.Rows[slot.Hour];

                    bool rowActive;

                    switch (slot.Day)
                    {
                        case DayOfWeek.Monday:
                            rowActive = row.Monday;
                            break;
                        case DayOfWeek.Tuesday:
                            rowActive = row.Tuesday;
                            break;
                        case DayOfWeek.Wednesday:
                            rowActive = row.Wednesday;
                            break;
                        case DayOfWeek.Thursday:
                            rowActive = row.Thursday;
                            break;
                        case DayOfWeek.Friday:
                            rowActive = row.Friday;
                            break;
                        case DayOfWeek.Saturday:
                            rowActive = row.Saturday;
                            break;
                        case DayOfWeek.Sunday:
                            rowActive = row.Sunday;
                            break;
                        default:
                            throw new NotImplementedException("Invalid day of week");
                    }

                    Assert.AreEqual(slot.Active, rowActive, $"{slot.Day} at {row.Time} was incorrect");
                }
            }
        }

        private TimeTable GetTimeTable()
        {
            var schedule = GetSingleItem();

            return schedule.TimeTable;
        }

        #endregion
    }
}
