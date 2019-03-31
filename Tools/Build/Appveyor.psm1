ipmo $PSScriptRoot\build.psm1

$script:SolutionDir = $script:SolutionDir = Get-SolutionRoot

. $PSScriptRoot\Functions\Import-ModuleFunctions.ps1
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

Export-ModuleMember Set-AppveyorBuildMode,Enable-AppveyorRDPAccess,Simulate-Environment