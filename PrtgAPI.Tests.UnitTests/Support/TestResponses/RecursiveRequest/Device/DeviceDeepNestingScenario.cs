using System.Linq;
using PrtgAPI.Tests.UnitTests.TreeNodes;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class DeviceDeepNestingScenario : DeviceUniqueChildGroupScenario
    {
        public DeviceDeepNestingScenario()
        {
            probe = new ProbeNode("Local Probe",
                new GroupNode("Servers",
                    new GroupNode("Linux Servers",
                        new DeviceNode("dc-1",
                            new SensorNode("Ping"),
                            new SensorNode("CPU Load")
                        ),
                        new DeviceNode("dc-2",
                                new SensorNode("Ping"),
                                new SensorNode("CPU Load")
                        ),
                        new GroupNode("Old Linux Servers",
                            new DeviceNode("old-arch-1"),
                            new GroupNode("Decomissioned Linux Servers",
                                new DeviceNode("old-arch-2")
                            )
                        )
                    )
                ),
                new GroupNode("Servers",
                    new DeviceNode("exch-1",
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
                case 2: //Get all devices under the "Servers" group
                case 3: //Get all groups under the "Servers" group
                case 4: //Get all devices under the "Linux Servers" group
                    return base.GetResponse(address, content);
                case 5: //Get all groups under the group "Linux Servers"
                    AssertGroupRequest(address, content, "filter_parentid=2002");

                    return GetGroupResponse(probe.Groups.First().Groups.First().Groups);
                case 6: //Get all devices under the group "Old Linux Servers"
                    AssertDeviceRequest(address, content, "filter_name=@sub()&filter_parentid=2003");

                    return GetDeviceResponse(probe.Groups.First().Groups.First().Groups.First().Devices);
                case 7: //Get all groups under the group "Old Linux Servers"
                    AssertGroupRequest(address, content, "filter_parentid=2003");

                    return GetGroupResponse(probe.Groups.First().Groups.First().Groups.First().Groups);
                case 8: //Get all devices under the group "Decomissioned Linux Servers"
                    AssertDeviceRequest(address, content, "filter_name=@sub()&filter_parentid=2004");

                    return GetDeviceResponse(probe.Groups.First().Groups.First().Groups.First().Groups.First().Devices);
                default:
                    throw UnknownRequest(address);
            }
        }
    }
}
