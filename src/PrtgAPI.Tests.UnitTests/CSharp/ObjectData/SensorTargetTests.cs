using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Parameters;
using PrtgAPI.Tests.UnitTests.Support;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectData
{
    [TestClass]
    public class SensorTargetTests : BaseTest
    {
        [UnitTest]
        [TestMethod]
        public void SensorTarget_CanExecute()
        {
            var files = client.Targets.GetExeXmlFiles(1001);

            Assert.AreEqual(2, files.Count);
        }

        [UnitTest]
        [TestMethod]
        public async Task SensorTarget_CanExecuteAsync()
        {
            var files = await client.Targets.GetExeXmlFilesAsync(1001);

            Assert.AreEqual(2, files.Count);
        }

        [UnitTest]
        [TestMethod]
        public void SensorTarget_CanAbort()
        {
            var result = client.Targets.GetExeXmlFiles(1001, f => false);

            Assert.AreEqual(null, result);
        }

        [UnitTest]
        [TestMethod]
        public void SensorTarget_Generic_CanExecute()
        {
            var files = client.Targets.GetSensorTargets(1001, "exexml");

            Assert.AreEqual(2, files.Count);
        }

        [UnitTest]
        [TestMethod]
        public async Task SensorTarget_Generic_CanExecuteAsync()
        {
            var files = await client.Targets.GetSensorTargetsAsync(1001, "exexml");

            Assert.AreEqual(2, files.Count);
        }

        [UnitTest]
        [TestMethod]
        public void SensorTarget_Generic_Resolves_WithTable()
        {
            var files = client.Targets.GetSensorTargets(1001, "exexml", "exefile");

            Assert.AreEqual(2, files.Count);
        }

        [UnitTest]
        [TestMethod]
        public void SensorTarget_Generic_Throws_WithInvalidTable()
        {
            AssertEx.Throws<ArgumentException>(
                () => client.Targets.GetSensorTargets(1001, "exexml", "blah"),
                "Cannot find any tables named 'blah'. Available tables: 'exefile'."
            );;
        }

        [UnitTest]
        [TestMethod]
        public void SensorTarget_Generic_HasCorrectProperties()
        {
            var file = client.Targets.GetSensorTargets(1001, "exexml").First();

            Assert.AreEqual(3, file.Properties.Length);
            Assert.AreEqual("Demo Batchfile - Returns static values in four channels.bat", file.Properties[0]);
            Assert.AreEqual("Demo Batchfile - Returns static values in four channels.bat", file.Properties[1]);
            Assert.AreEqual(string.Empty, file.Properties[2]);
        }

        [UnitTest]
        [TestMethod]
        public async Task SensorTarget_CanAbortAsync()
        {
            var result = await client.Targets.GetExeXmlFilesAsync(1001, f => false);

            Assert.AreEqual(null, result);
        }

        [UnitTest]
        [TestMethod]
        public void SensorTarget_FailedRequest()
        {
            var faultyClient = Initialize_Client(new FaultySensorTargetResponse(FaultySensorTargetResponse.Scenario.Credentials));

            AssertEx.Throws<PrtgRequestException>(() => faultyClient.Targets.GetExeXmlFiles(1001), "Failed to retrieve data from device; required credentials");
        }

        [UnitTest]
        [TestMethod]
        public void SensorTarget_ReadOnly()
        {
            var client = Initialize_ReadOnlyClient(new MultiTypeResponse());

            AssertEx.Throws<PrtgRequestException>(() => client.Targets.GetExeXmlFiles(1001), "you may not have sufficient permissions on the specified object. The server responded");
        }

        [UnitTest]
        [TestMethod]
        public async Task SensorTarget_ReadOnlyAsync()
        {
            var client = Initialize_ReadOnlyClient(new MultiTypeResponse());

            await AssertEx.ThrowsAsync<PrtgRequestException>(async () => await client.Targets.GetExeXmlFilesAsync(1001), "you may not have sufficient permissions on the specified object. The server responded");
        }

        [UnitTest]
        [TestMethod]
        public void SensorTarget_SensorQueryTarget_ParsesTarget()
        {
            var client = Initialize_Client(new SensorQueryTargetValidatorResponse(new[]
            {
                UnitRequest.SensorTypes(1001), //Get initial target

                UnitRequest.SensorTypes(1001), //Verify specified target
                UnitRequest.BeginAddSensorQuery(1001, "snmplibrary_nolist", "APC+UPS.oidlib"),
                UnitRequest.AddSensorProgress(1001, 2),
                UnitRequest.EndAddSensorQuery(1001, 2)
            }));

            var target = client.GetSensorTypes(1001).First(t => t.Id == "snmplibrary").QueryTargets.First();

            client.Targets.GetSensorTargets(1001, "snmplibrary", queryParameters: target);
        }

        [UnitTest]
        [TestMethod]
        public void SensorTarget_SensorQueryTarget_Throws_WhenTargetIsInvalid()
        {
            var client = Initialize_Client(new SensorQueryTargetValidatorResponse(new[]
            {
                UnitRequest.SensorTypes(1001)
            }));

            AssertEx.Throws<InvalidOperationException>(
                () => client.Targets.GetSensorTargets(1001, "snmplibrary", queryParameters: (SensorQueryTarget)"test"),
                "Query target 'test' is not a valid target for sensor type 'snmplibrary' on device ID 1001. Please specify one of the following targets: 'APC UPS.oidlib',"
            );
        }

        [UnitTest]
        [TestMethod]
        public void SensorTarget_SensorQueryTarget_Throws_WhenTargetIsNotRequired()
        {
            var client = Initialize_Client(new SensorQueryTargetValidatorResponse(new[]
            {
                UnitRequest.SensorTypes(1001)
            }));

            AssertEx.Throws<InvalidOperationException>(
                () => client.Targets.GetSensorTargets(1001, "ptfadsreplfailurexml", queryParameters: (SensorQueryTarget)"test"),
                "Cannot specify query target 'test' on sensor type 'ptfadsreplfailurexml': type does not support query targets."
            );
        }

        [UnitTest]
        [TestMethod]
        public void SensorTarget_SensorQueryTarget_Throws_WhenTargetMissing()
        {
            var client = Initialize_Client(new SensorQueryTargetValidatorResponse(new[]
            {
                UnitRequest.SensorTypes(1001)
            }));

            AssertEx.Throws<InvalidOperationException>(
                () => client.Targets.GetSensorTargets(1001, "snmplibrary"),
                "Failed to process query for sensor type 'snmplibrary': a sensor query target is required, however none was specified. Please specify one of the following targets: 'APC UPS.oidlib',"
            );
        }

        [UnitTest]
        [TestMethod]
        public void SensorTarget_SensorQueryTarget_Throws_WhenTypeIsInvalid()
        {
            AssertEx.Throws<InvalidOperationException>(
                () => client.Targets.GetSensorTargets(1001, "potato", queryParameters: (SensorQueryTarget)"test"),
                "Failed to validate query target 'test' on sensor type 'potato': sensor type 'potato' is not valid."
            );
        }

        [UnitTest]
        [TestMethod]
        public void SensorTarget_SensorQueryTarget_Throws_WhenTypeIsInvalid_NoTargetSpecified()
        {
            AssertEx.Throws<InvalidOperationException>(
                () => client.Targets.GetSensorTargets(1001, "potato"),
                "Cannot process query for sensor type 'potato': sensor type 'potato' is not valid."
            );
        }

        [UnitTest]
        [TestMethod]
        public void SensorTarget_SensorQueryParameters_ParsesParameters()
        {
            var client = Initialize_Client(new SensorQueryTargetParametersValidatorResponse(new[]
            {
                UnitRequest.SensorTypes(1001),
                UnitRequest.BeginAddSensorQuery(1001, "ptfadsreplfailurexml"),
                UnitRequest.ContinueAddSensorQuery(2055, 7, "database_=XE&sid_type_=0&prefix_=0"),
                UnitRequest.AddSensorProgress(1001, 7),
                UnitRequest.EndAddSensorQuery(1001, 7)
            }));

            client.Targets.GetSensorTargets(1001, "ptfadsreplfailurexml", queryParameters: new SensorQueryTargetParameters
            {
                ["database"] = "XE",
                ["sid_type"] = 0,
                ["prefix"] = 0
            });
        }

        [UnitTest]
        [TestMethod]
        public void SensorTarget_SensorQueryParameters_Throws_WhenParametersAreMissing()
        {
            var client = Initialize_Client(new SensorQueryTargetParametersValidatorResponse(new[]
            {
                UnitRequest.SensorTypes(1001),
                UnitRequest.BeginAddSensorQuery(1001, "ptfadsreplfailurexml")
            }));

            AssertEx.Throws<InvalidOperationException>(
                () => client.Targets.GetSensorTargets(1001, "ptfadsreplfailurexml", queryParameters: new SensorQueryTargetParameters()),
                "Failed to process request for sensor type 'oracletablespace': sensor query target parameters did not include mandatory parameters 'database_',"
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task SensorTarget_SensorQueryTarget_ParsesTargetAsync()
        {
            var client = Initialize_Client(new SensorQueryTargetValidatorResponse(new[]
            {
                UnitRequest.SensorTypes(1001), //Get initial target

                UnitRequest.SensorTypes(1001), //Verify specified target
                UnitRequest.BeginAddSensorQuery(1001, "snmplibrary_nolist", "APC+UPS.oidlib"),
                UnitRequest.AddSensorProgress(1001, 2),
                UnitRequest.EndAddSensorQuery(1001, 2)
            }));

            var target = (await client.GetSensorTypesAsync(1001)).First(t => t.Id == "snmplibrary").QueryTargets.First();

            await client.Targets.GetSensorTargetsAsync(1001, "snmplibrary", queryParameters: target);
        }

        [UnitTest]
        [TestMethod]
        public async Task SensorTarget_SensorQueryTarget_Throws_WhenTargetIsInvalidAsync()
        {
            var client = Initialize_Client(new SensorQueryTargetValidatorResponse(new[]
            {
                UnitRequest.SensorTypes(1001)
            }));

            await AssertEx.ThrowsAsync<InvalidOperationException>(
                async () => await client.Targets.GetSensorTargetsAsync(1001, "snmplibrary", queryParameters: (SensorQueryTarget)"test"),
                "Query target 'test' is not a valid target for sensor type 'snmplibrary' on device ID 1001. Please specify one of the following targets: 'APC UPS.oidlib',"
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task SensorTarget_SensorQueryTarget_Throws_WhenTargetIsNotRequiredAsync()
        {
            var client = Initialize_Client(new SensorQueryTargetValidatorResponse(new[]
            {
                UnitRequest.SensorTypes(1001)
            }));

            await AssertEx.ThrowsAsync<InvalidOperationException>(
                async () => await client.Targets.GetSensorTargetsAsync(1001, "ptfadsreplfailurexml", queryParameters: (SensorQueryTarget)"test"),
                "Cannot specify query target 'test' on sensor type 'ptfadsreplfailurexml': type does not support query targets."
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task SensorTarget_SensorQueryTarget_Throws_WhenTargetMissingAsync()
        {
            var client = Initialize_Client(new SensorQueryTargetValidatorResponse(new[]
            {
                UnitRequest.SensorTypes(1001)
            }));

            await AssertEx.ThrowsAsync<InvalidOperationException>(
                async () => await client.Targets.GetSensorTargetsAsync(1001, "snmplibrary"),
                "Failed to process query for sensor type 'snmplibrary': a sensor query target is required, however none was specified. Please specify one of the following targets: 'APC UPS.oidlib',"
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task SensorTarget_SensorQueryTarget_Throws_WhenTypeIsInvalidAsync()
        {
            await AssertEx.ThrowsAsync<InvalidOperationException>(
                async () => await client.Targets.GetSensorTargetsAsync(1001, "potato", queryParameters: (SensorQueryTarget)"test"),
                "Failed to validate query target 'test' on sensor type 'potato': sensor type 'potato' is not valid."
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task SensorTarget_SensorQueryTarget_Throws_WhenTypeIsInvalid_NoTargetSpecifiedAsync()
        {
            await AssertEx.ThrowsAsync<InvalidOperationException>(
                async () => await client.Targets.GetSensorTargetsAsync(1001, "potato"),
                "Cannot process query for sensor type 'potato': sensor type 'potato' is not valid."
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task SensorTarget_SensorQueryParameters_ParsesParametersAsync()
        {
            var client = Initialize_Client(new SensorQueryTargetParametersValidatorResponse(new[]
            {
                UnitRequest.SensorTypes(1001),
                UnitRequest.BeginAddSensorQuery(1001, "ptfadsreplfailurexml"),
                UnitRequest.ContinueAddSensorQuery(2055, 7, "database_=XE&sid_type_=0&prefix_=0"),
                UnitRequest.AddSensorProgress(1001, 7),
                UnitRequest.EndAddSensorQuery(1001, 7)
            }));

            await client.Targets.GetSensorTargetsAsync(1001, "ptfadsreplfailurexml", queryParameters: new SensorQueryTargetParameters
            {
                ["database"] = "XE",
                ["sid_type"] = 0,
                ["prefix"] = 0
            });
        }

        [UnitTest]
        [TestMethod]
        public async Task SensorTarget_SensorQueryParameters_Throws_WhenParametersAreMissingAsync()
        {
            var client = Initialize_Client(new SensorQueryTargetParametersValidatorResponse(new[]
            {
                UnitRequest.SensorTypes(1001),
                UnitRequest.BeginAddSensorQuery(1001, "ptfadsreplfailurexml")
            }));

            await AssertEx.ThrowsAsync<InvalidOperationException>(
                async () => await client.Targets.GetSensorTargetsAsync(1001, "ptfadsreplfailurexml", queryParameters: new SensorQueryTargetParameters()),
                "Failed to process request for sensor type 'oracletablespace': sensor query target parameters did not include mandatory parameters 'database_',"
            );
        }

        private PrtgClient client => Initialize_Client(new ExeFileTargetResponse());
    }
}
