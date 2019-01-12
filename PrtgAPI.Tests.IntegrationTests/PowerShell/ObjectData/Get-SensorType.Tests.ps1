. $PSScriptRoot\..\..\Support\PowerShell\IntegrationTestSafe.ps1

Describe "Get-SensorType_IT" -Tag @("PowerShell", "IntegrationTest") {
    It "retrieves all sensor types" {
        (Get-SensorType).Count | Should BeGreaterThan 200
    }

    It "doesn't retrieve types from the root group" {
        
        (Get-Group -Id 0 | Get-SensorType).Count | Should Be 0
    }

    It "filters by name" {
        $types = Get-SensorType *vmware*

        $types.Count | Should BeGreaterThan 0
        $types.Count | Should BeLessThan 10
    }

    It "retrieves as a readonly user" {
        ReadOnlyClient {
            Get-SensorType
        }
    }
}