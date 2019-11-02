using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unit = PrtgAPI.Tests.UnitTests.ObjectData.Query;

namespace PrtgAPI.Tests.IntegrationTests.ObjectData.Query
{
    [TestClass]
    public class AnyTests : BaseQueryTest
    {
        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Any_NoPredicate()
        {
            ExecuteNow(q => q.Any(), Assert.IsTrue);
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Any_WithPredicate()
        {
            var upSensor = client.GetSensor(Settings.UpSensor);

            ExecuteNow(q => q.Any(s => s.Name == upSensor.Name), Assert.IsTrue);
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Query_Any_WithPredicate_AfterWhere()
        {
            var upSensor = client.GetSensor(Settings.UpSensor);

            ExecuteNow(q => q.Where(s => s.Id == Settings.DownSensor).Any(s => s.Name == upSensor.Name), Assert.IsFalse);
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_AnyTests_HasAllTests()
        {
            HasAllTests(typeof(Unit.AnyTests));
        }
    }
}
