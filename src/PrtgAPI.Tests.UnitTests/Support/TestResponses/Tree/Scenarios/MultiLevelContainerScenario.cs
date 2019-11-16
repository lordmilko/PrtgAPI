using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class MultiLevelContainerScenario : TreeScenario
    {
        private bool async;

        public MultiLevelContainerScenario(bool async)
        {
            this.async = async;
        }

        protected override IWebResponse GetResponse(string address, Content content)
        {
            switch (requestNum)
            {
                case 1:
                    Assert.AreEqual(UnitRequest.Groups("filter_objid=0"), address);
                    return new GroupResponse(new GroupItem(name: "Root", objid: "0"));

                case 2: //Probes
                    Assert.AreEqual(UnitRequest.Probes("filter_parentid=0"), address);
                    return new ProbeResponse(new ProbeItem(objid: "1001", groupnum: "2", groupnumRaw: "2", devicenum: "3", devicenumRaw: "3"));

                case 3: //Probe -> Device
                    Assert.AreEqual(UnitRequest.Devices("filter_parentid=1001"), address);
                    return new DeviceResponse(new DeviceItem(objid: "3001", totalsens: "2", totalsensRaw: "2"));

                case 4: //Probe -> Group
                    Assert.AreEqual(UnitRequest.Groups("filter_parentid=1001"), address);
                    return new GroupResponse(new GroupItem(objid: "2001", groupnum: "1", groupnumRaw: "1", devicenum: "2", devicenumRaw: "2"));

                case 5: //Probe -> Device -> Sensor
                    Assert.AreEqual(UnitRequest.Sensors("filter_parentid=3001"), address);
                    return new SensorResponse(new SensorItem(objid: "4001"));

                case 6: //Probe -> Group -> Device
                    Assert.AreEqual(UnitRequest.Devices("filter_parentid=2001"), address);
                    return new DeviceResponse(new DeviceItem(objid: "3002", totalsens: "2", totalsensRaw: "2"));

                case 7: //Probe -> Group -> Group
                    Assert.AreEqual(UnitRequest.Groups("filter_parentid=2001"), address);
                    return new GroupResponse(new GroupItem(objid: "2002", name: "VMware", groupnum: "0", groupnumRaw: "0", devicenum: "1", devicenumRaw: "1"));

                case 8: //Probe -> Group -> Device -> Sensor
                    Assert.AreEqual(UnitRequest.Sensors("filter_parentid=3002"), address);
                    return new SensorResponse(new SensorItem(objid: "4002"));

                case 9: //Probe -> Group -> Group -> Device
                    Assert.AreEqual(UnitRequest.Devices("filter_parentid=2002"), address);
                    return new DeviceResponse(new DeviceItem(objid: "3003", totalsens: "2", totalsensRaw: "2"));

                case 10: //Probe -> Group -> Group -> Sensor
                    Assert.AreEqual(UnitRequest.Sensors("filter_parentid=3003"), address);
                    return new SensorResponse(new SensorItem(objid: "4003"));

                case 11:
                    Assert.AreEqual(UnitRequest.Triggers(1001), address);
                    return new NotificationTriggerResponse();

                case 12:
                    if (!async)
                        goto default;
                    Assert.AreEqual(UnitRequest.Notifications(), address);
                    return new NotificationActionResponse(new NotificationActionItem());

                default:
                    throw UnknownRequest(address);
            }
        }
    }
}
