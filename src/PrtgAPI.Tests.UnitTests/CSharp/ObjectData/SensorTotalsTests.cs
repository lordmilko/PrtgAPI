using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestItems;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectData
{
    [TestClass]
    public class SensorTotalsTests : BaseTest
    {
        [UnitTest]
        [TestMethod]
        public void SensorTotals_AllFields_HaveValues()
        {
            var client = Initialize_Client(new SensorTotalsResponse(new SensorTotalsItem()));

            var result = client.GetSensorTotals();

            AssertEx.AllPropertiesAreNotDefault(result);
        }

        [UnitTest]
        [TestMethod]
        public async Task SensorTotals_CanExecuteAsync()
        {
            var client = Initialize_Client(new SensorTotalsResponse(new SensorTotalsItem()));

            var result = await client.GetSensorTotalsAsync();

            AssertEx.AllPropertiesAreNotDefault(result);
        }

        [UnitTest]
        [TestMethod]
        public void SensorTotals_ReadOnly()
        {
            var client = Initialize_ReadOnlyClient(new SensorTotalsResponse(new SensorTotalsItem()));

            var result = client.GetSensorTotals();

            AssertEx.AllPropertiesRetrieveValues(result);
        }

        [UnitTest]
        [TestMethod]
        public async Task SensorTotals_ReadOnlyAsync()
        {
            var client = Initialize_ReadOnlyClient(new SensorTotalsResponse(new SensorTotalsItem()));

            var result = await client.GetSensorTotalsAsync();

            AssertEx.AllPropertiesRetrieveValues(result);
        }

        [UnitTest]
        [TestMethod]
        public void SensorTotals_Probe_Normal()
        {
            var first = new ProbeItem(name: "First");
            var second = new ProbeItem(name: "Second");

            var response = new ProbeResponse(first, second);

            var client = Initialize_Client(response);

            var result = client.GetTotalObjects(Content.Probes);

            Assert.AreEqual(2, result);
        }

        [UnitTest]
        [TestMethod]
        public async Task SensorTotals_Probe_NormalAsync()
        {
            var first = new ProbeItem(name: "First");
            var second = new ProbeItem(name: "Second");

            var response = new ProbeResponse(first, second);

            var client = Initialize_Client(response);

            var result = await client.GetTotalObjectsAsync(Content.Probes);

            Assert.AreEqual(2, result);
        }

        [UnitTest]
        [TestMethod]
        public void SensorTotals_Probe_RemoveNonProbe()
        {
            var first = new ProbeItem(name: "First");
            var second = new ProbeItem(name: "Second");
            var third = new ProbeItem(typeRaw: "AutonomousDevice");

            var response = new ProbeResponse(first, second, third);

            var client = Initialize_Client(response);

            var result = client.GetTotalObjects(Content.Probes);

            Assert.AreEqual(2, result);
        }

        [UnitTest]
        [TestMethod]
        public async Task SensorTotals_Probe_RemoveNonProbeAsync()
        {
            var first = new ProbeItem(name: "First");
            var second = new ProbeItem(name: "Second");
            var third = new ProbeItem(typeRaw: "AutonomousDevice");

            var response = new ProbeResponse(first, second, third);

            var client = Initialize_Client(response);

            var result = await client.GetTotalObjectsAsync(Content.Probes);

            Assert.AreEqual(2, result);
        }
    }
}
