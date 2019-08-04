using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Parameters;
using PrtgAPI.Tests.UnitTests.Support;

namespace PrtgAPI.Tests.IntegrationTests.ObjectData
{
    [TestClass]
    public class NotificationTriggerTests : BasePrtgClientTest
    {
        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_NotificationTrigger_GetNotificationTriggers_HasExpectedCount()
        {
            var triggers = client.GetNotificationTriggers(Settings.Device);

            AssertEx.AreEqual(Settings.NotificationTiggersOnDevice, triggers.Count(t => !t.Inherited), nameof(Settings.NotificationTiggersOnDevice));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_NotificationTrigger_GetNotificationTriggers_Sensor_ThrowsWithInvalidChannel()
        {
            try
            {
                try
                {
                    client.GetNotificationTriggers(Settings.UpSensor);
                }
                catch (Exception ex)
                {
                    AssertEx.Fail($"Failed to retrieve initial triggers of sensor: {ex.Message}");
                }

                AddInvalidTrigger();

                AssertEx.Throws<InvalidStateException>(
                    () => client.GetNotificationTriggers(Settings.UpSensor),
                    "Object may be in a corrupted state"
                );
            }
            finally
            {
                ServerManager.RepairConfig();
                ServerManager.WaitForObjects();
            }
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public async Task Data_NotificationTrigger_GetNotificationTriggers_Sensor_ThrowsWithInvalidChannelAsync()
        {
            try
            {
                try
                {
                    await client.GetNotificationTriggersAsync(Settings.UpSensor);
                }
                catch (Exception ex)
                {
                    AssertEx.Fail($"Failed to retrieve initial triggers of sensor: {ex.Message}");
                }

                AddInvalidTrigger();

                await AssertEx.ThrowsAsync<InvalidStateException>(
                    async () => await client.GetNotificationTriggersAsync(Settings.UpSensor),
                    "Object may be in a corrupted state"
                );
            }
            finally
            {
                ServerManager.RepairConfig();
                ServerManager.WaitForObjects();
            }
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Data_NotificationTrigger_ReadOnlyUser()
        {
            var triggers = readOnlyClient.GetNotificationTriggers(Settings.Device);

            foreach (var trigger in triggers)
            {
                AssertEx.AllPropertiesRetrieveValues(trigger);
                Assert.IsNotNull(trigger.OnNotificationAction);

                AssertEx.AllPropertiesRetrieveValues(trigger.OnNotificationAction);
                Assert.IsNull(trigger.OnNotificationAction.Schedule);
            }
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public async Task Data_NotificationTrigger_ReadOnlyUserAsync()
        {
            var triggers = await readOnlyClient.GetNotificationTriggersAsync(Settings.Device);

            foreach (var trigger in triggers)
            {
                AssertEx.AllPropertiesRetrieveValues(trigger);
                Assert.IsNotNull(trigger.OnNotificationAction);

                AssertEx.AllPropertiesRetrieveValues(trigger.OnNotificationAction);
                Assert.IsNull(trigger.OnNotificationAction.Schedule);
            }
        }

        private void AddInvalidTrigger()
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
            var engine = client.GetInternalProperty("RequestEngine");
            var methods = engine.GetType().GetMethods(bindingFlags).Where(m => m.Name == "ExecuteRequest").ToList();
            var method = methods.First(m => m.GetParameters().Any(p => p.ParameterType.Name == "IHtmlParameters"));

            var channel = client.GetChannels(Settings.ChannelSensor).First(c => c.Id == Settings.Channel);

            var param = new ThresholdTriggerParameters(Settings.UpSensor)
            {
                Channel = channel
            };

            method.Invoke(engine, new object[] { param, null, CancellationToken.None });
        }
    }
}
