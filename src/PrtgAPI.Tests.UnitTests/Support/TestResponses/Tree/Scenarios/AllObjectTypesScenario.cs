using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class AllObjectTypesScenario : TreeScenario
    {
        protected override IWebResponse GetResponse(string address)
        {
            switch (requestNum)
            {
                case 1:
                    Assert.AreEqual(UnitRequest.Probes("filter_parentid=0"), address);
                    return new ProbeResponse(new ProbeItem(objid: "1001", groupnum: "2", groupnumRaw: "2", devicenum: "3", devicenumRaw: "3", notifiesx: "Inherited"));

                case 2:
                    Assert.AreEqual(UnitRequest.Groups(), address);
                    return new GroupResponse(
                        new GroupItem(objid: "0", parentId: "-1000", name: "Root"),
                        new GroupItem(objid: "2001", parentId: "1001", groupnum: "1", groupnumRaw: "1", devicenum: "2", devicenumRaw: "2"),
                        new GroupItem(objid: "2002", parentId: "2001", name: "VMware", groupnum: "0", groupnumRaw: "0", devicenum: "1", devicenumRaw: "1")
                    );

                case 3:
                    Assert.AreEqual(UnitRequest.Devices(), address);
                    return new DeviceResponse(
                        new DeviceItem(objid: "3001", parentId: "1001", totalsens: "2", totalsensRaw: "2"),
                        new DeviceItem(objid: "3002", parentId: "2001", totalsens: "2", totalsensRaw: "2"),
                        new DeviceItem(objid: "3003", parentId: "2002", totalsens: "2", totalsensRaw: "2")
                    );

                case 4:
                    Assert.AreEqual(UnitRequest.Sensors(), address);
                    return new SensorResponse(
                        new SensorItem(objid: "4001", parentId: "3001"),
                        new SensorItem(objid: "4002", parentId: "3002"),
                        new SensorItem(objid: "4003", parentId: "3003")
                    );

                case 5:
                    Assert.AreEqual(UnitRequest.RequestObjectData(4001), address);
                    return new SensorSettingsResponse();

                case 6:
                    Assert.AreEqual(UnitRequest.RequestObjectData(3001), address);
                    return new SensorSettingsResponse();

                case 7:
                    Assert.AreEqual(UnitRequest.RequestObjectData(4002), address);
                    return new SensorSettingsResponse();

                case 8:
                    Assert.AreEqual(UnitRequest.RequestObjectData(3002), address);
                    return new SensorSettingsResponse();

                case 9:
                    Assert.AreEqual(UnitRequest.RequestObjectData(4003), address);
                    return new SensorSettingsResponse();

                case 10:
                    Assert.AreEqual(UnitRequest.RequestObjectData(3003), address);
                    return new SensorSettingsResponse();

                case 11:
                    Assert.AreEqual(UnitRequest.RequestObjectData(2002), address);
                    return new SensorSettingsResponse();

                case 12:
                    Assert.AreEqual(UnitRequest.RequestObjectData(2001), address);
                    return new SensorSettingsResponse();

                case 13:
                    Assert.AreEqual(UnitRequest.RequestObjectData(1001), address);
                    return new SensorSettingsResponse();

                case 14:
                    Assert.AreEqual(UnitRequest.RequestObjectData(0), address);
                    return new SensorSettingsResponse();

                default:
                    throw UnknownRequest(address);
            }
        }
    }
}
