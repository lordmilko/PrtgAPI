namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    public enum TakeIteratorScenario
    {
        #region Sensors

        Sensors,
        SensorsWithFilter,
        SensorsFromGroup,
        SensorsWithFilterFromGroup,

        SensorsInsufficient,
        SensorsWithFilterInsufficient,
        SensorsWithFilterFromGroupInsufficient,
        SensorsWithFilterInsufficientOneLeft,
        SensorsWithFilterInsufficientNoneLeft,
        SensorsWithFilterInsufficientNegativeLeft,

        SensorsWithFilterFromDuplicateGroup,
        SensorsWithFilterFromDuplicateGroupInsufficient,

        SensorsFromGroupNoRecurse,
        SensorsWithFilterFromGroupNoRecurse,

        #endregion
        #region Logs

        Logs,
        LogsWithFilter,

        LogsInsufficient,
        LogsWithFilterInsufficient,
        LogsWithFilterInsufficientOneLeft,
        LogsWithFilterInsufficientNoneLeft,
        LogsWithFilterInsufficientNegativeLeft,

        LogsForceStream,
        LogsWithFilterForceStream,

        LogsWithFilterInsufficientForceStream,
        LogsWithFilterInsufficientOneLeftForceStream,
        LogsWithFilterInsufficientNoneLeftForceStream,

        //Can't have negative force stream, since we don't ask for any records before getting the total count

        #endregion
    }
}
