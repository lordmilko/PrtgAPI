using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestItems;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectTests
{
    [TestClass]
    public class SensorTotalsTests
    {
        [TestMethod]
        public void SensorTotals_AllFields_HaveValues()
        {
            var webClient = new MockWebClient(new SensorTotalsResponse(new SensorTotalsItem()));
            var client = new PrtgClient("prtg.example.com", "username", "12345678", AuthMode.PassHash, webClient);
            var result = client.GetSensorTotals();

            Assert2.AllPropertiesAreNotDefault(result);
        }

        [TestMethod]
        public async Task SensorTotals_CanExecuteAsync()
        {
            var webClient = new MockWebClient(new SensorTotalsResponse(new SensorTotalsItem()));
            var client = new PrtgClient("prtg.example.com", "username", "12345678", AuthMode.PassHash, webClient);
            await client.GetSensorTotalsAsync();
        }
    }
}
