Import-Module $PSScriptRoot\..\ci.psm1 -Scope Local
Import-Module $PSScriptRoot\..\Travis.psm1 -DisableNameChecking -Scope Local

$skipBuildModule = $true
. $PSScriptRoot\..\..\..\src\PrtgAPI.Tests.UnitTests\Support\PowerShell\Build.ps1

Describe "Travis" {
    It "simulates Travis" {
        WithoutTestDrive {
            Simulate-Travis
        }
    }
}