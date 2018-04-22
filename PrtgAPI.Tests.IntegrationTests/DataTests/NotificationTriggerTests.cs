using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Parameters;

namespace PrtgAPI.Tests.IntegrationTests.DataTests
{
    [TestClass]
    public class NotificationTriggerTests : BasePrtgClientTest
    {
        [TestMethod]
        public void Data_NotificationTrigger_GetNotificationTriggers_HasExpectedCount()
        {
            var triggers = client.GetNotificationTriggers(Settings.Device);

            AssertEx.AreEqual(Settings.NotificationTiggersOnDevice, triggers.Count(t => !t.Inherited), nameof(Settings.NotificationTiggersOnDevice));
        }

        [TestMethod]
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

        private void AddInvalidTrigger()
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;

            var htmlFunction = client.GetType().Assembly.GetType("PrtgAPI.HtmlFunction");
            var editSettings = htmlFunction.GetField("EditSettings").GetValue(null);

            var requestEngine = client.GetType().GetField("requestEngine", bindingFlags).GetValue(client);

            var args = new[] { htmlFunction, typeof(Parameters.Parameters), typeof(Func<HttpResponseMessage, string>) };

            var method = requestEngine.GetType().GetMethod("ExecuteRequest", bindingFlags, null, args, null);

            var channel = client.GetChannels(Settings.ChannelSensor).First(c => c.Id == Settings.Channel);

            var param = new ThresholdTriggerParameters(Settings.UpSensor)
            {
                Channel = channel
            };

            method.Invoke(requestEngine, new[] { editSettings, param, null });
        }
    }
}
