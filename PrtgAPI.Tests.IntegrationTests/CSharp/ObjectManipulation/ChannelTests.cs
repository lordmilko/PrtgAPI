using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.IntegrationTests
{
    [TestClass]
    public class ChannelTests : BasePrtgClientTest
    {
        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Action_Channel_SetChannelProperty_Limits()
        {
            SetAndRevertChannelProperty(3000, ChannelProperty.UpperErrorLimit, c => c.UpperErrorLimit);
            SetAndRevertChannelProperty(4000, ChannelProperty.LowerErrorLimit, c => c.LowerErrorLimit);
            SetAndRevertChannelProperty(5000, ChannelProperty.UpperWarningLimit, c => c.UpperWarningLimit);
            SetAndRevertChannelProperty(6000, ChannelProperty.LowerWarningLimit, c => c.LowerWarningLimit);
            SetAndRevertChannelProperty("testMessage", ChannelProperty.ErrorLimitMessage, c => c.ErrorLimitMessage);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
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

            client.SetObjectProperty(Settings.ChannelSensor, Settings.Channel, property, newValue);
            var newChannel = client.GetChannels(Settings.ChannelSensor).First(c => c.Id == Settings.Channel);
            AssertEx.AreEqual(newValue, getProperty(newChannel), "New channel value did not apply properly");

            client.SetObjectProperty(Settings.ChannelSensor, Settings.Channel, property, getProperty(initialChannel));
            var finalChannel = client.GetChannels(Settings.ChannelSensor).First(c => c.Id == Settings.Channel);
            AssertEx.AreEqual(getProperty(initialChannel), getProperty(finalChannel), "Channel value did not revert properly");
        }

        private async Task SetAndRevertChannelPropertyAsync<T>(T newValue, ChannelProperty property, Func<Channel, T> getProperty)
        {
            var initialChannel = (await client.GetChannelsAsync(Settings.ChannelSensor)).First(c => c.Id == Settings.Channel);
            AssertEx.AreNotEqual(getProperty(initialChannel), newValue, "Initial channel value was not expected value");

            await client.SetObjectPropertyAsync(Settings.ChannelSensor, Settings.Channel, property, newValue);
            var newChannel = (await client.GetChannelsAsync(Settings.ChannelSensor)).First(c => c.Id == Settings.Channel);
            AssertEx.AreEqual(newValue, getProperty(newChannel), "New channel value did not apply properly");

            await client.SetObjectPropertyAsync(Settings.ChannelSensor, Settings.Channel, property, getProperty(initialChannel));
            var finalChannel = (await client.GetChannelsAsync(Settings.ChannelSensor)).First(c => c.Id == Settings.Channel);
            AssertEx.AreEqual(getProperty(initialChannel), getProperty(finalChannel), "Channel value did not revert properly");
        }
    }
}
