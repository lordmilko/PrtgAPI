. $PSScriptRoot\Support\UnitTest.ps1

Describe "Get-Device" -Tag @("PowerShell", "UnitTest") {

    It "can deserialize" {
        $devices = Get-Device
        $devices.Count | Should Be 1
    }
}