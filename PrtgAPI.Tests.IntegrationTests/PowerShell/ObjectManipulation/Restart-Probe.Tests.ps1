. $PSScriptRoot\..\..\Support\PowerShell\IntegrationTest.ps1

Describe "Restart-Probe_IT" -Tag @("PowerShell", "IntegrationTest") {
    It "waits for all probes to restart" {
        Restart-Probe

        $probes = Get-Probe

        foreach($probe in $probes)
        {
            $probe.ProbeStatus | Should Be Connected
        }
    }

    It "waits for a probe to restart" {
        $probe = Get-Probe -Id (Settings Probe)

        $probe | Restart-Probe

        $newProbe = Get-Probe -Id (Settings Probe)

        $probe.ProbeStatus | Should Be Connected
    }

    It "times out restarting a probe" {
        { Restart-Probe -Timeout 1 } | Should Throw "Timed out waiting for 1 probe to restart"

        # Wait for the server to come back online
        Restart-Probe -Wait
    }
}