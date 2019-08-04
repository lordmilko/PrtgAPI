using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class TakeFilteredLogsInsufficientForceStreamScenario : TakeScenario
    {
        protected override IWebResponse GetResponse(string address, Content content)
        {
            switch (requestNum)
            {
                case 1: //Request how many objects exist
                    Assert.AreEqual(UnitRequest.Logs("count=1&columns=objid,name&filter_name=ping", null), address);
                    return new MessageResponse(Enumerable.Range(0, 600).Select(i => new MessageItem()).ToArray());

                case 2: //Request 2 ping logs. We instead return 2 "pong" logs
                    Assert.AreEqual(UnitRequest.Logs("count=2&start=1&filter_name=ping", UrlFlag.Columns), address);
                    return new MessageResponse(new MessageItem("Pong1"), new MessageItem("Pong1"));

                case 3: //Request the next 2 sensors
                    Assert.AreEqual(UnitRequest.Logs("count=2&start=3&filter_name=ping", UrlFlag.Columns), address);
                    return new MessageResponse(
                        new MessageItem("Pong1"), //Skipped by BaseResponse
                        new MessageItem("Pong2"), //Skipped by BaseResponse
                        new MessageItem("Pong3"),
                        new MessageItem("Pong4")
                    );

                case 4: //Still haven't gotten all the sensors we want. Ask for 500 sensors then
                    Assert.AreEqual(UnitRequest.Logs("count=500&start=5&filter_name=ping", UrlFlag.Columns), address);
                    return new MessageResponse(Enumerable.Range(0, 504).Select(i =>
                    {
                        if (i == 500)
                            return new MessageItem("Ping");

                        return new MessageItem();
                    }).ToArray());

                case 5: //Still haven't gotten all the sensors we want. Ask for the remaining 96
                    Assert.AreEqual(UnitRequest.Logs("count=96&start=505&filter_name=ping", UrlFlag.Columns), address);
                    return new MessageResponse(Enumerable.Range(0, 600).Select(i => new MessageItem()).ToArray());

                default:
                    throw UnknownRequest(address);
            }
        }
    }
}
