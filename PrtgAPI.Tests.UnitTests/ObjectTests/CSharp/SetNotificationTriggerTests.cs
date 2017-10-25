using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Parameters;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.CSharp
{
    [TestClass]
    public class SetNotificationTriggerTests : BaseTest
    {
        [TestMethod]
        public void SetNotificationTrigger_CanExecute()
        {
            var client = Initialize_Client(new SetNotificationTriggerResponse());

            var parameters = new StateTriggerParameters(1001, 1);

            client.SetNotificationTrigger(parameters);
        }

        [TestMethod]
        public async Task SetNotificationTrigger_CanExecuteAsync()
        {
            var client = Initialize_Client(new SetNotificationTriggerResponse());

            var parameters = new StateTriggerParameters(1001, 1);

            await client.SetNotificationTriggerAsync(parameters);
        }

        [TestMethod]
        public void SetNotificationTrigger_SetsAChannel_OnASensor()
        {
            var client = Initialize_Client(new SetNotificationTriggerResponse());

            var parameters = new ThresholdTriggerParameters(1001, 1)
            {
                Channel = client.GetChannels(1001).First()
            };

            client.SetNotificationTrigger(parameters);
        }

        [TestMethod]
        public async Task SetNotificationTrigger_SetsAChannel_OnASensorAsync()
        {
            var client = Initialize_Client(new SetNotificationTriggerResponse());

            var parameters = new ThresholdTriggerParameters(1001, 1)
            {
                Channel = (await client.GetChannelsAsync(1001)).First()
            };

            await client.SetNotificationTriggerAsync(parameters);
        }
    }
}
