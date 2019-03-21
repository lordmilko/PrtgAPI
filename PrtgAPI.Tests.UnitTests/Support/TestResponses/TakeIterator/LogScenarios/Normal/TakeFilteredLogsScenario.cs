using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class TakeFilteredLogsScenario : TakeScenario
    {
        protected override IWebResponse GetResponse(string address, Content content)
        {
            switch (requestNum)
            {
                case 1:
                    Assert.AreEqual(UnitRequest.Logs("count=2&start=1&filter_name=ping", UrlFlag.Columns), address);
                    return new MessageResponse(new MessageItem("Ping"), new MessageItem("Ping"), new MessageItem("Ping"));

                default:
                    throw UnknownRequest(address);
            }
        }
    }
}
