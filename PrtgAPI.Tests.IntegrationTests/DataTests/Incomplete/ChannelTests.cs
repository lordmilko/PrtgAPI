using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.IntegrationTests.ObjectTests
{
    [TestClass]
    public class ChannelTests : BasePrtgClientTest
    {
        [TestMethod]
        public void Data_Channel_GetChannels_HasExpectedResults()
        {
            var channels = client.GetChannels(Settings.ChannelSensor);

            Assert.AreEqual(Settings.ChannelsInTestSensor, channels.Count, nameof(Settings.ChannelsInTestSensor));

            var channel = channels.First(c => c.Id == Settings.Channel);

            Assert.AreEqual(Settings.ChannelErrorLimit, channel.UpperErrorLimit, nameof(Settings.ChannelErrorLimit));
            Assert.AreEqual(Settings.ChannelErrorMessage, channel.ErrorLimitMessage, nameof(Settings.ChannelErrorMessage));
            Assert.AreEqual(Settings.ChannelWarningLimit, channel.UpperWarningLimit, nameof(Settings.ChannelWarningLimit));
            Assert.AreEqual(Settings.ChannelWarningMessage, channel.WarningLimitMessage, nameof(Settings.ChannelWarningMessage));
        }
    }
}
