using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Reflection;
using PrtgAPI.Request;
using PrtgAPI.Tests.UnitTests.Support;
using PrtgAPI.Tests.UnitTests.Support.TestItems;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectData
{
    [TestClass]
#if MSTEST2
    [DoNotParallelize]
#endif
    public class NotificationTriggerTests : NotificationTriggerBaseTests
    {
        #region Normal

        [UnitTest]
        [TestMethod]
        public void NotificationTrigger_CanDeserialize() => Object_CanDeserialize_Multiple();

        [UnitTest]
        [TestMethod]
        public async Task NotificationTrigger_CanDeserializeAsync() => await Object_CanDeserializeAsync_Multiple();

        [UnitTest]
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

        [UnitTest]
        [TestMethod]
        public void NotificationTrigger_ResolvesASensorChannel()
        {
            var client = GetResolvesASensorChannelResponseClient();

            var triggers = client.GetNotificationTriggers(4001).First();

            Assert.AreEqual(triggers.Channel.channel.GetType(), typeof (Channel));
        }

        [UnitTest]
        [TestMethod]
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

        [UnitTest]
        [TestMethod]
        public void NotificationTrigger_Throws_WhenChannelCantBeResolved()
        {
            var client = Initialize_Client(new NotificationTriggerResponse(NotificationTriggerItem.ThresholdTrigger(channel: "Backup State")));

            AssertEx.Throws<InvalidStateException>(
                () => client.GetNotificationTriggers(1001).First(),
                "Object may be in a corrupted state"
            );
        }

        [UnitTest]
        [TestMethod]
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

        [UnitTest]
        [TestMethod]
        public void NotificationTrigger_CanRemove()
        {
            var triggerClient = Initialize_Client(new NotificationTriggerResponse(NotificationTriggerItem.StateTrigger()));
            var trigger = triggerClient.GetNotificationTriggers(0).First();

            Execute(
                c => c.RemoveNotificationTrigger(trigger),
                "deletesub.htm?id=0&subid=1"
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task NotificationTrigger_CanRemoveAsync()
        {
            var triggerClient = Initialize_Client(new NotificationTriggerResponse(NotificationTriggerItem.StateTrigger()));
            var trigger = (await triggerClient.GetNotificationTriggersAsync(0)).First();

            await ExecuteAsync(
                async c => await c.RemoveNotificationTriggerAsync(trigger),
                "deletesub.htm?id=0&subid=1"
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task NotificationTriggerTypes_CanExecuteAsync()
        {
            var client = Initialize_Client(new SetNotificationTriggerResponse());

            await client.GetNotificationTriggerTypesAsync(1001);
        }

        [UnitTest]
        [TestMethod]
        public void NotificationTrigger_LoadsAction_Lazy()
        {
            var client = Initialize_Client(new NotificationTriggerResponse(NotificationTriggerItem.StateTrigger(offNotificationAction: "302|Email to all members of group PRTG Administrator")));

            var validator = new EventValidator(client, new[]
            {
                //First - get all triggers
                UnitRequest.Triggers(1001),
                UnitRequest.RequestObjectData(810),

                //Second - touch a trigger's action's unsupported property
                UnitRequest.Notifications("filter_objid=301&filter_objid=302"),
                UnitRequest.NotificationProperties(301),

                //Third - touch an unsupported property of another action
                UnitRequest.NotificationProperties(302)
            });

            validator.MoveNext(2);
            var triggers = client.GetNotificationTriggers(1001);

            validator.MoveNext(2);
            var val = triggers.First().OnNotificationAction.Postpone;

            validator.MoveNext();
            var val2 = triggers.First().OffNotificationAction.Postpone;

            Assert.IsTrue(validator.Finished, "Did not process all requests");
        }

        [UnitTest]
        [TestMethod]
        public async Task NotificationTrigger_LoadsAction_Efficiently_WhenAsync()
        {
            var client = Initialize_Client(new NotificationTriggerResponse(
                NotificationTriggerItem.StateTrigger(offNotificationAction: "302|Email to all members of group PRTG Administrator")
            ) {HasSchedule = new[] {302}});

            var validator = new EventValidator(client, new[]
            {
                //First
                UnitRequest.Triggers(1001),
                UnitRequest.RequestObjectData(810),
                UnitRequest.Notifications("filter_objid=301&filter_objid=302"),
                UnitRequest.NotificationProperties(301),
                UnitRequest.NotificationProperties(302),
                UnitRequest.Schedules("filter_objid=623"),
                UnitRequest.ScheduleProperties(623)
            });

            validator.MoveNext(7);
            var triggers = await client.GetNotificationTriggersAsync(1001);

            var val = triggers.First().OnNotificationAction.Postpone;

            Assert.AreEqual("Weekdays [GMT+0800]", triggers.First().OffNotificationAction.Schedule.Name);

            Assert.IsTrue(validator.Finished, "Did not process all requests");
        }

        [UnitTest]
        [TestMethod]
        public void NotificationTrigger_LoadsAction_Properties_Lazy()
        {
            var ignore = new[]
            {
                "Id",
                "Name"
            };

            var properties = typeof(NotificationAction).GetProperties()
                .Where(p => !ignore.Contains(p.Name) && p.GetCustomAttributes<XmlElementAttribute>().Any());

            foreach (var property in properties)
            {
                var client = Initialize_Client(new NotificationTriggerResponse(
                    NotificationTriggerItem.StateTrigger(offNotificationAction: "302|Email to all members of group PRTG Administrator")
                ) {HasSchedule = new[] {302}});

                var validator = new EventValidator(client, new[]
                {
                    //First - get all triggers
                    UnitRequest.Triggers(1001),
                    UnitRequest.RequestObjectData(810),

                    //Second - touch a lazy propert of a Notification Action
                    UnitRequest.Notifications("filter_objid=301&filter_objid=302"),
                    UnitRequest.NotificationProperties(302)
                });

                try
                {
                    validator.MoveNext(2);
                    var triggers = client.GetNotificationTriggers(1001);

                    validator.MoveNext(2);
                    var action = triggers.First().OffNotificationAction;
                    var value = property.GetValue(action);

                    Assert.IsTrue(validator.Finished, "Did not process all requests");
                }
                catch (Exception ex)
                {
                    if (ex is TargetInvocationException)
                        ex = ex.InnerException;

                    throw new AssertFailedException($"Encountered an error while processing property '{property.Name}': {ex.Message}", ex);
                }
            }
        }

        [UnitTest]
        [TestMethod]
        public void NotificationTrigger_LoadsSchedule_Lazy()
        {
            var client = Initialize_Client(new NotificationTriggerResponse(
                NotificationTriggerItem.StateTrigger(offNotificationAction: "302|Email to all members of group PRTG Administrator"),
                NotificationTriggerItem.ChangeTrigger("303|Ticket Notification"))
            { HasSchedule = new[] {301, 302, 303}}
            );

            var validator = new EventValidator(client, new[]
            {
                //First - get all triggers
                UnitRequest.Triggers(1001),
                UnitRequest.RequestObjectData(810),

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

            validator.MoveNext(2);
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

        [UnitTest]
        [TestMethod]
        public async Task NotificationTrigger_LoadsSchedule_LazyAsync()
        {
            var client = Initialize_Client(new NotificationTriggerResponse(
                NotificationTriggerItem.StateTrigger(offNotificationAction: "302|Email to all members of group PRTG Administrator"),
                NotificationTriggerItem.ChangeTrigger("303|Ticket Notification"))
            { HasSchedule = new[] { 301, 302, 303 } }
            );

            var validator = new EventValidator(client, new[]
            {
                //First - get all triggers. Automatically retrieves all notifications, and their properties.
                //All of the schedules of the notifications are retrieved, as well as each schedule's properties
                UnitRequest.Triggers(1001),
                UnitRequest.RequestObjectData(810),

                UnitRequest.Notifications("filter_objid=301&filter_objid=302&filter_objid=303"),
                UnitRequest.NotificationProperties(301),
                UnitRequest.NotificationProperties(302),
                UnitRequest.NotificationProperties(303),

                UnitRequest.Schedules("filter_objid=623"),
                UnitRequest.ScheduleProperties(623),
            });

            validator.MoveNext(8);
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

        [UnitTest]
        [TestMethod]
        public void NotificationTrigger_LoadsAction_Lazy_AllPropertiesAreSet()
        {
            var client = Initialize_Client(new NotificationTriggerResponse(NotificationTriggerItem.StateTrigger(offNotificationAction: "302|Email to all members of group PRTG Administrator")));

            var triggers = client.GetNotificationTriggers(1001);
            var val = triggers.First().OnNotificationAction;

            AssertEx.AllPropertiesAreNotDefault(val);
        }

        [UnitTest]
        [TestMethod]
        public async Task NotificationTrigger_LoadsAction_Lazy_AllPropertiesAreSetAsync()
        {
            var client = Initialize_Client(new NotificationTriggerResponse(NotificationTriggerItem.StateTrigger(offNotificationAction: "302|Email to all members of group PRTG Administrator")));

            var triggers = await client.GetNotificationTriggersAsync(1001);
            var val = triggers.First().OnNotificationAction;

            AssertEx.AllPropertiesAreNotDefault(val);
        }

        [UnitTest]
        [TestMethod]
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

        [UnitTest]
        [TestMethod]
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

        [UnitTest]
        [TestMethod]
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

        [UnitTest]
        [TestMethod]
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

        [UnitTest]
        [TestMethod]
        public void NotificationTrigger_DoubleEscapeQuotes()
        {
            var client = Initialize_Client(new NotificationTriggerResponse(NotificationTriggerItem.StateTrigger(sensorName: "Test \"Name\"")));

            var triggers = client.GetNotificationTriggers(1001);
        }

        [UnitTest]
        [TestMethod]
        public void NotificationTrigger_Parses_DoubleThreshold()
        {
            var threshold = 1.1;

            var client = Initialize_Client(new NotificationTriggerResponse(NotificationTriggerItem.ThresholdTrigger(threshold: threshold.ToString())));

            var trigger = client.GetNotificationTriggers(1001).Single();

            Assert.AreEqual(threshold, trigger.Threshold);
        }

        [UnitTest]
        [TestMethod]
        public void NotificationTrigger_Parses_LongThreshold()
        {
            var threshold = 5000000000;

            var client = Initialize_Client(new NotificationTriggerResponse(NotificationTriggerItem.ThresholdTrigger(threshold: threshold.ToString())));

            var trigger = client.GetNotificationTriggers(1001).Single();

            Assert.AreEqual(threshold, trigger.Threshold);
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

        #endregion
        #region Translation
        #region English vs. Foreign

        [UnitTest]
        [TestMethod]
        public void NotificationTrigger_Translates_State()
        {
            var translation = GetStandardState();

            TestTriggerTranslation(translation);
        }

        [UnitTest]
        [TestMethod]
        public void NotificationTrigger_Translates_Volume()
        {
            var translation = GetStandardVolume();

            TestTriggerTranslation(translation, true);
        }

        [UnitTest]
        [TestMethod]
        public void NotificationTrigger_Translates_Speed()
        {
            var translation = GetStandardSpeed();

            TestTriggerTranslation(translation, true);
        }

        [UnitTest]
        [TestMethod]
        public void NotificationTrigger_Translates_Threshold()
        {
            var translation = GetStandardThreshold();

            TestTriggerTranslation(translation, true);
        }

        [UnitTest]
        [TestMethod]
        public void NotificationTrigger_Translates_Change()
        {
            var translation = GetStandardChange();

            TestTriggerTranslation(translation);
        }

        private TriggerTranslation GetStandardState()
        {
            var stateConfig = new object[]
            {
                /* latency               */ 60,
                /* escLatency            */ 300,
                /* repeatival            */ 0,
                /* offNotificationAction */ new[] {"-1|None|", "-1|なし|", "-1|no notification||", "-1|通知なし||"},
                /* escNotificationAction */ new[] {"-1|None|", "-1|なし|", "-1|no notification||", "-1|通知なし||"},
                /* onNotificationAction  */ new[] {"302|Ticket Notification", "302|Ticket Notification|ticketnoti:チケット"},
                /* nodest                */ new[] {"Down", "ダウン"},
                /* nodestOptions         */ new[] {"Down", "ダウン", "Warning", "警告", "Unusual", "異常", "Down (Partial)", "一部ダウン", "Up", "稼働中", "Unknown", "原因不明状態"},
                /* parentId              */ 0,
                /* subId                 */ 1,
                /* typeName              */ new[] {"State Trigger", "状態トリガー"},
            };

            var translation = TriggerTranslator.StateTrigger(stateConfig);

            return translation;
        }

        private TriggerTranslation GetStandardVolume()
        {
            var volumeConfig = new object[]
            {
                /* channel              */ new[] {"Total", "合計"},
                /* channelOptions       */ new[] {"Primary", "プライマリ", "Total", "合計", "Traffic In", "受信トラフィック", "Traffic Out", "送信トラフィック"},
                /* threshold            */ 30,
                /* unitSize             */ "Byte",
                /* period               */ new[] {"Hour", "時間"},
                /* periodOptions        */ new[] {"Hour", "時間", "Day", "日", "Week", "週", "Month", "月"},
                /* onNotificationAction */ new[] {"-1|None|", "-1|なし|", "-1|no notification||", "-1|通知なし||"},
                /* parentId             */ 0,
                /* subId                */ 3,
                /* typeName             */ new[] {"Volume Trigger", "ボリュームトリガー" }
            };

            var translation = TriggerTranslator.VolumeTrigger(volumeConfig);

            return translation;
        }

        private TriggerTranslation GetStandardSpeed()
        {
            var speedConfig = new object[]
            {
                /* latency               */ 40,
                /* offNotificationAction */ new[] {"302|Ticket Notification", "302|Ticket Notification|ticketnoti:チケット"},
                /* channel               */ new[] {"Traffic In", "受信トラフィック"},
                /* channelOptions        */ new[] {"Primary", "プライマリ", "Total", "合計", "Traffic In", "受信トラフィック", "Traffic Out", "送信トラフィック"},
                /* condition             */ new[] {"Above", "以上の"},
                /* conditionOptions      */ new[] {"Above", "以上の", "Below", "以下の", "Equal to", "と同じ", "Not Equal to", "と同じでない"},
                /* threshold             */ 20,
                /* unitSize              */ "Mbit",
                /* unitTime              */ new[] {"s", "秒", "second", "秒"},
                /* unitTimeOptions       */ new[] {"second", "秒", "minute", "分", "hour", "時間", "day", "日"},
                /* onNotificationAction  */ new[] {"-1|None|", "-1|なし|", "-1|no notification||", "-1|通知なし||"},
                /* parentId              */ 0,
                /* subId                 */ 2,
                /* typeName              */ new[] {"Speed Trigger", "速度トリガー"}
            };

            var translation = TriggerTranslator.SpeedTrigger(speedConfig);

            return translation;
        }

        private TriggerTranslation GetStandardThreshold()
        {
            var thresholdConfig = new object[]
            {
                /* latency               */ 20,
                /* offNotificationAction */ new[] {"302|Ticket Notification", "302|Ticket Notification|ticketnoti:チケット"},
                /* channel               */ new[] {"Traffic In", "受信トラフィック"},
                /* channelOptions        */ new[] {"Primary", "プライマリ", "Total", "合計", "Traffic In", "受信トラフィック", "Traffic Out", "送信トラフィック"},
                /* condition             */ new[] {"Above", "以上の"},
                /* conditionOptions      */ new[] {"Above", "以上の", "Below", "以下の", "Equal to", "と同じ", "Not Equal to", "と同じでない"},
                /* threshold             */ 10,
                /* onNotificationAction  */ new[] {"-1|None|", "-1|なし|", "-1|no notification||", "-1|通知なし||"},
                /* parentId              */ 0,
                /* subId                 */ 1,
                /* typeName              */ new[] {"Threshold Trigger", "閾値トリガー" }
            };

            var translation = TriggerTranslator.ThresholdTrigger(thresholdConfig);

            return translation;
        }

        private TriggerTranslation GetStandardChange()
        {
            var changeConfig = new object[]
            {
                /* onNotificationAction */ "301|Email to all members of group PRTG Users Group",
                /* parentId             */ "0",
                /* subId                */ "8",
                /* typeName             */ new[] {"Change Trigger", "変化トリガー"}
            };

            var translation = TriggerTranslator.ChangeTrigger(changeConfig);

            return translation;
        }

        #endregion

        [UnitTest]
        [TestMethod]
        public async Task NotificationTrigger_Translates_TriggerAsync()
        {
            var translation = GetStandardState();

            await TestTriggerTranslationAsync(translation);
        }

        [UnitTest]
        [TestMethod]
        public void NotificationTrigger_Translates_Enum_WithMismatchedRawName()
        {
            //e.g. triggers.json says TimeUnit is "Minute", but table.xml says "Min."

            var xml = NotificationTriggerItem.SpeedTrigger(unitTime: "Min.");
            var json = NotificationTriggerJsonItem.SpeedTrigger(unitTime: "Minute", unitTimeInput: new[] { "Sekunde", "Minute", "Stunde", "Tag" });

            var addresses = new[]
            {
                UnitRequest.Triggers(1),
                UnitRequest.RequestObjectData(810),
                UnitRequest.TriggerTypes(1),
                UnitRequest.Objects("filter_objid=1")
            };

            var client = TranslationClient(xml, json, false, addresses);

            var trigger = client.GetNotificationTriggers(1).Single();

            Assert.AreEqual(TimeUnit.Minute, trigger.UnitTime);
        }

        [UnitTest]
        [TestMethod]
        public void NotificationTrigger_Translates_InheritedOnly()
        {
            var xml = NotificationTriggerItem.SpeedTrigger(parentId: "2", condition: "以下の");
            var json = NotificationTriggerJsonItem.SpeedTrigger(condition: "以下の", conditionInput: new[] { "以上の", "以下の", "と同じ", "と同じでない" });

            var addresses = new[]
            {
                UnitRequest.Triggers(1),
                UnitRequest.RequestObjectData(810),
                UnitRequest.TriggerTypes(2),
                UnitRequest.Objects("filter_objid=1")
            };

            var client = TranslationClient(xml, json, false, addresses);

            var trigger = client.GetNotificationTriggers(1).Single();

            Assert.IsTrue(trigger.Inherited);

            Assert.AreEqual(TriggerCondition.Below, trigger.Condition);
        }

        [UnitTest]
        [TestMethod]
        public void NotificationTrigger_Translates_InheritedAfterNormal()
        {
            var possibleConditions = new[] {"以上の", "以下の", "と同じ", "と同じでない"};

            var xml = NotificationTriggerItem.SpeedTrigger(parentId: "1", condition: "以下の", subId: "1");
            var xmlInherited = NotificationTriggerItem.SpeedTrigger(parentId: "2", condition: "と同じ", subId: "1");

            var json = NotificationTriggerJsonItem.SpeedTrigger(condition: "以下の", conditionInput: possibleConditions, subId: "1");

            var addresses = new[]
            {
                UnitRequest.Triggers(1),
                UnitRequest.RequestObjectData(810),
                UnitRequest.TriggerTypes(1),
                UnitRequest.Objects("filter_objid=1")
            };

            var client = TranslationClient(
                new[] {xml, xmlInherited},
                new[] {new[] {json}},
                false,
                addresses
            );

            var triggers = client.GetNotificationTriggers(1);

            Assert.AreEqual(2, triggers.Count);
            Assert.IsTrue(triggers[1].Inherited);
            Assert.AreEqual(TriggerCondition.Below, triggers[0].Condition);
            Assert.AreEqual(TriggerCondition.Equals, triggers[1].Condition);
        }

        [UnitTest]
        [TestMethod]
        public void NotificationTrigger_Translates_PrioritizesIdByCount()
        {
            var obj1aXml = NotificationTriggerItem.VolumeTrigger(parentId: "1", subId: "1");
            var obj2aXml = NotificationTriggerItem.StateTrigger(parentId: "2", subId: "2");
            var obj2bXml = NotificationTriggerItem.StateTrigger(parentId: "2", subId: "3");
            var obj3aXml = NotificationTriggerItem.SpeedTrigger(parentId: "3", subId: "4");
            var obj3bXml = NotificationTriggerItem.SpeedTrigger(parentId: "3", subId: "5");
            var obj3cXml = NotificationTriggerItem.SpeedTrigger(parentId: "3", subId: "6");

            var obj1aJson = NotificationTriggerJsonItem.VolumeTrigger(subId: "1");
            var obj2aJson = NotificationTriggerJsonItem.StateTrigger(subId: "2");
            var obj2bJson = NotificationTriggerJsonItem.StateTrigger(subId: "3");
            var obj3aJson = NotificationTriggerJsonItem.SpeedTrigger(subId: "4");
            var obj3bJson = NotificationTriggerJsonItem.SpeedTrigger(subId: "5");
            var obj3cJson = NotificationTriggerJsonItem.SpeedTrigger(subId: "6");

            var addresses = new[]
            {
                UnitRequest.Triggers(1),
                UnitRequest.RequestObjectData(810),
                UnitRequest.TriggerTypes(3),
                UnitRequest.TriggerTypes(2),
                UnitRequest.TriggerTypes(1),
                UnitRequest.Objects("filter_objid=1")
            };

            var client = TranslationClient(
                new[] {obj1aXml, obj2aXml, obj2bXml, obj3aXml, obj3bXml, obj3cXml},
                new[] {
                    new[] {obj3aJson, obj3bJson, obj3cJson},
                    new[] {obj2aJson, obj2bJson},
                    new[] {obj1aJson}
                },
                false,
                addresses
            );

            var triggers = client.GetNotificationTriggers(1);

            Assert.AreEqual(6, triggers.Count);
        }

        #region Non Inherited, Cache

        [UnitTest]
        [TestMethod]
        public void NotificationTrigger_Translates_UsingCache_NonInherited_State()
        {
            var translation = GetStandardState();

            TestTranslationCache(translation);
        }

        [UnitTest]
        [TestMethod]
        public void NotificationTrigger_Translates_UsingCache_NonInherited_Volume()
        {
            var translation = GetStandardVolume();

            TestTranslationCache(translation, true);
        }

        [UnitTest]
        [TestMethod]
        public void NotificationTrigger_Translates_UsingCache_NonInherited_Speed()
        {
            var translation = GetStandardSpeed();

            TestTranslationCache(translation, true);
        }

        [UnitTest]
        [TestMethod]
        public void NotificationTrigger_Translates_UsingCache_NonInherited_Threshold()
        {
            var translation = GetStandardThreshold();

            TestTranslationCache(translation, true);
        }

        [UnitTest]
        [TestMethod]
        public void NotificationTrigger_Translates_UsingCache_NonInherited_Change()
        {
            var translation = GetStandardChange();

            TestTranslationCache(translation);
        }

        [UnitTest]
        [TestMethod]
        public void NotificationTrigger_Translates_UsingCache_NonInherited_NoAction_Change()
        {
            var changeConfig = new object[]
            {
                /* onNotificationAction */ new[] {"-1|None|", "-1|なし|", "-1|no notification||", "-1|通知なし||"},
                /* parentId             */ "0",
                /* subId                */ "8",
                /* typeName             */ new[] {"Change Trigger", "変化トリガー"}
            };

            var translation = TriggerTranslator.ChangeTrigger(changeConfig);

            TestTranslationCache(translation);
        }

        #endregion

        [UnitTest]
        [TestMethod]
        public void NotificationTrigger_Translates_InheritedAfterNormal_SomePropertiesMissing()
        {
            var firstPossibleConditions = new[] { "以上の", "以下の", };
            var secondPossibleConditions = new[] { "以上の", "以下の", "と同じ", "と同じでない" };

            var xml = NotificationTriggerItem.SpeedTrigger(parentId: "2000", condition: "以下の", subId: "1");
            var xmlInherited = NotificationTriggerItem.SpeedTrigger(parentId: "1000", condition: "と同じ", subId: "1");

            var json = NotificationTriggerJsonItem.SpeedTrigger(condition: "以下の", conditionInput: firstPossibleConditions, subId: "1");
            var jsonInherited = NotificationTriggerJsonItem.SpeedTrigger(condition: "と同じ", conditionInput: secondPossibleConditions, subId: "1");

            var addresses = new[]
            {
                UnitRequest.Triggers(2000),
                UnitRequest.RequestObjectData(810),
                UnitRequest.TriggerTypes(2000),
                UnitRequest.TriggerTypes(1000),
                UnitRequest.Objects("filter_objid=2000")
            };

            var client = TranslationClient(
                new[] { xml, xmlInherited },
                new[] { new[] { json }, new[] { jsonInherited } },
                false,
                addresses
            );

            var triggers = client.GetNotificationTriggers(2000);

            Assert.AreEqual(2, triggers.Count);
            Assert.AreEqual(TriggerCondition.Below, triggers[0].Condition);
            Assert.AreEqual(TriggerCondition.Equals, triggers[1].Condition);
        }

        [UnitTest]
        [TestMethod]
        public void NotificationTrigger_Translates_NotificationAction_None()
        {
            var xml = NotificationTriggerItem.SpeedTrigger(onNotificationAction: "-1|なし|");
            var json = NotificationTriggerJsonItem.SpeedTrigger(onNotificationAction: "-1|通知なし");

            var addresses = new[]
            {
                UnitRequest.Triggers(1),
                UnitRequest.RequestObjectData(810),
                UnitRequest.TriggerTypes(1),
                UnitRequest.Objects("filter_objid=1"),
            };

            var client = TranslationClient(xml, json, false, addresses);

            var trigger = client.GetNotificationTriggers(1).Single();

            Assert.AreEqual("None", trigger.OnNotificationAction.Name);
        }

        [UnitTest]
        [TestMethod]
        public void NotificationTrigger_Translates_NotificationAction_CachedInherited_None()
        {
            var xml = NotificationTriggerItem.SpeedTrigger(onNotificationAction: "-1|なし|", parentId: "2");
            var json = NotificationTriggerJsonItem.SpeedTrigger(onNotificationAction: "-1|通知なし");

            var addresses = new[]
            {
                UnitRequest.Triggers(1),
                UnitRequest.RequestObjectData(810),
                UnitRequest.TriggerTypes(2),
                UnitRequest.Objects("filter_objid=1"),

                UnitRequest.Triggers(1),
                UnitRequest.Objects("filter_objid=1")
            };

            var client = TranslationClient(xml, json, false, addresses);

            var trigger1 = client.GetNotificationTriggers(1).Single();
            Assert.AreEqual("None", trigger1.OnNotificationAction.Name);

            var trigger2 = client.GetNotificationTriggers(1).Single();
            Assert.AreEqual("None", trigger2.OnNotificationAction.Name);

            CompareTranslatedProperties(trigger1, trigger2, new string[]{});
        }

        [UnitTest]
        [TestMethod]
        public void NotificationTrigger_Translates_NotificationAction_NotNull_Skips()
        {
            var xml = NotificationTriggerItem.SpeedTrigger(onNotificationAction: "302|Ticket Notification|ticketnoti:チケット", parentId: "2");
            var json = NotificationTriggerJsonItem.SpeedTrigger(onNotificationAction: "302|Ticket Notification");

            var addresses = new[]
            {
                UnitRequest.Triggers(1),
                UnitRequest.RequestObjectData(810),
                UnitRequest.TriggerTypes(2),
                UnitRequest.Objects("filter_objid=1")
            };

            var client = TranslationClient(xml, json, false, addresses);

            var trigger = client.GetNotificationTriggers(1).Single();
            Assert.AreEqual("Ticket Notification", trigger.OnNotificationAction.Name);
        }

        [UnitTest]
        [TestMethod]
        public void NotificationTrigger_Translates_NotificationAction_NotNull_CachedInherited_Skips()
        {
            var xml = NotificationTriggerItem.SpeedTrigger(onNotificationAction: "302|Ticket Notification|ticketnoti:チケット", parentId: "2");
            var json = NotificationTriggerJsonItem.SpeedTrigger(onNotificationAction: "302|Ticket Notification");

            var addresses = new[]
            {
                UnitRequest.Triggers(1),
                UnitRequest.RequestObjectData(810),
                UnitRequest.TriggerTypes(2),
                UnitRequest.Objects("filter_objid=1"),

                UnitRequest.Triggers(1),
                UnitRequest.Objects("filter_objid=1")
            };

            var client = TranslationClient(xml, json, false, addresses);

            var trigger1 = client.GetNotificationTriggers(1).Single();
            Assert.AreEqual("Ticket Notification", trigger1.OnNotificationAction.Name);

            var trigger2 = client.GetNotificationTriggers(1).Single();
            Assert.AreEqual("Ticket Notification", trigger2.OnNotificationAction.Name);

            CompareTranslatedProperties(trigger1, trigger2, new string[] {});
        }

        [UnitTest]
        [TestMethod]
        public void NotificationTrigger_Translates_Channel_Sensor()
        {
            var channelInput = new[]
            {
                "Foreign 1",
                "Foreign 2",
                "Percent Available Memory",
                "Foreign 4"
            };

            var xml = NotificationTriggerItem.SpeedTrigger(channel: "Percent Available Memory");
            var json = NotificationTriggerJsonItem.SpeedTrigger(channel: "Percent Available Memory", channelInput: channelInput);

            var addresses = new[]
            {
                UnitRequest.Triggers(4000),
                UnitRequest.RequestObjectData(810),
                UnitRequest.TriggerTypes(1),
                UnitRequest.Objects("filter_objid=4000"),
                UnitRequest.Channels(4000),
                UnitRequest.ChannelProperties(4000, 1)
            };

            var client = TranslationClient(xml, json, false, addresses);

            var trigger = client.GetNotificationTriggers(4000).Single();

            Assert.AreEqual("Percent Available Memory", trigger.channelObj.Name);
        }

        [UnitTest]
        [TestMethod]
        public void NotificationTrigger_Translates_LongThreshold()
        {
            var threshold = 5000000000;

            var xml = NotificationTriggerItem.ThresholdTrigger(threshold: threshold.ToString());
            var json = NotificationTriggerJsonItem.ThresholdTrigger(threshold: threshold.ToString());

            var addresses = new[]
            {
                UnitRequest.Triggers(4000),
                UnitRequest.RequestObjectData(810),
                UnitRequest.TriggerTypes(1),
                UnitRequest.Objects("filter_objid=4000"),
                UnitRequest.Channels(4000),
                UnitRequest.ChannelProperties(4000, 1)
            };

            var client = TranslationClient(xml, json, false, addresses);

            var trigger = client.GetNotificationTriggers(4000).Single();

            Assert.AreEqual(threshold, trigger.Threshold);
        }

        #region Multiple Languages, Single Client

        [UnitTest]
        [TestMethod]
        public void NotificationTrigger_Translates_MultipleLanguages_InOneSession_State()
        {
            var stateConfig = new object[]
            {
                /* latency               */ 60,
                /* escLatency            */ 300,
                /* repeatival            */ 0,
                /* offNotificationAction */ new[] {"-1|Keine|", "-1|なし|", "-1|keine Benachrichtigung||", "-1|通知なし||"},
                /* escNotificationAction */ new[] {"-1|Keine|", "-1|なし|", "-1|keine Benachrichtigung||", "-1|通知なし||"},
                /* onNotificationAction  */ new[] {"302|Ticket Notification", "302|Ticket Notification|ticketnoti:チケット"},
                /* nodest                */ new[] {"Fehler", "ダウン"},
                /* nodestOptions         */ new[] {"Fehler", "ダウン", "Warnung", "警告", "Ungewöhnlich", "異常", "Fehler (Teilweise)", "一部ダウン", "OK", "稼働中", "Unbekannt", "原因不明状態"},
                /* parentId              */ 0,
                /* subId                 */ 1,
                /* typeName              */ new[] {"Trigger: Zustand", "状態トリガー"},
            };

            var translation = TriggerTranslator.StateTrigger(stateConfig);

            TestMultipleLanguageTriggerTranslation(translation);
        }

        [UnitTest]
        [TestMethod]
        public void NotificationTrigger_Translates_MultipleLanguages_InOneSession_Volume()
        {
            var volumeConfig = new object[]
            {
                /* channel              */ new[] {"Summe", "合計"},
                /* channelOptions       */ new[] {"Primär", "プライマリ", "Summe", "合計", "Datenverkehr eingehend", "受信トラフィック", "Datenverkehr ausgehend", "送信トラフィック"},
                /* threshold            */ 30,
                /* unitSize             */ "Byte",
                /* period               */ new[] {"Stunde", "時間"},
                /* periodOptions        */ new[] {"Stunde", "時間", "Tag", "日", "Woche", "週", "Monat", "月"},
                /* onNotificationAction */ new[] {"-1|Keine|", "-1|なし|", "-1|keine Benachrichtigung||", "-1|通知なし||"},
                /* parentId             */ 0,
                /* subId                */ 3,
                /* typeName             */ new[] {"Trigger: Volumen", "ボリュームトリガー" }
            };

            var translation = TriggerTranslator.VolumeTrigger(volumeConfig);

            TestMultipleLanguageTriggerTranslation(translation, true);
        }

        [UnitTest]
        [TestMethod]
        public void NotificationTrigger_Translates_MultipleLanguages_InOneSession_Speed()
        {
            var speedConfig = new object[]
            {
                /* latency               */ 40,
                /* offNotificationAction */ new[] {"302|Ticket Notification", "302|Ticket Notification|ticketnoti:チケット"},
                /* channel               */ new[] {"Datenverkehr eingehend", "受信トラフィック"},
                /* channelOptions        */ new[] {"Primär", "プライマリ", "Summe", "合計", "Datenverkehr eingehend", "受信トラフィック", "Datenverkehr ausgehend", "送信トラフィック"},
                /* condition             */ new[] {"Über", "以上の"},
                /* conditionOptions      */ new[] {"Über", "以上の", "Unter", "以下の", "Gleich", "と同じ", "Ungleich", "と同じでない"},
                /* threshold             */ 20,
                /* unitSize              */ "Mbit",
                /* unitTime              */ new[] {"Sec.", "秒", "Sekunde", "秒"},
                /* unitTimeOptions       */ new[] {"Sekunde", "秒", "Minute", "分", "Stunde", "時間", "Tag", "日"},
                /* onNotificationAction  */ new[] {"-1|Keine|", "-1|なし|", "-1|keine Benachrichtigung||", "-1|通知なし||"},
                /* parentId              */ 0,
                /* subId                 */ 2,
                /* typeName              */ new[] {"Trigger: Geschwindigkeit", "速度トリガー"}
            };

            var translation = TriggerTranslator.SpeedTrigger(speedConfig);

            TestMultipleLanguageTriggerTranslation(translation, true);
        }

        [UnitTest]
        [TestMethod]
        public void NotificationTrigger_Translates_MultipleLanguages_InOneSession_Threshold()
        {
            var thresholdConfig = new object[]
            {
                /* latency               */ 20,
                /* offNotificationAction */ new[] {"302|Ticket Notification", "302|Ticket Notification|ticketnoti:チケット"},
                /* channel               */ new[] {"Datenverkehr eingehend", "受信トラフィック"},
                /* channelOptions        */ new[] {"Primär", "プライマリ", "Summe", "合計", "Datenverkehr eingehend", "受信トラフィック", "Datenverkehr ausgehend", "送信トラフィック"},
                /* condition             */ new[] {"Über", "以上の"},
                /* conditionOptions      */ new[] {"Über", "以上の", "Unter", "以下の", "Gleich", "と同じ", "Ungleich", "と同じでない"},
                /* threshold             */ 10,
                /* onNotificationAction  */ new[] { "-1|Keine|", "-1|なし|", "-1|keine Benachrichtigung||", "-1|通知なし||"},
                /* parentId              */ 0,
                /* subId                 */ 1,
                /* typeName              */ new[] {"Trigger: Schwellenwert", "閾値トリガー" }
            };

            var translation = TriggerTranslator.ThresholdTrigger(thresholdConfig);

            TestMultipleLanguageTriggerTranslation(translation, true);
        }

        [UnitTest]
        [TestMethod]
        public void NotificationTrigger_Translates_MultipleLanguages_InOneSession_Change()
        {
            var changeConfig = new object[]
            {
                /* onNotificationAction */ "301|Email to all members of group PRTG Users Group",
                /* parentId             */ "0",
                /* subId                */ "8",
                /* typeName             */ new[] { "Trigger: Änderung", "変化トリガー"}
            };

            var translation = TriggerTranslator.ChangeTrigger(changeConfig);

            TestMultipleLanguageTriggerTranslation(translation);
        }

        #endregion

        private void TestTriggerTranslation(TriggerTranslation translation, bool hasChannel = false)
        {
            var english = new[]
            {
                UnitRequest.Triggers(0),
                UnitRequest.RequestObjectData(810)
            }.ToList();

            var japanese = new[]
            {
                UnitRequest.Triggers(0),
                UnitRequest.RequestObjectData(810),
                UnitRequest.TriggerTypes(0)
            }.ToList();

            if (hasChannel)
            {
                english.Add(UnitRequest.Objects("filter_objid=0"));
                japanese.Add(UnitRequest.Objects("filter_objid=0"));
            }

            var enClient = TranslationClient(translation.EnglishXml, translation.EnglishJson, true, english.ToArray());
            var jpClient = TranslationClient(translation.JapaneseXml, translation.JapaneseJson, false, japanese.ToArray());

            var enTrigger = enClient.GetNotificationTriggers(0).Single();
            var jpTrigger = jpClient.GetNotificationTriggers(0).Single();

            CompareTranslatedProperties(enTrigger, jpTrigger);
        }

        private async Task TestTriggerTranslationAsync(TriggerTranslation translation, bool hasChannel = false)
        {
            var english = new[]
            {
                UnitRequest.Triggers(0),
                UnitRequest.RequestObjectData(810),
                UnitRequest.Notifications("filter_objid=302"),
                UnitRequest.NotificationProperties(302),
                UnitRequest.Schedules(),
                UnitRequest.ScheduleProperties(623)
            }.ToList();

            var japanese = new[]
            {
                UnitRequest.Triggers(0),
                UnitRequest.RequestObjectData(810),
                UnitRequest.TriggerTypes(0),
                UnitRequest.Notifications("filter_objid=302"),
                UnitRequest.NotificationProperties(302),
                UnitRequest.Schedules(),
                UnitRequest.ScheduleProperties(623)
            }.ToList();

            if (hasChannel)
            {
                english.Add(UnitRequest.Objects("filter_objid=0"));
                japanese.Add(UnitRequest.Objects("filter_objid=0"));
            }

            var enClient = TranslationClient(translation.EnglishXml, translation.EnglishJson, true, english.ToArray());
            var jpClient = TranslationClient(translation.JapaneseXml, translation.JapaneseJson, false, japanese.ToArray());

            var enTrigger = (await enClient.GetNotificationTriggersAsync(0).ConfigureAwait(false)).Single();
            var jpTrigger = (await jpClient.GetNotificationTriggersAsync(0).ConfigureAwait(false)).Single();

            CompareTranslatedProperties(enTrigger, jpTrigger);
        }

        private void TestTranslationCache(TriggerTranslation translation, bool hasChannel = false)
        {
            var addresses = new List<string>
            {
                UnitRequest.Triggers(0),
                UnitRequest.RequestObjectData(810),
                UnitRequest.TriggerTypes(0)
            };

            if (hasChannel)
            {
                addresses.Add(UnitRequest.Objects("filter_objid=0"));
            }

            addresses.Add(UnitRequest.Triggers(0));

            if (hasChannel)
            {
                addresses.Add(UnitRequest.Objects("filter_objid=0"));
            }

            var client = TranslationClient(translation.JapaneseXml, translation.JapaneseJson, false, addresses.ToArray());

            var first = client.GetNotificationTriggers(0).Single();
            Assert.IsFalse(first.Inherited);

            var second = client.GetNotificationTriggers(0).Single();
            Assert.IsFalse(second.Inherited);

            CompareTranslatedProperties(first, second, new string[] {});
        }

        private void CompareTranslatedProperties(NotificationTrigger firstTrigger, NotificationTrigger secondTrigger, string[] different = null)
        {
            if (different == null)
            {
                different = new[]
                {
                    "TypeName"
                };
            }

            var properties = typeof(NotificationTrigger).GetProperties();

            foreach (var property in properties)
            {
                var firstVal = property.GetValue(firstTrigger);
                var secondVal = property.GetValue(secondTrigger);

                if (different.Contains(property.Name))
                    Assert.AreNotEqual(firstVal, secondVal, $"{property.Name} was equal");
                else
                {
                    if (firstVal is NotificationAction)
                    {
                        var enAction = firstVal as NotificationAction;
                        var jpAction = secondVal as NotificationAction;

                        Assert.AreEqual(enAction.Name, jpAction.Name, $"{property.Name} Name was not equal");
                        Assert.AreEqual(enAction.Id, jpAction.Id, $"{property.Name} Id was not equal");
                    }
                    else
                        Assert.AreEqual(firstVal, secondVal, $"{property.Name} was not equal");
                }
            }
        }

        private void TestMultipleLanguageTriggerTranslation(TriggerTranslation translation, bool hasChannel = false)
        {
            var addresses = new List<string>
            {
                UnitRequest.Triggers(1),
                UnitRequest.RequestObjectData(810),
                UnitRequest.TriggerTypes(0)
            };

            if (hasChannel)
            {
                addresses.Add(UnitRequest.Objects("filter_objid=1"));
            }

            addresses.Add(UnitRequest.Triggers(1));
            addresses.Add(UnitRequest.TriggerTypes(0));

            if (hasChannel)
            {
                addresses.Add(UnitRequest.Objects("filter_objid=1"));
            }

            var client = TranslationClient(
                new[] { new[] {translation.EnglishXml}, new[] {translation.JapaneseXml}},
                new[] { new[] {translation.EnglishJson}, new[] {translation.JapaneseJson}},
                false,
                addresses.ToArray()
            );

            var firstTrigger = client.GetNotificationTriggers(1).Single();
            var secondTrigger = client.GetNotificationTriggers(1).Single();

            CompareTranslatedProperties(firstTrigger, secondTrigger);
        }

        private PrtgClient TranslationClient(NotificationTriggerItem xml, NotificationTriggerJsonItem json,
            bool isEnglish, string[] addresses)
        {
            return TranslationClient(new[] { xml }, new[] { new[] { json } }, isEnglish, addresses);
        }

        private PrtgClient TranslationClient(NotificationTriggerItem[] xml, NotificationTriggerJsonItem[][] json,
            bool isEnglish, string[] addresses)
        {
            return TranslationClient(new[] { xml }, json, isEnglish, addresses);
        }

        private PrtgClient TranslationClient(NotificationTriggerItem[][] xml, NotificationTriggerJsonItem[][] json,
            bool isEnglish, string[] addresses)
        {
            var translation = new NotificationTriggerTranslationResponse(xml, json, isEnglish);

            var addressValidator = new AddressValidatorResponse(addresses, true, translation);

            return Initialize_Client(addressValidator);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            var map = (Dictionary<Type, EnumTranslation>)ReflectionExtensions.GetInternalStaticField(typeof(NotificationTriggerTranslator), "TranslationMap");

            map.Clear();
        }

        #endregion
    }
}
