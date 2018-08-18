using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.TreeNodes;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses
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
                default:
                    return base.GetResponse(address, content);
            }
        }
    }
}
