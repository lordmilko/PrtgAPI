using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.IntegrationTests.ObjectManipulation
{
    [TestClass]
    public class SetObjectPropertyTests : BasePrtgClientTest
    {
        [TestMethod]
        [IntegrationTest]
        public void Action_SetObjectProperty_ResolvesLocation()
        {
            var initial = client.GetDeviceProperties(Settings.Device);

            client.SetObjectProperty(Settings.Device, ObjectProperty.Location, "23 Fleet Street, Boston");

            var newSettings = client.GetDeviceProperties(Settings.Device);

            AssertEx.AreNotEqual(initial.Location, newSettings.Location, "Initial and new location were the same");

            AssertEx.AreEqual("23 Fleet St, Boston, MA 02113, United States", newSettings.Location, "Location was not set properly");

            client.SetObjectProperty(Settings.Device, ObjectProperty.Location, null);
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Action_SetObjectProperty_ResolvesLocationAsync()
        {
            var initial = await client.GetDevicePropertiesAsync(Settings.Device);

            await client.SetObjectPropertyAsync(Settings.Device, ObjectProperty.Location, "23 Fleet Street, Boston");

            var newSettings = await client.GetDevicePropertiesAsync(Settings.Device);

            AssertEx.AreNotEqual(initial.Location, newSettings.Location, "Initial and new location were the same");

            AssertEx.AreEqual("23 Fleet St, Boston, MA 02113, United States", newSettings.Location, "Location was not set properly");

            await client.SetObjectPropertyAsync(Settings.Device, ObjectProperty.Location, null);
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_SetObjectProperty_RetrievesInheritedInterval()
        {
            var device = client.GetDevice(Settings.Device);
            var interval = client.GetObjectProperty<ScanningInterval>(device, ObjectProperty.Interval);

            Assert.AreEqual(true, device.InheritInterval);
            Assert.AreEqual(60, interval.TimeSpan.TotalSeconds);

            var sensor = client.GetSensor(Settings.PausedSensor);
            var sensorInterval = client.GetObjectProperty<ScanningInterval>(sensor, ObjectProperty.Interval);

            Assert.AreEqual(600, sensor.Interval.TotalSeconds);
            Assert.AreEqual(600, sensorInterval.TimeSpan.TotalSeconds);
            Assert.AreEqual(false, sensor.InheritInterval);

            try
            {
                client.SetObjectProperty(sensor, ObjectProperty.InheritInterval, true);

                var newSensor = client.GetSensor(Settings.PausedSensor);

                Assert.AreEqual(true, newSensor.InheritInterval);
                Assert.AreEqual(60, newSensor.Interval.TotalSeconds);

                var newInterval = client.GetObjectProperty<ScanningInterval>(sensor, ObjectProperty.Interval);
                Assert.AreEqual(600, newInterval.TimeSpan.TotalSeconds);
            }
            finally
            {
                var finalSensor = client.GetSensor(Settings.PausedSensor);

                if (finalSensor.InheritInterval)
                    client.SetObjectProperty(sensor, ObjectProperty.InheritInterval, false);
            }
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_SetObjectProperty_PrimaryChannel()
        {
            var sensor = client.GetSensor(Settings.WarningSensor);

            var newChannel = client.GetChannel(sensor, "Processor 1");
            var initialChannel = client.GetSensorProperties(sensor).PrimaryChannel;
            Assert.IsNotNull(initialChannel, "Initial channel should not have been null");
            Assert.AreNotEqual(initialChannel.Name, newChannel.Name, "Initial channel name matched");
            Assert.AreNotEqual(initialChannel.Id, newChannel.Id, "Initial channel ID matched");

            try
            {
                client.SetObjectProperty(sensor, ObjectProperty.PrimaryChannel, newChannel);
                var updatedChannel = client.GetSensorProperties(sensor).PrimaryChannel;

                Assert.AreEqual(newChannel.Name, updatedChannel.Name, "Updated channel name is incorrecet");
                Assert.AreEqual(newChannel.Id, updatedChannel.Id, "Updated channel ID is incorrect");
            }
            finally
            {
                client.SetObjectProperty(sensor, ObjectProperty.PrimaryChannel, initialChannel);
            }
        }
    }
}
