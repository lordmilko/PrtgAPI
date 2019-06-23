function Invoke-AppveyorBeforeBuild
{
    param(
        [Parameter(Position = 0)]
        [switch]$IsCore = $script:APPEYOR_BUILD_CORE
    )

    Write-LogHeader "Restoring NuGet Packages (Core: $IsCore)"

    if($IsCore)
    {
        throw ".NET Core is not currently supported"
    }
    else
    {
        Invoke-Process { nuget restore $env:APPVEYOR_BUILD_FOLDER\PrtgAPI.sln }
    }
}