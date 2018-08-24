using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.TreeNodes;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class SensorDeepNestingScenario : SensorDuplicateGroupScenario
    {
        public SensorDeepNestingScenario()
        {
            probe = new ProbeNode("Local Probe",
                new GroupNode("Servers",
                    new GroupNode("Linux Servers",
                        new DeviceNode("dc-1",
                            new SensorNode("Ping"),
                            new SensorNode("CPU Load")
                        ),
                        new DeviceNode("dc-2",
                            new SensorNode("Disk Free"),
                            new SensorNode("Memory")
                        ),
                        new GroupNode("Old Linux Servers",
                            new DeviceNode("old-arch-1",
                                new SensorNode("Uptime")
                            ),
                            new GroupNode("Decomissioned Linux Servers",
                                new DeviceNode("old-arch-2",
                                    new SensorNode("Volume IO C:")
                                )
                            )                
                        )
                    )
                ),
                new GroupNode("Servers",
                    new DeviceNode("exch-1",
                        new SensorNode("SSL Security Check")
                    )
                )
            );
        }

        protected GroupNode Servers1 => probe.Groups.First(g => g.Id == 2000);

        protected GroupNode LinuxServers => Servers1.Groups.First(g => g.Id == 2002);

        protected GroupNode OldLinuxServers => LinuxServers.Groups.First(g => g.Id == 2003);

        protected GroupNode DecomissionedLinuxServers => OldLinuxServers.Groups.First(g => g.Id == 2004);

        protected override IWebResponse GetResponse(string address, Content content)
        {
            switch (requestNum)
            {
                case 1: //Get all groups. We say there is only one group, named "Servers" (pretending we may have piped from a probe or a group)
                case 2: //Check whether any other groups exist named "Servers"
                case 3: //Get all devices under the group instead, of which there are none (they're all grandchildren, etc)
                    return base.GetResponse(address, content);
                case 4: //Get all groups of the parent group
                    Assert.AreEqual(Content.Groups, content);
                    Assert.IsTrue(address.Contains("filter_parentid=2000"));

                    return new GroupResponse(Servers1.Groups.Select(g => g.GetTestItem()).ToArray());
                case 5: //Check whether any other groups exist named "Linux Servers"
                    Assert.AreEqual(Content.Groups, content); //todo: what if multiple groups exist with the child group name?
                    Assert.IsTrue(address.Contains("filter_name=Linux+Servers"));

                    return new GroupResponse(LinuxServers.GetTestItem());
                case 6: //Get all sensors under the child group "Linux Servers"
                    Assert.AreEqual(Content.Sensors, content);
                    Assert.IsTrue(address.Contains("filter_name=@sub()&filter_group=Linux+Servers"));

                    return new SensorResponse(LinuxServers.GetSensors(false).Select(s => s.GetTestItem()).ToArray());
                case 7: //Get all groups under the group "Linux Servers"
                    Assert.AreEqual(Content.Groups, content);
                    Assert.IsTrue(address.Contains("filter_parentid=2002"));

                    return new GroupResponse(
                        LinuxServers.Groups.Select(g => g.GetTestItem()).ToArray()
                    );
                case 8: //Check whether any other groups exist named "Old Linux Servers"
                    Assert.AreEqual(Content.Groups, content);
                    Assert.IsTrue(address.Contains("filter_name=Old+Linux+Servers"));

                    return new GroupResponse(LinuxServers.Groups.First(g => g.Name == "Old Linux Servers").GetTestItem());
                case 9: //Get all sensors under the grand-child group "Old Linux Servers"
                    Assert.AreEqual(Content.Sensors, content);
                    Assert.IsTrue(address.Contains("filter_group=Old+Linux+Servers"));

                    return new SensorResponse(
                        OldLinuxServers
                            .GetSensors(false)
                            .Select(s => s.GetTestItem()).ToArray()
                    );
                case 10: //Get all groups under the group "Old Linux Servers"
                    AssertGroupRequest(address, content, "filter_parentid=2003");

                    return new GroupResponse(OldLinuxServers.Groups.Select(g => g.GetTestItem()).ToArray());
                case 11: //Check whether any other groups exist named "Decomissioned Linux Servers"
                    AssertGroupRequest(address, content, "filter_name=Decomissioned+Linux+Servers");

                    return new GroupResponse(DecomissionedLinuxServers.GetTestItem());
                case 12: //Get all sensors under the great-grandchild group "Decomissioned Linux Servers"
                    AssertSensorRequest(address, content, "filter_name=@sub()&filter_group=Decomissioned+Linux+Servers");

                    return new SensorResponse(DecomissionedLinuxServers.GetSensors(false).Select(s => s.GetTestItem()).ToArray());
                default:
                    throw UnknownRequest(address);
            }
        }
    }
}
