using System.Linq;
using PrtgAPI.Tests.UnitTests.TreeNodes;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class GroupUniqueChildGroupScenario : GroupUniqueGroupScenario
    {
        public GroupUniqueChildGroupScenario()
        {
            probe = new ProbeNode("Local Probe",
                new GroupNode("Servers",
                    new GroupNode("Windows Servers",
                        new GroupNode("Domain Controllers",
                            new DeviceNode("dc-1",
                                new SensorNode("Ping"),
                                new SensorNode("CPU Load")
                            ),
                            new DeviceNode("dc-2",
                                new SensorNode("Ping"),
                                new SensorNode("CPU Load")
                            )
                        )
                    )
                ),
                new GroupNode("Clients",
                    new DeviceNode("wks-1",
                        new SensorNode("Ping")
                    )
                )
            );
        }

        protected override IWebResponse GetResponse(string address, Content content)
        {
            switch (requestNum)
            {
                case 1: //Get all groups. We say there is only one group, named "Servers"
                    return base.GetResponse(address, content);

                case 2: //Get all groups under the parent group (returns "Windows Servers")
                    AssertGroupRequest(address, content, "filter_parentid=2000");

                    return GetGroupResponse(probe.Groups.First().Groups);
                case 3: //Get all groups under the child group (returns "Domain Controllers")
                    AssertGroupRequest(address, content, "filter_parentid=2002");

                    return GetGroupResponse(probe.Groups.First().Groups.First().Groups);
                default:
                    throw UnknownRequest(address);
            }
        }
    }
}
