using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.IntegrationTests.ObjectData
{
    [TestClass]
    public class ProbeTests : BasePrtgClientTest
    {
        [TestMethod]
        [IntegrationTest]
        public void Data_Probe_GetProbes_HasAnyResults()
        {
            HasAnyResults(client.GetProbes);
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Probe_GetProbes_ReturnsJustProbes()
        {
            ReturnsJustObjectsOfType(client.GetProbes, 0, Settings.ProbesInTestServer, BaseType.Probe);
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Probe_ReadOnlyUser()
        {
            var probe = readOnlyClient.GetProbe(Settings.Probe);

            AssertEx.AllPropertiesRetrieveValues(probe);
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Data_Probe_ReadOnlyUserAsync()
        {
            var probe = await readOnlyClient.GetProbeAsync(Settings.Probe);

            AssertEx.AllPropertiesRetrieveValues(probe);
        }
    }
}
