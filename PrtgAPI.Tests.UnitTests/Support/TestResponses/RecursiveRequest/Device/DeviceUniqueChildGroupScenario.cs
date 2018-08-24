using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.TreeNodes;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class DeviceUniqueChildGroupScenario : DeviceUniqueGroupScenario
    {
        public DeviceUniqueChildGroupScenario()
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
                case 1:
                case 2:
                    return base.GetResponse(address, content);

                case 3: //Get all groups under the group
                    Assert.AreEqual(Content.Groups, content);
                    Assert.IsTrue(address.Contains("filter_parentid=2000"));
                    return new GroupResponse(probe.Groups.First(g => g.Name == "Servers").Groups.Select(g => g.GetTestItem()).ToArray());

                case 4: //Get all devices under the child group that match the initial filter
                    Assert.AreEqual(Content.Devices, content);
                    Assert.IsTrue(address.Contains("filter_name=@sub()&filter_parentid=2002"));
                    return new DeviceResponse(probe.Groups.First(g => g.Name == "Servers").Groups.First(g => g.Id == 2002).Devices.Select(d => d.GetTestItem()).ToArray());

                default:
                    throw UnknownRequest(address);
            }
        }
    }
}
