using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.ObjectTests.Support;

namespace PrtgAPI.Tests.UnitTests.ObjectTests
{
    [TestClass]
    public class ChannelTests : ObjectTests<Channel, ChannelItem, ChannelResponse>
    {
        [TestMethod]
        public void Channel_CanDeserialize() => Object_CanDeserialize();

        [TestMethod]
        public void Channel_AllFields_HaveValues() => Object_AllFields_HaveValues();

        protected override List<Channel> GetObjects(PrtgClient client) => client.GetChannels(1234);

        protected override ChannelItem GetItem() => new ChannelItem();

        protected override ChannelResponse GetResponse(ChannelItem[] items) => new ChannelResponse(items);
    }
}
