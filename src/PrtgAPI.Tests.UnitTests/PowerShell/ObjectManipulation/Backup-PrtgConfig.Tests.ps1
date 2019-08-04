. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

Describe "Backup-PrtgConfig" -Tag @("PowerShell", "UnitTest") {
    It "can execute" {
        SetAddressValidatorResponse "api/savenow.htm?" $true

        Backup-PrtgConfig
    }
}