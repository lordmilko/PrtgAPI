. $PSScriptRoot\Support\IntegrationTestSafe.ps1

Describe "IT_Get-Group" {
	It "has correct number of groups" {
		$groups = Get-Group

		$groups.Count | Should Be (Settings GroupsInTestServer)
	}

	It "can filter by name" {
		$groups = Get-Group (Settings GroupName)

		$groups.Count | Should BeGreaterThan 0

		foreach($group in $groups)
		{
			$group.Name | Should Be (Settings GroupName)
		}
	}

	It "can filter by starting wildcard" {
		throw
	}

	It "can filter by ending wildcard" {
		throw
	}

	It "can filter by wildcard contains" {
		throw
	}

	It "can filter by Id" {
		$group = Get-Group -Id (Settings Group)

		$group.Count | Should Be 1
		$group.Id | Should Be (Settings Group)
	}

	It "can filter by tags" {
		$group = Get-Device -Tags (Settings GroupTag)

		$group.Count | Should Be 1
	}

	It "can pipe from groups" {
		$group = Get-Group -Id (Settings Group)

		$groups = $group | Get-Group

		$groups.Count | Should be (Settings GroupsInTestGroup)
	}

	It "can pipe from probes" {
		$probe = Get-Probe -Id (Settings Probe)

		$groups = $probe | Get-Group

		$groups.Count | Should Be (Settings GroupsInTestProbe)
	}

	It "can pipe from search filters" {
		throw
	}
}