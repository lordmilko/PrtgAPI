using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class TakeUnfilteredLogsForceStreamScenario : TakeScenario
    {
        protected override IWebResponse GetResponse(string address, Content content)
        {
            switch (requestNum)
            {
                case 1:
                    Assert.AreEqual(UnitRequest.LogCount, address);
                    return GetTotalLogsResponse();

                case 2:
                    Assert.AreEqual(UnitRequest.Logs("count=2&start=1", UrlFlag.Columns), address);
                    return new MessageResponse(new MessageItem(), new MessageItem(), new MessageItem());

                default:
                    throw UnknownRequest(address);
            }
        }
    }
}
