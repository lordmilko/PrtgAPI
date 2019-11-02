using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.IntegrationTests.ObjectData
{
    [TestClass]
    public class DeviceTests : BasePrtgClientTest
    {
        [TestMethod]
        [IntegrationTest]
        public void Data_Device_GetDevices_HasAnyResults()
        {
            HasAnyResults(client.GetDevices);
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Device_GetDevices_ReturnsJustDevices()
        {
            ReturnsJustObjectsOfType(client.GetDevices, Settings.Group, Settings.DevicesInTestGroup, BaseType.Device);
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Device_ReadOnlyUser()
        {
            var device = readOnlyClient.GetDevice(Settings.Device);

            AssertEx.AllPropertiesRetrieveValues(device);
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Data_Device_ReadOnlyUserAsync()
        {
            var device = await readOnlyClient.GetDeviceAsync(Settings.Device);

            AssertEx.AllPropertiesRetrieveValues(device);
        }
    }
}
