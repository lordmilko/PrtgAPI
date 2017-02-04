using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        public void Action_NotificationTrigger_State_AddWithoutCustomization()
        {
            var parameters = new StateTriggerParameters(Settings.Probe);

            AddRemoveTrigger(parameters, true);
        }

        [TestMethod]
        public void Action_NotificationTrigger_Change_AddWithoutCustomization()
        {
            var parameters = new ChangeTriggerParameters(Settings.Probe);

            AddRemoveTrigger(parameters, true);
        }

        [TestMethod]
        public void Action_NotificationTrigger_Volume_AddWithoutCustomization()
        {
            var parameters = new VolumeTriggerParameters(Settings.Probe);

            AddRemoveTrigger(parameters, true);
        }

        [TestMethod]
        public void Action_NotificationTrigger_Speed_AddWithoutCustomization()
        {
            var parameters = new SpeedTriggerParameters(Settings.Probe);

            AddRemoveTrigger(parameters, true);
        }

        [TestMethod]
        public void Action_NotificationTrigger_Threshold_AddWithoutCustomization()
        {
            var parameters = new ThresholdTriggerParameters(Settings.Probe);

            AddRemoveTrigger(parameters, true);
        }

        #endregion
        #region Add With Customization

        [TestMethod]
        public void Action_NotificationTrigger_State_AddWithCustomization()
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
        public void Action_NotificationTrigger_Change_AddWithCustomization()
        {
            var action = client.GetNotificationActions().First();

            var parameters = new ChangeTriggerParameters(Settings.Probe)
            {
                OnNotificationAction = action
            };

            AddRemoveTrigger(parameters, false);
        }

        [TestMethod]
        public void Action_NotificationTrigger_Volume_AddWithCustomization()
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
        public void Action_NotificationTrigger_Speed_AddWithCustomization()
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
        public void Action_NotificationTrigger_Threshold_AddWithCustomization()
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


        private void AddRemoveTrigger(TriggerParameters parameters, bool empty)
        {
            var initialTriggers = client.GetNotificationTriggers(parameters.ObjectId).Where(t => !t.Inherited).ToList();
            client.AddNotificationTrigger(parameters); //i wonder if the new trigger returns the details of the new trigger in the url
            //there was an exception on my state property!

            Thread.Sleep(5000);
            var afterTriggers = client.GetNotificationTriggers(parameters.ObjectId).Where(t => !t.Inherited).ToList();
            Assert.IsTrue(afterTriggers.Count == initialTriggers.Count + 1, $"Initial triggers was {initialTriggers.Count}, but after adding a trigger the number of triggers was {afterTriggers.Count}");
            var newTrigger = afterTriggers.First(a => initialTriggers.All(b => b.SubId != a.SubId));

            ValidateNewTrigger(parameters, newTrigger, empty);

            Thread.Sleep(5000);
            client.RemoveNotificationTrigger(newTrigger);

            Thread.Sleep(5000);
            var removeTriggers = client.GetNotificationTriggers(parameters.ObjectId).Where(t => !t.Inherited).ToList();
            Assert.IsTrue(initialTriggers.Count == removeTriggers.Count, $"Initial triggers was {initialTriggers.Count}, however after and removing a trigger the number of triggers was {removeTriggers.Count}");
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

                        Assert.AreEqual(paramValue, triggerValue, triggerProp.Name);

                        //when we create a trigger without customization, some fields get default values
                        //we should have verification of those values, but ONLY when we're doing
                        //verification without customization. maybe we should have a bool on validatenewtrigger
                        //that indicates whether this is without customization, and ONLY THEN do we say ok
                        //paramValue can be null but triggerValue can be <something>
                    }
                }

                if (!found)
                    Assert.Fail($"Couldn't find notification trigger property that corresponded to parameter property '{paramProp.Name}'");
            }
        }
    }
}
