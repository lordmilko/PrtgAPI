
namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class SensorDeepNestingGreatGrandChildScenario : SensorDeepNestingGrandChildScenario
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
                case 7: //Get all sensors under all devices under group "Linux Servers"

                case 8: //Get all child groups of the "Linux Servers" group
                case 9: //Check whether the "Old Linux Servers" group name is unique. Lie and say that multiple exist
                case 10: //Get all devices under the "Old Linux Servers" group
                case 11: //Get all sensors under all devices under group "Old Linux Servers"
                    return base.GetResponse(address, content);

                case 12: //Get all child groups of group "Old Linux Servers"
                    AssertGroupRequest(address, content, "filter_parentid=2003");

                    return GetGroupResponse(OldLinuxServers.Groups);
                case 13: //Check whether the "Decomissioned Linux Servers" group is unique. Lie and say that multiple exist
                    AssertGroupRequest(address, content, "filter_name=Decomissioned+Linux+Servers");

                    return new GroupResponse(DecomissionedLinuxServers.GetTestItem(), DecomissionedLinuxServers.GetTestItem());
                case 14: //Get all devices under the "Decomissioned Linux Servers" group
                    AssertDeviceRequest(address, content, "filter_parentid=2004");

                    return GetDeviceResponse(DecomissionedLinuxServers.Devices);
                case 15: //Get all sensors under all devices under group "Decomissioned Linux Servers"
                    AssertSensorRequest(address, content, "filter_name=@sub(Uptime)&filter_parentid=3003");

                    return GetSensorResponse(DecomissionedLinuxServers.GetSensors(false));
                default:
                    throw UnknownRequest(address);
            }
        }
    }
}
