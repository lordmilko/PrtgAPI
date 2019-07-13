function Clear-AppveyorBuild
{
    [CmdletBinding()]
    param(
        [Parameter(Position = 0)]
        [switch]$IsCore = $script:APPEYOR_BUILD_CORE,

        [switch]$NuGetOnly
    )

    Write-LogHeader "Cleaning Appveyor build folder (Core: $IsCore)"

    $clearArgs = @{
        BuildFolder = $env:APPVEYOR_BUILD_FOLDER
        Configuration = $env:CONFIGURATION
        NuGetOnly = $NuGetOnly
        IsCore = $IsCore
    }

    Clear-CIBuild @clearArgs
}