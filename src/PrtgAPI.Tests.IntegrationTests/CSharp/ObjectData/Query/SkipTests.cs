using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Parameters;
using Unit = PrtgAPI.Tests.UnitTests.ObjectData.Query;

namespace PrtgAPI.Tests.IntegrationTests.ObjectData.Query
{
    [TestClass]
    public class SkipTests : BaseQueryTest
    {
        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Skip_Standalone()
        {
            var sensors = client.GetSensors().Skip(1).ToList();

            ExecuteNow(q => q.Skip(1).ToList(), s => AssertEx.AreEqualLists(sensors, s, new PrtgObjectComparer(), "Lists were not equal"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Skip_Twice()
        {
            var sensors = client.GetSensors();

            ExecuteQuery(q => q.Skip(1).Skip(1), s => Assert.AreEqual(sensors.Count - 2, s.Count));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Skip_ThreeTimes()
        {
            ExecuteClient(q => q.QueryDevices().Skip(1).Skip(1).Skip(1), s => Assert.AreEqual(1, s.Count()));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Skip_Logs()
        {
            var logs = client.GetLogs(Settings.UpSensor, RecordAge.All, null);

            ExecuteClient(c => c.QueryLogs(l => l.Id == Settings.UpSensor).Skip(1), s => Assert.AreEqual(logs.Count - 1, s.Count()));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Skip_ToTake()
        {
            var sensor = client.GetSensors().Skip(1).First();

            ExecuteNow(q => q.Skip(1).Take(1).ToList(), s => Assert.AreEqual(sensor.Id, s.Single().Id));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Skip_FromTake()
        {
            var sensor = client.GetSensors().Skip(1).First();

            ExecuteNow(q => q.Take(2).Skip(1).ToList(), s => Assert.AreEqual(sensor.Id, s.Single().Id));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Skip_ToWhere()
        {
            var sensors = client.GetSensors().Skip(1).Where(s => s.Name.Contains("Ping")).ToList();

            ExecuteQuery(q => q.Skip(1).Where(s => s.Name.Contains("Ping")), s => AssertEx.AreEqualLists(sensors, s, new PrtgObjectComparer(), "Lists were not equal"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Skip_FromWhere()
        {
            ExecuteQuery(q => q.Where(s => s.Name.Contains("Volume")).Skip(1), s => Assert.AreEqual(2, s.Count));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Skip_OnePage()
        {
            //the start should be offset by 2
            //and what if we had skipped 1001 to begin with?
            ExecuteNow(q => q.Skip(501).ToList(), s =>
            {
                Assert.AreEqual(0, s.Count);
            });
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Skip_TwoPages()
        {
            //the start should be offset by 2
            //and what if we had skipped 1001 to begin with?
            ExecuteNow(q => q.Skip(1002).ToList(), s =>
            {
                Assert.AreEqual(0, s.Count);
            });
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Skip_All()
        {
            ExecuteNow(q => q.Skip(1200).ToList(), s =>
            {
                Assert.AreEqual(0, s.Count);
            });
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Skip_All_WithCount()
        {
            var parameters = new SensorParameters
            {
                Start = 3
            };

            ExecuteClient(c => c.StreamSensors(parameters, true), s => s.ToList());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_Skip_MoreThanAll()
        {
            ExecuteNow(q => q.Skip(1300).ToList(), s =>
            {
                Assert.AreEqual(0, s.Count);
            });
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_SkipTests_HasAllTests()
        {
            HasAllTests(typeof(Unit.SkipTests));
        }
    }
}
