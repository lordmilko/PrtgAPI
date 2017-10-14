using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Parameters;
using PrtgAPI.Tests.UnitTests.Helpers;

namespace PrtgAPI.Tests.IntegrationTests.ActionTests
{
    [TestClass]
    public class NotificationTriggerTests : BasePrtgClientTest
    {
        #region Add Without Customization

        [TestMethod]
        public void Action_NotificationTrigger_AddWithoutCustomization_State()
        {
            var parameters = new StateTriggerParameters(Settings.Probe);

            AddRemoveTrigger(parameters, true);
        }

        [TestMethod]
        public void Action_NotificationTrigger_AddWithoutCustomization_Change()
        {
            var parameters = new ChangeTriggerParameters(Settings.Probe);

            AddRemoveTrigger(parameters, true);
        }

        [TestMethod]
        public void Action_NotificationTrigger_AddWithoutCustomization_Volume()
        {
            var parameters = new VolumeTriggerParameters(Settings.Probe);

            AddRemoveTrigger(parameters, true);
        }

        [TestMethod]
        public void Action_NotificationTrigger_AddWithoutCustomization_Speed()
        {
            var parameters = new SpeedTriggerParameters(Settings.Probe);

            AddRemoveTrigger(parameters, true);
        }

        [TestMethod]
        public void Action_NotificationTrigger_AddWithoutCustomization_Threshold()
        {
            var parameters = new ThresholdTriggerParameters(Settings.Probe);

            AddRemoveTrigger(parameters, true);
        }

        #endregion
        #region Add With Customization

        [TestMethod]
        public void Action_NotificationTrigger_AddWithCustomization_State()
        {
            var actions = client.GetNotificationActions();

            var parameters = new StateTriggerParameters(Settings.Probe)
            {
                OnNotificationAction = actions.First(),
                OffNotificationAction = actions.Skip(1).First(),
                EscalationNotificationAction = actions.Skip(2).First(),
                Latency = 90,
                EscalationLatency = 400,
                RepeatInterval = 3,
                State = TriggerSensorState.PartialDown
            };

            AddRemoveTrigger(parameters, false);
        }

        [TestMethod]
        public void Action_NotificationTrigger_AddWithCustomization_Change()
        {
            var action = client.GetNotificationActions().First();

            var parameters = new ChangeTriggerParameters(Settings.Probe)
            {
                OnNotificationAction = action
            };

            AddRemoveTrigger(parameters, false);
        }

        [TestMethod]
        public void Action_NotificationTrigger_AddWithCustomization_Volume()
        {
            var actions = client.GetNotificationActions();

            var parameters = new VolumeTriggerParameters(Settings.Probe)
            {
                OnNotificationAction = actions.First(),
                Channel = TriggerChannel.Total,
                Period = TriggerPeriod.Month,
                UnitSize = TriggerVolumeUnitSize.GByte,
            };

            AddRemoveTrigger(parameters, false);
        }

        [TestMethod]
        public void Action_NotificationTrigger_AddWithCustomization_Speed()
        {
            var actions = client.GetNotificationActions();

            var parameters = new SpeedTriggerParameters(Settings.Probe)
            {
                OnNotificationAction = actions.First(),
                OffNotificationAction = actions.Skip(1).First(),
                Channel = TriggerChannel.TrafficOut,
                Latency = 100,
                Condition = TriggerCondition.NotEquals,
                Threshold = 3,
                UnitTime = TriggerUnitTime.Min,
                UnitSize = TriggerUnitSize.Gbit
            };

            AddRemoveTrigger(parameters, false);
        }

        [TestMethod]
        public void Action_NotificationTrigger_AddWithCustomization_Threshold()
        {
            var actions = client.GetNotificationActions();

            var parameters = new ThresholdTriggerParameters(Settings.Probe)
            {
                OnNotificationAction = actions.First(),
                OffNotificationAction = actions.Skip(0).First(),
                Latency = 40,
                Threshold = 4,
                Condition = TriggerCondition.Equals,
                Channel = TriggerChannel.Total
            };

            AddRemoveTrigger(parameters, false);
        }

        #endregion
        #region Add From Existing

        [TestMethod]
        public void Action_NotificationTrigger_CreateFromExistingTrigger_State()
        {
            AddRemoveTriggerFromExisting(TriggerType.State, trigger => new StateTriggerParameters(Settings.Device, trigger));
        }

        [TestMethod]
        public void Action_NotificationTrigger_CreateFromExistingTrigger_Threshold_Device()
        {
            AddRemoveTriggerFromExisting(TriggerType.Threshold, trigger => new ThresholdTriggerParameters(Settings.Device, trigger));
        }

        [TestMethod]
        public void Action_NotificationTrigger_CreateFromExistingTrigger_Threshold_Sensor()
        {
            //Create the initial trigger

            var channel = client.GetChannels(Settings.ChannelSensor).First();

            var parameters = new ThresholdTriggerParameters(Settings.ChannelSensor)
            {
                Channel = channel
            };

            //Verify it was created successfully

            client.AddNotificationTrigger(parameters);

            var triggers = client.GetNotificationTriggers(Settings.ChannelSensor).Where(t => t.Inherited == false).ToList();

            Assert2.AreEqual(1, triggers.Count, "Initial trigger did not add successfully");
            Assert2.IsTrue(triggers.First().Channel.ToString() == channel.Name, "Initial trigger channel did not serialize properly");

            //Clone the trigger

            var newParameters = new ThresholdTriggerParameters(Settings.ChannelSensor, triggers.First());

            client.AddNotificationTrigger(newParameters);

            //Verify it was created successfully

            var newTriggers = client.GetNotificationTriggers(Settings.ChannelSensor).Where(t => t.Inherited == false).ToList();

            Assert2.AreEqual(2, newTriggers.Count, "Second trigger did not add successfully");

            foreach (var trigger in newTriggers)
            {
                Assert2.IsTrue(trigger.Channel.ToString() == channel.Name, "Second trigger channel did not serialize properly");
            }
        }

        [TestMethod]
        public void Action_NotificationTrigger_CreateFromExistingTrigger_Speed()
        {
            AddRemoveTriggerFromExisting(TriggerType.Speed, trigger => new SpeedTriggerParameters(Settings.Device, trigger));
        }

        [TestMethod]
        public void Action_NotificationTrigger_CreateFromExistingTrigger_Volume()
        {
            AddRemoveTriggerFromExisting(TriggerType.Volume, trigger => new VolumeTriggerParameters(Settings.Device, trigger));
        }

        [TestMethod]
        public void Action_NotificationTrigger_CreateFromExistingTrigger_Change()
        {
            AddRemoveTriggerFromExisting(TriggerType.Change, trigger => new ChangeTriggerParameters(Settings.Device, trigger));
        }

        private void AddRemoveTriggerFromExisting(TriggerType triggerType, Func<NotificationTrigger, TriggerParameters> getParameters)
        {
            var initialTriggers = client.GetNotificationTriggers(Settings.Device).Where(t => t.Type == triggerType && !t.Inherited).ToList();

            Assert2.AreEqual(1, initialTriggers.Count, $"Did not have initial expected number of {triggerType} triggers");

            var parameters = getParameters(initialTriggers.First());

            client.AddNotificationTrigger(parameters);

            var triggersNew = client.GetNotificationTriggers(Settings.Device).Where(t => t.Type == triggerType && !t.Inherited).ToList();

            Assert2.AreEqual(2, triggersNew.Count, "Trigger was not added successfully");

            var newTrigger = triggersNew.First(a => initialTriggers.All(b => b.SubId != a.SubId));
            client.RemoveNotificationTrigger(newTrigger);

            var postRemoveTriggers = client.GetNotificationTriggers(Settings.Device).Where(t => t.Type == triggerType && !t.Inherited).ToList();

            Assert2.IsTrue(initialTriggers.Count == postRemoveTriggers.Count, $"Initial triggers was {initialTriggers.Count}, however after and removing a trigger the number of triggers was {postRemoveTriggers.Count}");
        }

        #endregion

        private void AddRemoveTrigger(TriggerParameters parameters, bool empty)
        {
            var initialTriggers = client.GetNotificationTriggers(parameters.ObjectId).Where(t => !t.Inherited).ToList();
            client.AddNotificationTrigger(parameters); //i wonder if the new trigger returns the details of the new trigger in the url
            //there was an exception on my state property!

            Thread.Sleep(5000);
            var afterTriggers = client.GetNotificationTriggers(parameters.ObjectId).Where(t => !t.Inherited).ToList();
            Assert2.IsTrue(afterTriggers.Count == initialTriggers.Count + 1, $"Initial triggers was {initialTriggers.Count}, but after adding a trigger the number of triggers was {afterTriggers.Count}");
            var newTrigger = afterTriggers.First(a => initialTriggers.All(b => b.SubId != a.SubId));

            ValidateNewTrigger(parameters, newTrigger, empty);

            Thread.Sleep(5000);
            client.RemoveNotificationTrigger(newTrigger);

            Thread.Sleep(5000);
            var removeTriggers = client.GetNotificationTriggers(parameters.ObjectId).Where(t => !t.Inherited).ToList();
            Assert2.IsTrue(initialTriggers.Count == removeTriggers.Count, $"Initial triggers was {initialTriggers.Count}, however after and removing a trigger the number of triggers was {removeTriggers.Count}");
        }

        private void ValidateNewTrigger(TriggerParameters parameters, NotificationTrigger trigger, bool empty)
        {
            foreach (var paramProp in parameters.GetType().GetProperties2())
            {
                bool found = false;

                foreach (var triggerProp in trigger.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if ((paramProp.Name == "TriggerInternal" && triggerProp.Name == "Trigger") ||
                        (paramProp.Name == "State" && triggerProp.Name == "StateTrigger") ||
                        (paramProp.Name == triggerProp.Name))
                    {
                        found = true;
                        var paramValue = paramProp.GetValue(parameters)?.ToString();
                        var triggerValue = triggerProp.GetValue(trigger)?.ToString();

                        if (empty && paramValue == null)
                        {
                            switch (triggerProp.Name)
                            {
                                case nameof(NotificationTrigger.Latency):
                                    paramValue = "60";
                                    break;
                                case nameof(NotificationTrigger.EscalationLatency):
                                    paramValue = "300";
                                    break;
                                case nameof(NotificationTrigger.Threshold):
                                    paramValue = "0";
                                    break;
                                case nameof(NotificationTrigger.RepeatInterval):
                                    paramValue = "0";
                                    break;
                            }
                        }

                        Assert2.AreEqual(paramValue, triggerValue, triggerProp.Name);

                        //when we create a trigger without customization, some fields get default values
                        //we should have verification of those values, but ONLY when we're doing
                        //verification without customization. maybe we should have a bool on validatenewtrigger
                        //that indicates whether this is without customization, and ONLY THEN do we say ok
                        //paramValue can be null but triggerValue can be <something>
                    }
                }

                if (!found)
                    Assert2.Fail($"Couldn't find notification trigger property that corresponded to parameter property '{paramProp.Name}'");
            }
        }
    }
}
