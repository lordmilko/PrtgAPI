ipmo $PSScriptRoot\ci.psm1 -Scope Local

$script:SolutionDir = $script:SolutionDir = Get-SolutionRoot

. $PSScriptRoot\Helpers\Import-ModuleFunctions.ps1
. Import-ModuleFunctions "$PSScriptRoot\Travis"

$env:CONFIGURATION = "Release"

if($env:TRAVIS)
{
    # Travis doesn't deal with showing progress bars very well
    $global:ProgressPreference = "SilentlyContinue"
}