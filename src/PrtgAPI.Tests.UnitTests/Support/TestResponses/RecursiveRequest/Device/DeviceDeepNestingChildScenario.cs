using System.Linq;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    /// <summary>
    /// Test response for retrieveing devices from a child group of a parent group.
    /// </summary>
    class DeviceDeepNestingChildScenario : DeviceDeepNestingScenario
    {
        protected override IWebResponse GetResponse(string address, Content content)
        {
            switch (requestNum)
            {
                case 1: //Get the "Servers" group
                    return base.GetResponse(address, content);
                case 2: //Get all devices under the "Servers" group
                    AssertDeviceRequest(address, content, "filter_name=@sub(dc)&filter_parentid=2000");

                    return GetDeviceResponse(probe.Groups.First().Devices);
                case 3: //Get all groups under the "Servers" group.
                    AssertGroupRequest(address, content, "filter_parentid=2000");

                    return GetGroupResponse(probe.Groups.First().Groups);
                case 4: //Get all devices under the "Linux Servers" group
                    AssertDeviceRequest(address, content, "filter_name=@sub(dc)&filter_parentid=2002");

                    return GetDeviceResponse(probe.Groups.First().Groups.First().Devices);
                case 5: //Get all groups under the "Linux Servers" group. Say there aren't any
                    AssertGroupRequest(address, content, "filter_parentid=2002");

                    return GetGroupResponse(null);
                default:
                    throw UnknownRequest(address);
            }
        }
    }
}
