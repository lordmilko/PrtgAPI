using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectTests
{
    [TestClass]
    public class GetSensorHistoryTests : BaseTest
    {
        [TestMethod]
        public void GetSensorHistory_CanExecute()
        {
            var client = Initialize_Client(new SensorHistoryResponse());

            client.GetSensorHistory(1001);
        }

        [TestMethod]
        public async Task GetSensorHistory_CanExecuteAsync()
        {
            var client = Initialize_Client(new SensorHistoryResponse());

            await client.GetSensorHistoryAsync(1001);
        }
    }
}
