using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class TakeFilteredLogsInsufficientOneLeftForceStreamScenario : TakeScenario
    {
        protected override IWebResponse GetResponse(string address, Content content)
        {
            switch (requestNum)
            {
                case 1: //Request how many objects exist
                    Assert.AreEqual(UnitRequest.Logs("count=1&columns=objid,name&filter_name=ping", null), address);
                    return new MessageResponse(Enumerable.Range(0, 1).Select(i => new MessageItem()).ToArray());
                case 2: //Request the 1 remaining log
                    Assert.AreEqual(UnitRequest.Logs("count=1&start=1&filter_name=ping", UrlFlag.Columns), address);
                    return new MessageResponse(
                            new MessageItem("Ping"),
                            new MessageItem("Pong")
                        )
                        { Stream = true };
                default:
                    throw UnknownRequest(address);
            }
        }
    }
}
