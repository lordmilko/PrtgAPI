using PrtgAPI.Tests.UnitTests.TreeNodes;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class GroupDuplicateGroupScenario : GroupUniqueGroupScenario
    {
        public GroupDuplicateGroupScenario()
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
