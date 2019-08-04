using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class TakeFilteredLogsInsufficientNoneLeftForceStreamScenario : TakeScenario
    {
        protected override IWebResponse GetResponse(string address, Content content)
        {
            switch (requestNum)
            {
                case 1: //Request how many objects exist
                    Assert.AreEqual(UnitRequest.Logs("count=1&columns=objid,name&filter_name=ping", null), address);
                    return new MessageResponse(Enumerable.Range(0, 0).Select(i => new MessageItem()).ToArray());
                default:
                    throw UnknownRequest(address);
            }
        }
    }
}
