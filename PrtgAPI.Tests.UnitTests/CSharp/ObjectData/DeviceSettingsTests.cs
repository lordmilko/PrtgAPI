using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectData
{
    [TestClass]
    public class DeviceSettingsTests : BaseTest
    {
        [TestMethod]
        [TestCategory("UnitTest")]
        public void DeviceSettings_CanDeserialize()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            var settings = client.GetDeviceProperties(2212);

            AssertEx.AllPropertiesAreNotDefault(settings);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task DeviceSettings_CanDeserializeAsync()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            var settings = await client.GetDevicePropertiesAsync(2212);

            AssertEx.AllPropertiesAreNotDefault(settings);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task DeviceSettings_RawAsync()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            var property = await client.GetObjectPropertyRawAsync(1001, "name_");

            Assert.AreEqual("testName", property, "Name was not correct");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void DeviceSettings_Coordinates()
        {
            var client = Initialize_Client(new MultiTypeResponse());
            var settings = client.GetDeviceProperties(1001);

            settings.coordinates = null;
            Assert.AreEqual(null, settings.Coordinates, "Coordinates were not null");

            settings.coordinates = "1,2,3";
            Assert.AreEqual(null, settings.Coordinates, "Coordinates were not null");

            settings.coordinates = "0,0";
            Assert.AreEqual(null, settings.Coordinates, "Coordinates were not null");

            settings.coordinates = "-1,-2";
            Assert.AreEqual(-1, settings.Coordinates.Longitude);
            Assert.AreEqual(-2, settings.Coordinates.Latitude);

            Assert.AreEqual("-2,-1", settings.Coordinates.ToString());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void DeviceSettings_ReadOnly()
        {
            var client = Initialize_ReadOnlyClient(new MultiTypeResponse());

            AssertEx.Throws<InvalidOperationException>(() => client.GetDeviceProperties(1001), "Cannot retrieve properties for read-only device with ID 1001.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task DeviceSettings_ReadOnlyAsync()
        {
            var client = Initialize_ReadOnlyClient(new MultiTypeResponse());

            await AssertEx.ThrowsAsync<InvalidOperationException>(async () => await client.GetDevicePropertiesAsync(1001), "Cannot retrieve properties for read-only device with ID 1001.");
        }
    }
}
