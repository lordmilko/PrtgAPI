using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class TakeFilteredSensorsFromGroupScenario : TakeScenario
    {
        protected override IWebResponse GetResponse(string address, Content content)
        {
            switch (requestNum)
            {
                case 1: //Get a group
                    Assert.AreEqual(UnitRequest.Groups("count=1", UrlFlag.Columns), address);
                    return new GroupResponse(new GroupItem());

                case 2: //Are there any other groups called "Windows Infrastructure"?
                    Assert.AreEqual(UnitRequest.Groups("count=*&filter_name=Windows+Infrastructure", UrlFlag.Columns), address);
                    return new GroupResponse(new GroupItem());

                case 3: //Get all sensors under the group "Windows Infrastructure"
                    Assert.AreEqual(UnitRequest.Sensors("count=*&filter_name=@sub(ping)&filter_group=Windows+Infrastructure", UrlFlag.Columns), address);
                    return new SensorResponse(new SensorItem(name: "First"), new SensorItem(name: "Ping"));

                case 4: //Get the child groups of "Windows Infrastructure"
                    Assert.AreEqual(UnitRequest.Groups("count=*&filter_parentid=2211", UrlFlag.Columns), address);
                    return new GroupResponse(new GroupItem(name: "Child Group"));

                case 5: //Are there any other groups called "Child Group"?
                    Assert.AreEqual(UnitRequest.Groups("count=*&filter_name=Child+Group", UrlFlag.Columns), address);
                    return new GroupResponse(new GroupItem(name: "Child Group"));

                case 6: //Get all sensors under the group "Child Group"
                    Assert.AreEqual(UnitRequest.Sensors("count=*&filter_name=@sub(ping)&filter_group=Child+Group", UrlFlag.Columns), address);
                    return new SensorResponse(new SensorItem(name: "Second"), new SensorItem(name: "Ping"), new SensorItem(name: "Ping"));

                default:
                    throw UnknownRequest(address);
            }
        }
    }
}