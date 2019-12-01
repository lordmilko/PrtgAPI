namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    public enum TreeRequestScenario
    {
        /// <summary>
        /// A container with no children.
        /// </summary>
        StandaloneContainer,

        /// <summary>
        /// A container with one level of children.
        /// </summary>
        ContainerWithChild,

        /// <summary>
        /// A container with a non-container child which contains children.
        /// </summary>
        ContainerWithGrandChild,

        /// <summary>
        /// A container that contains one or more child contains, with all containers containing non-container children.
        /// </summary>
        MultiLevelContainer,

        ObjectPipeToContainerWithChild,

        ActionPipeToContainerWithChild,

        FastPath,

        ObjectToFastPath,

        SlowPathSensorsOnly,
        SlowPathDevicesOnly,
        SlowPathGroupsOnly,
        SlowPathProbesOnly,
        SlowPathTriggersOnly,
        SlowPathPropertiesOnly,

        FastPathSensorsOnly,
        FastPathDevicesOnly,
        FastPathGroupsOnly,
        FastPathProbesOnly,
        FastPathTriggersOnly,
        FastPathPropertiesOnly
    }
}
