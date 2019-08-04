using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support;
using PrtgAPI.Tests.UnitTests.Support.TestItems;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectData
{
    [TestClass]
    public class NotificationTriggerTests : NotificationTriggerBaseTests
    {
        [TestMethod]
        [TestCategory("UnitTest")]
        public void NotificationTrigger_CanDeserialize() => Object_CanDeserialize_Multiple();

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task NotificationTrigger_CanDeserializeAsync() => await Object_CanDeserializeAsync_Multiple();

        [TestMethod]
        [TestCategory("UnitTest")]
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
        [TestCategory("UnitTest")]
        public void NotificationTrigger_ResolvesASensorChannel()
        {
            var client = GetResolvesASensorChannelResponseClient();

            var triggers = client.GetNotificationTriggers(4001).First();

            Assert.AreEqual(triggers.Channel.channel.GetType(), typeof (Channel));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task NotificationTrigger_ResolvesASensorChannelAsync()
        {
            var client = GetResolvesASensorChannelResponseClient();

            var triggers = (await client.GetNotificationTriggersAsync(4001)).First();

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
        [TestCategory("UnitTest")]
        public void NotificationTrigger_Throws_WhenChannelCantBeResolved()
        {
            var client = Initialize_Client(new NotificationTriggerResponse(NotificationTriggerItem.ThresholdTrigger(channel: "Backup State")));

            AssertEx.Throws<InvalidStateException>(
                () => client.GetNotificationTriggers(1001).First(),
                "Object may be in a corrupted state"
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task NotificationTrigger_Throws_WhenChannelCantBeResolvedAsync()
        {
            var client = Initialize_Client(
                new NotificationTriggerResponse(
                    NotificationTriggerItem.ThresholdTrigger(channel: "Backup State")
                )
            );

            await AssertEx.ThrowsAsync<InvalidStateException>(
                async () => (await client.GetNotificationTriggersAsync(1001)).First(),
                "Object may be in a corrupted state"
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void NotificationTrigger_CanRemove()
        {
            var triggerClient = Initialize_Client(new NotificationTriggerResponse(NotificationTriggerItem.StateTrigger()));
            var trigger = triggerClient.GetNotificationTriggers(0).First();

            Execute(
                c => c.RemoveNotificationTrigger(trigger),
                "deletesub.htm?id=0&subid=1"
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task NotificationTrigger_CanRemoveAsync()
        {
            var triggerClient = Initialize_Client(new NotificationTriggerResponse(NotificationTriggerItem.StateTrigger()));
            var trigger = (await triggerClient.GetNotificationTriggersAsync(0)).First();

            await ExecuteAsync(
                async c => await c.RemoveNotificationTriggerAsync(trigger),
                "deletesub.htm?id=0&subid=1"
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task NotificationTriggerTypes_CanExecuteAsync()
        {
            var client = Initialize_Client(new SetNotificationTriggerResponse());

            await client.GetNotificationTriggerTypesAsync(1001);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void NotificationTrigger_LoadsAction_Lazy()
        {
            var client = Initialize_Client(new NotificationTriggerResponse(NotificationTriggerItem.StateTrigger(offNotificationAction: "302|Email to all members of group PRTG Administrator")));

            var validator = new EventValidator<string>(new[]
            {
                //First - get all triggers
                UnitRequest.Triggers(1001),

                //Second - touch a trigger's action's unsupported property
                UnitRequest.Notifications("filter_objid=301&filter_objid=302"),
                UnitRequest.NotificationProperties(301),

                //Third - touch an unsupported property of another action
                UnitRequest.NotificationProperties(302)
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
        [TestCategory("UnitTest")]
        public async Task NotificationTrigger_LoadsAction_Efficiently_WhenAsync()
        {
            var client = Initialize_Client(new NotificationTriggerResponse(
                NotificationTriggerItem.StateTrigger(offNotificationAction: "302|Email to all members of group PRTG Administrator")
            ) {HasSchedule = new[] {302}});

            var validator = new EventValidator<string>(new[]
            {
                //First
                UnitRequest.Triggers(1001),
                UnitRequest.Notifications("filter_objid=301&filter_objid=302"),
                UnitRequest.NotificationProperties(301),
                UnitRequest.NotificationProperties(302),
                UnitRequest.Schedules("filter_objid=623"),
                UnitRequest.ScheduleProperties(623)
            });

            client.LogVerbose += (s, e) =>
            {
                var message = Regex.Replace(e.Message, "(.+ request )(.+)", "$2");

                Assert.AreEqual(validator.Get(message), message);
            };

            validator.MoveNext(6);
            var triggers = await client.GetNotificationTriggersAsync(1001);

            var val = triggers.First().OnNotificationAction.Postpone;

            Assert.AreEqual("Weekdays [GMT+0800]", triggers.First().OffNotificationAction.Schedule.Name);

            Assert.IsTrue(validator.Finished, "Did not process all requests");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void NotificationTrigger_LoadsSchedule_Lazy()
        {
            var client = Initialize_Client(new NotificationTriggerResponse(
                NotificationTriggerItem.StateTrigger(offNotificationAction: "302|Email to all members of group PRTG Administrator"),
                NotificationTriggerItem.ChangeTrigger("303|Ticket Notification"))
            { HasSchedule = new[] {301, 302, 303}}
            );

            var validator = new EventValidator<string>(new[]
            {
                //First - get all triggers
                UnitRequest.Triggers(1001),

                //Second - touch a trigger's action's schedule
                UnitRequest.Notifications("filter_objid=301&filter_objid=302&filter_objid=303"),
                UnitRequest.NotificationProperties(301),
                UnitRequest.Schedules("filter_objid=623"),
                UnitRequest.ScheduleProperties(623),

                //Third - touch the same schedule on another action
                UnitRequest.NotificationProperties(302),
                UnitRequest.Schedules("filter_objid=623"),
                UnitRequest.ScheduleProperties(623),

                //Fourth - touch a different schedule on another action
                UnitRequest.NotificationProperties(303),
                UnitRequest.Schedules("filter_objid=623"),
                UnitRequest.ScheduleProperties(623),
            });

            client.LogVerbose += (s, e) =>
            {
                var message = Regex.Replace(e.Message, "(.+ request )(.+)", "$2");

                Assert.AreEqual(validator.Get(message), message);
            };

            validator.MoveNext();
            var triggers = client.GetNotificationTriggers(1001);

            validator.MoveNext(4);
            var val = triggers.First().OnNotificationAction.Schedule;

            validator.MoveNext(3);
            var val2 = triggers.First().OffNotificationAction.Schedule;

            var val3 = triggers.First().EscalationNotificationAction.Schedule; //should be action "None"
            Assert.AreEqual(null, val3);

            validator.MoveNext(3);
            var trueVal3 = triggers.Skip(1).First().OnNotificationAction.Schedule;

            var firstAgain = triggers.First().OnNotificationAction.Schedule;
            var secondAgain = triggers.First().OffNotificationAction.Schedule;
            var thirdFakeAgain = triggers.First().EscalationNotificationAction.Schedule;
            var thirdRealAgain = triggers.Skip(1).First().OnNotificationAction.Schedule;

            Assert.IsTrue(validator.Finished, "Did not process all requests");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task NotificationTrigger_LoadsSchedule_LazyAsync()
        {
            var client = Initialize_Client(new NotificationTriggerResponse(
                NotificationTriggerItem.StateTrigger(offNotificationAction: "302|Email to all members of group PRTG Administrator"),
                NotificationTriggerItem.ChangeTrigger("303|Ticket Notification"))
            { HasSchedule = new[] { 301, 302, 303 } }
            );

            var validator = new EventValidator<string>(new[]
            {
                //First - get all triggers. Automatically retrieves all notifications, and their properties.
                //All of the schedules of the notifications are retrieved, as well as each schedule's properties
                UnitRequest.Triggers(1001),

                UnitRequest.Notifications("filter_objid=301&filter_objid=302&filter_objid=303"),
                UnitRequest.NotificationProperties(301),
                UnitRequest.NotificationProperties(302),
                UnitRequest.NotificationProperties(303),

                UnitRequest.Schedules("filter_objid=623"),
                UnitRequest.ScheduleProperties(623),
            });

            client.LogVerbose += (s, e) =>
            {
                var message = Regex.Replace(e.Message, "(.+ request )(.+)", "$2");

                Assert.AreEqual(validator.Get(message), message);
            };

            validator.MoveNext(7);
            var triggers = await client.GetNotificationTriggersAsync(1001);

            var val = triggers.First().OnNotificationAction.Schedule;

            var val2 = triggers.First().OffNotificationAction.Schedule;

            var val3 = triggers.First().EscalationNotificationAction.Schedule; //should be action "None"
            Assert.AreEqual(null, val3);

            var trueVal3 = triggers.Skip(1).First().OnNotificationAction.Schedule;

            var firstAgain = triggers.First().OnNotificationAction.Schedule;
            var secondAgain = triggers.First().OffNotificationAction.Schedule;
            var thirdFakeAgain = triggers.First().EscalationNotificationAction.Schedule;
            var thirdRealAgain = triggers.Skip(1).First().OnNotificationAction.Schedule;

            Assert.IsTrue(validator.Finished, "Did not process all requests");

            Assert.IsTrue(validator.Finished, "Did not process all requests");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void NotificationTrigger_LoadsAction_Lazy_AllPropertiesAreSet()
        {
            var client = Initialize_Client(new NotificationTriggerResponse(NotificationTriggerItem.StateTrigger(offNotificationAction: "302|Email to all members of group PRTG Administrator")));

            var triggers = client.GetNotificationTriggers(1001);
            var val = triggers.First().OnNotificationAction;

            AssertEx.AllPropertiesAreNotDefault(val);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task NotificationTrigger_LoadsAction_Lazy_AllPropertiesAreSetAsync()
        {
            var client = Initialize_Client(new NotificationTriggerResponse(NotificationTriggerItem.StateTrigger(offNotificationAction: "302|Email to all members of group PRTG Administrator")));

            var triggers = await client.GetNotificationTriggersAsync(1001);
            var val = triggers.First().OnNotificationAction;

            AssertEx.AllPropertiesAreNotDefault(val);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void NotificationTrigger_LoadsSchedule_Lazy_AllPropertiesAreSet()
        {
            var client = Initialize_Client(new NotificationTriggerResponse(
                    NotificationTriggerItem.StateTrigger(offNotificationAction: "302|Email to all members of group PRTG Administrator"))
                { HasSchedule = new[] { 301 } }
            );

            var triggers = client.GetNotificationTriggers(1001);
            var action = triggers.First().OnNotificationAction;
            var schedule = action.Schedule;

            AssertEx.AllPropertiesAreNotDefault(schedule, p =>
            {
                if (p.Name == "Tags")
                    return true;

                return false;
            });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task NotificationTrigger_LoadsSchedule_Lazy_AllPropertiesAreSetAsync()
        {
            var client = Initialize_Client(new NotificationTriggerResponse(
                    NotificationTriggerItem.StateTrigger(offNotificationAction: "302|Email to all members of group PRTG Administrator"))
                { HasSchedule = new[] { 301 } }
            );

            var triggers = await client.GetNotificationTriggersAsync(1001);
            var action = triggers.First().OnNotificationAction;
            var schedule = action.Schedule;

            AssertEx.AllPropertiesAreNotDefault(schedule, p =>
            {
                if (p.Name == "Tags")
                    return true;

                return false;
            });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void NotificationTrigger_ReadOnlyUser()
        {
            var client = Initialize_ReadOnlyClient(new MultiTypeResponse());

            var triggers = client.GetNotificationTriggers(1001);

            foreach (var trigger in triggers)
            {
                AssertEx.AllPropertiesRetrieveValues(trigger);
                Assert.IsNotNull(trigger.OnNotificationAction);

                AssertEx.AllPropertiesRetrieveValues(trigger.OnNotificationAction);
                Assert.IsNull(trigger.OnNotificationAction.Schedule);
            }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task NotificationTrigger_ReadOnlyUserAsync()
        {
            var client = Initialize_ReadOnlyClient(new MultiTypeResponse());

            var triggers = await client.GetNotificationTriggersAsync(1001);

            foreach (var trigger in triggers)
            {
                AssertEx.AllPropertiesRetrieveValues(trigger);
                Assert.IsNotNull(trigger.OnNotificationAction);

                AssertEx.AllPropertiesRetrieveValues(trigger.OnNotificationAction);
                Assert.IsNull(trigger.OnNotificationAction.Schedule);
            }
        }

        private void ChangeTrigger_AllFields_HaveValues(string propertyName, object val)
        {
            switch (propertyName)
            {
                case nameof(NotificationTrigger.Latency):
                case nameof(NotificationTrigger.Channel):
                case nameof(NotificationTrigger.Unit):
                case nameof(NotificationTrigger.UnitSize):
                case nameof(NotificationTrigger.UnitTime):
                case nameof(NotificationTrigger.Period):
                case nameof(NotificationTrigger.OffNotificationAction):
                case nameof(NotificationTrigger.EscalationLatency):
                case nameof(NotificationTrigger.EscalationNotificationAction):
                case nameof(NotificationTrigger.RepeatInterval):
                case nameof(NotificationTrigger.State):
                case nameof(NotificationTrigger.Threshold):
                case nameof(NotificationTrigger.DisplayThreshold):
                    break;
                default:
                    Assert.IsTrue(val != null, $"Property '{propertyName}' of trigger type '{TriggerType.Change}' did not have a value."); //is threshold null or an empty string?
                    break;
            }
        }

        private void StateTrigger_AllFields_HaveValues(string propertyName, object val)
        {
            switch (propertyName)
            {
                case nameof(NotificationTrigger.Channel):
                case nameof(NotificationTrigger.Unit):
                case nameof(NotificationTrigger.UnitSize):
                case nameof(NotificationTrigger.UnitTime):
                case nameof(NotificationTrigger.Period):
                case nameof(NotificationTrigger.Threshold):
                    break;
                default:
                    Assert.IsTrue(val != null, $"Property '{propertyName}' of trigger type '{TriggerType.State}' did not have a value.");
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
                case nameof(NotificationTrigger.State):
                case nameof(NotificationTrigger.UnitSize):
                case nameof(NotificationTrigger.UnitTime):
                case nameof(NotificationTrigger.Period):
                    break;
                default:
                    Assert.IsTrue(val != null, $"Property '{propertyName}' of trigger type '{TriggerType.Threshold}' did not have a value.");
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
                case nameof(NotificationTrigger.State):
                case nameof(NotificationTrigger.Period):
                    break;
                default:
                    Assert.IsTrue(val != null, $"Property '{propertyName}' of trigger type '{TriggerType.Speed}' did not have a value.");
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
                case nameof(NotificationTrigger.State):
                case nameof(NotificationTrigger.UnitTime):
                    break;
                default:
                    Assert.IsTrue(val != null, $"Property '{propertyName}' of trigger type '{TriggerType.Volume}' did not have a value.");
                    break;
            }
        }
    }
}
