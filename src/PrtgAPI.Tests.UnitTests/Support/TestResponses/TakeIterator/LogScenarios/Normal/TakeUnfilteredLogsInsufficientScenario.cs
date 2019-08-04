using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class TakeUnfilteredLogsInsufficientScenario : TakeScenario
    {
        protected override IWebResponse GetResponse(string address, Content content)
        {
            switch (requestNum)
            {
                case 1:
                    Assert.AreEqual(UnitRequest.Logs("count=2&start=1", UrlFlag.Columns), address);
                    return new MessageResponse(new MessageItem());

                default:
                    throw UnknownRequest(address);
            }
        }
    }
}
