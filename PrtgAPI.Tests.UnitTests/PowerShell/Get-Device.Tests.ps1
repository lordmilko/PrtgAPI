. $PSScriptRoot\Support\UnitTest.ps1

Describe "Get-Device" {

    It "can deserialize" {
        $devices = Get-Device
        $devices.Count | Should Be 1
    }
}