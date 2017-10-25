using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestItems;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectTests
{
    [TestClass]
    public class NotificationTriggerTests : NotificationTriggerBaseTests
    {
        [TestMethod]
        public void NotificationTrigger_CanDeserialize() => Object_CanDeserialize_Multiple();

        [TestMethod]
        public async Task NotificationTrigger_CanDeserializeAsync() => await Object_CanDeserializeAsync_Multiple();

        [TestMethod]
        public void NotificationTrigger_AllFields_HaveValues()
        {
            var objs = GetMultipleItems();

            foreach (var obj in objs)
            {
                foreach (var prop in obj.GetType().GetProperties())
                {
                    var val = prop.GetValue(obj);

                    switch (obj.Type)
                    {
                        case TriggerType.Change:
                            ChangeTrigger_AllFields_HaveValues(prop.Name, val);
                            break;
                        case TriggerType.State:
                            StateTrigger_AllFields_HaveValues(prop.Name, val);
                            break;
                        case TriggerType.Threshold:
                            ThresholdTrigger_AllFields_HaveValues(prop.Name, val);
                            break;
                        case TriggerType.Speed:
                            SpeedTrigger_AllFields_HaveValues(prop.Name, val);
                            break;
                        case TriggerType.Volume:
                            VolumeTrigger_AllFields_HaveValues(prop.Name, val);
                            break;
                        default:
                            throw new NotImplementedException($"TriggerType '{obj.Type}' does not have a property validator.");
                    }
                }
            }
        }

        [TestMethod]
        public void NotificationTrigger_ResolvesASensorChannel()
        {
            var client = GetResolvesASensorChannelResponseClient();

            var triggers = client.GetNotificationTriggers(1001).First();

            Assert.AreEqual(triggers.Channel.channel.GetType(), typeof (Channel));
        }

        [TestMethod]
        public async Task NotificationTrigger_ResolvesASensorChannelAsync()
        {
            var client = GetResolvesASensorChannelResponseClient();

            var triggers = (await client.GetNotificationTriggersAsync(1001)).First();

            Assert.AreEqual(triggers.Channel.channel.GetType(), typeof(Channel));
        }

        private PrtgClient GetResolvesASensorChannelResponseClient()
        {
            var notificationItem = NotificationTriggerItem.ThresholdTrigger(channel: "Backup State");
            var channelItem = new ChannelItem(name: "Backup State");

            var client = Initialize_Client(new NotificationTriggerResponse(new[] { notificationItem }, new[] { channelItem }));

            return client;
        }

        [TestMethod]
        public void NotificationTrigger_Throws_WhenChannelCantBeResolved()
        {
            var client = Initialize_Client(new NotificationTriggerResponse(NotificationTriggerItem.ThresholdTrigger(channel: "Backup State")));

            try
            {
                var triggers = client.GetNotificationTriggers(1001).First();
                Assert.Fail("Expected an exception to be raised, however no error occurred");
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("Object may be in a corrupted state"))
                    throw;
            }
        }

        private void ChangeTrigger_AllFields_HaveValues(string propertyName, object val)
        {
            switch (propertyName)
            {
                case nameof(NotificationTrigger.Latency):
                case nameof(NotificationTrigger.Channel):
                case nameof(NotificationTrigger.Unit):
                case nameof(NotificationTrigger.OffNotificationAction):
                case nameof(NotificationTrigger.EscalationLatency):
                case nameof(NotificationTrigger.EscalationNotificationAction):
                case nameof(NotificationTrigger.RepeatInterval):
                case nameof(NotificationTrigger.Threshold):
                    break;
                default:
                    Assert.IsTrue(val != null, $"Property '{propertyName}' of trigger type '{TriggerType.Change}' had value did not have a value."); //is threshold null or an empty string?
                    break;
            }
        }

        private void StateTrigger_AllFields_HaveValues(string propertyName, object val)
        {
            switch (propertyName)
            {
                case nameof(NotificationTrigger.Channel):
                case nameof(NotificationTrigger.Unit):
                    break;
                default:
                    Assert.IsTrue(val != null, $"Property '{propertyName}' of trigger type '{TriggerType.State}' had value did not have a value.");
                    break;
            }
        }

        private void ThresholdTrigger_AllFields_HaveValues(string propertyName, object val)
        {
            switch (propertyName)
            {
                case nameof(NotificationTrigger.Unit):
                case nameof(NotificationTrigger.EscalationLatency):
                case nameof(NotificationTrigger.EscalationNotificationAction):
                case nameof(NotificationTrigger.RepeatInterval):
                    break;
                default:
                    Assert.IsTrue(val != null, $"Property '{propertyName}' of trigger type '{TriggerType.Threshold}' had value did not have a value.");
                    break;
            }
        }

        private void SpeedTrigger_AllFields_HaveValues(string propertyName, object val)
        {
            switch (propertyName)
            {
                case nameof(NotificationTrigger.EscalationLatency):
                case nameof(NotificationTrigger.EscalationNotificationAction):
                case nameof(NotificationTrigger.RepeatInterval):
                    break;
                default:
                    Assert.IsTrue(val != null, $"Property '{propertyName}' of trigger type '{TriggerType.Speed}' had value did not have a value.");
                    break;
            }
        }

        private void VolumeTrigger_AllFields_HaveValues(string propertyName, object val)
        {
            switch (propertyName)
            {
                case nameof(NotificationTrigger.Latency):
                case nameof(NotificationTrigger.OffNotificationAction):
                case nameof(NotificationTrigger.EscalationLatency):
                case nameof(NotificationTrigger.EscalationNotificationAction):
                case nameof(NotificationTrigger.RepeatInterval):
                    break;
                default:
                    Assert.IsTrue(val != null, $"Property '{propertyName}' of trigger type '{TriggerType.Volume}' had value did not have a value.");
                    break;
            }
        }
    }
}
