using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.ObjectTests.Items;
using PrtgAPI.Tests.UnitTests.ObjectTests.Responses;

namespace PrtgAPI.Tests.UnitTests.ObjectTests
{
    [TestClass]
    public class ChannelTests : ObjectTests<Channel, ChannelItem, ChannelResponse>
    {
        [TestMethod]
        public void Channel_CanDeserialize() => Object_CanDeserialize();

        [TestMethod]
        public void Channel_AllFields_HaveValues() => Object_AllFields_HaveValues();

        [TestMethod]
        public void Channel_SetProperty_Enable_LimitsEnabled_UrlCorrect()
        {
            Channel_SetProperty(ChannelProperty.LimitsEnabled, true);
        }

        [TestMethod]
        public void Channel_SetProperty_Disable_LimitsEnabled_UrlCorrect()
        {
            Channel_SetProperty(ChannelProperty.LimitsEnabled, false);
        }

        [TestMethod]
        public void Channel_SetProperty_Enable_UpperErrorLimit_UrlCorrect()
        {
            Channel_SetProperty(ChannelProperty.UpperErrorLimit, 100);
        }

        [TestMethod]
        public void Channel_SetProperty_Disable_UpperErrorLimit_UrlCorrect()
        {
            Channel_SetProperty(ChannelProperty.UpperErrorLimit, string.Empty);
        }

        private void Channel_SetProperty(ChannelProperty property, object value)
        {
            var channelId = 1;

            var client = Initialize_Client(new SetChannelPropertyResponse(property, channelId, value));

            client.SetObjectProperty(1234, channelId, property, value);
        }

        protected override List<Channel> GetObjects(PrtgClient client) => client.GetChannels(1234);

        public override ChannelItem GetItem() => new ChannelItem();

        protected override ChannelResponse GetResponse(ChannelItem[] items) => new ChannelResponse(items);
    }
}
