using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class FastPathPropertiesOnlyScenario : TreeScenario
    {
        protected override IWebResponse GetResponse(string address)
        {
            switch (requestNum)
            {
                case 1:
                    Assert.AreEqual(UnitRequest.Groups("filter_objid=0"), address);
                    return new GroupResponse(new GroupItem(objid: "0", name: "Root", notifiesx: "Inherited 1 State"));

                case 2:
                    Assert.AreEqual(UnitRequest.RequestObjectData(0), address);
                    return new SensorSettingsResponse();

                default:
                    throw UnknownRequest(address);
            }
        }
    }
}