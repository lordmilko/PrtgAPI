. $PSScriptRoot\..\..\Support\PowerShell\IntegrationTestSafe.ps1

Describe "Get-Group_IT" -Tag @("PowerShell", "IntegrationTest") {
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
        $groups = Get-Group Ro*

        $groups.Count | Should BeGreaterThan 0

        foreach($group in $groups)
        {
            $group.Name | Should BeLike "Ro*"
        }
    }

    It "can filter by ending wildcard" {
        $groups = Get-Group *ot

        $groups.Count | Should BeGreaterThan 0

        foreach($group in $groups)
        {
            $group.Name | Should BeLike "*ot"
        }
    }

    It "can filter by wildcard contains" {
        $groups = Get-Group *oo*

        $groups.Count | Should BeGreaterThan 0

        foreach($group in $groups)
        {
            $group.Name | Should BeLike "*oo*"
        }
    }

    It "can filter by Id" {
        $group = Get-Group -Id (Settings Group)

        $group.Count | Should Be 1
        $group.Id | Should Be (Settings Group)
    }

    It "can filter by tags" {
        $group = Get-Group -Tags (Settings GroupTag)

        $group.Count | Should Be (Settings GroupsInTestProbe)
    }

    It "can pipe from groups" {
        $group = Get-Group -Id (Settings Group)

        $groups = $group | Get-Group -Recurse:$false

        $groups.Count | Should be (Settings GroupsInTestGroup)
    }

    It "can pipe from probes" {
        $probe = Get-Probe -Id (Settings Probe)

        $groups = $probe | Get-Group

        $groups.Count | Should Be (Settings GroupsInTestProbe)
    }

    It "can pipe from search filters" {
        $groups = New-SearchFilter name contains Root | Get-Group

        $groups.Count | Should Be 1
    }

    It "can recursively retrieve groups from a group" {
        $count = (Settings GroupsInTestGroup) + 1

        $groups = Get-Group -Id (Settings Group) | Get-Group

        ($groups.Count) | Should Be $count
    }

    It "can recursively retrieve groups from a group when specifying -Count" {

        $group = Get-Group -Id (Settings Group)

        $groups = $group | Get-Group -Count 1

        $groups.Count | Should Be 1

        $nonrecurseGroups = $group | Get-Group -Recurse:$false
        $allGroups = $group | Get-Group

        $nonrecurseGroups.Count | Should Be (Settings GroupsInTestGroup)
        $allGroups.Count | Should BeGreaterThan $nonrecurseSensors.Count

        $allGroupsViaCount = $group | Get-Group -Count $group.TotalGroups

        $allGroupsViaCount.Count | Should Be $group.TotalGroups
    }

    It "can recursively retrieve a grandchild group when filtering by name" {

        $group = Get-Group -Id (Settings Group)

        $grandChild = $group | Get-Group "NestedGroup"

        $grandChild.Name | Should Be "NestedGroup"
    }

    It "uses dynamic parameters" {
        $groups = Get-Group -Position 2

        $groups.Count | Should BeGreaterThan 0

        foreach($group in $groups)
        {
            $group.Position | Should Be 2
        }
    }

    It "uses dynamic parameters in conjunction with regular parameters" {
        $groups = @(Get-Group (Settings GroupName) -Position 2)

        $groups.Count | Should BeGreaterThan 0

        foreach($group in $groups)
        {
            $group.Name | Should Be (Settings GroupName)
            $group.Position | Should Be 2
        }
    }

    It "uses wildcards with dynamic parameters" {
        $groups = Get-Group -Message *o*

        $groups.Count | Should BeGreaterThan 0

        foreach($group in $groups)
        {
            $group.Message | Should BeLike "*o*"
        }
    }

    It "filters by probe name" {
        $groups = Get-Group -Probe (Settings ProbeName)

        $groups.Count | Should Be (Settings GroupsInTestProbe)
    }

    It "retrieves as a readonly user" {
        ReadOnlyClient {
            Get-Group
        }
    }
}