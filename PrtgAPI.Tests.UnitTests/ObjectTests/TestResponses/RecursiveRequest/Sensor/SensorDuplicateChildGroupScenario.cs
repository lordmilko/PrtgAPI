using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.TreeNodes;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses
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
            GroupNode group;
            DeviceNode device;

            switch (requestNum)
            {
                case 1: //Get all groups. We say there is only one group, named "Servers" (pretending we may have piped from a probe or a group)
                case 2: //Check whether any other groups exist named "Servers"
                case 3: //Get all devices under the group instead
                case 4: //Get all sensors under the first device
                case 5: //Get all sensors under the second device
                    return base.GetResponse(address, content);

                case 6: //Get all child groups of the parent group
                    Assert.AreEqual(Content.Groups, content);
                    Assert.IsTrue(address.Contains("filter_parentid=2000"));
                    group = probe.Groups.First(g => g.Name == "Servers");
                    return new GroupResponse(group.Groups.Select(g => g.GetTestItem()).ToArray());

                case 7: //Get all devices of the child group
                    Assert.AreEqual(Content.Devices, content);
                    Assert.IsTrue(address.Contains("filter_parentid=2002"));

                    group = probe.Groups.First(g => g.Name == "Servers");
                    group = group.Groups.First(g => g.Id == 2002);

                    return new DeviceResponse(group.Devices.Select(d => d.GetTestItem()).ToArray());

                case 8: //Get all sensors of the child group's device
                    Assert.AreEqual(Content.Sensors, content);
                    Assert.IsTrue(address.Contains("filter_name=@sub()&filter_parentid=3002"));

                    group = probe.Groups.First(g => g.Name == "Servers");
                    group = group.Groups.First(g => g.Id == 2002);
                    device = group.Devices.First(d => d.Id == 3002);

                    return new SensorResponse(device.Sensors.Select(s => s.GetTestItem()).ToArray());

                default:
                    throw UnknownRequest();
            }
        }
    }
}