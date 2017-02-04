using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Parameters;

namespace PrtgAPI.Tests.IntegrationTests
{
    [TestClass]
    public class NotificationTriggerTests : BasePrtgClientTest
    {
        [TestMethod]
        public void Data_NotificationTrigger_GetNotificationTriggers_HasExpectedCount()
        {
            var triggers = client.GetNotificationTriggers(Settings.Device);

            Assert.AreEqual(Settings.NotificationTiggersOnDevice, triggers.Count(t => !t.Inherited), nameof(Settings.NotificationTiggersOnDevice));
        } 

        private void AssertEquals(string fieldName, object field1, object field2)
        {
            Assert.IsTrue(field1.ToString() == field2.ToString(), $"{fieldName} was '{field1}' instead of '{field2}'");
        }
    }
}
