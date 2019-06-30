Import-Module $PSScriptRoot\..\ci.psm1
Import-Module $PSScriptRoot\..\Travis.psm1 -DisableNameChecking

. $PSScriptRoot\Support.ps1

Describe "Travis" {
    It "simulates Travis" {
        WithoutTestDrive {
            Simulate-Travis
        }
    }
}