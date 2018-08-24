using System.Collections.Generic;

namespace PrtgAPI.Tests.UnitTests.TreeNodes
{
    /// <summary>
    /// Represents a container that directly contains groups.
    /// </summary>
    interface IGroupContainer : IDeviceContainer
    {
        List<GroupNode> Groups { get; set; }

        int TotalGroups { get; }
    }
}
