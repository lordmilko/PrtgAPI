using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Parameters;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectTests
{
    [TestClass]
    public class AddNotificationTriggerTests : BaseTest
    {
        [TestMethod]
        public void AddNotificationTrigger_SupportedType()
        {
            var client = Initialize_Client(new SetNotificationTriggerResponse());

            var parameters = new ThresholdTriggerParameters(1001)
            {
                Channel = new TriggerChannel(1)
            };

            client.AddNotificationTrigger(parameters);
        }

        [TestMethod]
        public async Task AddNotificationTrigger_SupportedTypeAsync()
        {
            var client = Initialize_Client(new SetNotificationTriggerResponse());

            var parameters = new ThresholdTriggerParameters(1001)
            {
                Channel = new TriggerChannel(1)
            };

            await client.AddNotificationTriggerAsync(parameters);
        }

        [TestMethod]
        public void AddNotificationTrigger_UnsupportedType()
        {
            var client = Initialize_Client(new SetNotificationTriggerResponse());

            var parameters = new StateTriggerParameters(1001);

            AssertEx.Throws<InvalidTriggerTypeException>(() => client.AddNotificationTrigger(parameters), "Trigger type 'State' is not a valid trigger type");
        }

        [TestMethod]
        public async Task AddNotificationTrigger_UnsupportedTypeAsync()
        {
            var client = Initialize_Client(new SetNotificationTriggerResponse());

            var parameters = new StateTriggerParameters(1001);

            await AssertEx.ThrowsAsync<InvalidTriggerTypeException>(async() => await client.AddNotificationTriggerAsync(parameters), "Trigger type 'State' is not a valid trigger type");
        }

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

            AssertEx.Throws<InvalidOperationException>(() => client.AddNotificationTrigger(parameters), "Channel '1' is not a valid value for Device, Group or Probe");
        }

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

            await AssertEx.ThrowsAsync<InvalidOperationException>(async () => await client.AddNotificationTriggerAsync(parameters), "Channel '1' is not a valid value for Device, Group or Probe");
        }

        [TestMethod]
        public void AddNotificationTrigger_EnumToSensor()
        {
            var client = Initialize_Client(new SetNotificationTriggerResponse());

            var parameters = new ThresholdTriggerParameters(1001);

            AssertEx.Throws<InvalidOperationException>(() => client.AddNotificationTrigger(parameters), "Channel 'Primary' is not a valid value for sensor");
        }

        [TestMethod]
        public async Task AddNotificationTrigger_EnumToSensorAsync()
        {
            var client = Initialize_Client(new SetNotificationTriggerResponse());

            var parameters = new ThresholdTriggerParameters(1001);

            await AssertEx.ThrowsAsync<InvalidOperationException>(async () => await client.AddNotificationTriggerAsync(parameters), "Channel 'Primary' is not a valid value for sensor");
        }
    }
}
