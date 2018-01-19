using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectTests
{
    [TestClass]
    public class SensorTargetTests : BaseTest
    {
        [TestMethod]
        public void SensorTarget_CanExecute()
        {
            var files = client.Targets.GetExeXmlFiles(1001, null);

            Assert.AreEqual(2, files.Count);
        }

        [TestMethod]
        public async Task SensorTarget_CanExecuteAsync()
        {
            var files = await client.Targets.GetExeXmlFilesAsync(1001, null);

            Assert.AreEqual(2, files.Count);
        }

        [TestMethod]
        public void SensorTarget_CanAbort()
        {
            var result = client.Targets.GetExeXmlFiles(1001, f => false);

            Assert.AreEqual(null, result);
        }

        [TestMethod]
        public async Task SensorTarget_CanAbortAsync()
        {
            var result = await client.Targets.GetExeXmlFilesAsync(1001, f => false);

            Assert.AreEqual(null, result);
        }

        [TestMethod]
        [ExpectedException(typeof(PrtgRequestException))]
        public void SensorTarget_FailedRequest()
        {
            var faultyClient = Initialize_Client(new FaultySensorTargetResponse(FaultySensorTargetResponse.Scenario.Credentials));

            faultyClient.Targets.GetExeXmlFiles(1001);
        }

        private PrtgClient client => Initialize_Client(new ExeFileTargetResponse());
    }
}
