
namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class GroupRecurseAvailableCount : GroupDeepNestingScenario
    {
        protected override IWebResponse GetResponse(string address, Content content)
        {
            switch (requestNum)
            {
                case 1: //Get the parent group "Windows Servers"
                    AssertGroupRequest(address, content, "filter_name=@sub(Windows)");

                    return GetGroupResponse(WindowsServers);
                case 2: //Get all groups under the "Windows Servers" group. Returns 3 groups, satisfying the request for 1 of 2 groups
                        //since we only yield the first child, then try and look for the next two under him
                        //(we don't yield all of the children of a given parent at once together)
                    AssertGroupRequest(address, content, "filter_parentid=2002");

                    return GetGroupResponse(DomainExchangeSqlServers);
                case 3: //Get all groups under the "Domain Controllers" group, looking for 1 more group to satisfy the count
                    AssertGroupRequest(address, content, "filter_parentid=2003");

                    return GetGroupResponse(DomainControllerDCs);
                default:
                    throw UnknownRequest(address);
            }
        }

        protected IWebResponse GetOffByOneBaseResponse(string address, Content content)
        {
            requestNum++;
            var response = base.GetResponse(address, content);
            requestNum--;
            return response;
        }
    }
}
