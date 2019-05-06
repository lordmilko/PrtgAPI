. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

Describe "Get-SensorType" -Tag @("PowerShell", "UnitTest") {

    SetMultiTypeResponse

    It "retrieves all sensor types" {

        SetAddressValidatorResponse @(
            [Request]::SensorTypes(1)
        )

        $types = Get-SensorType

        $types.Count | Should Be 9
    }

    It "retrieves sensor types from an object" {

        SetAddressValidatorResponse @(
            [Request]::Devices("", [Request]::DefaultObjectFlags)
            [Request]::SensorTypes(3000)
        )

        $types = Get-Device | select -First 1 | Get-SensorType

        $types.Count | Should Be 9
    }

    It "retrieves types by ID" {
        
        SetAddressValidatorResponse @(
            [Request]::SensorTypes(1001)
        )

        Get-SensorType -Id 1001
    }

    It "filters retrieved types" {

        SetMultiTypeResponse

        $types = Get-SensorType *repl*

        $types.Count | Should Be 1

        $types.Name | Should Be "Active Directory Replication Errors"
    }
}