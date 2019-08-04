using System.Collections.Generic;

namespace PrtgAPI.Tests.UnitTests.TreeNodes
{
    /// <summary>
    /// Represents a container that may directly contain sensors, or may contain sensors indirectly via its children.
    /// </summary>
    interface ISensorContainer : ITreeNode
    {
        List<SensorNode> GetSensors(bool recurse);
    }
}
