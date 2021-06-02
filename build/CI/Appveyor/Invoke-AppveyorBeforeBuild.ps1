function Invoke-AppveyorBeforeBuild
{
    param(
        [Parameter(Position = 0)]
        [switch]$IsCore = $script:APPEYOR_BUILD_CORE
    )

    Write-LogHeader "Restoring NuGet Packages (Core: $IsCore)"

    if($IsCore)
    {
        Invoke-Process { dotnet restore (Join-Path $env:APPVEYOR_BUILD_FOLDER "PrtgAPIv17.sln") "-p:EnableSourceLink=true" }
    }
    else
    {
        Invoke-Process { nuget restore (Join-Path $env:APPVEYOR_BUILD_FOLDER "PrtgAPI.sln") }
    }

    Set-AppveyorVersion -IsCore:$IsCore
}