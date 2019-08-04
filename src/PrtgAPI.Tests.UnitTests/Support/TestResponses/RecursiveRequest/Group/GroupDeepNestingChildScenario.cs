using System.Linq;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class GroupDeepNestingChildScenario : GroupDeepNestingScenario
    {
        protected override IWebResponse GetResponse(string address, Content content)
        {
            switch (requestNum)
            {
                case 1: //Get the "Servers" group
                    return base.GetResponse(address, content);
                case 2: //Get all groups under the "Servers" group
                    AssertGroupRequest(address, content, "filter_parentid=2000");

                    return GetGroupResponse(Servers.Groups);
                case 3: //Get all groups under the "Windows Servers" group.
                    AssertGroupRequest(address, content, "filter_parentid=2002");

                    return GetGroupResponse(WindowsServers.First().Groups);
                case 4: //Get all groups under the "Domain Controllers" group. Say there aren't any
                    AssertGroupRequest(address, content, "filter_parentid=2003");

                    return GetGroupResponse(null);
                case 5: //Get all groups under the "Exchange Servers" group. Say there aren't any
                    AssertGroupRequest(address, content, "filter_parentid=2004");

                    return GetGroupResponse(null);
                case 6: //Get all groups under the "SQL Servers" group. Say there aren't any
                    AssertGroupRequest(address, content, "filter_parentid=2005");

                    return GetGroupResponse(null);
                default:
                    throw UnknownRequest(address);
            }
        }
    }
}
