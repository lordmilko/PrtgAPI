using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestItems;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectData
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

            AssertEx.AllPropertiesAreNotDefault(result);
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
