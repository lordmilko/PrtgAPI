using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.IntegrationTests
{
    [TestClass]
    public class ChannelTests : BasePrtgClientTest
    {
        [TestMethod]
        public void Action_Channel_SetChannelProperty_UpperErrorLimit()
        {
            SetAndRevertChannelProperty(3000, ChannelProperty.UpperErrorLimit, c => c.UpperErrorLimit);
        }

        private void SetAndRevertChannelProperty<T>(T newValue, ChannelProperty property, Func<Channel, T> getProperty)
        {
            var initialChannel = client.GetChannels(Settings.ChannelSensor).First(c => c.Id == Settings.Channel);
            Assert.AreNotEqual(getProperty(initialChannel), newValue);

            client.SetObjectProperty(Settings.ChannelSensor, Settings.Channel, property, newValue);
            var newChannel = client.GetChannels(Settings.ChannelSensor).First(c => c.Id == Settings.Channel);
            Assert.AreEqual(newValue, getProperty(newChannel));

            client.SetObjectProperty(Settings.ChannelSensor, Settings.Channel, property, getProperty(initialChannel));
            var finalChannel = client.GetChannels(Settings.ChannelSensor).First(c => c.Id == Settings.Channel);
            Assert.AreEqual(getProperty(initialChannel), getProperty(finalChannel));
        }
    }
}
