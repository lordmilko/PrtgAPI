. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

Describe "Clear-PrtgCache" -Tag @("PowerShell", "UnitTest") {
    It "clears general caches" {
        SetAddressValidatorResponse "api/clearcache.htm?" $true

        Clear-PrtgCache General
    }

    It "clears the graph cache" {
        SetAddressValidatorResponse "api/recalccache.htm?" $true

        Clear-PrtgCache GraphData -Force
    }
}