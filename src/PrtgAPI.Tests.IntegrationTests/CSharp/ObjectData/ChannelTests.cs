using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.IntegrationTests.ObjectData
{
    [TestClass]
    public class ChannelTests : BasePrtgClientTest
    {
        [TestMethod]
        [IntegrationTest]
        public void Data_Channel_GetChannels_HasExpectedResults()
        {
            var channels = client.GetChannels(Settings.ChannelSensor);

            AssertEx.AreEqual(Settings.ChannelsInTestSensor, channels.Count, nameof(Settings.ChannelsInTestSensor));

            var channel = channels.First(c => c.Id == Settings.Channel);

            AssertEx.AreEqual(Settings.ChannelErrorLimit, channel.UpperErrorLimit, nameof(Settings.ChannelErrorLimit));
            AssertEx.AreEqual(Settings.ChannelErrorMessage, channel.ErrorLimitMessage, nameof(Settings.ChannelErrorMessage));
            AssertEx.AreEqual(Settings.ChannelWarningLimit, channel.UpperWarningLimit, nameof(Settings.ChannelWarningLimit));
            AssertEx.AreEqual(Settings.ChannelWarningMessage, channel.WarningLimitMessage, nameof(Settings.ChannelWarningMessage));
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_Channel_ReadOnlyUser()
        {
            var channel = readOnlyClient.GetChannel(Settings.ChannelSensor, Settings.Channel);

            AssertEx.AllPropertiesRetrieveValues(channel);
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Data_Channel_ReadOnlyUserAsync()
        {
            var channel = await readOnlyClient.GetChannelAsync(Settings.ChannelSensor, Settings.Channel);

            AssertEx.AllPropertiesRetrieveValues(channel);
        }
    }
}
