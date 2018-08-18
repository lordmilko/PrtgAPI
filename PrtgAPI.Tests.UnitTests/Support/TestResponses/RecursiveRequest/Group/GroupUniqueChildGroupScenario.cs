using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.TreeNodes;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses
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

                case 2: //Get all groups under the parent group that match the initial filter (returns "Windows Servers")
                    Assert.AreEqual(Content.Groups, content);
                    Assert.IsTrue(address.Contains("filter_name=@sub()&filter_parentid=2000"));
                    return new GroupResponse(probe.Groups.First(g => g.Name == "Servers").Groups.Select(g => g.GetTestItem()).ToArray());

                case 3: //Get all groups under the child group that match the initial filter (returns "Domain Controllers")
                    Assert.AreEqual(Content.Groups, content);
                    Assert.IsTrue(address.Contains("filter_name=@sub()&filter_parentid=2002"));
                    return new GroupResponse(probe.Groups.First(g => g.Name == "Servers").Groups.First(g => g.Id == 2002).Groups.Select(g => g.GetTestItem()).ToArray());

                default:
                    throw UnknownRequest(address);
            }
        }
    }
}
