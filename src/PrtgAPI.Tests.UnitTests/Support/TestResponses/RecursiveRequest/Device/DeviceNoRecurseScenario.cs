using PrtgAPI.Tests.UnitTests.TreeNodes;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class DeviceNoRecurseScenario : DeviceUniqueGroupScenario
    {
        public DeviceNoRecurseScenario()
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
            switch (requestNum)
            {
                default:
                    return base.GetResponse(address, content);
            }
        }
    }
}
