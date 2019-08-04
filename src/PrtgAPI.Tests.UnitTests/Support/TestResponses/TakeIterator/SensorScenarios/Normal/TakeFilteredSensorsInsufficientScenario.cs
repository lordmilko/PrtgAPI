using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class TakeFilteredSensorsInsufficientScenario : TakeScenario
    {
        protected override IWebResponse GetResponse(string address, Content content)
        {
            switch (requestNum)
            {
                case 1: //Request 2 ping sensors. We instead return 2 "pong" sensors
                    Assert.AreEqual(UnitRequest.Sensors("count=2&filter_name=ping", UrlFlag.Columns), address);
                    return new SensorResponse(new SensorItem(name: "Pong1"), new SensorItem(name: "Pong2"));

                case 2: //We're going to have to stream. Request how many objects exist
                    Assert.AreEqual(UnitRequest.Sensors("count=0&filter_name=ping", null), address);
                    return new SensorResponse(Enumerable.Range(0, 600).Select(i => new SensorItem()).ToArray());

                case 3: //Request the next 2 sensors
                    Assert.AreEqual(UnitRequest.Sensors("count=2&filter_name=ping&start=2", UrlFlag.Columns), address);
                    return new SensorResponse(
                        new SensorItem(name: "Pong1"), //Skipped by BaseResponse
                        new SensorItem(name: "Pong2"), //Skipped by BaseResponse
                        new SensorItem(name: "Pong3"),
                        new SensorItem(name: "Pong4")
                    );

                case 4: //Still haven't gotten all the sensors we want. Ask for 500 sensors then
                    Assert.AreEqual(UnitRequest.Sensors("count=500&filter_name=ping&start=4", UrlFlag.Columns), address);
                    return new SensorResponse(Enumerable.Range(0, 504).Select(i =>
                    {
                        if (i == 500)
                            return new SensorItem(name: "Ping");

                        return new SensorItem();
                    }).ToArray());

                case 5: //Still haven't gotten all the sensors we want. Ask for the remaining 96
                    Assert.AreEqual(UnitRequest.Sensors("count=96&filter_name=ping&start=504", UrlFlag.Columns), address);
                    return new SensorResponse(Enumerable.Range(0, 600).Select(i => new SensorItem()).ToArray());

                default:
                    throw UnknownRequest(address);
            }
        }
    }
}