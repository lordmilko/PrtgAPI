. $PSScriptRoot\Support\Standalone.ps1

Describe "Get-SensorType" -Tag @("PowerShell", "UnitTest") {

    SetMultiTypeResponse

    It "retrieves all sensor types" {
        $types = Get-SensorType

        $types.Count | Should Be 3
    }

    It "retrieves sensor types from an object" {
        $types = Get-Device -Count 1 | Get-SensorType

        $types.Count | Should Be 3
    }

    It "filters retrieved types" {
        $types = Get-SensorType *repl*

        $types.Count | Should Be 1

        $types.Name | Should Be "Active Directory Replication Errors"
    }
}