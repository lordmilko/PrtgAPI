using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class ObjectPipeToContainerWithChildScenario : TreeScenario
    {
        private MultiTypeResponse standardResponse = new MultiTypeResponse();

        protected override IWebResponse GetResponse(string address)
        {
            switch (requestNum)
            {
                case 1:
                    Assert.AreEqual(UnitRequest.Probes("count=1&filter_parentid=0", UrlFlag.Columns), address);
                    return new BasicResponse(standardResponse.GetResponseText(ref address));

                //Probe 1

                case 2:
                    Assert.AreEqual(UnitRequest.Groups("filter_probe=127.0.0.10"), address);
                    return new BasicResponse(standardResponse.GetResponseText(ref address));

                //Tree

                case 3:
                    Assert.AreEqual(UnitRequest.Devices("filter_parentid=2000"), address);
                    return new DeviceResponse(new DeviceItem(objid: "3000", totalsens: "1", totalsensRaw: "1"));

                case 4:
                    Assert.AreEqual(UnitRequest.Sensors("filter_parentid=3000"), address);
                    return new SensorResponse(new SensorItem(objid: "4000"));

                case 5:
                    Assert.AreEqual(UnitRequest.Devices("filter_parentid=2001"), address);
                    return new DeviceResponse(new DeviceItem(objid: "3001", totalsens: "1", totalsensRaw: "1"));

                case 6:
                    Assert.AreEqual(UnitRequest.Sensors("filter_parentid=3001"), address);
                    return new SensorResponse(new SensorItem(objid: "4001"));

                default:
                    throw UnknownRequest(address);
            }
        }
    }
}
