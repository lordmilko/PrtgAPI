using System.Linq;
using PrtgAPI.Tests.UnitTests.TreeNodes;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class GroupUniqueGroupScenario : GroupScenario
    {
        public GroupUniqueGroupScenario()
        {
            probe = new ProbeNode("Local Probe",
                new GroupNode("Servers",
                    new GroupNode("Windows Servers",
                        new DeviceNode("dc-1",
                            new SensorNode("Ping"),
                            new SensorNode("CPU Load")
                        ),
                        new DeviceNode("dc-2",
                            new SensorNode("Ping"),
                            new SensorNode("CPU Load")
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
                case 1: //Get the "Servers" group
                    AssertGroupRequest(address, content, "filter_name=Servers");

                    return new GroupResponse(probe.Groups.First().GetTestItem());

                case 2: //Get all groups under the "Servers" group
                    AssertGroupRequest(address, content, "filter_parentid=2000");

                    return GetGroupResponse(probe.Groups.First().Groups);
                default:
                    throw UnknownRequest(address);
            }
        }
    }
}
