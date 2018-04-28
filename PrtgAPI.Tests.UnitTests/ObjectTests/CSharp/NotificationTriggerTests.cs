using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestItems;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectTests
{
    class EventValidator<T>
    {
        private bool ready { get; set; }
        private int i { get; set; } = -1;

        private int count;

        private T[] list;

        private object lockObj = new object();

        public EventValidator(T[] list)
        {
            this.list = list;
        }

        public bool Finished => i == list.Length - 1;

        public void MoveNext(int count = 1)
        {
            i++;
            this.count = count;
            ready = true;
        }

        public T Get(string next)
        {
            lock (lockObj)
            {
                count--;

                if (ready)
                {
                    Assert.IsTrue(i <= list.Length - 1, $"More requests were received than stored in list. Next record is: {next}");

                    var val = list[i];

                    if (count == 0)
                        ready = false;
                    else
                        i++;

                    return val;
                }

                throw new InvalidOperationException($"Was not ready for request {next}");
            }
        }
    }

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

            AssertEx.Throws<InvalidStateException>(
                () => client.GetNotificationTriggers(1001).First(),
                "Object may be in a corrupted state"
            );
        }

        [TestMethod]
        public async Task NotificationTrigger_Throws_WhenChannelCantBeResolvedAsync()
        {
            var client = Initialize_Client(new NotificationTriggerResponse(NotificationTriggerItem.ThresholdTrigger(channel: "Backup State")));

            await AssertEx.ThrowsAsync<InvalidStateException>(
                async () => (await client.GetNotificationTriggersAsync(1001)).First(),
                "Object may be in a corrupted state"
            );
        }

        [TestMethod]
        public void NotificationTrigger_CanRemove()
        {
            var triggerClient = Initialize_Client(new NotificationTriggerResponse(NotificationTriggerItem.StateTrigger()));
            var trigger = triggerClient.GetNotificationTriggers(0).First();

            var client = Initialize_Client(new AddressValidatorResponse("deletesub.htm?id=0&subid=1"));

            client.RemoveNotificationTrigger(trigger);
        }

        [TestMethod]
        public async Task NotificationTrigger_CanRemoveAsync()
        {
            var triggerClient = Initialize_Client(new NotificationTriggerResponse(NotificationTriggerItem.StateTrigger()));
            var trigger = (await triggerClient.GetNotificationTriggersAsync(0)).First();

            var client = Initialize_Client(new AddressValidatorResponse("deletesub.htm?id=0&subid=1"));

            await client.RemoveNotificationTriggerAsync(trigger);
        }

        [TestMethod]
        public async Task NotificationTriggerTypes_CanExecuteAsync()
        {
            var client = Initialize_Client(new SetNotificationTriggerResponse());

            await client.GetNotificationTriggerTypesAsync(1001);
        }

        [TestMethod]
        public void NotificationTrigger_LoadsAction_Lazy()
        {
            var client = Initialize_Client(new NotificationTriggerResponse(NotificationTriggerItem.StateTrigger(offNotificationAction: "302|Email to all members of group PRTG Administrator")));

            var validator = new EventValidator<string>(new[]
            {
                //First
                "https://prtg.example.com/api/table.xml?id=1001&content=triggers&columns=content,objid&username=username&passhash=12345678",

                //Second
                "https://prtg.example.com/api/table.xml?content=notifications&columns=baselink,type,tags,active,objid,name&count=*&filter_objid=301&filter_objid=302&username=username&passhash=12345678",
                "https://prtg.example.com/controls/editnotification.htm?id=301&username=username&passhash=12345678",

                //Third
                "https://prtg.example.com/controls/editnotification.htm?id=302&username=username&passhash=12345678"
            });

            client.LogVerbose += (s, e) =>
            {
                var message = Regex.Replace(e.Message, "(.+ request )(.+)", "$2");

                Assert.AreEqual(validator.Get(message), message);
            };

            validator.MoveNext();
            var triggers = client.GetNotificationTriggers(1001);

            validator.MoveNext(2);
            var val = triggers.First().OnNotificationAction.Postpone;

            validator.MoveNext();
            var val2 = triggers.First().OffNotificationAction.Postpone;

            Assert.IsTrue(validator.Finished, "Did not process all requests");
        }

        [TestMethod]
        public async Task NotificationTrigger_LoadsAction_Efficiently()
        {
            var client = Initialize_Client(new NotificationTriggerResponse(NotificationTriggerItem.StateTrigger(offNotificationAction: "302|Email to all members of group PRTG Administrator")));

            var validator = new EventValidator<string>(new[]
            {
                //First
                "https://prtg.example.com/api/table.xml?id=1001&content=triggers&columns=content,objid&username=username&passhash=12345678",
                "https://prtg.example.com/api/table.xml?content=notifications&columns=baselink,type,tags,active,objid,name&count=*&filter_objid=301&filter_objid=302&username=username&passhash=12345678",
                "https://prtg.example.com/controls/editnotification.htm?id=301&username=username&passhash=12345678",
                "https://prtg.example.com/controls/editnotification.htm?id=302&username=username&passhash=12345678"
            });

            client.LogVerbose += (s, e) =>
            {
                var message = Regex.Replace(e.Message, "(.+ request )(.+)", "$2");

                Assert.AreEqual(validator.Get(message), message);
            };

            validator.MoveNext(4);
            var triggers = await client.GetNotificationTriggersAsync(1001);

            var val = triggers.First().OnNotificationAction.Postpone;

            Assert.IsTrue(validator.Finished, "Did not process all requests");
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
