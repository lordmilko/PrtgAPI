using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Parameters;
using PrtgAPI.Tests.UnitTests.Support;
using PrtgAPI.Tests.UnitTests.Support.TestItems;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;

namespace PrtgAPI.Tests.UnitTests.CSharp.ObjectManipulation
{
    [TestClass]
    public class SetTriggerPropertyTests : BaseTest
    {
        [UnitTest]
        [TestMethod]
        public void SetTriggerProperty_Normal()
        {
            var trigger = GetTrigger();

            Execute(
                c => c.SetTriggerProperty(trigger, TriggerProperty.Threshold, 20.0),
                UnitRequest.EditSettings("id=1&subid=7&threshold_7=20")
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task SetTriggerProperty_NormalAsync()
        {
            var trigger = GetTrigger();

            await ExecuteAsync(
                async c => await c.SetTriggerPropertyAsync(trigger, TriggerProperty.Threshold, 20.0),
                UnitRequest.EditSettings("id=1&subid=7&threshold_7=20")
            );
        }

        [UnitTest]
        [TestMethod]
        public void SetTriggerProperty_MultipleParameters()
        {
            var trigger = GetTrigger();

            Execute(
                c => c.SetTriggerProperty(trigger,
                    new TriggerParameter(TriggerProperty.Threshold, 20.1),
                    new TriggerParameter(TriggerProperty.Latency, 30)
                ),
                UnitRequest.EditSettings("id=1&subid=7&threshold_7=20.1&latency_7=30")
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task SetTriggerProperty_MultipleParametersAsync()
        {
            var trigger = GetTrigger();

            await ExecuteAsync(
                async c => await c.SetTriggerPropertyAsync(trigger,
                    new TriggerParameter(TriggerProperty.Threshold, 20),
                    new TriggerParameter(TriggerProperty.Latency, 30)
                ),
                UnitRequest.EditSettings("id=1&subid=7&threshold_7=20&latency_7=30")
            );
        }

        [UnitTest]
        [TestMethod]
        public void SetTriggerProperty_NotificationAction()
        {
            var trigger = GetTrigger();

            var client = Initialize_Client(new MultiTypeResponse());

            var action = client.GetNotificationActions().First();

            Execute(
                c => c.SetTriggerProperty(trigger, TriggerProperty.OnNotificationAction, action),
                UnitRequest.EditSettings("id=1&subid=7&onnotificationid_7=300%7CEmail+and+push+notification+to+admin")
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task SetTriggerProperty_NotificationActionAsync()
        {
            var trigger = GetTrigger();

            var client = Initialize_Client(new MultiTypeResponse());

            var action = client.GetNotificationActions().First();

            await ExecuteAsync(
                async c => await c.SetTriggerPropertyAsync(trigger, TriggerProperty.OnNotificationAction, action),
                UnitRequest.EditSettings("id=1&subid=7&onnotificationid_7=300%7CEmail+and+push+notification+to+admin")
            );
        }

        [UnitTest]
        [TestMethod]
        public void SetTriggerProperty_Channel()
        {
            var triggerItem = NotificationTriggerItem.ThresholdTrigger(channel: "Backup State", parentId: "4000");
            var channelItem = new ChannelItem(name: "Backup State");
            var response = new NotificationTriggerResponse(new[] {triggerItem}, new[] {channelItem});
            var client = Initialize_Client(response);

            var trigger = client.GetNotificationTriggers(4000).First(t => t.Type == TriggerType.Threshold);

            Execute(
                c =>
                {
                    var channel = c.GetChannel(4000, 1);

                    c.SetTriggerProperty(trigger, TriggerProperty.Channel, channel);
                },
                new[]
                {
                    UnitRequest.Channels(4000),
                    UnitRequest.ChannelProperties(4000, 1),
                    UnitRequest.Sensors("filter_objid=4000"),
                    UnitRequest.Channels(4000),
                    UnitRequest.ChannelProperties(4000, 1),
                    UnitRequest.EditSettings("id=4000&subid=7&channel_7=1")
                }
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task SetTriggerProperty_ChannelAsync()
        {
            var triggerItem = NotificationTriggerItem.ThresholdTrigger(channel: "Backup State", parentId: "4000");
            var channelItem = new ChannelItem(name: "Backup State");
            var response = new NotificationTriggerResponse(new[] { triggerItem }, new[] { channelItem });
            var client = Initialize_Client(response);

            var trigger = client.GetNotificationTriggers(4000).First(t => t.Type == TriggerType.Threshold);

            await ExecuteAsync(
                async c =>
                {
                    var channel = await c.GetChannelAsync(4000, 1);

                    await c.SetTriggerPropertyAsync(trigger, TriggerProperty.Channel, channel);
                },
                new[]
                {
                    UnitRequest.Channels(4000),
                    UnitRequest.ChannelProperties(4000, 1),
                    UnitRequest.Sensors("filter_objid=4000"),
                    UnitRequest.Channels(4000),
                    UnitRequest.ChannelProperties(4000, 1),
                    UnitRequest.EditSettings("id=4000&subid=7&channel_7=1")
                }
            );
        }

        [UnitTest]
        [TestMethod]
        public void SetTriggerProperty_NullNotificationAction()
        {
            var trigger = GetTrigger();

            Execute(
                c => c.SetTriggerProperty(trigger, TriggerProperty.OnNotificationAction, null),
                UnitRequest.EditSettings("id=1&subid=7&onnotificationid_7=-1%7CNone")
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task SetTriggerProperty_NullNotificationActionAsync()
        {
            var trigger = GetTrigger();

            await ExecuteAsync(
                async c => await c.SetTriggerPropertyAsync(trigger, TriggerProperty.OnNotificationAction, null),
                UnitRequest.EditSettings("id=1&subid=7&onnotificationid_7=-1%7CNone")
            );
        }

        [UnitTest]
        [TestMethod]
        public void SetTriggerProperty_NullChannel()
        {
            var trigger = GetTrigger();

            var client = Initialize_Client(new MultiTypeResponse());

            AssertEx.Throws<ArgumentNullException>(
                () => client.SetTriggerProperty(trigger, TriggerProperty.Channel, null),
                "Value 'null' could not be assigned to property 'Channel' of type 'PrtgAPI.TriggerChannel'"
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task SetTriggerProperty_NullChannelAsync()
        {
            var trigger = GetTrigger();

            var client = Initialize_Client(new MultiTypeResponse());

            await AssertEx.ThrowsAsync<ArgumentNullException>(
                async () => await client.SetTriggerPropertyAsync(trigger, TriggerProperty.Channel, null),
                "Value 'null' could not be assigned to property 'Channel' of type 'PrtgAPI.TriggerChannel'"
            );
        }

        private NotificationTrigger GetTrigger()
        {
            var notificationItem = NotificationTriggerItem.ThresholdTrigger(channel: "Primary");

            var client = Initialize_Client(new NotificationTriggerResponse(new[] { notificationItem }));

            var trigger = client.GetNotificationTriggers(1).First(t => t.Type == TriggerType.Threshold);

            return trigger;
        }
    }
}
