using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.IntegrationTests.ObjectData
{
    [TestClass]
    public class SensorTargetTests : BasePrtgClientTest
    {
        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void SensorTargets_Retrieves_FromSensorWithTargets()
        {
            var targets = client.Targets.GetSensorTargets(Settings.Device, "wmivolume");

            Assert.AreEqual(4, targets.Count, "Did not have expected number of targets");

            Assert.AreEqual("System Reserved", targets[0].Properties[2]);
            Assert.AreEqual("Local Disk", targets[1].Properties[3]);
            Assert.AreEqual("Local Disk", targets[2].Properties[3]);
            Assert.AreEqual("Compact Disk", targets[3].Properties[3]);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void SensorTargets_Throws_Retrieving_FromSensorWithoutTargets()
        {
            AssertEx.Throws<ArgumentException>(() => client.Targets.GetSensorTargets(Settings.Device, "http"), "Cannot guess sensor target table. Please specify tableName");
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void SensorTargets_Timeout_CustomTimeout()
        {
            AssertEx.Throws<TimeoutException>(
                () => client.Targets.GetWmiServices(Settings.Device, timeout: 0),
                "Failed to retrieve sensor information within a reasonable period of time."
            );
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public async Task SensorTargets_Timeout_CustomTimeoutAsync()
        {
            await AssertEx.ThrowsAsync<TimeoutException>(
                async () => await client.Targets.GetWmiServicesAsync(Settings.Device, timeout: 0),
                "Failed to retrieve sensor information within a reasonable period of time."
            );
        }
    }
}
