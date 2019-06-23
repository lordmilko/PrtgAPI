function Invoke-AppveyorAfterTest
{
    param(
        [Parameter(Position = 0)]
        [switch]$IsCore = $script:APPEYOR_BUILD_CORE
    )

    Measure-AppveyorCoverage $IsCore
}