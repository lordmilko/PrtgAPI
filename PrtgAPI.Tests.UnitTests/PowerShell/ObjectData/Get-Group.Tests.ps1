. $PSScriptRoot\..\..\Support\PowerShell\UnitTest.ps1

Describe "Get-Group" -Tag @("PowerShell", "UnitTest") {

    It "can deserialize" {
        $groups = Get-Group
        $groups.Count | Should Be 1
    }

    It "can filter by status" {
        $items = (GetItem),(GetItem),(GetItem),(GetItem)
        $items.Count | Should Be 4

        $items[0].StatusRaw = "5" # Down
        $items[1].StatusRaw = "3" # Up
        $items[2].StatusRaw = "3" # Up
        $items[3].StatusRaw = "8" # PausedByDependency

        WithItems $items {
            $groups = Get-Group -Status Up,Paused

            $groups.Count | Should Be 3
        }
    }

    It "can filter valid wildcards" {
        $obj1 = GetItem
        $obj2 = GetItem

        $obj2.Tags = "testbananatest"

        WithItems ($obj1, $obj2) {
            $groups = Get-Group -Tags *banana*
            $groups.Count | Should Be 1
        }
    }

    It "can ignore invalid wildcards" {
        $obj1 = GetItem
        $obj2 = GetItem

        $obj2.Tags = "testbananatest"

        WithItems ($obj1, $obj2) {
            $groups = Get-Group -Tags *apple*
            $groups.Count | Should Be 0
        }
    }

    It "filters by probe name" {
        SetAddressValidatorResponse "filter_probe=@sub(2)"

        $sensors = Get-Group -Probe 2*
        $sensors.Count | Should Be 0

        SetAddressValidatorResponse "filter_probe=@sub(1)"

        $sensors = Get-Group -Probe 1*
        $sensors.Count | Should Be 2
    }

    Context "Group Recursion" {
        It "retrieves groups from a uniquely named group" {
            SetResponseAndClientWithArguments "RecursiveRequestResponse" "GroupUniqueGroup"

            $groups = Get-Group Servers | Get-Group *

            $groups.Count | Should Be 1
        }

        It "retrieves groups from a group with a duplicated name" {
            SetResponseAndClientWithArguments "RecursiveRequestResponse" "GroupDuplicateGroup"

            $groups = Get-Group Servers | Get-Group *

            $groups.Count | Should Be 1
        }

        It "retrieves groups from a uniquely named group containing child groups" {
            SetResponseAndClientWithArguments "RecursiveRequestResponse" "GroupUniqueChildGroup"

            $groups = Get-Group Servers | Get-Group *

            $groups.Count | Should Be 2
        }

        It "retrieves groups from a group with a duplicated name containing child groups" {
            SetResponseAndClientWithArguments "RecursiveRequestResponse" "GroupDuplicateChildGroup"

            $groups = Get-Group Servers | Get-Group *

            $groups.Count | Should Be 2
        }

        It "retrieves only one level of groups with -Recurse:`$false" {
            SetResponseAndClientWithArguments "RecursiveRequestResponse" "GroupNoRecurse"

            $groups = Get-Group Servers | Get-Group * -Recurse:$false

            $groups.Count | Should Be 1
        }

        It "retrieves groups from a hierarchy 6 levels deep" {
            SetResponseAndClientWithArguments "RecursiveRequestResponse" "GroupDeepNesting"

            $groups = Get-Group Servers | Get-Group *

            $groups.Count | Should Be 33
        }

        It "retrieves groups from a child group with a name filter" {

            SetResponseAndClientWithArguments "RecursiveRequestResponse" "GroupDeepNestingChild"

            $groups = Get-Group Servers | Get-Group Domain*

            $groups.Count | Should Be 1
        }

        It "retrieves groups from a grandchild group with a name filter" {
            SetResponseAndClientWithArguments "RecursiveRequestResponse" "GroupDeepNestingGrandChild"

            $groups = Get-Group Servers | Get-Group "Server 2003*"

            $groups.Count | Should Be 1
        }

        It "retrieves groups from a great-grandchild group with a name filter" {
            SetResponseAndClientWithArguments "RecursiveRequestResponse" "GroupDeepNestingGreatGrandChild"

            $groups = Get-Group Servers | Get-Group "Active 2003*"

            $groups.Count | Should Be 1
        }

        It "retrieves a single object while recursing" {
            SetResponseAndClientWithArguments "RecursiveRequestResponse" "GroupRecurseAvailableSingleCount"

            $group = @(Get-Group Windows* | Get-Group -Count 1)

            $group.Count | Should Be 1

            $group[0].Name | Should Be "Domain Controllers"
        }

        It "retrieves a specified count while recursing" {

            SetResponseAndClientWithArguments "RecursiveRequestResponse" "GroupRecurseAvailableCount"

            $groups = Get-Group Windows* | Get-Group -Count 2

            $groups.Count | Should Be 2

            $groups[0].Name | Should Be "Domain Controllers"
            $groups[1].Name | Should Be "Server 2003 DCs"
        }

        It "retrieves the maximum amount of records with a -Count that is too high" {

            SetResponseAndClientWithArguments "RecursiveRequestResponse" "GroupRecurseUnavailableCount"

            $groups = Get-Group Windows* | Get-Group -Count 9999

            $groups.Count | Should Be 32

            $groups[0].Name | Should Be "Domain Controllers"

            $groups[1].Name | Should Be "Server 2003 DCs"
            $groups[2].Name | Should Be "Active 2003 DCs"
            $groups[3].Name | Should Be "Fully Active 2003 DCs"
            $groups[4].Name | Should Be "Partially Active 2003 DCs"
            $groups[5].Name | Should Be "Inactive 2003 DCs"

            $groups[6].Name | Should Be "Server 2008 DCs"
            $groups[7].Name | Should Be "Active 2008 DCs"
            $groups[8].Name | Should Be "Inactive 2008 DCs"

            $groups[9].Name | Should Be "Server 2012 DCs"
            $groups[10].Name | Should Be "Active 2012 DCs"
            $groups[11].Name | Should Be "Inactive 2012 DCs"

            $groups[12].Name | Should Be "Exchange Servers"
            $groups[13].Name | Should Be "Server 2003 Exchanges"
            $groups[14].Name | Should Be "Active 2003 Exchanges"
            $groups[15].Name | Should Be "Inactive 2003 Exchanges"

            $groups[16].Name | Should Be "Server 2008 Exchanges"
            $groups[17].Name | Should Be "Active 2008 Exchanges"
            $groups[18].Name | Should Be "Inactive 2008 Exchanges"

            $groups[19].Name | Should Be "Server 2012 Exchanges"
            $groups[20].Name | Should Be "Active 2012 Exchanges"
            $groups[21].Name | Should Be "Inactive 2012 Exchanges"

            $groups[22].Name | Should Be "SQL Servers"
            $groups[23].Name | Should Be "Server 2003 SQLs"
            $groups[24].Name | Should Be "Active 2003 SQLs"
            $groups[25].Name | Should Be "Inactive 2003 SQLs"

            $groups[26].Name | Should Be "Server 2008 SQLs"
            $groups[27].Name | Should Be "Active 2008 SQLs"
            $groups[28].Name | Should Be "Inactive 2008 SQLs"

            $groups[29].Name | Should Be "Server 2012 SQLs"
            $groups[30].Name | Should Be "Active 2012 SQLs"
            $groups[31].Name | Should Be "Inactive 2012 SQLs"
        }

        It "retrieves the specified number of records with -Recurse:`$false" {

            SetResponseAndClientWithArguments "RecursiveRequestResponse" "GroupNoRecurseAvailableCount"

            $groups = Get-Group Windows* | Get-Group -Count 2 -Recurse:$false

            $groups.Count | Should Be 2

            $groups[0].Name | Should Be "Domain Controllers"
            $groups[1].Name | Should Be "Exchange Servers"
        }

        It "retrieves the maximum number of records with a -Count that is too high with -Recurse:`$false" {

            SetResponseAndClientWithArguments "RecursiveRequestResponse" "GroupNoRecurseUnavailableCount"

            $groups = Get-Group Windows* | Get-Group -Count 9999 -Recurse:$false

            $groups.Count | Should Be 3

            $groups[0].Name | Should Be "Domain Controllers"
            $groups[1].Name | Should Be "Exchange Servers"
            $groups[2].Name | Should Be "SQL Servers"
        }
    }

    Context "Dynamic" {
        It "uses dynamic parameters" {
            SetAddressValidatorResponse "filter_position=0000000030"

            Get-Group -Position 3
        }

        It "throws using a dynamic parameter not supported by this type" {
            { Get-Group -Host dc-1 } | Should Throw "A parameter cannot be found that matches parameter name 'Host'"
        }

        It "uses dynamic parameters in conjunction with regular parameters" {

            SetAddressValidatorResponse "filter_name=@sub(servers)&filter_objid=3&filter_parentid=30"

            Get-Group *servers* -Id 3 -ParentId 30
        }

        It "uses wildcards with a dynamic parameter" {
            
            SetAddressValidatorResponse "filter_message=@sub(1)"

            $group = @(Get-Group -Count 3 -Message "*1")

            $group.Count | Should Be 1

            $group.Name | Should Be "Windows Infrastructure1"
        }

        It "uses a bool with a dynamic parameter" {

            SetAddressValidatorResponse "filter_active=-1"

            Get-Group -Active $true
        }
    }
}