using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unit = PrtgAPI.Tests.UnitTests.ObjectTests.CSharp.Query.Linq;

namespace PrtgAPI.Tests.IntegrationTests.QueryTests
{
    [TestClass]
    public class CountTests : BaseQueryTest
    {
        [TestMethod]
        public void Data_Query_Count_NoPredicate()
        {
            ExecuteNow(q => q.Count(), r => Assert.AreEqual(Settings.SensorsInTestServer, r));
        }

        [TestMethod]
        public void Data_Query_Count_WithPredicate()
        {
            ExecuteNow(q => q.Count(s => s.Id == Settings.UpSensor), r => Assert.AreEqual(1, r));
        }

        [TestMethod]
        public void Data_CountTests_HasAllTests()
        {
            HasAllTests(typeof(Unit.CountTests));
        }
    }
}
