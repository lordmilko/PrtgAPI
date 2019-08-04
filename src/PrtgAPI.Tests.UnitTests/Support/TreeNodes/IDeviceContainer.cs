using System.Collections.Generic;

namespace PrtgAPI.Tests.UnitTests.TreeNodes
{
    /// <summary>
    /// Represents a container that directly contains devices.
    /// </summary>
    interface IDeviceContainer : ISensorContainer
    {
        List<DeviceNode> Devices { get; set; }

        int TotalDevices { get; }

        List<DeviceNode> GetDevices(bool recurse);
    }
}
