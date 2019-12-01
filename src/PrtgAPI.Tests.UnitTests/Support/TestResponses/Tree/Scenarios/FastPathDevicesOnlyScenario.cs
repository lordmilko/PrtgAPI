using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class FastPathDevicesOnlyScenario : TreeScenario
    {
        protected override IWebResponse GetResponse(string address)
        {
            switch (requestNum)
            {
                case 1:
                    Assert.AreEqual(UnitRequest.Groups("filter_objid=0"), address);
                    return new GroupResponse(new GroupItem(objid: "0", name: "Root"));

                case 2:
                    Assert.AreEqual(UnitRequest.Devices(), address);
                    return new DeviceResponse(new DeviceItem(objid: "2001", parentId: "1001"));

                default:
                    throw UnknownRequest(address);
            }
        }
    }
}