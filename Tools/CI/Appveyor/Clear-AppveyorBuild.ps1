function Clear-AppveyorBuild
{
    [CmdletBinding()]
    param(
        [Parameter(Position = 0)]
        [switch]$IsCore = $script:APPEYOR_BUILD_CORE,

        [switch]$NuGetOnly
    )

    Write-LogHeader "Cleaning Appveyor build folder (Core: $IsCore)"

    Clear-CIBuild $env:APPVEYOR_BUILD_FOLDER -IsCore:$IsCore -NuGetOnly:$NuGetOnly
}