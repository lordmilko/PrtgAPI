
namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class GroupRecurseAvailableSingleCount : GroupRecurseAvailableCount
    {
        protected override IWebResponse GetResponse(string address, Content content)
        {
            switch (requestNum)
            {
                case 1:
                case 2:
                    return base.GetResponse(address, content);
                default:
                    throw UnknownRequest(address);
            }
        }
    }
}
