using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unit = PrtgAPI.Tests.UnitTests.ObjectData.Query;

namespace PrtgAPI.Tests.IntegrationTests.ObjectData.Query
{
    [TestClass]
    public class FirstTests : BaseQueryTest
    {
        [TestMethod]
        [IntegrationTest]
        public void Data_Query_First_NoPredicate()
        {
            var first = client.QuerySensors().First();

            ExecuteNow(q => q.First(), r => Assert.AreEqual(first.Name, r.Name));
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_First_WithPredicate()
        {
            var upSensor = client.GetSensor(Settings.UpSensor);

            ExecuteNow(q => q.First(s => s.Id == upSensor.Id), r => Assert.AreEqual(upSensor.Name, r.Name));
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_First_NoPredicate_WithWhere()
        {
            var upSensor = client.GetSensor(Settings.UpSensor);

            ExecuteNow(q => q.Where(s => s.Name.Contains(upSensor.Name)).First(), r => Assert.AreEqual(upSensor.Name, r.Name));
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_First_WithPredicate_WithWhere()
        {
            var pingSensors = client.GetSensors(Property.Name, "Ping");

            Assert.IsTrue(pingSensors.Count > 1, "Did not have more than one Ping sensor");

            ExecuteNow(q => q.Where(s => s.Name.Contains("Pi")).First(s => s.Id == pingSensors.Last().Id), r => Assert.AreEqual("Ping", r.Name));
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_First_NoPredicate_AfterTake()
        {
            var first = client.GetSensors().First();

            ExecuteNow(q => q.Take(2).First(), r => Assert.AreEqual(first.Name, r.Name));
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_First_Predicate_AfterTake()
        {
            var second = client.GetSensors().Skip(1).First();

            ExecuteNow(q => q.Take(2).First(s => s.Id == second.Id), r => Assert.AreEqual(second.Name, r.Name));
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_First_Backwards()
        {
            var sensor = client.GetSensors().OrderByDescending(s => s.Name).First();

            ExecuteNow(q => q.OrderByDescending(s => s.Name).First(), r => Assert.AreEqual(sensor.Name, r.Name));
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_FirstOrDefault_NoPredicate()
        {
            var first = client.GetSensors().First();

            ExecuteNow(q => q.FirstOrDefault(), r => Assert.AreEqual(first.Id, r.Id));
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_FirstOrDefault_WithPredicate()
        {
            ExecuteNow(q => q.FirstOrDefault(s => s.Name == "FAKE_SENSOR"), r => Assert.AreEqual(null, r));
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_FirstTests_HasAllTests()
        {
            HasAllTests(typeof(Unit.FirstTests));
        }
    }
}
