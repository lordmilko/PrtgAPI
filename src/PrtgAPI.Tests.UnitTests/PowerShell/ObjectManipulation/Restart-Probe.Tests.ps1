. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

Describe "Restart-Probe" -Tag @("PowerShell", "UnitTest") {
    It "restarts all probes" {
        SetAddressValidatorResponse @(
            [Request]::Probes("count=0&filter_parentid=0", $null)
            [Request]::Get("api/restartprobes.htm?")
        )

        Restart-Probe -Wait:$false -Force
    }

    It "restarts specific probes" {
        $probes = Run Probe {

            $obj1 = GetItem
            $obj2 = GetItem
            $obj2.ObjId = 2

            WithItems ($obj1, $obj2) {
                Get-Probe
            }
        }

        $probes.Count | Should Be 2

        SetAddressValidatorResponse @(
            [Request]::Get("api/restartprobes.htm?id=1")
            [Request]::Get("api/restartprobes.htm?id=2")
        )

        $probes | Restart-Probe -Wait:$false -Force
    }

    It "waits for all probes to restart" {
        SetResponseAndClient "RestartProbeResponse"

        Restart-Probe -Force
    }

    It "waits for specified probes to restart" {
        SetResponseAndClient "RestartProbeResponse"

        $probes = Get-Probe

        $probes | Restart-Probe -Force
    }

    It "times out waiting for all probes to restart" {
        SetResponseAndClient "RestartProbeResponse"

        { Restart-Probe -Timeout 1 -Force } | Should Throw "Timed out waiting for 2 probes to restart"
    }

    It "times out waiting for some probes to restart" {
        SetResponseAndClient "RestartProbeResponse"

        { Restart-Probe -Timeout 19 -Force } | Should Throw "Timed out waiting for 1 probe to restart"
    }

    It "passes through when waiting" {
        SetResponseAndClient "RestartProbeResponse"

        $probe = Get-Probe -Count 1

        $newProbe = $probe | Restart-Probe -Wait -PassThru -Force

        $newProbe | Should Be $probe
    }

    It "passes through when not waiting" {
        $probe = Get-Probe -Count 1

        $newProbe = $probe | Restart-Probe -Wait:$false -PassThru -Force

        $newProbe | Should Be $probe
    }
}