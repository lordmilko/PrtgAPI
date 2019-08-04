
namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class GroupNoRecurseUnavailableCount : GroupRecurseAvailableCount
    {
        protected override IWebResponse GetResponse(string address, Content content)
        {
            switch (requestNum)
            {
                case 1: //Get the parent group "Windows Servers"
                    return base.GetResponse(address, content);
                case 2: //Get 2 children from under the parent group
                    AssertGroupRequest(address, content, "count=500&filter_parentid=2002", UrlFlag.Columns);

                    return GetGroupResponse(DomainExchangeSqlServers);
                default:
                    throw UnknownRequest(address);
            }
        }
    }
}
