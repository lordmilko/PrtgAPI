. $PSScriptRoot\..\..\Support\PowerShell\IntegrationTestSafe.ps1

Describe "Get-Probe_IT" -Tag @("PowerShell", "IntegrationTest") {
    It "has correct number of probes" {
        $probes = Get-Probe

        $probes.Count | Should Be (Settings ProbesInTestServer)
    }

    It "can filter by name" {
        $probes = Get-Probe (Settings ProbeName)

        $probes.Count | Should BeGreaterThan 0

        foreach($probe in $probes)
        {
            $probe.Name | Should Be (Settings ProbeName)
        }
    }

    It "can filter by starting wildcard" {
        $probes = Get-Probe Local*

        $probes.Count | Should BeGreaterThan 0

        foreach($probe in $probes)
        {
            $probe | Should BeLike "Local*"
        }
    }

    It "can filter by ending wildcard" {
        $probes = Get-Probe *Probe

        $probes.Count | Should BeGreaterThan 0

        foreach($probe in $probes)
        {
            $probe | Should BeLike "*Probe"
        }
    }

    It "can filter by wildcard contains" {
        $probes = Get-Probe *pro*

        $probes.Count | Should BeGreaterThan 0

        foreach($probe in $probes)
        {
            $probe | Should BeLike "*pro*"
        }
    }

    It "can filter by Id" {
        $probe = Get-Probe -Id (Settings Probe)

        $probe.Count | Should Be 1
        $probe.Id | Should Be (Settings Probe)
    }

    It "can filter by tags" {
        $probe = Get-Probe -Tags (Settings ProbeTag)

        $probe.Count | Should BeGreaterThan 0

        $probe.Tags | Should Match (Settings ProbeTag)
    }

    It "can filter by probe status" {
        $probe = Get-Probe -ProbeStatus Connected

        $probe.Count | Should BeGreaterThan 0

        $probe.ProbeStatus | Should Be "Connected"

        $disconnected = Get-Probe -ProbeStatus Disconnected

        $disconnected | Should BeNullOrEmpty
    }

    It "can pipe from search filters" {
        $probe = New-SearchFilter name equals "Local Probe" | Get-Probe

        $probe.Count | Should BeGreaterThan 0

        $probe.Name | Should Be "Local Probe"
    }

    It "uses dynamic parameters" {
        $probes = Get-Probe -Position 1

        $probes.Count | Should BeGreaterThan 0

        foreach($probe in $probes)
        {
            $probe.Position | Should Be 1
        }
    }

    It "uses dynamic parameters in conjunction with regular parameters" {
        $probes = @(Get-Probe (Settings ProbeName) -Position 1)

        $probes.Count | Should BeGreaterThan 0

        foreach($probe in $probes)
        {
            $probe.Name | Should Be (Settings ProbeName)
            $probe.Position | Should Be 1
        }
    }

    It "uses wildcards with dynamic parameters" {
        $probes = Get-Probe -Message *o*

        $probes.Count | Should BeGreaterThan 0

        foreach($probe in $probes)
        {
            $probe.Message | Should BeLike "*o*"
        }
    }

    It "retrieves as a readonly user" {
        ReadOnlyClient {
            Get-Probe
        }
    }
}