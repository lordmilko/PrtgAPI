ipmo $PSScriptRoot\ci.psm1 -Scope Local

$script:SolutionDir = $script:SolutionDir = Get-SolutionRoot

. $PSScriptRoot\Helpers\Import-ModuleFunctions.ps1
. Import-ModuleFunctions "$PSScriptRoot\Appveyor"

$script:APPEYOR_BUILD_CORE = $true

function Set-AppveyorBuildMode
{
    param(
        [switch]$IsCore
    )

    $script:APPEYOR_BUILD_CORE = $IsCore
}

function Enable-AppveyorRDPAccess
{
    $blockRdp = $true; iex ((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/appveyor/ci/master/scripts/enable-rdp.ps1'))
}

function Get-DebugTargetFramework
{
    [xml]$xml = gc (Join-Path $env:APPVEYOR_BUILD_FOLDER "src\Directory.Build.props")
    
    $debugVersion = ($xml.project.PropertyGroup.targetframeworks|where condition -ne "`$(IsUnix)")."#text"

    if(!$debugVersion)
    {
        throw "Could not find debug TargetFramework in Directory.Build.props"
    }

    return $debugVersion
}

function GetVersion
{
    param(
        [Parameter(Mandatory = $true, Position = 0)]
        [switch]$IsCore
    )

    return (Get-CIVersion -IsCore:$IsCore).File.ToString(3)
}

Export-ModuleMember Set-AppveyorBuildMode,Enable-AppveyorRDPAccess,Simulate-Environment,Get-DebugTargetFramework