using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Parameters;
using PrtgAPI.Tests.UnitTests.Support;

namespace PrtgAPI.Tests.IntegrationTests.ObjectManipulation
{
    [TestClass]
    public class NotificationTriggerTests : BasePrtgClientTest
    {
        #region Add Without Customization

        [TestMethod]
        [IntegrationTest]
        public void Action_NotificationTrigger_AddWithoutCustomization_State()
        {
            var parameters = new StateTriggerParameters(Settings.Probe);

            AddRemoveTrigger(parameters, true);
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_NotificationTrigger_AddWithoutCustomization_Change()
        {
            var parameters = new ChangeTriggerParameters(Settings.Probe);

            AddRemoveTrigger(parameters, true);
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_NotificationTrigger_AddWithoutCustomization_Volume()
        {
            var parameters = new VolumeTriggerParameters(Settings.Probe);

            AddRemoveTrigger(parameters, true);
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_NotificationTrigger_AddWithoutCustomization_Speed()
        {
            var parameters = new SpeedTriggerParameters(Settings.Probe);

            AddRemoveTrigger(parameters, true);
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_NotificationTrigger_AddWithoutCustomization_Threshold()
        {
            var parameters = new ThresholdTriggerParameters(Settings.Probe);

            AddRemoveTrigger(parameters, true);
        }

        #endregion
        #region Add With Customization

        [TestMethod]
        [IntegrationTest]
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
                State = TriggerSensorState.DownPartial
            };

            AddRemoveTrigger(parameters, false);
        }

        [TestMethod]
        [IntegrationTest]
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
        [IntegrationTest]
        public void Action_NotificationTrigger_AddWithCustomization_Volume()
        {
            var actions = client.GetNotificationActions();

            var parameters = new VolumeTriggerParameters(Settings.Probe)
            {
                OnNotificationAction = actions.First(),
                Channel = TriggerChannel.Total,
                Period = TriggerPeriod.Month,
                UnitSize = DataUnit.GByte,
            };

            AddRemoveTrigger(parameters, false);
        }

        [TestMethod]
        [IntegrationTest]
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
                UnitTime = TimeUnit.Minute,
                UnitSize = DataUnit.Gbit
            };

            AddRemoveTrigger(parameters, false);
        }

        [TestMethod]
        [IntegrationTest]
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
        [IntegrationTest]
        public void Action_NotificationTrigger_CreateFromExistingTrigger_State()
        {
            AddRemoveTriggerFromExisting(TriggerType.State, trigger => new StateTriggerParameters(Settings.Device, trigger));
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_NotificationTrigger_CreateFromExistingTrigger_Threshold_Device()
        {
            AddRemoveTriggerFromExisting(TriggerType.Threshold, trigger => new ThresholdTriggerParameters(Settings.Device, trigger));
        }

        [TestMethod]
        [IntegrationTest]
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

            AssertEx.AreEqual(1, triggers.Count, "Initial trigger did not add successfully");
            AssertEx.IsTrue(triggers.First().Channel.ToString() == channel.Name, "Initial trigger channel did not serialize properly");

            //Clone the trigger

            var newParameters = new ThresholdTriggerParameters(Settings.ChannelSensor, triggers.First());

            client.AddNotificationTrigger(newParameters);

            //Verify it was created successfully

            var newTriggers = client.GetNotificationTriggers(Settings.ChannelSensor).Where(t => t.Inherited == false).ToList();

            AssertEx.AreEqual(2, newTriggers.Count, "Second trigger did not add successfully");

            foreach (var trigger in newTriggers)
            {
                AssertEx.IsTrue(trigger.Channel.ToString() == channel.Name, "Second trigger channel did not serialize properly");
            }
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_NotificationTrigger_CreateFromExistingTrigger_Speed()
        {
            AddRemoveTriggerFromExisting(TriggerType.Speed, trigger => new SpeedTriggerParameters(Settings.Device, trigger));
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_NotificationTrigger_CreateFromExistingTrigger_Volume()
        {
            AddRemoveTriggerFromExisting(TriggerType.Volume, trigger => new VolumeTriggerParameters(Settings.Device, trigger));
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_NotificationTrigger_CreateFromExistingTrigger_Change()
        {
            AddRemoveTriggerFromExisting(TriggerType.Change, trigger => new ChangeTriggerParameters(Settings.Device, trigger));
        }

        private void AddRemoveTriggerFromExisting(TriggerType triggerType, Func<NotificationTrigger, TriggerParameters> getParameters)
        {
            var initialTriggers = client.GetNotificationTriggers(Settings.Device).Where(t => t.Type == triggerType && !t.Inherited).ToList();

            AssertEx.AreEqual(1, initialTriggers.Count, $"Did not have initial expected number of {triggerType} triggers");

            var parameters = getParameters(initialTriggers.First());

            client.AddNotificationTrigger(parameters);

            var triggersNew = client.GetNotificationTriggers(Settings.Device).Where(t => t.Type == triggerType && !t.Inherited).ToList();

            AssertEx.AreEqual(2, triggersNew.Count, "Trigger was not added successfully");

            var newTrigger = triggersNew.First(a => initialTriggers.All(b => b.SubId != a.SubId));
            client.RemoveNotificationTrigger(newTrigger);

            var postRemoveTriggers = client.GetNotificationTriggers(Settings.Device).Where(t => t.Type == triggerType && !t.Inherited).ToList();

            AssertEx.IsTrue(initialTriggers.Count == postRemoveTriggers.Count, $"Initial triggers was {initialTriggers.Count}, however after and removing a trigger the number of triggers was {postRemoveTriggers.Count}");
        }

        #endregion
        #region TriggerChannel

        [TestMethod]
        [IntegrationTest]
        public void Action_NotificationTrigger_TriggerChannel_StandardTriggerChannel_OnSensor()
        {
            var sensor = client.GetSensors(Property.Tags, FilterOperator.Contains, "wmicpu").First();

            AssertEx.Throws<InvalidOperationException>(() => TestTriggerChannel(sensor.Id, TriggerChannel.Total), "Channel 'Total' is not a valid value");
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_NotificationTrigger_TriggerChannel_StandardTriggerChannel_OnContainer()
        {
            TestTriggerChannel(Settings.Probe, TriggerChannel.Total);
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_NotificationTrigger_TriggerChannel_Channel_OnSensor()
        {
            var sensor = client.GetSensors(Property.Tags, FilterOperator.Contains, "wmicpu").First();
            var channel = client.GetChannel(sensor.Id, "Processor 1");

            TestTriggerChannel(sensor.Id, channel);
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_NotificationTrigger_TriggerChannel_Channel_OnContainer()
        {
            var sensor = client.GetSensors(Property.Tags, FilterOperator.Contains, "wmicpu").First();
            var channel = client.GetChannel(sensor.Id, "Total");

            AssertEx.Throws<InvalidOperationException>(() => TestTriggerChannel(Settings.Probe, channel), "Channel 'Total' of type 'Channel' is not a valid channel");
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_NotificationTrigger_TriggerChannel_ChannelId_OnSensor()
        {
            var sensor = client.GetSensors(Property.Tags, FilterOperator.Contains, "wmicpu").First();
            var channel = client.GetChannel(sensor.Id, "Total").Id;

            TestTriggerChannel(sensor.Id, new TriggerChannel(channel), (paramValue, triggerValue, propertyName) =>
            {
                if (propertyName != "Channel")
                    return false;

                var first = PrtgAPIHelpers.GetTriggerChannelSource((TriggerChannel)paramValue);
                var second = PrtgAPIHelpers.GetTriggerChannelSource((TriggerChannel)triggerValue);

                if (first is int)
                    Assert.AreEqual(first, ((Channel)second).Id);
                else
                    Assert.AreEqual(((Channel)first).Id, ((Channel)second).Id);

                return true;
            });
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_NotificationTrigger_TriggerChannel_ChannelId_OnContainer()
        {
            var sensor = client.GetSensors(Property.Tags, FilterOperator.Contains, "wmicpu").First(); ;
            var channel = client.GetChannel(sensor.Id, "Total").Id;

            AssertEx.Throws<InvalidOperationException>(() => TestTriggerChannel(Settings.Probe, new TriggerChannel(channel)), "Channel ID '0' is not a valid channel");
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_NotificationTrigger_TriggerChannel_InvalidChannelId_OnSensor()
        {
            var sensor = client.GetSensors(Property.Tags, FilterOperator.Contains, "wmicpu").First();

            AssertEx.Throws<InvalidOperationException>(() => TestTriggerChannel(sensor.Id, new TriggerChannel(99)), "Channel ID '99' is not a valid channel");
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_NotificationTrigger_TriggerChannel_InvalidChannelId_OnContainer()
        {
            AssertEx.Throws<InvalidOperationException>(() => TestTriggerChannel(Settings.Probe, new TriggerChannel(99)), "Channel ID '99' is not a valid channel");
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_NotificationTrigger_TriggerChannel_Channel_WithStandardTriggerChannelName_OnSensor()
        {
            var sensor = client.GetSensors(Property.Tags, FilterOperator.Contains, "wmicpu").First();
            var channel = client.GetChannel(sensor.Id, "Total");

            TestTriggerChannel(sensor.Id, channel);
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_NotificationTrigger_TriggerChannel_Channel_WithStandardTriggerChannelName_OnContainer()
        {
            var sensor = client.GetSensors(Property.Tags, FilterOperator.Contains, "wmicpu").First();
            var channel = client.GetChannel(sensor.Id, "Total");

            AssertEx.Throws<InvalidOperationException>(() => TestTriggerChannel(Settings.Probe, channel), "Channel 'Total' of type 'Channel' is not a valid channel");
        }

        private void TestTriggerChannel(int objectId, TriggerChannel channel, Func<object, object, string, bool> validator = null)
        {
            var parameters = new ThresholdTriggerParameters(objectId)
            {
                Channel = channel
            };

            DoubleAddRemoveTrigger(
                parameters,
                t => new ThresholdTriggerParameters(objectId, t),
                validator
            );
        }

        #endregion

        private void DoubleAddRemoveTrigger(TriggerParameters parameters, Func<NotificationTrigger, TriggerParameters> cloneBuilder, Func<object, object, string, bool> validator = null)
        {
            AddRemoveTrigger(
                parameters,
                true,
                t => AddRemoveTrigger(cloneBuilder(t), true, validator: validator),
                validator
            );
        }

        private void AddRemoveTrigger(TriggerParameters parameters, bool empty, Action<NotificationTrigger> action = null, Func<object, object, string, bool> validator = null)
        {
            var initialTriggers = client.GetNotificationTriggers(parameters.ObjectId).Where(t => !t.Inherited).ToList();
            client.AddNotificationTrigger(parameters); //i wonder if the new trigger returns the details of the new trigger in the url
            //there was an exception on my state property!

            Thread.Sleep(5000);
            var afterTriggers = client.GetNotificationTriggers(parameters.ObjectId).Where(t => !t.Inherited).ToList();
            AssertEx.IsTrue(afterTriggers.Count == initialTriggers.Count + 1, $"Initial triggers was {initialTriggers.Count}, but after adding a trigger the number of triggers was {afterTriggers.Count}");
            var newTrigger = afterTriggers.First(a => initialTriggers.All(b => b.SubId != a.SubId));

            ValidateNewTrigger(parameters, newTrigger, empty, validator);

            action?.Invoke(newTrigger);

            Thread.Sleep(5000);
            client.RemoveNotificationTrigger(newTrigger);

            Thread.Sleep(5000);
            var removeTriggers = client.GetNotificationTriggers(parameters.ObjectId).Where(t => !t.Inherited).ToList();
            AssertEx.IsTrue(initialTriggers.Count == removeTriggers.Count, $"Initial triggers was {initialTriggers.Count}, however after and removing a trigger the number of triggers was {removeTriggers.Count}");
        }

        private void ValidateNewTrigger(TriggerParameters parameters, NotificationTrigger trigger, bool empty, Func<object, object, string, bool> validator = null)
        {
            if (validator == null)
                validator = (o, t, n) => false;

            foreach (var paramProp in parameters.GetType().GetNormalProperties())
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
                                case nameof(NotificationTrigger.DisplayThreshold):
                                    paramValue = "0";
                                    break;
                                case nameof(NotificationTrigger.RepeatInterval):
                                    paramValue = "0";
                                    break;
                            }
                        }

                        if (!validator(paramProp.GetValue(parameters), triggerProp.GetValue(trigger), triggerProp.Name))
                            AssertEx.AreEqual(paramValue, triggerValue, triggerProp.Name);

                        //when we create a trigger without customization, some fields get default values
                        //we should have verification of those values, but ONLY when we're doing
                        //verification without customization. maybe we should have a bool on validatenewtrigger
                        //that indicates whether this is without customization, and ONLY THEN do we say ok
                        //paramValue can be null but triggerValue can be <something>
                    }
                }

                if (!found)
                    AssertEx.Fail($"Couldn't find notification trigger property that corresponded to parameter property '{paramProp.Name}'");
            }
        }

        [TestMethod]
        [IntegrationTest]
        public void AddNotificationTrigger_Resolves()
        {
            Resolves(true);
        }

        [TestMethod]
        [IntegrationTest]
        public void AddNotificationTrigger_DoesntResolve()
        {
            Resolves(false);
        }

        [TestMethod]
        [IntegrationTest]
        public async Task AddNotificationTrigger_ResolvesAsync()
        {
            await ResolvesAsync(true);
        }

        [TestMethod]
        [IntegrationTest]
        public async Task AddNotificationTrigger_DoesntResolveAsync()
        {
            await ResolvesAsync(false);
        }

        private void Resolves(bool resolve)
        {
            var device = client.GetDevices(Property.Id, Settings.Device).Single();
            var deviceTriggers = client.GetNotificationTriggers(device.Id).Where(t => t.Type == TriggerType.Threshold).ToList();
            AssertEx.AreEqual(1, deviceTriggers.Count, "Found incorrect number of source triggers");

            var probe = client.GetProbes(Property.Id, Settings.Probe).Single();
            var probeTrggers = client.GetNotificationTriggers(probe.Id).Where(t => t.Type == TriggerType.Threshold).ToList();
            AssertEx.AreEqual(0, probeTrggers.Count, "Found incorrect number of existing triggers on target object");

            var parameters = new ThresholdTriggerParameters(Settings.Probe, deviceTriggers.Single());
            var newTrigger = client.AddNotificationTrigger(parameters, resolve);

            var manualTrigger = client.GetNotificationTriggers(probe.Id).Where(t => t.Type == TriggerType.Threshold).ToList();

            try
            {
                if (resolve)
                {
                    AssertEx.AreEqual(1, manualTrigger.Count, "Found incorrect number of new triggers on target object");

                    AssertEx.AreEqual(manualTrigger.Single().ParentId, newTrigger.ParentId, "Parent ID was not correct");
                    AssertEx.AreEqual(manualTrigger.Single().SubId, newTrigger.SubId, "Sub ID was not correct");
                }
                else
                {
                    AssertEx.AreEqual(null, newTrigger, "New trigger was not null");
                }
            }
            finally
            {
                if (resolve)
                    client.RemoveNotificationTrigger(newTrigger);
                else
                    client.RemoveNotificationTrigger(manualTrigger.Single());
            }
            
        }

        private async Task ResolvesAsync(bool resolve)
        {
            var device = (await client.GetDevicesAsync(Property.Id, Settings.Device)).Single();
            var deviceTriggers = (await client.GetNotificationTriggersAsync(device.Id)).Where(t => t.Type == TriggerType.Threshold).ToList();
            AssertEx.AreEqual(1, deviceTriggers.Count, "Found incorrect number of source triggers");

            var probe = (await client.GetProbesAsync(Property.Id, Settings.Probe)).Single();
            var probeTrggers = (await client.GetNotificationTriggersAsync(probe.Id)).Where(t => t.Type == TriggerType.Threshold).ToList();
            AssertEx.AreEqual(0, probeTrggers.Count, "Found incorrect number of existing triggers on target object");

            var parameters = new ThresholdTriggerParameters(Settings.Probe, deviceTriggers.Single());
            var newTrigger = await client.AddNotificationTriggerAsync(parameters, resolve);

            var manualTrigger = (await client.GetNotificationTriggersAsync(probe.Id)).Where(t => t.Type == TriggerType.Threshold).ToList();

            try
            {
                if (resolve)
                {
                    AssertEx.AreEqual(1, manualTrigger.Count, "Found incorrect number of new triggers on target object");

                    AssertEx.AreEqual(manualTrigger.Single().ParentId, newTrigger.ParentId, "Parent ID was not correct");
                    AssertEx.AreEqual(manualTrigger.Single().SubId, newTrigger.SubId, "Sub ID was not correct");
                }
                else
                {
                    AssertEx.AreEqual(null, newTrigger, "New trigger was not null");
                }
            }
            finally
            {
                if (resolve)
                    await client.RemoveNotificationTriggerAsync(newTrigger);
                else
                    await client.RemoveNotificationTriggerAsync(manualTrigger.Single());
            }
        }
    }
}
