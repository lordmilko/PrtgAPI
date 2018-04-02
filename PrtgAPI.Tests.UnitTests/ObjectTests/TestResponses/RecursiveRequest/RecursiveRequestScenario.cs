namespace PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses
{
    public enum RecursiveRequestScenario
    {
        SensorUniqueGroup,
        SensorDuplicateGroup,
        SensorUniqueChildGroup,
        SensorDuplicateChildGroup,
        SensorNoRecurse,
        SensorDeepNesting,

        DeviceUniqueGroup,
        DeviceDuplicateGroup,
        DeviceUniqueChildGroup,
        DeviceDuplicateChildGroup,
        DeviceNoRecurse,
        DeviceDeepNesting,

        GroupUniqueGroup,
        GroupDuplicateGroup,
        GroupUniqueChildGroup,
        GroupDuplicateChildGroup,
        GroupNoRecurse,
        GroupDeepNesting
    }
}