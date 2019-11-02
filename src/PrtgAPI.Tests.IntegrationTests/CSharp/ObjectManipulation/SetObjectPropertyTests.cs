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
    }
}
