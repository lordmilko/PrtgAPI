using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Parameters;
using PrtgAPI.Tests.UnitTests.Support;

namespace PrtgAPI.Tests.IntegrationTests.ObjectManipulation
{
    [TestClass]
    public class SetChannelPropertyTests : BasePrtgClientTest
    {
        [TestMethod]
        [IntegrationTest]
        public void Action_Channel_SetChannelProperty_Limits()
        {
            SetAndRevertChannelProperty(3000, ChannelProperty.UpperErrorLimit, c => c.UpperErrorLimit);
            SetAndRevertChannelProperty(4000, ChannelProperty.LowerErrorLimit, c => c.LowerErrorLimit);
            SetAndRevertChannelProperty(5000, ChannelProperty.UpperWarningLimit, c => c.UpperWarningLimit);
            SetAndRevertChannelProperty(6000, ChannelProperty.LowerWarningLimit, c => c.LowerWarningLimit);
            SetAndRevertChannelProperty("testMessage", ChannelProperty.ErrorLimitMessage, c => c.ErrorLimitMessage);
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Action_Channel_SetChannelProperty_LimitsAsync()
        {
            await SetAndRevertChannelPropertyAsync(3000, ChannelProperty.UpperErrorLimit, c => c.UpperErrorLimit);
            await SetAndRevertChannelPropertyAsync(4000, ChannelProperty.LowerErrorLimit, c => c.LowerErrorLimit);
            await SetAndRevertChannelPropertyAsync(5000, ChannelProperty.UpperWarningLimit, c => c.UpperWarningLimit);
            await SetAndRevertChannelPropertyAsync(6000, ChannelProperty.LowerWarningLimit, c => c.LowerWarningLimit);
            await SetAndRevertChannelPropertyAsync("testMessage", ChannelProperty.ErrorLimitMessage, c => c.ErrorLimitMessage);
        }

        private void SetAndRevertChannelProperty<T>(T newValue, ChannelProperty property, Func<Channel, T> getProperty)
        {
            var initialChannel = client.GetChannels(Settings.ChannelSensor).First(c => c.Id == Settings.Channel);
            AssertEx.AreNotEqual(getProperty(initialChannel), newValue, "Initial channel value was not expected value");

            client.SetChannelProperty(Settings.ChannelSensor, Settings.Channel, property, newValue);
            var newChannel = client.GetChannels(Settings.ChannelSensor).First(c => c.Id == Settings.Channel);
            AssertEx.AreEqual(newValue, getProperty(newChannel), "New channel value did not apply properly");

            client.SetChannelProperty(Settings.ChannelSensor, Settings.Channel, property, getProperty(initialChannel));
            var finalChannel = client.GetChannels(Settings.ChannelSensor).First(c => c.Id == Settings.Channel);
            AssertEx.AreEqual(getProperty(initialChannel), getProperty(finalChannel), "Channel value did not revert properly");
        }

        private async Task SetAndRevertChannelPropertyAsync<T>(T newValue, ChannelProperty property, Func<Channel, T> getProperty)
        {
            var initialChannel = (await client.GetChannelsAsync(Settings.ChannelSensor)).First(c => c.Id == Settings.Channel);
            AssertEx.AreNotEqual(getProperty(initialChannel), newValue, "Initial channel value was not expected value");

            await client.SetChannelPropertyAsync(Settings.ChannelSensor, Settings.Channel, property, newValue);
            var newChannel = (await client.GetChannelsAsync(Settings.ChannelSensor)).First(c => c.Id == Settings.Channel);
            AssertEx.AreEqual(newValue, getProperty(newChannel), "New channel value did not apply properly");

            await client.SetChannelPropertyAsync(Settings.ChannelSensor, Settings.Channel, property, getProperty(initialChannel));
            var finalChannel = (await client.GetChannelsAsync(Settings.ChannelSensor)).First(c => c.Id == Settings.Channel);
            AssertEx.AreEqual(getProperty(initialChannel), getProperty(finalChannel), "Channel value did not revert properly");
        }

        #region Factor
            #region Manual (No Factor)
                #region Manual: Same Channel/Sensor

        [TestMethod]
        [IntegrationTest]
        public void Action_SetChannelProperty_Manual_SameID_AndSensor_ExecutesSingleRequest()
        {
            TestNoFactorManual(new[] {Settings.ChannelSensor, Settings.ChannelSensor }, Settings.Channel);
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Action_SetChannelProperty_Manual_SameID_AndSensor_ExecutesSingleRequestAsync()
        {
            await TestNoFactorManualAsync(new[] { Settings.ChannelSensor, Settings.ChannelSensor }, Settings.Channel);
        }

        private void TestNoFactorManual(int[] sensorIds, int channelId)
        {
            var newValue = true;

            var channels = new List<Channel>();

            foreach (var sensorId in sensorIds)
            {
                var channel = client.GetChannel(sensorId, channelId);
                channels.Add(channel);

                AssertEx.AreNotEqual(newValue, channel.SpikeFilterEnabled, $"{nameof(ChannelProperty.SpikeFilterEnabled)} is already true on channel '{channel}' (ID: {channel.Id}, Sensor ID: {channel.SensorId}). Cannot execute test");
            }

            try
            {
                client.SetChannelProperty(sensorIds, channelId, ChannelProperty.SpikeFilterEnabled, newValue);

                var newChannels = sensorIds.Select(sensorId => client.GetChannel(sensorId, channelId)).ToList();

                foreach (var channel in newChannels)
                {
                    Assert.AreEqual(true, channel.SpikeFilterEnabled, $"{nameof(ChannelProperty.SpikeFilterMax)} did not apply properly on channel '{channel}' (ID: {channel.Id}, Sensor ID: {channel.SensorId})");
                }
            }
            finally
            {
                foreach (var channel in channels)
                    client.SetChannelProperty(channel, ChannelProperty.SpikeFilterEnabled, channel.SpikeFilterEnabled);
            }
        }

        private async Task TestNoFactorManualAsync(int[] sensorIds, int channelId)
        {
            var newValue = true;

            var channels = new List<Channel>();

            foreach (var sensorId in sensorIds)
            {
                var channel = await client.GetChannelAsync(sensorId, channelId);
                channels.Add(channel);

                AssertEx.AreNotEqual(newValue, channel.SpikeFilterEnabled, $"{nameof(ChannelProperty.SpikeFilterEnabled)} is already true on channel '{channel}' (ID: {channel.Id}, Sensor ID: {channel.SensorId}). Cannot execute test");
            }

            try
            {
                await client.SetChannelPropertyAsync(sensorIds, channelId, ChannelProperty.SpikeFilterEnabled, newValue);

                var newChannels = sensorIds.Select(sensorId => client.GetChannel(sensorId, channelId)).ToList();

                foreach (var channel in newChannels)
                {
                    Assert.AreEqual(true, channel.SpikeFilterEnabled, $"{nameof(ChannelProperty.SpikeFilterMax)} did not apply properly on channel '{channel}' (ID: {channel.Id}, Sensor ID: {channel.SensorId})");
                }
            }
            finally
            {
                foreach (var channel in channels)
                    await client.SetChannelPropertyAsync(channel, ChannelProperty.SpikeFilterEnabled, channel.SpikeFilterEnabled);
            }
        }

                #endregion
            #endregion
            #region Manual (Factor)
                #region ManualFactor: Same Channel/Sensor

        [TestMethod]
        [IntegrationTest]
        public void Action_SetChannelProperty_ManualFactor_SameID_AndSensor_ExecutesSingleRequest()
        {
            TestFactorManual(new[] {WellKnownObjectId.CoreSystemHealth_1001, WellKnownObjectId.CoreSystemHealth_1001}, 3);
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Action_SetChannelProperty_ManualFactor_SameID_AndSensor_ExecutesSingleRequestAsync()
        {
            await TestFactorManualAsync(new[] {WellKnownObjectId.CoreSystemHealth_1001, WellKnownObjectId.CoreSystemHealth_1001}, 3);
        }

                #endregion
                #region ManualFactor: Same Channel, Different Sensors

        [TestMethod]
        [IntegrationTest]
        public void Action_SetChannelProperty_ManualFactor_SameID_DifferentSensors_SingleFactor_ExecutesSingleRequest()
        {
            TestFactorManual(new[] {WellKnownObjectId.CoreSystemHealth_1001, WellKnownObjectId.CoreHealth_1002 }, 3);
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Action_SetChannelProperty_ManualFactor_SameID_DifferentSensors_SingleFactor_ExecutesSingleRequestAsync()
        {
            await TestFactorManualAsync(new[] {WellKnownObjectId.CoreSystemHealth_1001, WellKnownObjectId.CoreHealth_1002 }, 3);
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_SetChannelProperty_ManualFactor_SameID_DifferentSensors_SingleFactor_MultipleFactorProperties_ExecutesSingleRequest()
        {
            TestFactorsManual(
                new[] { WellKnownObjectId.CoreSystemHealth_1001, WellKnownObjectId.CoreHealth_1002 },
                3,
                new ChannelParameter(ChannelProperty.UpperErrorLimit, 100),
                new ChannelParameter(ChannelProperty.LowerErrorLimit, 50)
            );
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Action_SetChannelProperty_ManualFactor_SameID_DifferentSensors_SingleFactor_MultipleFactorProperties_ExecutesSingleRequestAsync()
        {
            await TestFactorsManualAsync(
                new[] { WellKnownObjectId.CoreSystemHealth_1001, WellKnownObjectId.CoreHealth_1002 },
                3,
                new ChannelParameter(ChannelProperty.UpperErrorLimit, 100),
                new ChannelParameter(ChannelProperty.LowerErrorLimit, 50)
            );
        }

                #endregion
            #endregion
            #region Channel (No Factor)
                #region Channel: Same Channel/Sensor

        [TestMethod]
        [IntegrationTest]
        public void Action_SetChannelProperty_Channel_SameID_AndSensor_ExecutesSingleRequest()
        {
            var channel1 = client.GetChannel(Settings.ChannelSensor, Settings.Channel);
            var channel2 = client.GetChannel(Settings.ChannelSensor, Settings.Channel);

            TestNoFactor(channel1, channel2);
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Action_SetChannelProperty_Channel_SameID_AndSensor_ExecutesSingleRequestAsync()
        {
            var channel1 = await client.GetChannelAsync(Settings.ChannelSensor, Settings.Channel);
            var channel2 = await client.GetChannelAsync(Settings.ChannelSensor, Settings.Channel);

            await TestNoFactorAsync(channel1, channel2);
        }

                #endregion
                #region Channel: Different Channels, Same Sensor

        [TestMethod]
        [IntegrationTest]
        public void Action_SetChannelProperty_Channel_DifferentIDs_SameSensor_ExecutesSingleRequest()
        {
            var channels = client.GetChannels(Settings.ChannelSensor).Take(2).ToArray();
            AssertEx.AreEqual(2, channels.Length, "Could not retrieve desired number of channels");
            AssertEx.AreNotEqual(channels[0].Id, channels[1].Id, "Both channels had the same ID");

            TestNoFactor(channels);
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Action_SetChannelProperty_Channel_DifferentIDs_SameSensor_ExecutesSingleRequestAsync()
        {
            var channels = (await client.GetChannelsAsync(Settings.ChannelSensor)).Take(2).ToArray();
            AssertEx.AreEqual(2, channels.Length, "Could not retrieve desired number of channels");
            AssertEx.AreNotEqual(channels[0].Id, channels[1].Id, "Both channels had the same ID");

            await TestNoFactorAsync(channels);
        }

                #endregion
                #region Channel: Same Channel, Different Sensors

        [TestMethod]
        [IntegrationTest]
        public void Action_SetChannelProperty_Channel_SameID_DifferentSensors_ExecutesSingleRequest()
        {
            var channel1 = client.GetChannel(Settings.UpSensor, 0);
            var channel2 = client.GetChannel(Settings.ChannelSensor, 0);

            TestNoFactor(channel1, channel2);
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Action_SetChannelProperty_Channel_SameID_DifferentSensors_ExecutesSingleRequestAsync()
        {
            var channel1 = await client.GetChannelAsync(Settings.UpSensor, 0);
            var channel2 = await client.GetChannelAsync(Settings.ChannelSensor, 0);

            await TestNoFactorAsync(channel1, channel2);
        }

                #endregion
                #region Channel: Different Channels, Different Sensors

        [TestMethod]
        [IntegrationTest]
        public void Action_SetChannelProperty_Channel_DifferentIDs_DifferentSensors_ExecutesMultipleRequests()
        {
            var channel1 = client.GetChannel(Settings.UpSensor, 0);
            var channel2 = client.GetChannel(Settings.ChannelSensor, 1);

            TestNoFactor(channel1, channel2);
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Action_SetChannelProperty_Channel_DifferentIDs_DifferentSensors_ExecutesMultipleRequestsAsync()
        {
            var channel1 = await client.GetChannelAsync(Settings.UpSensor, 0);
            var channel2 = await client.GetChannelAsync(Settings.ChannelSensor, 1);

            await TestNoFactorAsync(channel1, channel2);
        }

                #endregion

        private void TestNoFactor(params Channel[] channels)
        {
            var newValue = true;

            foreach (var channel in channels)
            {
                AssertEx.AreNotEqual(newValue, channel.SpikeFilterEnabled, $"{nameof(ChannelProperty.SpikeFilterEnabled)} is already true on channel '{channel}' (ID: {channel.Id}, Sensor ID: {channel.SensorId}). Cannot execute test");
            }

            try
            {
                client.SetChannelProperty(channels, ChannelProperty.SpikeFilterEnabled, newValue);

                var newChannels = channels.Select(c => client.GetChannel(c.SensorId, c.Id)).ToList();

                foreach (var channel in newChannels)
                {
                    Assert.AreEqual(true, channel.SpikeFilterEnabled, $"{nameof(ChannelProperty.SpikeFilterMax)} did not apply properly on channel '{channel}' (ID: {channel.Id}, Sensor ID: {channel.SensorId})");
                }
            }
            finally
            {
                foreach (var channel in channels)
                    client.SetChannelProperty(channel, ChannelProperty.SpikeFilterEnabled, channel.SpikeFilterEnabled);
            }
        }

        private async Task TestNoFactorAsync(params Channel[] channels)
        {
            foreach (var channel in channels)
                SetFactors(channel, null);

            var newValue = true;

            foreach (var channel in channels)
            {
                AssertEx.AreNotEqual(newValue, channel.SpikeFilterEnabled, $"{nameof(ChannelProperty.SpikeFilterEnabled)} is already true on channel '{channel}' (ID: {channel.Id}, Sensor ID: {channel.SensorId}). Cannot execute test");
            }

            try
            {
                await client.SetChannelPropertyAsync(channels, ChannelProperty.SpikeFilterEnabled, newValue);

                var newChannels = channels.Select(c => client.GetChannel(c.SensorId, c.Id)).ToList();

                foreach (var channel in newChannels)
                {
                    Assert.AreEqual(true, channel.SpikeFilterEnabled, $"{nameof(ChannelProperty.SpikeFilterMax)} did not apply properly on channel '{channel}' (ID: {channel.Id}, Sensor ID: {channel.SensorId})");
                }
            }
            finally
            {
                foreach (var channel in channels)
                    await client.SetChannelPropertyAsync(channel, ChannelProperty.SpikeFilterEnabled, channel.SpikeFilterEnabled);
            }
        }

            #endregion
            #region Channel (Factor)
                #region ChannelFactor: Same Channel/Sensor

        [TestMethod]
        [IntegrationTest]
        public void Action_SetChannelProperty_ChannelFactor_SameID_AndSensor_ExecutesSingleRequest()
        {
            var channel1 = client.GetChannel(WellKnownObjectId.CoreSystemHealth_1001, 3);
            var channel2 = client.GetChannel(WellKnownObjectId.CoreSystemHealth_1001, 3);

            TestFactor(channel1, channel2);
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Action_SetChannelProperty_ChannelFactor_SameID_AndSensor_ExecutesSingleRequestAsync()
        {
            var channel1 = await client.GetChannelAsync(WellKnownObjectId.CoreSystemHealth_1001, 3); //Available Memory
            var channel2 = await client.GetChannelAsync(WellKnownObjectId.CoreSystemHealth_1001, 3); //Available Memory

            await TestFactorAsync(channel1, channel2);
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_SetChannelProperty_ChannelFactor_SameID_SameSensor_NeedsLimit()
        {
            var channel1 = client.GetChannel(WellKnownObjectId.CoreHealth_1002, 10); //Age of Code
            Assert.IsNotNull(channel1.UpperWarningLimit, "Channel1 is missing an UpperWarningLimit");

            var channel2 = client.GetChannel(WellKnownObjectId.CoreHealth_1002, 10); //Age of Code
            Assert.IsNotNull(channel2.UpperWarningLimit, "Channel2 is missing an UpperWarningLimit");

            TestFactors(
                new[] { channel1, channel2 },
                new[] { new ChannelParameter(ChannelProperty.ErrorLimitMessage, "test") },
                true
            );
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Action_SetChannelProperty_ChannelFactor_SameID_SameSensor_NeedsLimitAsync()
        {
            var channel1 = await client.GetChannelAsync(WellKnownObjectId.CoreHealth_1002, 10); //Age of Code
            Assert.IsNotNull(channel1.UpperWarningLimit, "Channel1 is missing an UpperWarningLimit");

            var channel2 = await client.GetChannelAsync(WellKnownObjectId.CoreHealth_1002, 10); //Age of Code
            Assert.IsNotNull(channel2.UpperWarningLimit, "Channel2 is missing an UpperWarningLimit");

            await TestFactorsAsync(
                new[] { channel1, channel2 },
                new[] { new ChannelParameter(ChannelProperty.ErrorLimitMessage, "test") },
                true
            );
        }

                #endregion
                #region ChannelFactor: Different Channels, Same Sensor

        [TestMethod]
        [IntegrationTest]
        public void Action_SetChannelProperty_ChannelFactor_DifferentIDs_SameSensor_ExecutesSingleRequest()
        {
            var channel1 = client.GetChannel(WellKnownObjectId.CoreProbeHealth_1003, 9); //Memory Usage
            var channel2 = client.GetChannel(WellKnownObjectId.CoreProbeHealth_1003, 1); //Message Queue

            Assert.IsNotNull(channel1.Factor, $"Channel {channel1} had a null factor.");
            Assert.IsNotNull(channel2.Factor, $"Channel {channel1} had a null factor.");
            AssertEx.AreNotEqual(channel1.Factor, channel2.Factor, "Both channel factors were the same.");

            TestFactorSafe(channel1, channel2);
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Action_SetChannelProperty_ChannelFactor_DifferentIDs_SameSensor_ExecutesSingleRequestAsync()
        {
            var channel1 = await client.GetChannelAsync(WellKnownObjectId.CoreProbeHealth_1003, 9); //Memory Usage
            var channel2 = await client.GetChannelAsync(WellKnownObjectId.CoreProbeHealth_1003, 1); //Message Queue

            Assert.IsNotNull(channel1.Factor, $"Channel {channel1} had a null factor.");
            Assert.IsNotNull(channel2.Factor, $"Channel {channel1} had a null factor.");
            AssertEx.AreNotEqual(channel1.Factor, channel2.Factor, "Both channel factors were the same.");

            await TestFactorSafeAsync(channel1, channel2);
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_SetChannelProperty_ChannelFactor_DifferentIDs_SameSensor_NeedsLimit()
        {
            var channel1 = client.GetChannel(WellKnownObjectId.CoreProbeHealth_1003, 9); //Memory Usage
            Assert.IsNotNull(channel1.UpperErrorLimit, "Channel1 is missing an UpperErrorLimit");

            var channel2 = client.GetChannel(WellKnownObjectId.CoreProbeHealth_1003, 2); //Open requests
            Assert.IsNotNull(channel2.UpperErrorLimit, "Channel2 is missing an UpperErrorLimit");
            
            TestFactors(
                new[] { channel1, channel2 },
                new[] { new ChannelParameter(ChannelProperty.ErrorLimitMessage, "test") },
                true
            );
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Action_SetChannelProperty_ChannelFactor_DifferentIDs_SameSensor_NeedsLimitAsync()
        {
            var channel1 = await client.GetChannelAsync(WellKnownObjectId.CoreProbeHealth_1003, 9); //Memory Usage
            Assert.IsNotNull(channel1.UpperErrorLimit, "Channel1 is missing an UpperErrorLimit");

            var channel2 = await client.GetChannelAsync(WellKnownObjectId.CoreProbeHealth_1003, 2); //Open requests
            Assert.IsNotNull(channel2.UpperErrorLimit, "Channel2 is missing an UpperErrorLimit");

            await TestFactorsAsync(
                new[] { channel1, channel2 },
                new[] { new ChannelParameter(ChannelProperty.ErrorLimitMessage, "test") },
                true
            );
        }

                #endregion
                #region ChannelFactor: Same Channel, Different Sensors
                    #region Single Factor / Single Factor Property

        [TestMethod]
        [IntegrationTest]
        public void Action_SetChannelProperty_ChannelFactor_SameID_DifferentSensors_SingleFactor_SingleFactorProperty_ExecutesSingleFactor()
        {
            var channel1 = client.GetChannel(WellKnownObjectId.CoreSystemHealth_1001, 3);
            var channel2 = client.GetChannel(WellKnownObjectId.CoreHealth_1002, 3);

            Assert.IsNotNull(channel1.Factor, $"Channel {channel1} had a null factor.");
            Assert.IsNotNull(channel2.Factor, $"Channel {channel1} had a null factor.");
            AssertEx.AreEqual(channel1.Factor, channel2.Factor, "Both channel factors were not the same.");

            TestFactor(channel1, channel2);
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Action_SetChannelProperty_ChannelFactor_SameID_DifferentSensors_SingleFactor_SingleFactorProperty_ExecutesSingleFactorAsync()
        {
            var channel1 = await client.GetChannelAsync(WellKnownObjectId.CoreSystemHealth_1001, 3);
            var channel2 = await client.GetChannelAsync(WellKnownObjectId.CoreHealth_1002, 3);

            Assert.IsNotNull(channel1.Factor, $"Channel {channel1} had a null factor.");
            Assert.IsNotNull(channel2.Factor, $"Channel {channel1} had a null factor.");
            AssertEx.AreEqual(channel1.Factor, channel2.Factor, "Both channel factors were not the same.");

            await TestFactorAsync(channel1, channel2);
        }

                    #endregion
                    #region Single Factor / Multiple Factor Properties

        [TestMethod]
        [IntegrationTest]
        public void Action_SetChannelProperty_ChannelFactor_SameID_DifferentSensors_SingleFactor_MultipleFactorProperties_ExecutesSingleFactor()
        {
            var channel1 = client.GetChannel(WellKnownObjectId.CoreSystemHealth_1001, 3);
            var channel2 = client.GetChannel(WellKnownObjectId.CoreHealth_1002, 3);

            Assert.IsNotNull(channel1.Factor, $"Channel {channel1} had a null factor.");
            Assert.IsNotNull(channel2.Factor, $"Channel {channel1} had a null factor.");
            AssertEx.AreEqual(channel1.Factor, channel2.Factor, "Both channel factors were not the same.");

            TestFactors(
                new[] {channel1, channel2},
                new ChannelParameter(ChannelProperty.UpperErrorLimit, 100),
                new ChannelParameter(ChannelProperty.LowerErrorLimit, 50)
            );
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Action_SetChannelProperty_ChannelFactor_SameID_DifferentSensors_SingleFactor_MultipleFactorProperties_ExecutesSingleFactorAsync()
        {
            var channel1 = await client.GetChannelAsync(WellKnownObjectId.CoreSystemHealth_1001, 3);
            var channel2 = await client.GetChannelAsync(WellKnownObjectId.CoreHealth_1002, 3);

            Assert.IsNotNull(channel1.Factor, $"Channel {channel1} had a null factor.");
            Assert.IsNotNull(channel2.Factor, $"Channel {channel1} had a null factor.");
            AssertEx.AreEqual(channel1.Factor, channel2.Factor, "Both channel factors were not the same.");

            await TestFactorsSafeAsync(
                new[] { channel1, channel2 },
                new ChannelParameter(ChannelProperty.UpperErrorLimit, 100),
                new ChannelParameter(ChannelProperty.LowerErrorLimit, 50)
            );
        }

                    #endregion
                    #region Multiple Factors / Single Factor Property

        [TestMethod]
        [IntegrationTest]
        public void Action_SetChannelProperty_ChannelFactor_SameID_DifferentSensors_MultipleFactors_SingleFactorProperty_ExecutesMultipleRequests()
        {
            var channel1 = client.GetChannel(WellKnownObjectId.CoreHealth_1002, 1);
            var channel2 = client.GetChannel(WellKnownObjectId.CoreProbeHealth_1003, 1);

            Assert.IsNotNull(channel1.Factor, $"Channel {channel1} had a null factor.");
            Assert.IsNotNull(channel2.Factor, $"Channel {channel1} had a null factor.");
            AssertEx.AreNotEqual(channel1.Factor, channel2.Factor, "Both channel factors were the same.");

            TestFactor(channel1, channel2);
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Action_SetChannelProperty_ChannelFactor_SameID_DifferentSensors_MultipleFactors_SingleFactorProperty_ExecutesMultipleRequestsAsync()
        {
            var channel1 = await client.GetChannelAsync(WellKnownObjectId.CoreHealth_1002, 1);
            var channel2 = await client.GetChannelAsync(WellKnownObjectId.CoreProbeHealth_1003, 1);

            Assert.IsNotNull(channel1.Factor, $"Channel {channel1} had a null factor.");
            Assert.IsNotNull(channel2.Factor, $"Channel {channel1} had a null factor.");
            AssertEx.AreNotEqual(channel1.Factor, channel2.Factor, "Both channel factors were the same.");

            await TestFactorAsync(channel1, channel2);
        }

                    #endregion
                    #region Multiple Factors / Multiple Factor Properties

        [TestMethod]
        [IntegrationTest]
        public void Action_SetChannelProperty_ChannelFactor_SameID_DifferentSensors_MultipleFactors_MultipleFactorProperties_ExecutesMultipleRequests()
        {
            var channel1 = client.GetChannel(WellKnownObjectId.CoreHealth_1002, 1);
            var channel2 = client.GetChannel(WellKnownObjectId.CoreProbeHealth_1003, 1);

            Assert.IsNotNull(channel1.Factor, $"Channel {channel1} had a null factor.");
            Assert.IsNotNull(channel2.Factor, $"Channel {channel1} had a null factor.");
            AssertEx.AreNotEqual(channel1.Factor, channel2.Factor, "Both channel factors were the same.");

            TestFactors(
                new[] {channel1, channel2},
                new ChannelParameter(ChannelProperty.UpperErrorLimit, 100),
                new ChannelParameter(ChannelProperty.LowerErrorLimit, 50)
             );
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Action_SetChannelProperty_ChannelFactor_SameID_DifferentSensors_MultipleFactors_MultipleFactorProperties_ExecutesMultipleRequestsAsync()
        {
            var channel1 = await client.GetChannelAsync(WellKnownObjectId.CoreHealth_1002, 1);
            var channel2 = await client.GetChannelAsync(WellKnownObjectId.CoreProbeHealth_1003, 1);

            Assert.IsNotNull(channel1.Factor, $"Channel {channel1} had a null factor.");
            Assert.IsNotNull(channel2.Factor, $"Channel {channel1} had a null factor.");
            AssertEx.AreNotEqual(channel1.Factor, channel2.Factor, "Both channel factors were the same.");

            await TestFactorsAsync(
                new[] { channel1, channel2 },
                new ChannelParameter(ChannelProperty.UpperErrorLimit, 100),
                new ChannelParameter(ChannelProperty.LowerErrorLimit, 50)
            );
        }

                    #endregion
                    #region Multiple Factors / ValueNullOrEmpty

        [TestMethod]
        [IntegrationTest]
        public void Action_SetChannelProperty_ChannelFactor_SameID_DifferentSensors_MultipleFactors_ValueNullOrEmpty_ExecutesSingleRequest()
        {
            var channel1 = client.GetChannel(WellKnownObjectId.CoreHealth_1002, 1);
            var channel2 = client.GetChannel(WellKnownObjectId.CoreProbeHealth_1003, 1);

            Assert.IsNotNull(channel1.Factor, $"Channel {channel1} had a null factor.");
            Assert.IsNotNull(channel2.Factor, $"Channel {channel1} had a null factor.");
            AssertEx.AreNotEqual(channel1.Factor, channel2.Factor, "Both channel factors were the same.");

            TestFactors(
                new[] { channel1, channel2 },
                new ChannelParameter(ChannelProperty.UpperErrorLimit, null)
            );
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Action_SetChannelProperty_ChannelFactor_SameID_DifferentSensors_MultipleFactors_ValueNullOrEmpty_ExecutesSingleRequestAsync()
        {
            var channel1 = await client.GetChannelAsync(WellKnownObjectId.CoreHealth_1002, 1);
            var channel2 = await client.GetChannelAsync(WellKnownObjectId.CoreProbeHealth_1003, 1);

            Assert.IsNotNull(channel1.Factor, $"Channel {channel1} had a null factor.");
            Assert.IsNotNull(channel2.Factor, $"Channel {channel1} had a null factor.");
            AssertEx.AreNotEqual(channel1.Factor, channel2.Factor, "Both channel factors were the same.");

            await TestFactorsAsync(
                new[] { channel1, channel2 },
                new ChannelParameter(ChannelProperty.UpperErrorLimit, null)
            );
        }

                    #endregion

        [TestMethod]
        [IntegrationTest]
        public void Action_SetChannelProperty_ChannelFactor_SameID_DifferentSensors_NeedsLimit()
        {
            var channel1 = client.GetChannel(WellKnownObjectId.CoreHealth_1002, 9);
            Assert.IsNotNull(channel1.LowerWarningLimit, "Channel1 is missing an LowerWarningLimit");

            var channel2 = client.GetChannel(WellKnownObjectId.CoreProbeHealth_1003, 9);
            Assert.IsNotNull(channel2.UpperWarningLimit, "Channel2 is missing an UpperWarningLimit");

            TestFactors(
                new[] { channel1, channel2 },
                new[] { new ChannelParameter(ChannelProperty.ErrorLimitMessage, "test") },
                true
            );
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Action_SetChannelProperty_ChannelFactor_SameID_DifferentSensors_NeedsLimitAsync()
        {
            var channel1 = await client.GetChannelAsync(WellKnownObjectId.CoreHealth_1002, 9);
            Assert.IsNotNull(channel1.LowerWarningLimit, "Channel1 is missing an LowerWarningLimit");

            var channel2 = await client.GetChannelAsync(WellKnownObjectId.CoreProbeHealth_1003, 9);
            Assert.IsNotNull(channel2.UpperWarningLimit, "Channel2 is missing an UpperWarningLimit");

            await TestFactorsAsync(
                new[] { channel1, channel2 },
                new[] { new ChannelParameter(ChannelProperty.ErrorLimitMessage, "test") },
                true
            );
        }

                #endregion
                #region ChannelFactor: Different Channels, Different Sensors

        [TestMethod]
        [IntegrationTest]
        public void Action_SetChannelProperty_ChannelFactor_DifferentIDs_DifferentSensors_ExecutesMultipleRequests()
        {
            var channel1 = client.GetChannel(WellKnownObjectId.CoreSystemHealth_1001, 3);
            var channel2 = client.GetChannel(WellKnownObjectId.CoreProbeHealth_1003, 1);

            Assert.IsNotNull(channel1.Factor, $"Channel {channel1} had a null factor.");
            Assert.IsNotNull(channel2.Factor, $"Channel {channel1} had a null factor.");
            AssertEx.AreNotEqual(channel1.Factor, channel2.Factor, "Both channel factors were the same.");

            TestFactorSafe(channel1, channel2);
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Action_SetChannelProperty_ChannelFactor_DifferentIDs_DifferentSensors_ExecutesMultipleRequestsAsync()
        {
            var channel1 = await client.GetChannelAsync(WellKnownObjectId.CoreSystemHealth_1001, 3);
            var channel2 = await client.GetChannelAsync(WellKnownObjectId.CoreProbeHealth_1003, 1);

            Assert.IsNotNull(channel1.Factor, $"Channel {channel1} had a null factor.");
            Assert.IsNotNull(channel2.Factor, $"Channel {channel1} had a null factor.");
            AssertEx.AreNotEqual(channel1.Factor, channel2.Factor, "Both channel factors were the same.");

            await TestFactorSafeAsync(channel1, channel2);
        }

        [TestMethod]
        [IntegrationTest]
        public void Action_SetChannelProperty_ChannelFactor_DifferentIDs_DifferentSensors_NeedsLimit()
        {
            var channel1 = client.GetChannel(WellKnownObjectId.CoreHealth_1002, 10); //Age of Code
            Assert.IsNotNull(channel1.UpperWarningLimit, "Channel1 is missing an UpperWarningLimit");

            var channel2 = client.GetChannel(WellKnownObjectId.CoreProbeHealth_1003, 2); //Open Requests
            Assert.IsNotNull(channel2.UpperErrorLimit, "Channel2 is missing an UpperErrorLimit");

            TestFactors(
                new[] {channel1, channel2},
                new[] {new ChannelParameter(ChannelProperty.ErrorLimitMessage, "test")},
                true
            );
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Action_SetChannelProperty_ChannelFactor_DifferentIDs_DifferentSensors_NeedsLimitAsync()
        {
            var channel1 = await client.GetChannelAsync(WellKnownObjectId.CoreHealth_1002, 10); //Age of Code
            Assert.IsNotNull(channel1.UpperWarningLimit, "Channel1 is missing an UpperWarningLimit");

            var channel2 = await client.GetChannelAsync(WellKnownObjectId.CoreProbeHealth_1003, 2); //Open Requests
            Assert.IsNotNull(channel2.UpperErrorLimit, "Channel2 is missing an UpperErrorLimit");

            await TestFactorsAsync(
                new[] { channel1, channel2 },
                new[] { new ChannelParameter(ChannelProperty.ErrorLimitMessage, "test") },
                true
            );
        }

                #endregion

        private void TestFactorManual(int[] sensorIds, int channelId)
        {
            TestFactorsManual(sensorIds, channelId, new ChannelParameter(ChannelProperty.UpperErrorLimit, 100));
        }

        private void TestFactorsManual(int[] sensorIds, int channelId, params ChannelParameter[] parameters)
        {
            var channels = sensorIds.Select(s => client.GetChannel(s, channelId)).ToList();

            foreach (var sensorId in sensorIds)
            {
                var channel = client.GetChannel(sensorId, channelId);

                Assert.IsNotNull(channel.Factor, $"Cannot use channel with factor 'null': must test with a factor that is not null");
                AssertEx.AreNotEqual(true, channel.LimitsEnabled, $"{nameof(ChannelProperty.LimitsEnabled)} is already true on channel '{channel}' (ID: {channel.Id}, Sensor ID: {channel.SensorId}). Cannot execute test");

                foreach (var parameter in parameters)
                {
                    var current = channel.GetType().GetProperty(parameter.Property.ToString()).GetValue(channel);
                    AssertEx.AreEqual(null, current, $"{parameter.Property} is not null on channel '{channel}' (ID: {channel.Id}, Sensor ID: {channel.SensorId}). Cannot execute test");
                }
            }

            try
            {
                client.SetChannelProperty(sensorIds, channelId, parameters);

                var newChannels = sensorIds.Select(s => client.GetChannel(s, channelId)).ToList();

                foreach (var channel in newChannels)
                {
                    Assert.AreEqual(true, channel.LimitsEnabled, $"{nameof(ChannelProperty.UpperErrorLimit)} did not apply properly on channel '{channel}' (ID: {channel.Id}, Sensor ID: {channel.SensorId})");

                    foreach (var parameter in parameters)
                    {
                        var value = channel.GetType().GetProperty(parameter.Property.ToString()).GetValue(channel);
                        Assert.AreEqual(Convert.ToDouble(parameter.Value), value, $"{parameter.Property} did not apply properly on channel '{channel}' (ID: {channel.Id}, Sensor ID: {channel.SensorId})");
                    }
                }
            }
            finally
            {
                foreach (var channel in channels)
                {
                    client.SetChannelProperty(channel, ChannelProperty.LimitsEnabled, channel.LimitsEnabled);
                }
            }
        }

        private async Task TestFactorManualAsync(int[] sensorIds, int channelId)
        {
            await TestFactorsManualAsync(sensorIds, channelId, new ChannelParameter(ChannelProperty.UpperErrorLimit, 100));
        }

        private async Task TestFactorsManualAsync(int[] sensorIds, int channelId, params ChannelParameter[] parameters)
        {
            var channels = sensorIds.Select(s => client.GetChannel(s, channelId)).ToList();

            foreach (var sensorId in sensorIds)
            {
                var channel = await client.GetChannelAsync(sensorId, channelId);

                Assert.IsNotNull(channel.Factor, $"Cannot use channel with factor 'null': must test with a factor that is not null");
                AssertEx.AreNotEqual(true, channel.LimitsEnabled, $"{nameof(ChannelProperty.LimitsEnabled)} is already true on channel '{channel}' (ID: {channel.Id}, Sensor ID: {channel.SensorId}). Cannot execute test");

                foreach (var parameter in parameters)
                {
                    var current = channel.GetType().GetProperty(parameter.Property.ToString()).GetValue(channel);
                    AssertEx.AreEqual(null, current, $"{parameter.Property} is not null on channel '{channel}' (ID: {channel.Id}, Sensor ID: {channel.SensorId}). Cannot execute test");
                }
            }

            try
            {
                await client.SetChannelPropertyAsync(sensorIds, channelId, parameters);

                var newChannels = sensorIds.Select(s => client.GetChannel(s, channelId)).ToList();

                foreach (var channel in newChannels)
                {
                    Assert.AreEqual(true, channel.LimitsEnabled, $"{nameof(ChannelProperty.UpperErrorLimit)} did not apply properly on channel '{channel}' (ID: {channel.Id}, Sensor ID: {channel.SensorId})");

                    foreach (var parameter in parameters)
                    {
                        var value = channel.GetType().GetProperty(parameter.Property.ToString()).GetValue(channel);
                        Assert.AreEqual(Convert.ToDouble(parameter.Value), value, $"{parameter.Property} did not apply properly on channel '{channel}' (ID: {channel.Id}, Sensor ID: {channel.SensorId})");
                    }
                }
            }
            finally
            {
                foreach (var channel in channels)
                {
                    await client.SetChannelPropertyAsync(channel, ChannelProperty.LimitsEnabled, channel.LimitsEnabled);
                }
            }
        }

        private void TestFactor(params Channel[] channels)
        {
            TestFactors(channels, new ChannelParameter(ChannelProperty.UpperErrorLimit, 100));
        }

        private void TestFactors(Channel[] channels, params ChannelParameter[] parameters) =>
            TestFactors(channels, parameters, false);

        private void TestFactors(Channel[] channels, ChannelParameter[] parameters, bool ignoreCurrent)
        {
            foreach (var channel in channels)
            {
                Assert.IsNotNull(channel.Factor, $"Cannot use channel with factor 'null': must test with a factor that is not null");

                if (!ignoreCurrent)
                {
                    AssertEx.AreNotEqual(true, channel.LimitsEnabled, $"{nameof(ChannelProperty.LimitsEnabled)} is already true on channel '{channel}' (ID: {channel.Id}, Sensor ID: {channel.SensorId}). Cannot execute test");

                    foreach (var parameter in parameters)
                    {
                        var current = channel.GetType().GetProperty(parameter.Property.ToString()).GetValue(channel);
                        AssertEx.AreEqual(null, current, $"{parameter.Property} is not null on channel '{channel}' (ID: {channel.Id}, Sensor ID: {channel.SensorId}). Cannot execute test");
                    }
                }
            }

            try
            {
                client.SetChannelProperty(channels, parameters);

                var newChannels = channels.Select(c => client.GetChannel(c.SensorId, c.Id)).ToList();

                foreach (var channel in newChannels)
                {
                    var upperErrorLimitProperty = parameters.FirstOrDefault(p => p.Property == ChannelProperty.UpperErrorLimit);

                    if (upperErrorLimitProperty != null && upperErrorLimitProperty.Value != null)
                        Assert.AreEqual(true, channel.LimitsEnabled, $"{nameof(ChannelProperty.UpperErrorLimit)} did not apply properly on channel '{channel}' (ID: {channel.Id}, Sensor ID: {channel.SensorId})");

                    foreach (var parameter in parameters)
                    {
                        var value = channel.GetType().GetProperty(parameter.Property.ToString()).GetValue(channel);

                        if (parameter.Value == null)
                            Assert.IsNull(value, $"{parameter.Property} did not apply properly on channel '{channel}' (ID: {channel.Id}, Sensor ID: {channel.SensorId})");
                        else
                            Assert.AreEqual(parameter.Value.ToString(), value.ToString(), $"{parameter.Property} did not apply properly on channel '{channel}' (ID: {channel.Id}, Sensor ID: {channel.SensorId})");
                    }
                }
            }
            finally
            {
                foreach (var channel in channels)
                {
                    client.SetChannelProperty(
                        channel,
                        new ChannelParameter(ChannelProperty.LimitsEnabled, channel.LimitsEnabled),
                        new ChannelParameter(ChannelProperty.UpperErrorLimit, channel.UpperErrorLimit),
                        new ChannelParameter(ChannelProperty.LowerErrorLimit, channel.LowerErrorLimit),
                        new ChannelParameter(ChannelProperty.UpperWarningLimit, channel.UpperWarningLimit),
                        new ChannelParameter(ChannelProperty.LowerWarningLimit, channel.LowerWarningLimit),
                        new ChannelParameter(ChannelProperty.ErrorLimitMessage, channel.ErrorLimitMessage),
                        new ChannelParameter(ChannelProperty.WarningLimitMessage, channel.WarningLimitMessage)
                   );
                }
            }
        }

        private void TestFactorSafe(params Channel[] channels)
        {
            TestFactorsSafe(channels, new ChannelParameter(ChannelProperty.UpperErrorLimit, 100));
        }

        private void TestFactorsSafe(Channel[] channels, params ChannelParameter[] parameters)
        {
            try
            {
                var newChannels = channels.Select(c =>
                {
                    client.SetChannelProperty(c, ChannelProperty.LimitsEnabled, false);

                    return client.GetChannel(c.SensorId, c.Id);
                }).ToArray();

                TestFactors(newChannels, parameters);
            }
            finally
            {
                foreach (var channel in channels)
                {
                    client.SetChannelProperty(
                        channel,
                        new ChannelParameter(ChannelProperty.LimitsEnabled, channel.LimitsEnabled),
                        new ChannelParameter(ChannelProperty.UpperErrorLimit, channel.UpperErrorLimit),
                        new ChannelParameter(ChannelProperty.LowerErrorLimit, channel.LowerErrorLimit),
                        new ChannelParameter(ChannelProperty.UpperWarningLimit, channel.UpperWarningLimit),
                        new ChannelParameter(ChannelProperty.LowerWarningLimit, channel.LowerWarningLimit)
                    );
                }
            }
        }

        private async Task TestFactorAsync(params Channel[] channels)
        {
            await TestFactorsAsync(channels, new ChannelParameter(ChannelProperty.UpperErrorLimit, 100));
        }

        private async Task TestFactorsAsync(Channel[] channels, params ChannelParameter[] parameters) =>
            await TestFactorsAsync(channels, parameters, false);

        private async Task TestFactorsAsync(Channel[] channels, ChannelParameter[] parameters, bool ignoreCurrent)
        {
            foreach (var channel in channels)
            {
                if (!ignoreCurrent)
                {
                    Assert.IsNotNull(channel.Factor, $"Cannot use channel with factor 'null': must test with a factor that is not null");
                    AssertEx.AreNotEqual(true, channel.LimitsEnabled, $"{nameof(ChannelProperty.LimitsEnabled)} is already true on channel '{channel}' (ID: {channel.Id}, Sensor ID: {channel.SensorId}). Cannot execute test");

                    foreach (var parameter in parameters)
                    {
                        var current = channel.GetType().GetProperty(parameter.Property.ToString()).GetValue(channel);
                        AssertEx.AreEqual(null, current, $"{parameter.Property} is not null on channel '{channel}' (ID: {channel.Id}, Sensor ID: {channel.SensorId}). Cannot execute test");
                    }
                }
            }

            try
            {
                await client.SetChannelPropertyAsync(channels, parameters);

                var newChannels = channels.Select(c => client.GetChannel(c.SensorId, c.Id)).ToList();

                foreach (var channel in newChannels)
                {
                    var upperErrorLimitProperty = parameters.FirstOrDefault(p => p.Property == ChannelProperty.UpperErrorLimit);

                    if (upperErrorLimitProperty != null && upperErrorLimitProperty.Value != null)
                        Assert.AreEqual(true, channel.LimitsEnabled, $"{nameof(ChannelProperty.UpperErrorLimit)} did not apply properly on channel '{channel}' (ID: {channel.Id}, Sensor ID: {channel.SensorId})");

                    foreach (var parameter in parameters)
                    {
                        var value = channel.GetType().GetProperty(parameter.Property.ToString()).GetValue(channel);

                        if (parameter.Value == null)
                            Assert.IsNull(value, $"{parameter.Property} did not apply properly on channel '{channel}' (ID: {channel.Id}, Sensor ID: {channel.SensorId})");
                        else
                            Assert.AreEqual(parameter.Value.ToString(), value.ToString(), $"{parameter.Property} did not apply properly on channel '{channel}' (ID: {channel.Id}, Sensor ID: {channel.SensorId})");
                    }
                }
            }
            finally
            {
                foreach (var channel in channels)
                {
                    await client.SetChannelPropertyAsync(channel, ChannelProperty.LimitsEnabled, channel.LimitsEnabled);
                }
            }
        }

        private async Task TestFactorSafeAsync(params Channel[] channels)
        {
            await TestFactorsSafeAsync(channels, new ChannelParameter(ChannelProperty.UpperErrorLimit, 100));
        }

        private async Task TestFactorsSafeAsync(Channel[] channels, params ChannelParameter[] parameters)
        {
            try
            {
                var newChannels = channels.Select(c =>
                {
                    client.SetChannelProperty(c, ChannelProperty.LimitsEnabled, false);

                    return client.GetChannel(c.SensorId, c.Id);
                }).ToArray();

                await TestFactorsAsync(newChannels, parameters);
            }
            finally
            {
                foreach (var channel in channels)
                {
                    await client.SetChannelPropertyAsync(
                        channel,
                        new ChannelParameter(ChannelProperty.LimitsEnabled, channel.LimitsEnabled),
                        new ChannelParameter(ChannelProperty.UpperErrorLimit, channel.UpperErrorLimit),
                        new ChannelParameter(ChannelProperty.LowerErrorLimit, channel.LowerErrorLimit),
                        new ChannelParameter(ChannelProperty.UpperWarningLimit, channel.UpperWarningLimit),
                        new ChannelParameter(ChannelProperty.LowerWarningLimit, channel.LowerWarningLimit)
                    );
                }
            }
        }

            #endregion

        private void SetFactors(Channel channel, double? value)
        {
            var factors = typeof(Channel).GetProperties(BindingFlags.Instance | BindingFlags.NonPublic).Where(p => p.Name.EndsWith("Factor") && p.Name != nameof(Channel.Factor));

            foreach (var property in factors)
            {
                property.SetValue(channel, value);
            }
        }

        #endregion

        [TestMethod]
        [IntegrationTest]
        public void Action_SetChannelProperty_HasAllTests()
        {
            var prefixes = new[]
            {
                "SetChannelProperty_Manual",
                "SetChannelProperty_ManualFactor",
                "SetChannelProperty_Channel",
                "SetChannelProperty_ChannelFactor"
            };

            var expected = TestHelpers.GetTests(typeof(UnitTests.ObjectManipulation.SetObjectPropertyTests))
                .Where(m => prefixes.Any(p => m.Name.StartsWith(p)))
                .Select(m => $"Action_{m.Name}".Replace("_v14", "").Replace("_v18", ""))
                .Distinct()
                .ToList();

            TestHelpers.Assert_TestClassHasMethods(GetType(), expected);
        }
    }
}
