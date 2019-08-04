. $PSScriptRoot\..\..\Support\PowerShell\IntegrationTest.ps1

Describe "Load-PrtgConfigFile_IT" -Tag @("PowerShell", "IntegrationTest") {
    It "loads general files" {
        Load-PrtgConfigFile General
    }

    It "loads sensor lookups" {
        Load-PrtgConfigFile Lookups
    }
}