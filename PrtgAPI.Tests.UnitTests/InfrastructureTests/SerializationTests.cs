using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestItems;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses;

namespace PrtgAPI.Tests.UnitTests.InfrastructureTests
{
    [TestClass]
    public class SerializationTests
    {
        [TestMethod]
        public void Serializer_ThrowsInvalidEnum()
        {
            var webClient = new MockWebClient(new SensorResponse(new SensorItem(status: "banana", statusRaw: "8765")));

            var client = new PrtgClient("prtg.example.com", "username", "password", AuthMode.PassHash, webClient);

            try
            {
                client.GetSensors();
            }
            catch (Exception ex)
            {
                if (ex.Message != "Could not deserialize value '8765' as it is not a valid member of type 'PrtgAPI.Status'. Could not process XML '<status>banana</status><status_raw>8765</status_raw><message><div class=\"status\">OK<div class=\"moreicon\"></div></div></message>'")
                    throw;
            }
        }
    }
}
