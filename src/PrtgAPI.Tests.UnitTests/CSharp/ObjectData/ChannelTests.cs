using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support;
using PrtgAPI.Tests.UnitTests.Support.TestItems;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectData
{
    [TestClass]
    public class ChannelTests : StandardObjectTests<Channel, ChannelItem, ChannelResponse>
    {
        [UnitTest]
        [TestMethod]
        public void Channel_CanDeserialize() => Object_CanDeserialize();

        [UnitTest]
        [TestMethod]
        public void Channel_GetObjectsOverloads_CanExecute()
        {
            var client = Initialize_Client_WithItems(GetItem());

            Assert.IsTrue(client.GetChannels(1001).Any());
            Assert.IsTrue(client.GetChannelsAsync(1001).Result.Any());

            Assert.IsTrue(client.GetChannels(1001, "Percent Available Memory").Any());
            Assert.IsTrue(client.GetChannelsAsync(1001, "Percent Available Memory").Result.Any());

            Assert.IsTrue(client.GetChannel(1001, 1) != null);
            Assert.IsTrue(client.GetChannelAsync(1001, 1).Result != null);

            Assert.IsTrue(client.GetChannel(1001, "Percent Available Memory") != null);
            Assert.IsTrue(client.GetChannelAsync(1001, "Percent Available Memory").Result != null);
        }

        [UnitTest]
        [TestMethod]
        public async Task Channel_CanDeserializeAsync() => await Object_CanDeserializeAsync();

        [UnitTest]
        [TestMethod]
        public void Channel_AllFields_HaveValues() => Object_AllFields_HaveValues();

        [UnitTest]
        [TestMethod]
        public void Channel_SetProperty_Enable_LimitsEnabled_UrlCorrect()
        {
            Channel_SetProperty(ChannelProperty.LimitsEnabled, true);
        }

        [UnitTest]
        [TestMethod]
        public void Channel_SetProperty_Disable_LimitsEnabled_UrlCorrect()
        {
            Channel_SetProperty(ChannelProperty.LimitsEnabled, false);
        }

        [UnitTest]
        [TestMethod]
        public void Channel_SetProperty_Enable_UpperErrorLimit_UrlCorrect()
        {
            Channel_SetProperty(ChannelProperty.UpperErrorLimit, 100);
        }

        [UnitTest]
        [TestMethod]
        public void Channel_SetProperty_Disable_UpperErrorLimit_UrlCorrect()
        {
            Channel_SetProperty(ChannelProperty.UpperErrorLimit, string.Empty);
        }

        private void Channel_SetProperty(ChannelProperty property, object value)
        {
            var channelId = 1;

            var client = Initialize_Client(new SetChannelPropertyResponse(property, channelId, value));

            client.SetChannelProperty(1234, channelId, property, value);
        }

        [UnitTest]
        [TestMethod]
        public void Channel_SetProperty_ValueLookup_NormalLookup() => Channel_SetValueLookup("potato", "potato%7Cpotato");

        [UnitTest]
        [TestMethod]
        public void Channel_SetProperty_ValueLookup_None() => Channel_SetValueLookup("none", "%7CNone");

        [UnitTest]
        [TestMethod]
        public void Channel_SetProperty_ValueLookup_Null() => Channel_SetValueLookup(null, "%7CNone");

        private void Channel_SetValueLookup(object value, string url)
        {
            Execute(
                c => c.SetChannelProperty(1001, 1, ChannelProperty.ValueLookup, value),
                UnitRequest.EditSettings($"id=1001&valuelookup_1={url}")
            );
        }

        [UnitTest]
        [TestMethod]
        public void Channel_Unit_CalculatesProperly()
        {
            Func<ChannelItem, Channel> func = i => GetObjects(Initialize_Client_WithItems(i)).First();

            var pairs = new Dictionary<string, string>
            {
                ["%"] = "26 %",
                ["m"] = "12 h 32 m",
                ["kbps"] = "<1 kbps",
                ["mbps"] = "< 1 mbps"
            };

            foreach (var pair in pairs)
            {
                var item = new ChannelItem(pair.Value);
                Assert.AreEqual(pair.Key, func(item).Unit);
            }
        }

        [UnitTest]
        [TestMethod]
        public void Channel_Filter_ByName_NoMatches()
        {
            var client = Initialize_Client_WithItems(GetItem());

            var channels = client.GetChannels(1234, "blah");

            Assert.AreEqual(0, channels.Count);
        }

        [UnitTest]
        [TestMethod]
        public void Channel_Filter_ByName_Match()
        {
            var client = Initialize_Client_WithItems(GetItem());

            var channels = client.GetChannels(1234, "Percent Available Memory");

            Assert.AreEqual(1, channels.Count);
        }

        [UnitTest]
        [TestMethod]
        public void Channel_ReadOnly()
        {
            var client = Initialize_ReadOnlyClient(GetResponse(new[] { GetItem() }));

            var channel = client.GetChannel(1001, 1);

            AssertEx.AllPropertiesRetrieveValues(channel);
        }

        [UnitTest]
        [TestMethod]
        public async Task Channel_ReadOnlyAsync()
        {
            var client = Initialize_ReadOnlyClient(GetResponse(new[] { GetItem() }));

            var channel = await client.GetChannelAsync(1001, 1);

            AssertEx.AllPropertiesRetrieveValues(channel);
        }

        protected override List<Channel> GetObjects(PrtgClient client) => client.GetChannels(1234);

        protected override async Task<List<Channel>> GetObjectsAsync(PrtgClient client) => await client.GetChannelsAsync(1234);

        public override ChannelItem GetItem() => new ChannelItem();

        protected override ChannelResponse GetResponse(ChannelItem[] items) => new ChannelResponse(items);
    }
}
