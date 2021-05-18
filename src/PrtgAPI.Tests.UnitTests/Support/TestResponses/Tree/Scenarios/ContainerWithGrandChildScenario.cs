using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class ContainerWithGrandChildScenario : TreeScenario
    {
        protected override IWebResponse GetResponse(string address)
        {
            switch (requestNum)
            {
                case 1: //Resolve Object
                    Assert.AreEqual(UnitRequest.Objects("filter_objid=1001"), address);
                    return new ProbeResponse(new ProbeItem(objid: "1001", groupnum: "2", groupnumRaw: "2", devicenum: "3", devicenumRaw: "3"));

                case 2:
                    Assert.AreEqual(UnitRequest.Probes("filter_objid=1001&filter_parentid=0"), address);
                    return new ProbeResponse(new ProbeItem(objid: "1001", groupnum: "1", groupnumRaw: "1", devicenum: "0", devicenumRaw: "0"));

                case 3:
                    Assert.AreEqual(UnitRequest.Groups("filter_parentid=1001"), address);
                    return new GroupResponse(new GroupItem(objid: "2002", groupnum: "0", groupnumRaw: "0", devicenum: "1", devicenumRaw: "1"));

                case 4:
                    Assert.AreEqual(UnitRequest.Devices("filter_parentid=2002"), address);
                    return new DeviceResponse(new DeviceItem(totalsens: "0", totalsensRaw: "0"));

                default:
                    throw UnknownRequest(address);
            }
        }
    }
}
