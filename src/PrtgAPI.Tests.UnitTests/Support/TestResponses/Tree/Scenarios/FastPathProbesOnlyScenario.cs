using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class FastPathProbesOnlyScenario : TreeScenario
    {
        protected override IWebResponse GetResponse(string address)
        {
            switch (requestNum)
            {
                case 1: //We want probes, so we get probes!
                    Assert.AreEqual(UnitRequest.Probes("filter_parentid=0"), address);
                    return new ProbeResponse(new ProbeItem(objid: "1001", groupnum: "0", groupnumRaw: "0", devicenum: "0", devicenumRaw: "0"));

                case 2: //Requesting any objects without the root node is incoherent
                    Assert.AreEqual(UnitRequest.Groups("filter_objid=0"), address);
                    return new GroupResponse(new GroupItem(name: "Root", objid: "0"));

                default:
                    throw UnknownRequest(address);
            }
        }
    }
}
