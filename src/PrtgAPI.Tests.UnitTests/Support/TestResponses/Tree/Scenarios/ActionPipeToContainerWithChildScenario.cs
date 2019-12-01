using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class ActionPipeToContainerWithChildScenario : TreeScenario
    {
        private MultiTypeResponse standardResponse = new MultiTypeResponse();

        protected override IWebResponse GetResponse(string address)
        {
            switch (requestNum)
            {
                case 1:
                    Assert.AreEqual(UnitRequest.Groups("count=1", UrlFlag.Columns), address);
                    return new BasicResponse(standardResponse.GetResponseText(ref address));

                case 2:
                    Assert.AreEqual(UnitRequest.Groups("filter_objid=9999"), address);
                    return new GroupResponse(new GroupItem(objid: "2000", groupnum: "0", groupnumRaw: "0", devicenum: "1", devicenumRaw: "1"));

                //Tree

                case 3:
                    Assert.AreEqual(UnitRequest.Devices("filter_parentid=2000"), address);
                    return new DeviceResponse(new DeviceItem(objid: "3001"), new DeviceItem(objid: "3002"));

                case 4:
                    Assert.AreEqual(UnitRequest.Sensors("filter_parentid=3001"), address);
                    return new SensorResponse(new SensorItem(objid: "4001"));

                case 5:
                    Assert.AreEqual(UnitRequest.Sensors("filter_parentid=3002"), address);
                    return new SensorResponse(new SensorItem(objid: "4002"));

                default:
                    throw UnknownRequest(address);
            }
        }
    }
}
