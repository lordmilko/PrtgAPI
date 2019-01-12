. $PSScriptRoot\..\..\Support\PowerShell\IntegrationTest.ps1

Describe "Clear-PrtgCache_IT" -Tag @("PowerShell", "IntegrationTest") {
    It "clears general caches" {
        Clear-PrtgCache General
    }

    It "clears graph data" {
        Clear-PrtgCache GraphData -Force

        # Wait on the service restarting
        Restart-PrtgCore -Force -Wait
    }
}