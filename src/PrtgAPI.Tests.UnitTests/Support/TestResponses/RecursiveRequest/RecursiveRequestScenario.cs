namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    public enum RecursiveRequestScenario
    {
        SensorUniqueGroup,
        SensorDuplicateGroup,
        SensorUniqueChildGroup,
        SensorDuplicateChildGroup,
        SensorNoRecurse,
        SensorDeepNesting,
        SensorDeepNestingChild,
        SensorDeepNestingGrandChild,
        SensorDeepNestingGreatGrandChild,

        DeviceUniqueGroup,
        DeviceDuplicateGroup,
        DeviceUniqueChildGroup,
        DeviceDuplicateChildGroup,
        DeviceNoRecurse,
        DeviceDeepNesting,
        DeviceDeepNestingChild,
        DeviceDeepNestingGrandChild,
        DeviceDeepNestingGreatGrandChild,

        GroupUniqueGroup,
        GroupDuplicateGroup,
        GroupUniqueChildGroup,
        GroupDuplicateChildGroup,
        GroupNoRecurse,
        GroupDeepNesting,
        GroupDeepNestingChild,
        GroupDeepNestingGrandChild,
        GroupDeepNestingGreatGrandChild,

        GroupRecurseAvailableCount,
        GroupRecurseUnavailableCount,
        GroupRecurseAvailableSingleCount,
        GroupNoRecurseAvailableCount,
        GroupNoRecurseUnavailableCount,
    }
}