using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.IntegrationTests.ObjectData
{
    [TestClass]
    public class NotificationActionTests : BasePrtgClientTest
    {
        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_NotificationAction_ReadOnlyUser()
        {
            var action = readOnlyClient.GetNotificationAction(Settings.NotificationAction);

            AssertEx.AllPropertiesRetrieveValues(action);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public async Task Data_NotificationAction_ReadOnlyUserAsync()
        {
            var action = await readOnlyClient.GetNotificationActionAsync(Settings.NotificationAction);

            AssertEx.AllPropertiesRetrieveValues(action);
        }
    }
}
