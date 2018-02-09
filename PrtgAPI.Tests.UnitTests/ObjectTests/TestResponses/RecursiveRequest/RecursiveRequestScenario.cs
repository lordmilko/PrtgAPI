namespace PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses
{
    public enum RecursiveRequestScenario
    {
        SensorUniqueGroup,
        SensorDuplicateGroup,
        SensorUniqueChildGroup,
        SensorDuplicateChildGroup,
        SensorNoRecurse,

        DeviceUniqueGroup,
        DeviceDuplicateGroup,
        DeviceUniqueChildGroup,
        DeviceDuplicateChildGroup,
        DeviceNoRecurse,

        GroupUniqueGroup,
        GroupDuplicateGroup,
        GroupUniqueChildGroup,
        GroupDuplicateChildGroup,
        GroupNoRecurse,

        GroupDeepNesting
    }
}