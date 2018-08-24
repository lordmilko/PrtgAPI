using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.IntegrationTests.ObjectData
{
    [TestClass]
    public class SensorTargetTests : BasePrtgClientTest
    {
        [TestMethod]
        public void SensorTargets_Retrieves_FromSensorWithTargets()
        {
            var targets = client.Targets.GetSensorTargets(Settings.Device, "wmivolume");

            Assert.AreEqual(3, targets.Count, "Did not have expected number of targets");

            Assert.AreEqual("System Reserved", targets[0].Properties[2]);
            Assert.AreEqual("Local Disk", targets[1].Properties[3]);
            Assert.AreEqual("Compact Disk", targets[2].Properties[3]);
        }

        [TestMethod]
        public void SensorTargets_Throws_Retrieving_FromSensorWithoutTargets()
        {
            AssertEx.Throws<ArgumentException>(() => client.Targets.GetSensorTargets(Settings.Device, "http"), "Cannot guess sensor target table. Please specify tableName");
        }
    }
}
