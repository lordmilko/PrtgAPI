using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.IntegrationTests.DataTests
{
    [TestClass]
    public class ChannelTests : BasePrtgClientTest
    {
        [TestMethod]
        public void Data_Channel_GetChannels_HasExpectedResults()
        {
            var channels = client.GetChannels(Settings.ChannelSensor);

            Assert2.AreEqual(Settings.ChannelsInTestSensor, channels.Count, nameof(Settings.ChannelsInTestSensor));

            var channel = channels.First(c => c.Id == Settings.Channel);

            Assert2.AreEqual(Settings.ChannelErrorLimit, channel.UpperErrorLimit, nameof(Settings.ChannelErrorLimit));
            Assert2.AreEqual(Settings.ChannelErrorMessage, channel.ErrorLimitMessage, nameof(Settings.ChannelErrorMessage));
            Assert2.AreEqual(Settings.ChannelWarningLimit, channel.UpperWarningLimit, nameof(Settings.ChannelWarningLimit));
            Assert2.AreEqual(Settings.ChannelWarningMessage, channel.WarningLimitMessage, nameof(Settings.ChannelWarningMessage));
        }
    }
}
