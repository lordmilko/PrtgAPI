using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.TreeNodes;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class SensorNoRecurseScenario : GroupScenario
    {
        public SensorNoRecurseScenario()
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
            switch (requestNum)
            {
                case 1: //Get all groups. We say there is only one group, named "Servers"
                    Assert.AreEqual(Content.Groups, content);
                    return new GroupResponse(probe.Groups.First(g => g.Name == "Servers").GetTestItem());

                case 2: //Get all sensors under all groups named "Servers"
                    Assert.AreEqual(Content.Sensors, content);
                    Assert.IsTrue(address.Contains("filter_name=@sub()&filter_group=Servers"));
                    return new SensorResponse(probe.Groups.Where(g => g.Name == "Servers").SelectMany(g => g.GetSensors(false).Select(s => s.GetTestItem())).ToArray());

                default:
                    throw UnknownRequest(address);
            }
        }
    }
}
