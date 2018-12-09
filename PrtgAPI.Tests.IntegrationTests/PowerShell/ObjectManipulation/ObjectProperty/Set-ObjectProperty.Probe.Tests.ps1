. $PSScriptRoot\..\..\..\Support\PowerShell\ObjectProperty.ps1

Describe "Set-ObjectProperty_Probe_IT" {

    It "Special" {
        $object = Get-Probe -Id (Settings Probe)

        GetValue "ProbeApproved" $true
    }
}