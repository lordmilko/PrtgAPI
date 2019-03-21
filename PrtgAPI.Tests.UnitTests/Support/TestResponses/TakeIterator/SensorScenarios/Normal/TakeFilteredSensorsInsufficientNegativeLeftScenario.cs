using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class TakeFilteredSensorsInsufficientNegativeLeftScenario : TakeScenario
    {
        protected override IWebResponse GetResponse(string address, Content content)
        {
            switch (requestNum)
            {
                case 1: //Request 2 ping sensors. We instead return 2 "pong" sensors
                    Assert.AreEqual(UnitRequest.Sensors("count=2&filter_name=ping", UrlFlag.Columns), address);
                    return new SensorResponse(new SensorItem(name: "Ping"), new SensorItem(name: "Pong2"));

                case 2: //We're going to have to stream. Request how many objects exist
                    Assert.AreEqual(UnitRequest.Sensors("count=0&filter_name=ping", null), address);
                    return new SensorResponse(Enumerable.Range(0, 1).Select(i => new SensorItem()).ToArray());

                case 3: //Request the 1 remaining sensor
                    Assert.AreEqual(UnitRequest.Sensors("count=0&filter_name=ping&start=2", UrlFlag.Columns), address);
                    return new SensorResponse(
                        new SensorItem(name: "Ping"), //Skipped by BaseResponse
                        new SensorItem(name: "Pong2")  //Skipped by BaseResponse
                    );

                default:
                    throw UnknownRequest(address);
            }
        }
    }
}
