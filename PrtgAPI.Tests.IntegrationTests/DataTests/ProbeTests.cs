using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.IntegrationTests
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
