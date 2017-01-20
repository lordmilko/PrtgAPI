using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;
using PrtgAPI.Tests.UnitTests.ObjectTests.Responses;

namespace PrtgAPI.Tests.UnitTests.ObjectTests
{
    [TestClass]
    public class SensorTotalsTests
    {
        [TestMethod]
        public void SensorTotals_AllFields_HaveValues()
        {
            var webClient = new MockWebClient(new SensorTotalsResponse(new Items.SensorTotalsItem()));
            var client = new PrtgClient("prtg.example.com", "username", "12345678", AuthMode.PassHash, webClient);
            var result = client.GetSensorTotals();

            Assert2.AllPropertiesAreNotDefault(result);
        }
    }
}
