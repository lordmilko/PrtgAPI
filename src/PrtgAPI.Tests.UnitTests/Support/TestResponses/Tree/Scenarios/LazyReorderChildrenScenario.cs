using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class LazyReorderChildrenScenario : TreeScenario
    {
        protected override IWebResponse GetResponse(string address)
        {
            switch (requestNum)
            {
                case 1:
                    Assert.AreEqual(address, UnitRequest.Objects("filter_objid=1001"));
                    return new ProbeResponse(new ProbeItem(objid: "1001", name: "Local Probe"));

                case 2:
                    Assert.AreEqual(address, UnitRequest.Probes("filter_objid=1001&filter_parentid=0"));
                    return new ProbeResponse(new ProbeItem(objid: "1001", name: "Local Probe"));
                
                //Probe -> Devices
                case 3:
                    Assert.AreEqual(address, UnitRequest.Devices("filter_parentid=1001"));
                    return new DeviceResponse(
                        new DeviceItem(objid: "3001", name: "dc-1", position: "10", positionRaw: "10"),
                        new DeviceItem(objid: "3002", name: "dc-2", position: "20", positionRaw: "20")
                    );
                
                //Probe -> Groups
                case 4:
                    Assert.AreEqual(address, UnitRequest.Groups("filter_parentid=1001"));
                    return new GroupResponse(new GroupItem(objid: "2001", name: "Servers", position: "30", positionRaw: "30"));

                //Probe -> Triggers
                case 5:
                    Assert.AreEqual(address, UnitRequest.Triggers(1001));
                    return new NotificationTriggerResponse(
                        NotificationTriggerItem.StateTrigger(onNotificationAction: "300|Trigger1", parentId: "1001"),
                        NotificationTriggerItem.StateTrigger(onNotificationAction: "301|Trigger2", parentId: "1001"),
                        NotificationTriggerItem.StateTrigger(onNotificationAction: "302|Trigger3", parentId: "1001")
                    );

                //Probe -> Devices -> Sensors
                case 6:
                    Assert.AreEqual(address, UnitRequest.Sensors("filter_parentid=3001"));
                    return new SensorResponse(
                        new SensorItem(name: "Sensor2", objid: "4002", position: "20", positionRaw: "20"),
                        new SensorItem(name: "Sensor1", objid: "4001", position: "10", positionRaw: "10")
                    );

                case 7:
                    Assert.AreEqual(address, UnitRequest.Sensors("filter_parentid=3002"));
                    return new SensorResponse();

                //Probe -> Groups -> Devices
                case 8:
                    Assert.AreEqual(address, UnitRequest.Devices("filter_parentid=2001"));
                    return new DeviceResponse();

                //Probe -> Groups -> Groups
                case 9:
                    Assert.AreEqual(address, UnitRequest.Groups("filter_parentid=2001"));
                    return new GroupResponse();

                default:
                    throw UnknownRequest(address);
            }
        }
    }
}
