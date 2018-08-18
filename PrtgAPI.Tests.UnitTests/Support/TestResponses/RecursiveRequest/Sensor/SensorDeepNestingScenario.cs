using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.TreeNodes;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses
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
                            new SensorNode("Ping"),
                            new SensorNode("CPU Load")
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
                case 1: //Get all groups. We say there is only one group, named "Servers" (pretending we may have piped from a probe or a group)
                case 2: //Check whether any other groups exist named "Servers"
                case 3: //Get all devices under the group instead
                    return base.GetResponse(address, content);
                case 4: //Get all groups of the parent group
                    Assert.AreEqual(Content.Groups, content);
                    Assert.IsTrue(address.Contains("filter_parentid=2000"));

                    return new GroupResponse(probe.Groups.First(g => g.Id == 2000).Groups.Select(g => g.GetTestItem()).ToArray());
                case 5: //Check whether any other groups exist named "Linux Servers"
                    Assert.AreEqual(Content.Groups, content); //todo: what if multiple groups exist with the child group name?
                    Assert.IsTrue(address.Contains("filter_name=Linux+Servers"));

                    return new GroupResponse(probe.Groups.First(g => g.Id == 2000).Groups.First(g => g.Name == "Linux Servers").GetTestItem());
                case 6: //Get all sensors under the child group
                    Assert.AreEqual(Content.Sensors, content);
                    Assert.IsTrue(address.Contains("filter_name=@sub()&filter_group=Linux+Servers"));

                    return new SensorResponse(probe.Groups.First(g => g.Id == 2000).Groups.First(g => g.Name == "Linux Servers").GetSensors(false).Select(s => s.GetTestItem()).ToArray());
                default:
                    throw UnknownRequest(address);
            }
        }
    }
}
