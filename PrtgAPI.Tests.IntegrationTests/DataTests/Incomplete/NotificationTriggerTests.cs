using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.IntegrationTests
{
    [TestClass]
    public class NotificationTriggerTests : BasePrtgClientTest
    {
        [TestMethod]
        public void Data_NotificationTrigger_GetNotificationTriggers_HasExpectedCount()
        {
            var triggers = client.GetNotificationTriggers(Settings.Device);

            Assert2.AreEqual(Settings.NotificationTiggersOnDevice, triggers.Count(t => !t.Inherited), nameof(Settings.NotificationTiggersOnDevice));
        }

        

        private void AssertEquals(string fieldName, object field1, object field2)
        {
            Assert2.IsTrue(field1.ToString() == field2.ToString(), $"{fieldName} was '{field1}' instead of '{field2}'");
        }
    }
}
