using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class TakeFilteredSensorsFromGroupNoRecurseScenario : TakeScenario
    {
        protected override IWebResponse GetResponse(string address, Content content)
        {
            switch (requestNum)
            {
                case 1: //Get a group
                    Assert.AreEqual(UnitRequest.Groups("count=1", UrlFlag.Columns), address);
                    return new GroupResponse(new GroupItem());

                case 2: //Get 2 sensors under the group "Windows Infrastructure"
                    Assert.AreEqual(UnitRequest.Sensors("count=2&filter_name=@sub(ping)&filter_group=Windows+Infrastructure", UrlFlag.Columns), address);
                    return new SensorResponse(new SensorItem(name: "First"), new SensorItem(name: "Second"));

                case 3: //Get total number of sensors
                    Assert.AreEqual(UnitRequest.Sensors("count=0&filter_name=@sub(ping)&filter_group=Windows+Infrastructure", null), address);
                    return new SensorResponse(Enumerable.Range(0, 600).Select(i => new SensorItem()).ToArray());

                case 4: //Get next 2 sensors
                    Assert.AreEqual(UnitRequest.Sensors("count=2&filter_name=@sub(ping)&filter_group=Windows+Infrastructure&start=2", UrlFlag.Columns), address);
                    return new SensorResponse(
                        new SensorItem(name: "Pong1"), //Skipped by BaseResponse
                        new SensorItem(name: "Pong2"), //Skipped by BaseResponse
                        new SensorItem(name: "Ping"),
                        new SensorItem(name: "Pong3")
                    );

                case 5: //Request a whole page at a time
                case 6: //Still haven't gotten all the sensors we want. Ask for 500 sensors then
                    Assert.AreEqual(UnitRequest.Sensors("count=500&filter_name=@sub(ping)&filter_group=Windows+Infrastructure&start=4", UrlFlag.Columns), address);
                    return new SensorResponse(Enumerable.Range(0, 504).Select(i =>
                    {
                        if (i == 500)
                            return new SensorItem(name: "Ping");

                        return new SensorItem();
                    }).ToArray());

                default:
                    throw UnknownRequest(address);
            }
        }
    }
}
