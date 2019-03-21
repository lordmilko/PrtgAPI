using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class TakeFilteredLogsInsufficientOneLeftScenario : TakeScenario
    {
        protected override IWebResponse GetResponse(string address, Content content)
        {
            switch (requestNum)
            {
                case 1: //Request 2 ping sensors. We instead return 2 "pong" logs
                    Assert.AreEqual(UnitRequest.Logs("count=2&start=1&filter_name=ping", UrlFlag.Columns), address);
                    return new MessageResponse(new MessageItem("Pong1"), new MessageItem("Pong2"));

                case 2: //We're going to have to stream. Request how many objects exist
                    Assert.AreEqual(UnitRequest.Logs("count=1&columns=objid,name&filter_name=ping", null), address);
                    return new MessageResponse(Enumerable.Range(0, 3).Select(i => new MessageItem()).ToArray());

                case 3: //Request the 1 remaining log
                    Assert.AreEqual(UnitRequest.Logs("count=1&start=3&filter_name=ping", UrlFlag.Columns), address);
                    return new MessageResponse(
                        new MessageItem("Pong1 Again"), //Skipped by BaseResponse
                        new MessageItem("Pong2 Again"), //Skipped by BaseResponse
                        new MessageItem("Ping")
                    ) {Stream = true};

                default:
                    throw UnknownRequest(address);
            }
        }
    }
}
