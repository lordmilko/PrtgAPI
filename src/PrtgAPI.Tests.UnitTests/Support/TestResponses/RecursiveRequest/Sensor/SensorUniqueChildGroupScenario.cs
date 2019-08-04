using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.TreeNodes;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class SensorUniqueChildGroupScenario : SensorUniqueGroupScenario
    {
        public SensorUniqueChildGroupScenario()
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
                new GroupNode("Clients",
                    new DeviceNode("wks-1",
                        new SensorNode("Ping")
                    )
                )
            );
        }

        protected override IWebResponse GetResponse(string address, Content content)
        {
            GroupNode group;

            switch (requestNum)
            {
                case 1: //Get all groups. We say there is only one group, named "Servers"
                case 2: //Check whether any other groups exist named "Servers"
                case 3: //Get all sensors under the group named "Servers"
                    return base.GetResponse(address, content);

                case 4: //Get all child groups of the parent group (returns "Linux Servers")
                    Assert.AreEqual(Content.Groups, content);
                    Assert.IsTrue(address.Contains("filter_parentid=2000"));
                    group = probe.Groups.First(g => g.Name == "Servers");
                    return new GroupResponse(group.Groups.Select(g => g.GetTestItem()).ToArray());

                case 5: //Check whether any other groups exist named "Linux Servers" (returns "Linux Servers")
                    Assert.AreEqual(Content.Groups, content);
                    Assert.IsTrue(address.Contains("filter_name=Linux+Servers"));
                    group = probe.Groups.First(g => g.Name == "Servers");
                    return new GroupResponse(group.Groups.Select(g => g.GetTestItem()).ToArray());

                case 6: //Get all sensors of the uniquely named child group
                    Assert.AreEqual(Content.Sensors, content);
                    Assert.IsTrue(address.Contains("filter_name=@sub()&filter_group=Linux+Servers"));
                    group = probe.Groups.First(g => g.Name == "Servers");
                    group = group.Groups.First(g => g.Id == 2002);

                    return new SensorResponse(group.GetSensors(false).Select(s => s.GetTestItem()).ToArray());

                default:
                    throw UnknownRequest(address);
            }
        }
    }
}
