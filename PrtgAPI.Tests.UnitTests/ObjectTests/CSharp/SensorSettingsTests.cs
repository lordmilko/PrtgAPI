using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.CSharp
{
    [TestClass]
    public class SensorSettingsTests : BaseTest
    {
        [TestMethod]
        public void SensorSettings_AccessesProperties()
        {
            var client = Initialize_Client(new SensorSettingsResponse());

            var settings = client.GetSensorProperties(1001);

            foreach (var prop in settings.GetType().GetProperties())
            {
                var val = prop.GetValue(settings);
            }
        }
    }
}
