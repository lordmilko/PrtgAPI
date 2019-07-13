function Invoke-AppveyorBuild
{
    [CmdletBinding()]
    param(
        [Parameter(Position = 0)]
        [switch]$IsCore = $script:APPEYOR_BUILD_CORE
    )

    Write-LogHeader "Building PrtgAPI (Core: $IsCore)"

    $additionalArgs = @()

    # .NET Core is not currently supported https://github.com/appveyor/ci/issues/2212
    if($env:APPVEYOR -and !$IsCore)
    {
        $additionalArgs += "/logger:`"C:\Program Files\AppVeyor\BuildAgent\Appveyor.MSBuildLogger.dll`""
    }

    if($IsCore)
    {
        $additionalArgs += "--no-restore"
    }

    Invoke-CIBuild $env:APPVEYOR_BUILD_FOLDER $additionalArgs -IsCore:$IsCore -SourceLink
}