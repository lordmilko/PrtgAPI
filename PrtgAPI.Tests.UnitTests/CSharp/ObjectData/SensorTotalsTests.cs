using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestItems;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectData
{
    [TestClass]
    public class SensorTotalsTests : BaseTest
    {
        [TestMethod]
        public void SensorTotals_AllFields_HaveValues()
        {
            var client = Initialize_Client(new SensorTotalsResponse(new SensorTotalsItem()));

            var result = client.GetSensorTotals();

            AssertEx.AllPropertiesAreNotDefault(result);
        }

        [TestMethod]
        public async Task SensorTotals_CanExecuteAsync()
        {
            var client = Initialize_Client(new SensorTotalsResponse(new SensorTotalsItem()));

            var result = await client.GetSensorTotalsAsync();

            AssertEx.AllPropertiesAreNotDefault(result);
        }

        [TestMethod]
        public void SensorTotals_ReadOnly()
        {
            var client = Initialize_ReadOnlyClient(new SensorTotalsResponse(new SensorTotalsItem()));

            var result = client.GetSensorTotals();

            AssertEx.AllPropertiesRetrieveValues(result);
        }

        [TestMethod]
        public async Task SensorTotals_ReadOnlyAsync()
        {
            var client = Initialize_ReadOnlyClient(new SensorTotalsResponse(new SensorTotalsItem()));

            var result = await client.GetSensorTotalsAsync();

            AssertEx.AllPropertiesRetrieveValues(result);
        }
    }
}
