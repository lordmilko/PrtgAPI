using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class SlowPathSensorsOnlyScenario : TreeScenario
    {
        protected override IWebResponse GetResponse(string address)
        {
            switch (requestNum)
            {
                case 1:
                    Assert.AreEqual(UnitRequest.Objects("filter_objid=3001"), address);
                    return new DeviceResponse(new DeviceItem(objid: "3001"));

                case 2:
                    Assert.AreEqual(UnitRequest.Devices("filter_objid=3001"), address);
                    return new DeviceResponse(new DeviceItem(objid: "3001"));

                case 3:
                    Assert.AreEqual(UnitRequest.Sensors("filter_parentid=3001"), address);
                    return new SensorResponse(new SensorItem(objid: "4001", parentId: "3001"));

                default:
                    throw UnknownRequest(address);
            }
        }
    }
}