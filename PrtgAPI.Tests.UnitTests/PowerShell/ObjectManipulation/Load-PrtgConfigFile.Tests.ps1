. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

Describe "Load-PrtgConfigFile" -Tag @("PowerShell", "UnitTest") {
    It "loads general files" {
        SetAddressValidatorResponse "api/reloadfilelists.htm?" $true

        Load-PrtgConfigFile General
    }

    It "loads sensor lookups" {
        SetAddressValidatorResponse "api/loadlookups.htm?" $true

        Load-PrtgConfigFile Lookups
    }
}