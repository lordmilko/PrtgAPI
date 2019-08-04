using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class TakeFilteredSensorsFromDuplicateGroupInsufficientScenario : TakeScenario
    {
        protected override IWebResponse GetResponse(string address, Content content)
        {
            switch (requestNum)
            {
                case 1: //Get a group
                    Assert.AreEqual(UnitRequest.Groups("count=1", UrlFlag.Columns), address);
                    return new GroupResponse(new GroupItem(objid: "2000"));

                case 2: //Are there any other groups called "Windows Infrastructure"?
                    Assert.AreEqual(UnitRequest.Groups("count=*&filter_name=Windows+Infrastructure", UrlFlag.Columns), address);
                    return new GroupResponse(new GroupItem(objid: "2000"), new GroupItem(objid: "2050"));

                case 3: //Get all devices under the group "Windows Infrastructure" that we're piping from
                    Assert.AreEqual(UnitRequest.Devices("count=*&filter_parentid=2000", UrlFlag.Columns), address);
                    return new DeviceResponse(new DeviceItem(name: "Device1", objid: "3000"), new DeviceItem(name: "Device2", objid: "3001"));

                case 4: //Get all sensors from the child devices
                    Assert.AreEqual(UnitRequest.Sensors("count=*&filter_name=@sub(ping)&filter_parentid=3000&filter_parentid=3001", UrlFlag.Columns), address);
                    return new SensorResponse(new SensorItem(name: "Ping", objid: "4000"), new SensorItem(name: "Pong2", objid: "4001"));

                case 5: //Get all child groups under "Windows Infrastructure
                    Assert.AreEqual(UnitRequest.Groups("count=*&filter_parentid=2000", UrlFlag.Columns), address);
                    return new GroupResponse(new GroupItem(name: "Child Group", objid: "2001", totalsens: "0"));

                default:
                    throw UnknownRequest(address);
            }
        }
    }
}
