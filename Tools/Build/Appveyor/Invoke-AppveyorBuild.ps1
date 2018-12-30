function Invoke-AppveyorBuild
{
    [CmdletBinding()]
    param(
        [Parameter(Position = 0)]
        [switch]$IsCore = $script:APPEYOR_BUILD_CORE
    )

    Write-LogHeader "Building PrtgAPI (Core: $IsCore)"

    if($IsCore)
    {
        throw ".NET Core is not currently supported"
    }
    else
    {
        Invoke-Process {

            $msbuild = Get-MSBuild

            $msbuildArgs = @(
                "$env:APPVEYOR_BUILD_FOLDER\PrtgAPI.sln"
                "/verbosity:minimal"
                "/p:Configuration=$env:CONFIGURATION"
            )

            if($env:APPVEYOR)
            {
                $msbuildArgs += "/logger:`"C:\Program Files\AppVeyor\BuildAgent\Appveyor.MSBuildLogger.dll`""
            }

            & $msbuild @msbuildArgs
        } -Host
    }
}