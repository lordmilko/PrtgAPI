. $PSScriptRoot\Support\IntegrationTestSafe.ps1

Describe "Get-Probe_IT" {
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

    It "can pipe from search filters" {
        $probe = New-SearchFilter name equals "Local Probe" | Get-Probe

        $probe.Count | Should BeGreaterThan 0

        $probe.Name | Should Be "Local Probe"
    }
}