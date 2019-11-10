using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class StandaloneContainerScenario : TreeScenario
    {
        protected override IWebResponse GetResponse(string address, Content content)
        {
            switch (requestNum)
            {
                case 1:
                    Assert.AreEqual(UnitRequest.Groups("filter_objid=0"), address);
                    return new GroupResponse(new GroupItem(name: "Root", objid: "0"));

                case 2:
                    Assert.AreEqual(UnitRequest.Probes("filter_parentid=0"), address);
                    return new ProbeResponse();

                case 3:
                    Assert.AreEqual(UnitRequest.Triggers(1), address);
                    return new NotificationTriggerResponse();

                default:
                    throw UnknownRequest(address);
            }
        }
    }
}
