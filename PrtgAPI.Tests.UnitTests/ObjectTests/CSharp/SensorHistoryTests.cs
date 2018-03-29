using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectTests
{
    [TestClass]
    public class SensorHistoryTests : BaseTest
    {
        [TestMethod]
        public void SensorHistory_CanExecute()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            var records = client.GetSensorHistory(1001);

            Assert.AreEqual(2, records.Count);
        }

        [TestMethod]
        public async Task SensorHistory_CanExecuteAsync()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            var records = await client.GetSensorHistoryAsync(1001);

            Assert.AreEqual(records.Count, 2);
        }

        [TestMethod]
        public void SensorHistory_CanStream()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            var records = client.StreamSensorHistory(1001).ToList();

            Assert.AreEqual(2, records.Count);
        }
    }
}
