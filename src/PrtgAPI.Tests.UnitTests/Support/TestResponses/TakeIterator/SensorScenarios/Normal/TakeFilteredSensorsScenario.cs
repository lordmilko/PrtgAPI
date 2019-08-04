using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class TakeFilteredSensorsScenario : TakeScenario
    {
        protected override IWebResponse GetResponse(string address, Content content)
        {
            switch (requestNum)
            {
                case 1:
                    Assert.AreEqual(UnitRequest.Sensors("count=2&filter_name=ping", UrlFlag.Columns), address);
                    return new SensorResponse(new SensorItem(name: "Ping"), new SensorItem(name: "Pong"), new SensorItem(name: "Pong"));

                case 2: //We're going to have to stream. Request how many objects exist
                    Assert.AreEqual(UnitRequest.Sensors("count=0&filter_name=ping", null), address);
                    return new SensorResponse(Enumerable.Range(0, 600).Select(i => new SensorItem()).ToArray());

                case 3: //Request the next 2 sensors
                    Assert.AreEqual(UnitRequest.Sensors("count=1&filter_name=ping&start=2", UrlFlag.Columns), address);
                    return new SensorResponse(
                        new SensorItem(name: "Pong1"), //Skipped by BaseResponse
                        new SensorItem(name: "Pong2"), //Skipped by BaseResponse
                        new SensorItem(name: "Ping")
                    );

                default:
                    throw UnknownRequest(address);
            }
        }
    }
}