using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unit = PrtgAPI.Tests.UnitTests.ObjectData.Query;

namespace PrtgAPI.Tests.IntegrationTests.ObjectData.Query
{
    [TestClass]
    public class OrderByTests : BaseQueryTest
    {
        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_OrderBy_MultipleProperties()
        {
            var sensors = client.GetSensors().OrderBy(s => s.Id).OrderBy(s => s.Name).ToList();

            ExecuteQuery(q => q.OrderBy(s => s.Id).OrderBy(s => s.Name), s => AssertEx.AreEqualLists(s, sensors, new PrtgObjectComparer(), "Sensors were not ordered correctly"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_OrderBy_ThreeTimes()
        {
            var sensors = client.GetSensors().OrderBy(s => s.Id).ToList();

            ExecuteQuery(q => q.OrderBy(s => s.Id).OrderBy(s => s.Name).OrderBy(s => s.Id), s => AssertEx.AreEqualLists(s, sensors, new PrtgObjectComparer(), "Sensors were not ordered correctly"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_OrderBy_ForwardsThenBackwards()
        {
            var sensors = client.GetSensors().OrderBy(s => s.Id).OrderByDescending(s => s.Name).ToList();

            ExecuteQuery(q => q.OrderBy(s => s.Id).OrderByDescending(s => s.Name), s => AssertEx.AreEqualLists(s, sensors, new PrtgObjectComparer(), "Sensors were not ordered correctly"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_OrderBy_KeyComparer()
        {
            var sensors = client.GetSensors().OrderBy(s => s.Name).ThenBy(s => s.Id).ToList();

            ExecuteQuery(
                q => q.OrderBy(s => s.Name, StringComparer.CurrentCulture),
                s => AssertEx.AreEqualLists(s, sensors, new PrtgObjectComparer(), "Sensors were not ordered correctly")
            );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_OrderBy_WithComparer_ToOrderBy_WithoutComparer()
        {
            var sensors = client.GetSensors().OrderBy(s => s.Id).ToList();

            ExecuteQuery(q => q.OrderBy(s => s.Name, StringComparer.CurrentCulture).OrderBy(s => s.Id), s => AssertEx.AreEqualLists(s, sensors, new PrtgObjectComparer(), "Sensors were not ordered correctly"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_Query_OrderBy_WithoutComparer_ToOrderBy_WithComparer()
        {
            var sensors = client.GetSensors().OrderBy(s => s.Id).OrderBy(s => s.Name, StringComparer.CurrentCulture).ToList();

            ExecuteQuery(q => q.OrderBy(s => s.Id).OrderBy(s => s.Name, StringComparer.CurrentCulture), s => AssertEx.AreEqualLists(s, sensors, new PrtgObjectComparer(), "Sensors were not ordered correctly"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_OrderByTests_HasAllTests()
        {
            HasAllTests(typeof(Unit.OrderByTests));
        }
    }
}
