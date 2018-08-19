using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unit = PrtgAPI.Tests.UnitTests.ObjectTests.CSharp.Query.Linq;

namespace PrtgAPI.Tests.IntegrationTests.QueryTests
{
    [TestClass]
    public class AnyTests : BaseQueryTest
    {
        [TestMethod]
        public void Data_Query_Any_NoPredicate()
        {
            ExecuteNow(q => q.Any(), Assert.IsTrue);
        }

        [TestMethod]
        public void Data_Query_Any_WithPredicate()
        {
            var upSensor = client.GetSensor(Settings.UpSensor);

            ExecuteNow(q => q.Any(s => s.Name == upSensor.Name), Assert.IsTrue);
        }

        [TestMethod]
        public void Data_Query_Any_WithPredicate_AfterWhere()
        {
            var upSensor = client.GetSensor(Settings.UpSensor);

            ExecuteNow(q => q.Where(s => s.Id == Settings.DownSensor).Any(s => s.Name == upSensor.Name), Assert.IsFalse);
        }

        [TestMethod]
        public void Data_AnyTests_HasAllTests()
        {
            HasAllTests(typeof(Unit.AnyTests));
        }
    }
}
