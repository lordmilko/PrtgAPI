using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Parameters;

namespace PrtgAPI.Tests.IntegrationTests.ObjectData
{
    [TestClass]
    public class SensorTargetTests : BasePrtgClientTest
    {
        [TestMethod]
        [IntegrationTest]
        public void Data_SensorTargets_Retrieves_FromSensorWithTargets()
        {
            var targets = client.Targets.GetSensorTargets(Settings.Device, "wmivolume");

            Assert.AreEqual(4, targets.Count, "Did not have expected number of targets");

            Assert.AreEqual("System Reserved", targets[0].Properties[2]);
            Assert.AreEqual("Local Disk", targets[1].Properties[3]);
            Assert.AreEqual("Local Disk", targets[2].Properties[3]);
            Assert.AreEqual("Compact Disk", targets[3].Properties[3]);
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_SensorTargets_Throws_Retrieving_FromSensorWithoutTargets()
        {
            AssertEx.Throws<ArgumentException>(() => client.Targets.GetSensorTargets(Settings.Device, "http"), "Cannot guess sensor target table. Please specify tableName");
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_SensorTargets_Timeout_CustomTimeout()
        {
            AssertEx.Throws<TimeoutException>(
                () => client.Targets.GetWmiServices(Settings.Device, timeout: 0),
                "Failed to retrieve sensor information within a reasonable period of time."
            );
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Data_SensorTargets_Timeout_CustomTimeoutAsync()
        {
            await AssertEx.ThrowsAsync<TimeoutException>(
                async () => await client.Targets.GetWmiServicesAsync(Settings.Device, timeout: 0),
                "Failed to retrieve sensor information within a reasonable period of time."
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_SensorTargets_AsReadOnlyUser_NoQueryTarget_Throws()
        {
            AssertEx.Throws<PrtgRequestException>(
                () => readOnlyClient.Targets.GetSensorTargets(Settings.Device, "exexml"),
                "you may not have sufficient permissions on the specified object. The server responded"
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_SensorTargets_AsReadOnlyUser_WithQueryTarget_Throws()
        {
            AssertEx.Throws<PrtgRequestException>(
                () => readOnlyClient.Targets.GetSensorTargets(Settings.Device, "snmplibrary", queryParameters: (SensorQueryTarget) "APC UPS.oidlib"),
                "you may not have sufficient permissions on the specified object. The server responded"
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_SensorTargets_AsReadOnlyUser_WithQueryTargetParameters_Throws()
        {
            AssertEx.Throws<PrtgRequestException>(
                () => readOnlyClient.Targets.GetSensorTargets(Settings.Device, "oracletablespace"),
                "you may not have sufficient permissions on the specified object. The server responded"
            );
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Data_SensorTargets_AsReadOnlyUser_NoQueryTarget_ThrowsAsync()
        {
            await AssertEx.ThrowsAsync<PrtgRequestException>(
                async () => await readOnlyClient.Targets.GetSensorTargetsAsync(Settings.Device, "exexml"),
                "you may not have sufficient permissions on the specified object. The server responded"
            );
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Data_SensorTargets_AsReadOnlyUser_WithQueryTarget_ThrowsAsync()
        {
            await AssertEx.ThrowsAsync<PrtgRequestException>(
                async () => await readOnlyClient.Targets.GetSensorTargetsAsync(Settings.Device, "snmplibrary", queryParameters: (SensorQueryTarget)"APC UPS.oidlib"),
                "you may not have sufficient permissions on the specified object. The server responded"
            );
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Data_SensorTargets_AsReadOnlyUser_WithQueryTargetParameters_ThrowsAsync()
        {
            await AssertEx.ThrowsAsync<PrtgRequestException>(
                async () => await readOnlyClient.Targets.GetSensorTargetsAsync(Settings.Device, "oracletablespace"),
                "you may not have sufficient permissions on the specified object. The server responded"
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_SensorTargets_SensorQueryTarget_ParsesTarget()
        {
            AssertEx.Throws<TimeoutException>(
                () => client.Targets.GetSensorTargets(Settings.Device, "snmplibrary", timeout: 3, queryParameters: (SensorQueryTarget)"APC UPS.oidlib"),
                "Failed to retrieve sensor information within a reasonable period of time"
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_SensorTargets_SensorQueryTarget_Throws_WhenTargetIsInvalid()
        {
            AssertEx.Throws<InvalidOperationException>(
                () => client.Targets.GetSensorTargets(Settings.Device, "snmplibrary", timeout: 3, queryParameters: (SensorQueryTarget)"test"),
                $"Query target 'test' is not a valid target for sensor type 'snmplibrary' on device ID {Settings.Device}. Please specify one of the following targets: 'APC UPS.oidlib',"
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_SensorTargets_SensorQueryTarget_Throws_WhenTargetIsNotRequired()
        {
            AssertEx.Throws<InvalidOperationException>(
                () => client.Targets.GetSensorTargets(Settings.Device, "exexml", queryParameters: (SensorQueryTarget)"test"),
                "Cannot specify query target 'test' on sensor type 'exexml': type does not support query targets."
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_SensorTargets_SensorQueryTarget_Throws_WhenTargetMissing()
        {
            AssertEx.Throws<InvalidOperationException>(
                () => client.Targets.GetSensorTargets(Settings.Device, "snmplibrary"),
                "Failed to process query for sensor type 'snmplibrary': a sensor query target is required, however none was specified. Please specify one of the following targets: 'APC UPS.oidlib',"
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_SensorTargets_SensorQueryParameters_ParsesParameters()
        {
            var queryParameters = new SensorQueryTargetParameters
            {
                ["database"] = "XE",
                ["sid_type"] = 0,
                ["prefix"] = 0
            };

            AssertEx.Throws<PrtgRequestException>(
                () => client.Targets.GetSensorTargets(Settings.Device, "oracletablespace", queryParameters: queryParameters),
                "Specified sensor type may not be valid on this device"
            );
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_SensorTargets_SensorQueryParameters_Throws_WhenParametersAreMissing()
        {
            AssertEx.Throws<InvalidOperationException>(
                () => client.Targets.GetSensorTargets(Settings.Device, "oracletablespace", queryParameters: new SensorQueryTargetParameters()),
                "Failed to process request for sensor type 'oracletablespace': sensor query target parameters did not include mandatory parameters 'database_', 'sid_type_', 'prefix_'."
            );
        }
    }
}
