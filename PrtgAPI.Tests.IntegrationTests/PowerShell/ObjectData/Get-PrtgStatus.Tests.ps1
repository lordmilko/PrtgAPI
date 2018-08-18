. $PSScriptRoot\..\..\Support\PowerShell\IntegrationTestSafe.ps1

Describe "Get-PrtgStatus_IT" {
    It "can execute" {
        $status = Get-PrtgStatus

        $status.GetType().Name | Should Be ServerStatus
    }
}