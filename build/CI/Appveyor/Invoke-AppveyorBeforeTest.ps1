function Invoke-AppveyorBeforeTest
{
    [CmdletBinding()]
    param(
        [Parameter(Position = 0)]
        [switch]$IsCore = $script:APPEYOR_BUILD_CORE
    )

    New-AppveyorPackage $IsCore
}