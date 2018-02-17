using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.IntegrationTests
{
    [TestClass]
    public class ChannelTests : BasePrtgClientTest
    {
        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Action_Channel_SetChannelProperty_UpperErrorLimit()
        {
            SetAndRevertChannelProperty(3000, ChannelProperty.UpperErrorLimit, c => c.UpperErrorLimit);
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
    }
}
