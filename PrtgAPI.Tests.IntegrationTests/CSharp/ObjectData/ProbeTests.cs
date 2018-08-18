using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.IntegrationTests.DataTests
{
    [TestClass]
    public class ProbeTests : BasePrtgClientTest
    {
        [TestMethod]
        public void Data_Probe_GetProbes_HasAnyResults()
        {
            HasAnyResults(client.GetProbes);
        }

        [TestMethod]
        public void Data_Probe_GetProbes_ReturnsJustProbes()
        {
            ReturnsJustObjectsOfType(client.GetProbes, 0, Settings.ProbesInTestServer, BaseType.Probe);
        }
    }
}
