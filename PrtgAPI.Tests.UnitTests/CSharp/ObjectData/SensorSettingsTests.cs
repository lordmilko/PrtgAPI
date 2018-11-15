using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectData
{
    [TestClass]
    public class SensorSettingsTests : BaseTest
    {
        [TestMethod]
        public void SensorSettings_AccessesProperties()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            var settings = client.GetSensorProperties(1001);

            foreach (var prop in settings.GetType().GetProperties())
            {
                var val = prop.GetValue(settings);
            }
        }

        [TestMethod]
        public async Task SensorSettings_AccessesPropertiesAsync()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            var settings = await client.GetSensorPropertiesAsync(1001);

            foreach (var prop in settings.GetType().GetProperties())
            {
                var val = prop.GetValue(settings);
            }
        }

        [TestMethod]
        public void SensorSettings_LoadsSchedule_Lazy_AllPropertiesAreSet()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            var settings = client.GetSensorProperties(1001);

            var schedule = settings.Schedule;

            AssertEx.AllPropertiesAreNotDefault(schedule, p =>
            {
                if (p.Name == "Tags")
                    return true;

                return false;
            });
        }

        [TestMethod]
        public async Task SensorSettings_LoadsSchedule_Lazy_AllPropertiesAreSetAsync()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            var settings = await client.GetSensorPropertiesAsync(1001);

            var schedule = settings.Schedule;

            AssertEx.AllPropertiesAreNotDefault(schedule, p =>
            {
                if (p.Name == "Tags")
                    return true;

                return false;
            });
        }

        [TestMethod]
        public void SensorSettings_ReadOnly()
        {
            var client = Initialize_ReadOnlyClient(new MultiTypeResponse());

            AssertEx.Throws<InvalidOperationException>(() => client.GetSensorProperties(1001), "Cannot retrieve properties for read-only sensor with ID 1001.");
        }

        [TestMethod]
        public async Task SensorSettings_ReadOnlyAsync()
        {
            var client = Initialize_ReadOnlyClient(new MultiTypeResponse());

            await AssertEx.ThrowsAsync<InvalidOperationException>(async () => await client.GetSensorPropertiesAsync(1001), "Cannot retrieve properties for read-only sensor with ID 1001.");
        }
    }
}
