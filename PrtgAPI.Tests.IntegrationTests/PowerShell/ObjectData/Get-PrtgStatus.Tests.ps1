. $PSScriptRoot\..\..\Support\PowerShell\IntegrationTestSafe.ps1

Describe "Get-PrtgStatus_IT" -Tag @("PowerShell", "IntegrationTest") {
    It "can execute" {
        $status = Get-PrtgStatus

        $status.GetType().Name | Should Be ServerStatus
    }

    It "retrieves as a readonly user" {
        ReadOnlyClient {
            Get-PrtgStatus
        }
    }
}