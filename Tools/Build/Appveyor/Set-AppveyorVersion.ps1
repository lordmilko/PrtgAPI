function Set-AppveyorVersion
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $false, Position = 0)]
        [string]$BuildFolder = $env:APPVEYOR_BUILD_FOLDER
    )

    try
    {
        Write-LogInfo "Calculating version"
        $version = Get-PrtgVersion $BuildFolder

        Write-LogInfo "`tSetting AppVeyor build to version '$version'"

        if($env:APPVEYOR)
        {
            Update-AppVeyorBuild -Version $version
        }
        else
        {
            $env:APPVEYOR_BUILD_VERSION = $version
        }
    }
    catch
    {
        $host.SetShouldExit(1)
    }
}