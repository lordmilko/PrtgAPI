using System.Linq;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class SensorDeepNestingChildScenario : SensorDeepNestingScenario
    {
        protected override IWebResponse GetResponse(string address, Content content)
        {
            switch (requestNum)
            {
                case 1: //Get the "Servers" parent group
                case 2: //Check whether the "Servers" group name is unique. It isn't
                case 3: //Get all devices under the group instead
                case 4: //Get all child groups of the parent "Servers" group. We just say we have the "Linux Servers" group
                    return base.GetResponse(address, content);
                case 5: //Check whether any other groups exist named "Linux Servers". Lie and say that multiple exist
                    AssertGroupRequest(address, content, "filter_name=Linux+Servers");

                    return new GroupResponse(LinuxServers.GetTestItem(), LinuxServers.GetTestItem());
                case 6: //Get all devices under the "Linux Servers" group
                    AssertDeviceRequest(address, content, "filter_parentid=2002");

                    return new DeviceResponse(LinuxServers.Devices.Select(d => d.GetTestItem()).ToArray());
                case 7: //Get all sensors under all devices under group "Linux Servers"
                    AssertSensorRequest(address, content, "filter_name=@sub(Ping)&filter_parentid=3000&filter_parentid=3001");

                    return GetSensorResponse(LinuxServers.GetSensors(false));
                case 8: //Get all child groups of the "Linux Servers" group. Say there aren't any
                    AssertGroupRequest(address, content, "filter_parentid=2002");

                    return GetGroupResponse(null);
                default:
                    throw UnknownRequest(address);
            }
        }
    }
}
