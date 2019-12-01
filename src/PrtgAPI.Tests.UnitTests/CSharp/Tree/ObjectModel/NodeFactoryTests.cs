using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestItems;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;
using PrtgAPI.Tree;

namespace PrtgAPI.Tests.UnitTests.Tree
{
    [TestClass]
    public class NodeFactoryTests
    {
        [UnitTest]
        [TestMethod]
        public void Tree_Factory_AllTypes_Single()
        {
            var client = BaseTest.Initialize_Client(new MultiTypeResponse());
            var factory = new PrtgNodeFactory(client);

            var node = factory.Probe(1000,
                factory.Group(2000,
                    factory.Device(3000,
                        factory.Sensor(4000,
                            factory.Property(3000, ObjectProperty.Name)
                        )
                    )
                )
            );

            var response = new NotificationTriggerResponse(
                NotificationTriggerItem.StateTrigger(parentId: "3000"),
                NotificationTriggerItem.SpeedTrigger(parentId: "3000")
            );

            var triggerClient = BaseTest.Initialize_Client(response);

            var triggerFactory = new PrtgNodeFactory(triggerClient);

            Assert.IsNotNull(triggerFactory.Trigger(3000, "Email and push notification to admin"));
        }

        [UnitTest]
        [TestMethod]
        public void Tree_Factory_AllTypes_Multiple()
        {
            var client = BaseTest.Initialize_Client(new MultiTypeResponse());
            var factory = new PrtgNodeFactory(client);

            var probes = factory.Probes(Property.Name, "Local Probe");
            var groups = factory.Groups(Property.Name, "Windows Infrastructure");
            var devices = factory.Devices(Property.Name, "dc-1");
            var sensors = factory.Sensors(Property.Name, "Ping");
            
            var properties = factory.Properties(3000);

            var response = new NotificationTriggerResponse(
                NotificationTriggerItem.StateTrigger(parentId: "3000"),
                NotificationTriggerItem.SpeedTrigger(parentId: "3000")
            );

            var triggerClient = BaseTest.Initialize_Client(response);
            var triggerFactory = new PrtgNodeFactory(triggerClient);

            var triggers = triggerFactory.Triggers(3000);
        }
    }
}
