using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class FastPathGroupsOnlyScenario : TreeScenario
    {
        protected override IWebResponse GetResponse(string address)
        {
            switch (requestNum)
            {
                case 1:
                    Assert.AreEqual(UnitRequest.Groups(), address);
                    return new GroupResponse(new GroupItem(objid: "0", name: "Root"), new GroupItem(objid: "3001", name: "Servers", parentId: "1001"));

                default:
                    throw UnknownRequest(address);
            }
        }
    }
}