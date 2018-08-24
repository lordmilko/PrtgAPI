using System.Linq;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class DeviceDeepNestingGrandChildScenario : DeviceDeepNestingChildScenario
    {
        protected override IWebResponse GetResponse(string address, Content content)
        {
            switch (requestNum)
            {
                case 1: //Get the "Servers" group
                    return base.GetResponse(address, content);
                case 2: //Get all devices under the "Servers" group
                    AssertDeviceRequest(address, content, "filter_name=old-arch-1&filter_parentid=2000");

                    return GetDeviceResponse(probe.Groups.First().Groups.First().Devices);
                case 3: //Get all groups under the "Servers" group
                    return base.GetResponse(address, content);
                case 4: //Get all devices under the "Linux Servers" group
                    AssertDeviceRequest(address, content, "filter_name=old-arch-1&filter_parentid=2002");

                    return GetDeviceResponse(probe.Groups.First().Groups.First().Devices);
                case 5: //Get all groups under the "Linux Servers" group.
                    AssertGroupRequest(address, content, "filter_parentid=2002");

                    return GetGroupResponse(probe.Groups.First().Groups.First().Groups);
                case 6: //Get all devices under the "Old Linux Servers" group
                    AssertDeviceRequest(address, content, "filter_name=old-arch-1&filter_parentid=2003");

                    return GetDeviceResponse(probe.Groups.First().Groups.First().Groups.First().Devices);
                case 7: //Get all groups under the "Old Linux Servers" group. Say there aren't any.
                    AssertGroupRequest(address, content, "filter_parentid=2003");

                    return GetGroupResponse(null);
                default:
                    throw UnknownRequest(address);
            }
        }
    }
}
