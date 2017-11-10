using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Parameters;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.CSharp
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
        [ExpectedException(typeof(InvalidTriggerTypeException))]
        public void AddNotificationTrigger_UnsupportedType()
        {
            var client = Initialize_Client(new SetNotificationTriggerResponse());

            var parameters = new StateTriggerParameters(1001);

            client.AddNotificationTrigger(parameters);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidTriggerTypeException))]
        public async Task AddNotificationTrigger_UnsupportedTypeAsync()
        {
            var client = Initialize_Client(new SetNotificationTriggerResponse());

            var parameters = new StateTriggerParameters(1001);

            await client.AddNotificationTriggerAsync(parameters);
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

            try
            {
                client.AddNotificationTrigger(parameters);
                Assert.Fail("Exception should have been thrown");
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("Channel '1' is not a valid value for Device, Group or Probe"))
                    throw;
            }
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

            try
            {
                await client.AddNotificationTriggerAsync(parameters);
                Assert.Fail("Exception should have been thrown");
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("Channel '1' is not a valid value for Device, Group or Probe"))
                    throw;
            }
        }

        [TestMethod]
        public void AddNotificationTrigger_EnumToSensor()
        {
            var client = Initialize_Client(new SetNotificationTriggerResponse());

            var parameters = new ThresholdTriggerParameters(1001);

            try
            {
                client.AddNotificationTrigger(parameters);
                Assert.Fail("Exception should have been thrown");
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("Channel 'Primary' is not a valid value for sensor"))
                    throw;
            }
        }

        [TestMethod]
        public async Task AddNotificationTrigger_EnumToSensorAsync()
        {
            var client = Initialize_Client(new SetNotificationTriggerResponse());

            var parameters = new ThresholdTriggerParameters(1001);

            try
            {
                await client.AddNotificationTriggerAsync(parameters);
                Assert.Fail("Exception should have been thrown");
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("Channel 'Primary' is not a valid value for sensor"))
                    throw;
            }
        }
    }
}
