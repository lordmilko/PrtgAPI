
namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class SensorDeepNestingGrandChildScenario : SensorDeepNestingChildScenario
    {
        protected override IWebResponse GetResponse(string address, Content content)
        {
            switch (requestNum)
            {
                case 1: //Get the "Servers" parent group
                case 2: //Check whether the "Servers" group name is unique. It isn't
                case 3: //Get all devices under the group instead
                case 4: //Get all child groups of the parent "Servers" group. We just say we have the "Linux Servers" group
                case 5: //Check whether any other groups exist named "Linux Servers". Lie and say that multiple exist
                case 6: //Get all devices under the "Linux Servers" group
                    return base.GetResponse(address, content);
                case 7: //Get all sensors under all devices under group "Linux Servers"
                    AssertSensorRequest(address, content, "filter_name=@sub(Uptime)&filter_parentid=3000&filter_parentid=3001");

                    return GetSensorResponse(null);
                case 8: //Get all child groups of the "Linux Servers" group
                    AssertGroupRequest(address, content, "filter_parentid=2002");

                    return new GroupResponse(OldLinuxServers.GetTestItem());
                case 9: //Check whether the "Old Linux Servers" group name is unique. Lie and say that multiple exist
                    AssertGroupRequest(address, content, "filter_name=Old+Linux+Servers");

                    return new GroupResponse(OldLinuxServers.GetTestItem(), OldLinuxServers.GetTestItem());
                case 10: //Get all devices under the "Old Linux Servers" group
                    AssertDeviceRequest(address, content, "filter_parentid=2003");

                    return GetDeviceResponse(OldLinuxServers.Devices);
                case 11: //Get all sensors under all devices under group "Old Linux Servers"
                    AssertSensorRequest(address, content, "filter_name=@sub(Uptime)&filter_parentid=3002");

                    return GetSensorResponse(OldLinuxServers.GetSensors(false));
                case 12: //Get all child groups of group "Old Linux Servers". Say there aren't any
                    AssertGroupRequest(address, content, "filter_parentid=2003");

                    return GetGroupResponse(null);
                default:
                    throw UnknownRequest(address);
            }
        }
    }
}
