function Invoke-AppveyorBuild
{
    [CmdletBinding()]
    param(
        [Parameter(Position = 0)]
        [switch]$IsCore = $script:APPEYOR_BUILD_CORE
    )

    Write-LogHeader "Building PrtgAPI (Core: $IsCore)"

    $additionalArgs = $null

    if($env:APPVEYOR)
    {
        $additionalArgs = "/logger:`"C:\Program Files\AppVeyor\BuildAgent\Appveyor.MSBuildLogger.dll`""
    }

    Invoke-CIBuild $env:APPVEYOR_BUILD_FOLDER $additionalArgs -IsCore:$IsCore
}