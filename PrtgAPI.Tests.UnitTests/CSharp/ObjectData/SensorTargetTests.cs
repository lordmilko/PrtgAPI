using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectTests
{
    [TestClass]
    public class SensorTargetTests : BaseTest
    {
        [TestMethod]
        public void SensorTarget_CanExecute()
        {
            var files = client.Targets.GetExeXmlFiles(1001);

            Assert.AreEqual(2, files.Count);
        }

        [TestMethod]
        public async Task SensorTarget_CanExecuteAsync()
        {
            var files = await client.Targets.GetExeXmlFilesAsync(1001);

            Assert.AreEqual(2, files.Count);
        }

        [TestMethod]
        public void SensorTarget_CanAbort()
        {
            var result = client.Targets.GetExeXmlFiles(1001, f => false);

            Assert.AreEqual(null, result);
        }

        [TestMethod]
        public void SensorTarget_Generic_CanExecute()
        {
            var files = client.Targets.GetSensorTargets(1001, "exexml");

            Assert.AreEqual(2, files.Count);
        }

        [TestMethod]
        public async Task SensorTarget_Generic_CanExecuteAsync()
        {
            var files = await client.Targets.GetSensorTargetsAsync(1001, "exexml");

            Assert.AreEqual(2, files.Count);
        }

        [TestMethod]
        public void SensorTarget_Generic_Resolves_WithTable()
        {
            var files = client.Targets.GetSensorTargets(1001, "exexml", "exefile");

            Assert.AreEqual(2, files.Count);
        }

        [TestMethod]
        public void SensorTarget_Generic_Throws_WithInvalidTable()
        {
            AssertEx.Throws<ArgumentException>(
                () => client.Targets.GetSensorTargets(1001, "exexml", "blah"),
                "Cannot find any tables named 'blah'. Available tables: 'exefile'."
            );;
        }

        [TestMethod]
        public void SensorTarget_Generic_HasCorrectProperties()
        {
            var file = client.Targets.GetSensorTargets(1001, "exexml").First();

            Assert.AreEqual(3, file.Properties.Length);
            Assert.AreEqual("Demo Batchfile - Returns static values in four channels.bat", file.Properties[0]);
            Assert.AreEqual("Demo Batchfile - Returns static values in four channels.bat", file.Properties[1]);
            Assert.AreEqual(string.Empty, file.Properties[2]);
        }

        [TestMethod]
        public async Task SensorTarget_CanAbortAsync()
        {
            var result = await client.Targets.GetExeXmlFilesAsync(1001, f => false);

            Assert.AreEqual(null, result);
        }

        [TestMethod]
        public void SensorTarget_FailedRequest()
        {
            var faultyClient = Initialize_Client(new FaultySensorTargetResponse(FaultySensorTargetResponse.Scenario.Credentials));

            AssertEx.Throws<PrtgRequestException>(() => faultyClient.Targets.GetExeXmlFiles(1001), "Failed to retrieve data from device; required credentials");
        }

        private PrtgClient client => Initialize_Client(new ExeFileTargetResponse());
    }
}
