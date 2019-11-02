using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Parameters;
using PrtgAPI.Tests.UnitTests.Support;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.ObjectManipulation
{
    [TestClass]
    public class AddNotificationTriggerTests : BaseTest
    {
        [UnitTest]
        [TestMethod]
        public void AddNotificationTrigger_SupportedType()
        {
            var client = Initialize_Client(new SetNotificationTriggerResponse());

            var parameters = new ThresholdTriggerParameters(1001)
            {
                Channel = new TriggerChannel(1)
            };

            client.AddNotificationTrigger(parameters, false);
        }

        [UnitTest]
        [TestMethod]
        public async Task AddNotificationTrigger_SupportedTypeAsync()
        {
            var client = Initialize_Client(new SetNotificationTriggerResponse());

            var parameters = new ThresholdTriggerParameters(1001)
            {
                Channel = new TriggerChannel(1)
            };

            await client.AddNotificationTriggerAsync(parameters, false);
        }

        [UnitTest]
        [TestMethod]
        public void AddNotificationTrigger_UnsupportedType()
        {
            var client = Initialize_Client(new SetNotificationTriggerResponse());

            var parameters = new StateTriggerParameters(1001);

            AssertEx.Throws<InvalidTriggerTypeException>(() => client.AddNotificationTrigger(parameters), "Trigger type 'State' is not a valid trigger type");
        }

        [UnitTest]
        [TestMethod]
        public async Task AddNotificationTrigger_UnsupportedTypeAsync()
        {
            var client = Initialize_Client(new SetNotificationTriggerResponse());

            var parameters = new StateTriggerParameters(1001);

            await AssertEx.ThrowsAsync<InvalidTriggerTypeException>(async() => await client.AddNotificationTriggerAsync(parameters), "Trigger type 'State' is not a valid trigger type");
        }

        [UnitTest]
        [TestMethod]
        public void AddNotificationTrigger_ChannelToContainer()
        {
            var dictionary = new Dictionary<Content, int>
            {
                [Content.Sensors] = 0
            };

            var client = Initialize_Client(new SetNotificationTriggerResponse(dictionary));

            var parameters = new ThresholdTriggerParameters(1001)
            {
                Channel = new TriggerChannel(1)
            };

            AssertEx.Throws<InvalidOperationException>(() => client.AddNotificationTrigger(parameters), "Channel ID '1' is not a valid channel for Device, Group or Probe");
        }

        [UnitTest]
        [TestMethod]
        public async Task AddNotificationTrigger_ChannelToContainerAsync()
        {
            var dictionary = new Dictionary<Content, int>
            {
                [Content.Sensors] = 0
            };

            var client = Initialize_Client(new SetNotificationTriggerResponse(dictionary));

            var parameters = new ThresholdTriggerParameters(1001)
            {
                Channel = new TriggerChannel(1)
            };

            await AssertEx.ThrowsAsync<InvalidOperationException>(async () => await client.AddNotificationTriggerAsync(parameters), "Channel ID '1' is not a valid channel for Device, Group or Probe");
        }

        [UnitTest]
        [TestMethod]
        public void AddNotificationTrigger_EnumToSensor()
        {
            var client = Initialize_Client(new SetNotificationTriggerResponse());

            var parameters = new ThresholdTriggerParameters(1001);

            AssertEx.Throws<InvalidOperationException>(() => client.AddNotificationTrigger(parameters), "Channel 'Primary' is not a valid value for sensor");
        }

        [UnitTest]
        [TestMethod]
        public async Task AddNotificationTrigger_EnumToSensorAsync()
        {
            var client = Initialize_Client(new SetNotificationTriggerResponse());

            var parameters = new ThresholdTriggerParameters(1001);

            await AssertEx.ThrowsAsync<InvalidOperationException>(async () => await client.AddNotificationTriggerAsync(parameters), "Channel 'Primary' is not a valid value for sensor");
        }

        [UnitTest]
        [TestMethod]
        public void AddNotificationTrigger_ResolveScenarios()
        {
            var client = Initialize_Client(new DiffBasedResolveResponse(false));

            var parameters = new StateTriggerParameters(1001)
            {
                OnNotificationAction = {Id = 301}
            };

            var resolvedTrigger = client.AddNotificationTrigger(parameters);

            Assert.AreEqual("Email to all members of group PRTG Users Group 2", resolvedTrigger.OnNotificationAction.ToString());

            var trigger = client.AddNotificationTrigger(parameters, false);
            Assert.AreEqual(null, trigger, "Trigger was not null");
        }

        [UnitTest]
        [TestMethod]
        public async Task AddNotificationTrigger_ResolveScenariosAsync()
        {
            var client = Initialize_Client(new DiffBasedResolveResponse(false));

            var parameters = new StateTriggerParameters(1001)
            {
                OnNotificationAction = { Id = 301 }
            };

            var resolvedTrigger = await client.AddNotificationTriggerAsync(parameters);

            Assert.AreEqual("Email to all members of group PRTG Users Group 2", resolvedTrigger.OnNotificationAction.ToString());

            var trigger = await client.AddNotificationTriggerAsync(parameters, false);
            Assert.AreEqual(null, trigger, "Trigger was not null");
        }

        [UnitTest]
        [TestMethod]
        public void AddNotificationTrigger_Throws_ResolvingMultiple()
        {
            var client = Initialize_Client(new DiffBasedResolveResponse(true));

            var parameters = new StateTriggerParameters(1001)
            {
                OnNotificationAction = { Id = 301 }
            };

            var str = "Could not uniquely identify created NotificationTrigger: multiple new objects ('Type = State, Inherited = False, OnNotificationAction = Email to all members of group PRTG Users Group 2',";

            AssertEx.Throws<ObjectResolutionException>(() => client.AddNotificationTrigger(parameters), str);
        }

        [UnitTest]
        [TestMethod]
        public async Task AddNotificationTrigger_Throws_ResolvingMultipleAsync()
        {
            var client = Initialize_Client(new DiffBasedResolveResponse(true));

            var parameters = new StateTriggerParameters(1001)
            {
                OnNotificationAction = { Id = 301 }
            };

            var str = "Could not uniquely identify created NotificationTrigger: multiple new objects ('Type = State, Inherited = False, OnNotificationAction = Email to all members of group PRTG Users Group 2',";

            await AssertEx.ThrowsAsync<ObjectResolutionException>(async () => await client.AddNotificationTriggerAsync(parameters), str);
        }

        [UnitTest]
        [TestMethod]
        public void AddNotificationTrigger_TriggerChannel_StandardTriggerChannel_OnSensor()
        {
            var urls = new[]
            {
                UnitRequest.TriggerTypes(1001),              //Validate Supported Triggers
                UnitRequest.Sensors("filter_objid=1001")     //Validate TriggerChannel target compatibility
            };

            AssertEx.Throws<InvalidOperationException>(() => TestTriggerChannel(TriggerChannel.Total, urls, true), "Channel 'Total' is not a valid value");
        }

        [UnitTest]
        [TestMethod]
        public void AddNotificationTrigger_TriggerChannel_StandardTriggerChannel_OnContainer()
        {
            var urls = new[]
            {
                UnitRequest.TriggerTypes(1001),              //Validate Supported Triggers
                UnitRequest.Sensors("filter_objid=1001"), //Validate TriggerChannel target compatibility
                UnitRequest.EditSettings("id=1001&subid=new&onnotificationid_new=-1%7CNone&class=threshold&offnotificationid_new=-1%7CNone&channel_new=-1&condition_new=0&threshold_new=0&latency_new=60&objecttype=nodetrigger") //Add Trigger
            };

            TestTriggerChannel(TriggerChannel.Total, urls, false);
        }

        [UnitTest]
        [TestMethod]
        public void AddNotificationTrigger_TriggerChannel_Channel_OnSensor()
        {
            var urls = new[]
            {
                UnitRequest.TriggerTypes(1001),              //Validate Supported Triggers
                UnitRequest.Sensors("filter_objid=1001"),    //Validate TriggerChannel target compatibility
                UnitRequest.Channels(1001),
                UnitRequest.ChannelProperties(1001, 1),
                UnitRequest.EditSettings("id=1001&subid=new&onnotificationid_new=-1%7CNone&class=threshold&offnotificationid_new=-1%7CNone&channel_new=1&condition_new=0&threshold_new=0&latency_new=60&objecttype=nodetrigger") //Add Trigger
            };

            var channel = new Channel
            {
                Name = "Percent Available Memory",
                Id = 1
            };

            TestTriggerChannel(channel, urls, true);
        }

        [UnitTest]
        [TestMethod]
        public void AddNotificationTrigger_TriggerChannel_Channel_OnContainer()
        {
            var urls = new[]
            {
                UnitRequest.TriggerTypes(1001),              //Validate Supported Triggers
                UnitRequest.Sensors("filter_objid=1001"),                                                                    //Validate TriggerChannel target compatibility
            };

            var channel = new Channel
            {
                Name = "Percent Available Memory",
                Id = 1
            };

            AssertEx.Throws<InvalidOperationException>(() => TestTriggerChannel(channel, urls, false), "Channel 'Percent Available Memory' of type 'Channel' is not a valid channel");
        }

        [UnitTest]
        [TestMethod]
        public void AddNotificationTrigger_TriggerChannel_ChannelId_OnSensor()
        {
            var urls = new[]
            {
                UnitRequest.TriggerTypes(1001),              //Validate Supported Triggers
                UnitRequest.Sensors("filter_objid=1001"),                                                                    //Validate TriggerChannel target compatibility
                UnitRequest.ChannelProperties(1001, 3),
                UnitRequest.EditSettings("id=1001&subid=new&onnotificationid_new=-1%7CNone&class=threshold&offnotificationid_new=-1%7CNone&channel_new=3&condition_new=0&threshold_new=0&latency_new=60&objecttype=nodetrigger") //Add Trigger
            };

            TestTriggerChannel(new TriggerChannel(3), urls, true);
        }

        [UnitTest]
        [TestMethod]
        public void AddNotificationTrigger_TriggerChannel_ChannelId_OnContainer()
        {
            var urls = new[]
            {
                UnitRequest.TriggerTypes(1001),              //Validate Supported Triggers
                UnitRequest.Sensors("filter_objid=1001")                                                                     //Validate TriggerChannel target compatibility
            };

            AssertEx.Throws<InvalidOperationException>(() => TestTriggerChannel(new TriggerChannel(3), urls, false), "Channel ID '3' is not a valid channel");
        }

        [UnitTest]
        [TestMethod]
        public void AddNotificationTrigger_TriggerChannel_InvalidChannelId_OnSensor()
        {
            var urls = new[]
            {
                UnitRequest.TriggerTypes(1001),              //Validate Supported Triggers
                UnitRequest.Sensors("filter_objid=1001"),                                                                    //Validate TriggerChannel target compatibility
                UnitRequest.ChannelProperties(1001, 99)
            };

            AssertEx.Throws<InvalidOperationException>(() => TestTriggerChannel(new TriggerChannel(99), urls, true), "Channel ID '99' is not a valid channel");
        }

        [UnitTest]
        [TestMethod]
        public void AddNotificationTrigger_TriggerChannel_InvalidChannelId_OnContainer()
        {
            var urls = new[]
            {
                UnitRequest.TriggerTypes(1001),              //Validate Supported Triggers
                UnitRequest.Sensors("filter_objid=1001")                                                                     //Validate TriggerChannel target compatibility
            };

            AssertEx.Throws<InvalidOperationException>(() => TestTriggerChannel(new TriggerChannel(99), urls, false), "Channel ID '99' is not a valid channel");
        }

        [UnitTest]
        [TestMethod]
        public void AddNotificationTrigger_TriggerChannel_Channel_WithStandardTriggerChannelName_OnSensor()
        {
            var urls = new[]
            {
                UnitRequest.TriggerTypes(1001),              //Validate Supported Triggers
                UnitRequest.Sensors("filter_objid=1001"),                                                                    //Validate TriggerChannel target compatibility
                UnitRequest.Channels(1001),
                UnitRequest.ChannelProperties(1001, 1),
                UnitRequest.EditSettings("id=1001&subid=new&onnotificationid_new=-1%7CNone&class=threshold&offnotificationid_new=-1%7CNone&channel_new=1&condition_new=0&threshold_new=0&latency_new=60&objecttype=nodetrigger") //Add Trigger
            };

            var channel = new Channel
            {
                Name = "Total",
                Id = 1
            };

            TestTriggerChannel(channel, urls, true, new ChannelItem(name: "Total", objId: "1"));
        }

        [UnitTest]
        [TestMethod]
        public void AddNotificationTrigger_TriggerChannel_Channel_WithStandardTriggerChannelName_OnContainer()
        {
            var urls = new[]
            {
                UnitRequest.TriggerTypes(1001),              //Validate Supported Triggers
                UnitRequest.Sensors("filter_objid=1001")                                                                     //Validate TriggerChannel target compatibility
            };

            var channel = new Channel
            {
                Name = "Total"
            };

            AssertEx.Throws<InvalidOperationException>(() => TestTriggerChannel(channel, urls, false), "Channel 'Total' of type 'Channel' is not a valid channel");
        }

        private void TestTriggerChannel(TriggerChannel channel, string[] urls, bool isSensor, ChannelItem channelItem = null)
        {
            var countOverride = new Dictionary<Content, int>
            {
                [Content.Sensors] = isSensor ? 2 : 0
            };

            var itemOverride = channelItem != null
                ? new Dictionary<Content, BaseItem[]>
                {
                    [Content.Channels] = new[] {channelItem}
                }
                : null;

            var parameters = new ThresholdTriggerParameters(1001)
            {
                Channel = channel
            };

            Execute(
                c => c.AddNotificationTrigger(parameters, false),
                urls,
                countOverride,
                itemOverride
            );
        }
    }
}
