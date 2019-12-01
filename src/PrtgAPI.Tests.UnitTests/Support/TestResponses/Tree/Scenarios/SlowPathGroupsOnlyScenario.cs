using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class SlowPathGroupsOnlyScenario : TreeScenario
    {
        protected override IWebResponse GetResponse(string address)
        {
            switch (requestNum)
            {
                case 1:
                    Assert.AreEqual(UnitRequest.Objects("filter_objid=1001"), address);
                    return new ProbeResponse(new ProbeItem(objid: "1001"));

                case 2:
                    Assert.AreEqual(UnitRequest.Probes("filter_objid=1001&filter_parentid=0"), address);
                    return new ProbeResponse(new ProbeItem(objid: "1001"));

                case 3:
                    Assert.AreEqual(UnitRequest.Groups("filter_parentid=1001"), address);
                    return new GroupResponse(new GroupItem(objid: "3001", name: "Servers", parentId: "1001"));

                case 4:
                    Assert.AreEqual(UnitRequest.Groups("filter_parentid=3001"), address);
                    return new GroupResponse();

                default:
                    throw UnknownRequest(address);
            }
        }
    }
}