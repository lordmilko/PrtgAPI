using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectTests
{
    [TestClass]
    public class DeviceSettingsTests : BaseTest
    {
        [TestMethod]
        public void DeviceSettings_CanDeserialize()
        {
            var client = Initialize_Client(new DeviceSettingsResponse());

            var settings = client.GetDeviceProperties(2212);

            Assert2.AllPropertiesAreNotDefault(settings);
        }

        [TestMethod]
        public async Task DeviceSettings_CanDeserializeAsync()
        {
            var client = Initialize_Client(new DeviceSettingsResponse());

            var settings = await client.GetDevicePropertiesAsync(2212);

            Assert2.AllPropertiesAreNotDefault(settings);
        }

        [TestMethod]
        public async Task DeviceSettings_RawAsync()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            var property = await client.GetObjectPropertyRawAsync(1001, "name_");

            Assert.AreEqual("testName", property, "Name was not correct");
        }
    }
}
