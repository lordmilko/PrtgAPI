using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Parameters;
using PrtgAPI.Tests.UnitTests.Support;
using Unit = PrtgAPI.Tests.UnitTests.ObjectData.Query;

namespace PrtgAPI.Tests.IntegrationTests.ObjectData.Query
{
    [TestClass]
    public class TakeTests : BaseQueryTest
    {
        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Take_ThenCount_NoPredicate()
        {
            ExecuteNow(q => q.Take(2).Count(), r => Assert.AreEqual(2, r));
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Take_Smaller_ThenBigger()
        {
            ExecuteNow(q => q.Take(2).Take(3).Count(), r => Assert.AreEqual(2, r));
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Take_Bigger_ThenSmaller()
        {
            ExecuteNow(q => q.Take(3).Take(2).Count(), r => Assert.AreEqual(2, r));
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Take_ThreeTimes()
        {
            ExecuteNow(q => q.Take(3).Take(1).Take(2).Count(), r => Assert.AreEqual(1, r));
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Take_ThenWhere()
        {
            var sensors = client.GetSensors();

            ExecuteNow(q => q.Take(2).Where(s => s.Id == sensors.Skip(1).First().Id).Count(), r => Assert.AreEqual(1, r));
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Take_ThenCount_WithPredicate()
        {
            var sensors = client.GetSensors();

            ExecuteNow(q => q.Take(2).Count(s => s.Id == sensors.Skip(1).First().Id), r => Assert.AreEqual(1, r));
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Where_ThenTake_LegalPredicate()
        {
            ExecuteNow(q => q.Where(s => s.Id == Settings.UpSensor).Take(2).Count(), r => Assert.AreEqual(1, r));
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Where_ThenTake_IllLegalPredicate_And()
        {
            ExecuteNow(q => q.Where(s => s.Id == Settings.UpSensor && s.Id == Settings.DownSensor).Take(2).Count(), r => Assert.AreEqual(0, r));
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Where_ThenTake_IllLegalPredicate_Or()
        {
            ExecuteNow(
                q => q.Where(s => s.Id == Settings.UpSensor || s.ParentId == Settings.Device).Take(2).Count(),
                r => Assert.AreEqual(2, r)
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Take_Until_Matched()
        {
            var response = client.QuerySensors(s => s.Name.Contains("Pi") && s.Name == "Ping").Take(2).ToList();

            Assert.AreEqual(2, response.Count);
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Take_Until_Matched_Manual()
        {
            var parameters = new SensorParameters
            {
                PageSize = 1,
                SearchFilters = new List<SearchFilter> { new SearchFilter(Property.Name, FilterOperator.Contains, "Pi") }
            };

            var response = client.StreamSensors(parameters, true).Where(s => s.Name == "Ping").Take(2).ToList();

            Assert.AreEqual(2, response.Count);
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Take_Until_Matched_TakeIterator()
        {
            Func<SensorParameters, Func<int>, IEnumerable<Sensor>> streamer = (p, c) =>
                PrtgAPIHelpers.StreamObjects(client, p, true, c);

            var parameters = new SensorParameters
            {
                SearchFilters = new List<SearchFilter> { new SearchFilter(Property.Name, FilterOperator.Contains, "Pi") }
            };

            var iterator = PrtgAPIHelpers.TakeIterator(
                2,
                parameters,
                streamer,
                () => client.GetTotalObjects(parameters.Content, parameters.SearchFilters?.ToArray()),
                r => r.Where(s => s.Name == "Ping")
            );

            var response = iterator.ToList();
            Assert.AreEqual(2, response.Count);
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_TakeTests_HasAllTests()
        {
            HasAllTests(typeof(Unit.TakeTests));
        }
    }
}
