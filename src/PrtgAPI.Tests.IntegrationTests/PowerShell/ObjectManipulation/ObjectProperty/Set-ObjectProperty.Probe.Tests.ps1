. $PSScriptRoot\..\..\..\Support\PowerShell\ObjectProperty.ps1

Describe "Set-ObjectProperty_Probe_IT" -Tag @("PowerShell", "IntegrationTest") {

    It "Special" {
        $object = Get-Probe -Id (Settings Probe)

        GetValue "ProbeApproved" $true
    }
}