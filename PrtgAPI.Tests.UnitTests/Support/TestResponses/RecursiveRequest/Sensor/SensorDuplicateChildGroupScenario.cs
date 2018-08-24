using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.TreeNodes;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class SensorDuplicateChildGroupScenario : SensorDuplicateGroupScenario
    {
        public SensorDuplicateChildGroupScenario()
        {
            probe = new ProbeNode("Local Probe",
                new GroupNode("Servers",
                    new DeviceNode("dc-1",
                        new SensorNode("Ping"),
                        new SensorNode("CPU Load")
                    ),
                    new DeviceNode("dc-2",
                        new SensorNode("Ping"),
                        new SensorNode("CPU Load")
                    ),
                    new GroupNode("Linux Servers",
                        new DeviceNode("arch-1",
                            new SensorNode("Ping"),
                            new SensorNode("HTTP")
                        ),
                        new GroupNode("Debian Servers",
                            new DeviceNode("debian-1",
                                new SensorNode("Ping"),
                                new SensorNode("DNS")
                            )
                        )
                    )
                ),
                new GroupNode("Servers",
                    new DeviceNode("exch-1",
                        new SensorNode("Ping")
                    ),
                    new GroupNode("Linux Servers",
                        new DeviceNode("rhel-1",
                            new SensorNode("Ping"),
                            new SensorNode("HTTP")
                        )
                    )
                )
            );
        }

        private GroupNode Servers => probe.Groups.First(g => g.Name == "Servers");

        private GroupNode LinuxServers => Servers.Groups.First(g => g.Name == "Linux Servers");

        protected override IWebResponse GetResponse(string address, Content content)
        {
            GroupNode group;
            DeviceNode device;

            switch (requestNum)
            {
                case 1: //Get all groups. We say there is only one group, named "Servers" (pretending we may have piped from a probe or a group)
                case 2: //Check whether any other groups exist named "Servers"
                case 3: //Get all devices under the group instead
                case 4: //Get all sensors under the first and second devices
                    return base.GetResponse(address, content);

                case 5: //Get all child groups of the parent group (returns "Linux Servers")
                    Assert.AreEqual(Content.Groups, content);
                    Assert.IsTrue(address.Contains("filter_parentid=2000"));
                    group = probe.Groups.First(g => g.Name == "Servers");
                    return new GroupResponse(group.Groups.Select(g => g.GetTestItem()).ToArray());

                case 6: //Check whether any other groups exist named "Linux Servers"
                    Assert.AreEqual(Content.Groups, content);
                    Assert.IsTrue(address.Contains("filter_name=Linux+Servers"));

                    var firstGroup = Servers.Groups.Select(g => g.GetTestItem()).ToList();
                    firstGroup.AddRange(probe.Groups.Skip(1).First().Groups.Select(g => g.GetTestItem()).ToList());

                    return new GroupResponse(firstGroup.ToArray());

                case 7: //Get all devices of the child group
                    Assert.AreEqual(Content.Devices, content);
                    Assert.IsTrue(address.Contains("filter_parentid=2002"));
                    return new DeviceResponse(LinuxServers.Devices.Select(d => d.GetTestItem()).ToArray());

                case 8: //Get all sensors of the child group's device (returns all sensors under group "Linux Servers")
                    Assert.AreEqual(Content.Sensors, content);
                    Assert.IsTrue(address.Contains("filter_name=@sub()&filter_parentid=3002"));
                    device = LinuxServers.Devices.First(d => d.Id == 3002);
                    return new SensorResponse(device.Sensors.Select(s => s.GetTestItem()).ToArray());

                case 9: //Get all groups under Linux Servers
                    Assert.AreEqual(Content.Groups, content);
                    Assert.IsTrue(address.Contains("filter_parentid=2002"));
                    return new GroupResponse(LinuxServers.Groups.Select(g => g.GetTestItem()).ToArray());

                case 10: //Check whether any other groups exist named "Debian Servers"
                    Assert.AreEqual(Content.Groups, content);
                    Assert.IsTrue(address.Contains("filter_name=Debian+Servers"));
                    return new GroupResponse(LinuxServers.Groups.Select(g => g.GetTestItem()).ToArray());

                case 11: //Get all sensors under the group "Debian Servers"
                    Assert.AreEqual(Content.Sensors, content);
                    Assert.IsTrue(address.Contains("filter_name=@sub()&filter_group=Debian+Servers"));
                    return new SensorResponse(LinuxServers.Groups.SelectMany(g => g.GetSensors(false)).Select(s => s.GetTestItem()).ToArray());

                default:
                    throw UnknownRequest(address);
            }
        }
    }
}