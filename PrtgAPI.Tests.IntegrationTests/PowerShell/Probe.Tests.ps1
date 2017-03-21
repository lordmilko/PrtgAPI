. $PSScriptRoot\Support\IntegrationTestSafe.ps1

Describe "IT_Get-Probe" {
	It "has correct number of probes" {
		$probes = Get-Probe

		$probes.Count | Should Be (Settings ProbesInTestServer)
	}

	It "can filter by name" {
		$probes = Get-Probe "Local Probe"

		$probes.Count | Should BeGreaterThan 0

		foreach($probe in $probes)
		{
			$probe.Name | Should Be "Local Probe"
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
		throw
	}

	It "can pipe from search filters" {
		throw
	}
}