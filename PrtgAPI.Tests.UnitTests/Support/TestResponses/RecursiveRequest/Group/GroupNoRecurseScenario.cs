using System.Linq;
using PrtgAPI.Tests.UnitTests.TreeNodes;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class GroupNoRecurseScenario : GroupUniqueGroupScenario
    {
        public GroupNoRecurseScenario()
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
                new GroupNode("Servers",
                    new GroupNode("Linux Servers",
                        new DeviceNode("arch-1",
                            new SensorNode("Ping")
                        )
                    )
                )
            );
        }

        protected override IWebResponse GetResponse(string address, Content content)
        {
            switch (requestNum)
            {
                case 1: //Get the parent group "Servers"
                    return base.GetResponse(address, content);
                case 2: //Execute a normal request including all search filters
                    AssertGroupRequest(address, content, "filter_name=@sub()&filter_parentid=2000");

                    return GetGroupResponse(probe.Groups.First().Groups);
                default:
                    throw UnknownRequest(address);
            }
        }
    }
}
