. $PSScriptRoot\..\..\Support\PowerShell\UnitTest.ps1

Describe "Get-Sensor" -Tag @("PowerShell", "UnitTest") {
    
    It "can deserialize" {
        $sensors = Get-Sensor
        $sensors.Count | Should Be 1
    }

    It "can filter valid wildcards" {
        $obj1 = GetItem
        $obj2 = GetItem

        $obj2.Tags = "testbananatest"

        WithItems ($obj1, $obj2) {
            $sensors = Get-Sensor -Tags *banana*
            $sensors.Count | Should Be 1
        }
    }

    It "filters with a wildcard in the middle" {
        $obj1 = GetItem
        $obj2 = GetItem

        $obj2.Tags = "testbananatest"

        WithItems ($obj1, $obj2) {
            $sensors = Get-Sensor -Tags *ba*a*
            $sensors.Count | Should Be 1
        }
    }

    It "can ignore invalid wildcards" {
        $obj1 = GetItem
        $obj2 = GetItem

        $obj2.Tags = "testbananatest"

        WithItems ($obj1, $obj2) {
            $sensors = Get-Sensor -Tags *apple*
            $sensors.Count | Should Be 0
        }
    }

    It "can process several IDs" {
        Get-Sensor -Id 1001,1002
    }

    It "can process multiple statuses" {
        Get-Sensor -Status Up,Down
    }

    It "filters via OR tags" {
        WithResponseArgs "AddressValidatorResponse" "filter_tags=@sub(wmi%2Cu)&filter_tags=@sub(wmimem)" {
            Get-Sensor -Tag wmi*u*,wmimem*
        }
    }

    It "filters via AND tags" {
        WithResponseArgs "AddressValidatorResponse" "filter_tags=@sub(wmi%2Cu%2Cwmimem)" {
            Get-Sensor -Tags wmi*u*,wmimem*
        }
    }

    Context "Group Recursion" {
        It "retrieves sensors from a uniquely named group" {
            SetResponseAndClientWithArguments "RecursiveRequestResponse" "SensorUniqueGroup"

            $sensors = Get-Group Servers | Get-Sensor *

            $sensors.Count | Should Be 4
        }

        It "retrieves sensors from a group with a duplicated name" {
            SetResponseAndClientWithArguments "RecursiveRequestResponse" "SensorDuplicateGroup"

            $sensors = Get-Group Servers | Get-Sensor *

            $sensors.Count | Should Be 4
        }

        It "retrieves sensors from a uniquely named group containing child groups" {
            SetResponseAndClientWithArguments "RecursiveRequestResponse" "SensorUniqueChildGroup"

            $sensors = Get-Group Servers | Get-Sensor *

            $sensors.Count | Should Be 6
        }

        It "retrieves sensors from a group with a duplicated name containing child groups" {
            SetResponseAndClientWithArguments "RecursiveRequestResponse" "SensorDuplicateChildGroup"

            $sensors = Get-Group Servers | Get-Sensor *

            $sensors.Count | Should Be 8
        }

        It "retrieves sensors from all groups with a duplicated name with -Recurse:`$false" {
            SetResponseAndClientWithArguments "RecursiveRequestResponse" "SensorNoRecurse"

            $sensors = Get-Group Servers | Get-Sensor * -Recurse:$false

            $sensors.Count | Should Be 5
        }

        It "retrieves sensors from a group hierarchy with no devices in the parent group" {
            SetResponseAndClientWithArguments "RecursiveRequestResponse" "SensorDeepNesting"

            $sensors = Get-Group Servers | Get-Sensor *

            $sensors.Count | Should Be 6
        }

        It "retrieves sensors from a child group with a name filter" {

            SetResponseAndClientWithArguments "RecursiveRequestResponse" "SensorDeepNestingChild"

            $sensors = Get-Group Servers | Get-Sensor Ping

            $sensors.Count | Should Be 1
        }

        It "retrieves sensors from a grandchild group with a name filter" {

            SetResponseAndClientWithArguments "RecursiveRequestResponse" "SensorDeepNestingGrandChild"

            $sensors = Get-Group Servers | Get-Sensor Uptime

            $sensors.Count | Should Be 1
        }

        It "retrieves sensors from a great-grandchild group with a name filter" {
            SetResponseAndClientWithArguments "RecursiveRequestResponse" "SensorDeepNestingGreatGrandChild"

            $sensors = Get-Group Servers | Get-Sensor Uptime

            $sensors.Count | Should Be 1
        }
    }
    
    Context "Take Iterator" {
        It "specifies a count without piping from groups" {

            SetResponseAndClientWithArguments "TakeIteratorResponse" "Sensors"

            $sensors = Get-Sensor -Count 2

            $sensors.Count | Should Be 2
        }

        It "specifies a count and a filter without piping from groups" {

            SetResponseAndClientWithArguments "TakeIteratorResponse" "SensorsWithFilter"

            $sensors = Get-Sensor ping -Count 2

            $sensors.Count | Should Be 2
        }

        It "specifies a count greater than the number that are available without piping from groups" {

            SetResponseAndClientWithArguments "TakeIteratorResponse" "SensorsInsufficient"

            $sensors = Get-Sensor -Count 2

            $sensors.Count | Should Be 1
        }

        It "specifies a count when piping from groups" {

            SetResponseAndClientWithArguments "TakeIteratorResponse" "SensorsFromGroup"

            $sensors = Get-Group -Count 1 | Get-Sensor -Count 2

            $sensors.Count | Should Be 2
        }

        It "specifies a count and a filter when piping from groups" {

            SetResponseAndClientWithArguments "TakeIteratorResponse" "SensorsWithFilterFromGroup"

            $sensors = Get-Group -Count 1 | Get-Sensor ping -Count 2

            $sensors.Count | Should Be 2
        }

        It "requests a full page after repeatedly failing to retrieve all required items" {

            SetResponseAndClientWithArguments "TakeIteratorResponse" "SensorsWithFilterInsufficient"

            $sensors = Get-Sensor ping -Count 2

            $sensors.Count | Should Be 1
        }

        It "tries to request a full page but there is only 1 record left after repeatedly failing to retrieve all required items" {
            SetResponseAndClientWithArguments "TakeIteratorResponse" "SensorsWithFilterInsufficientOneLeft"

            $sensors = Get-Sensor ping -Count 2

            $sensors.Count | Should Be 1
        }

        It "tries to request a full page but there are no records left after repeatedly failing to retrieve all required items" {
            SetResponseAndClientWithArguments "TakeIteratorResponse" "SensorsWithFilterInsufficientNoneLeft"

            $sensors = Get-Sensor ping -Count 2

            $sensors.Count | Should Be 1
        }

        It "tries to request a full page but there are negative records left after repeatedly failing to retrieve all required items" {
            SetResponseAndClientWithArguments "TakeIteratorResponse" "SensorsWithFilterInsufficientNegativeLeft"

            $sensors = Get-Sensor ping -Count 2

            $sensors.Count | Should Be 1
        }

        It "doesn't care after repeatedly failing to retrieve all required items when piping from groups" {

            SetResponseAndClientWithArguments "TakeIteratorResponse" "SensorsWithFilterFromGroupInsufficient"

            $sensors = Get-Group -Count 1 | Get-Sensor ping -Count 2

            $sensors.Count | Should Be 1
        }

        It "specifies a count and a filter when piping from a duplicate group" {

            SetResponseAndClientWithArguments "TakeIteratorResponse" "SensorsWithFilterFromDuplicateGroup"

            $sensors = Get-Group -Count 1 | Get-Sensor ping -Count 2

            $sensors.Count | Should Be 1
        }

        It "doesn't care after repeatedly failing to retrieve all required items from a duplicate group" {
            SetResponseAndClientWithArguments "TakeIteratorResponse" "SensorsWithFilterFromDuplicateGroupInsufficient"

            $sensors = Get-Group -Count 1 | Get-Sensor ping -Count 2

            $sensors.Count | Should Be 1
        }

        It "specifies a count when piping from groups with -Recurse:`$false" {
            SetResponseAndClientWithArguments "TakeIteratorResponse" "SensorsFromGroupNoRecurse"

            $sensors = Get-Group -Count 1 | Get-Sensor -Count 2 -Recurse:$false

            $sensors.Count | Should Be 1
        }

        It "specifies a count and a filter when piping from groups with -Recurse:`$false" {
            SetResponseAndClientWithArguments "TakeIteratorResponse" "SensorsWithFilterFromGroupNoRecurse"

            $sensors = Get-Group -Count 1 | Get-Sensor ping -Count 2 -Recurse:$false

            $sensors.Count | Should Be 2
        }
    }
    
    It "filters by name only using equals" {

        SetAddressValidatorResponse "filter_name=ping"

        Get-Sensor ping
    }

    It "filters by name and tags using contains" {
            
        SetAddressValidatorResponse "filter_name=@sub(ping)&filter_tags=@sub(wmicpu)"

        Get-Sensor ping -Tags wmicpu*
    }

    It "filters by name and devices using equals" {
            
        SetAddressValidatorResponse "filter_name=ping&filter_parentid=40"

        $device = Run Device { Get-Device }

        $device | Get-Sensor ping
    }

    It "filters by groups using contains" {
            
        SetAddressValidatorResponse "filter_name=@sub(ping)&filter_group=Windows+Infrastructure"

        $group = Run Group { Get-Group }

        $group | Get-Sensor ping -Recurse:$false
    }

    It "pipes and specifies a -Filter" {

        SetMultiTypeResponse

        $groups = Get-Group
        $groups.Count | Should Be 2

        SetAddressValidatorResponse @(
            [Request]::Sensors("filter_name=ping&filter_group=Windows+Infrastructure0", [Request]::DefaultObjectFlags)
            [Request]::Sensors("filter_name=ping&filter_group=Windows+Infrastructure1", [Request]::DefaultObjectFlags)
        )

        $groups | Get-Sensor -Filter (flt name eq ping) -Recurse:$false
    }

    It "filters by device name" {

        SetAddressValidatorResponse "filter_device=@sub(c)"

        $sensors = Get-Sensor -Device c*
        $sensors.Count | Should Be 0

        SetAddressValidatorResponse "filter_device=@sub(d)"

        $sensors = Get-Sensor -Device d*
        $sensors.Count | Should Be 2
    }

    It "filters by group name" {

        SetAddressValidatorResponse "filter_group=@sub(e)"

        $sensors = Get-Sensor -Group e*
        $sensors.Count | Should Be 0

        SetAddressValidatorResponse "filter_group=@sub(s)"

        $sensors = Get-Sensor -Group s*
        $sensors.Count | Should Be 2
    }

    It "filters by probe name" {
        SetAddressValidatorResponse "filter_probe=@sub(h)"

        $sensors = Get-Sensor -Probe h*
        $sensors.Count | Should Be 0

        SetAddressValidatorResponse "filter_probe=@sub(c)"

        $sensors = Get-Sensor -Probe c*
        $sensors.Count | Should Be 2
    }
    
    Context "Dynamic" {
        It "uses dynamic parameters" {
            SetAddressValidatorResponse "filter_position=0000000030"

            Get-Sensor -Position 3
        }

        It "throws using a dynamic parameter not supported by this type" {
            { Get-Sensor -Host dc-1 } | Should Throw "A parameter cannot be found that matches parameter name 'Host'"
        }

        It "uses dynamic parameters in conjunction with regular parameters" {

            SetAddressValidatorResponse "filter_name=@sub(ping)&filter_objid=3&filter_parentid=30"

            Get-Sensor *ping* -Id 3 -ParentId 30
        }

        It "uses wildcards with a dynamic parameter" {
            
            SetAddressValidatorResponse "filter_message=@sub(1)"

            $sensor = @(Get-Sensor -Count 3 -Message "*1")

            $sensor.Count | Should Be 1

            $sensor.Name | Should Be "Volume IO _Total1"
        }

        $date1 = (New-Object DateTime @(2000, 10, 2, 12, 10, 5, [DateTimeKind]::Utc))
        $date2 = (New-Object DateTime @(2000, 10, 3, 12, 10, 5, [DateTimeKind]::Utc))

        $singleCases = @(
            @{name = "bool";       expr = { Get-Sensor -Favorite $true };     expected = "filter_favorite=1"}
            @{name = "DateTime";   expr = { Get-Sensor -LastUp $date1 };      expected = "filter_lastup=36801.5070023148"}
            @{name = "integer";    expr = { Get-Sensor -ParentId 10 };        expected = "filter_parentid=10"}
            @{name = "StringEnum"; expr = { Get-Sensor -Type aggregation };   expected = "filter_type=@sub(aggregation)"}
            @{name = "TimeSpan";   expr = { Get-Sensor -UpDuration 00:01:00}; expected = "filter_uptimesince=000000000000060"}
        )

        $multipleCases = @(
            @{name = "DateTime";   expr = { Get-Sensor -LastUp $date1,$date2 };      expected = "filter_lastup=36801.5070023148&filter_lastup=36802.5070023148"}
            @{name = "integer";    expr = { Get-Sensor -ParentId 1,2 }; expected = "filter_parentid=1&filter_parentid=2"}
            @{name = "StringEnum"; expr = { Get-Sensor -Type aggregation,ping };   expected = "filter_type=@sub(aggregation)&filter_type=@sub(ping)"}
            @{name = "TimeSpan";   expr = { Get-Sensor -UpDuration 00:01:00,00:02:00}; expected = "filter_uptimesince=000000000000060&filter_uptimesince=000000000000120"}
        )

        It "uses a <name> with a dynamic parameter" -TestCases $singleCases {

            param($expr, $expected)

            SetAddressValidatorResponse $expected

            & $expr
        }

        It "uses multiple <name>s with a dynamic parameter" -TestCases $multipleCases {
            param($expr, $expected)

            SetAddressValidatorResponse $expected

            & $expr
        }

        It "specifies multiple dynamic parameters" {
            SetAddressValidatorResponse "filter_position=0000000040&filter_parentid=2"

            Get-Sensor -Position 4 -ParentId 2
        }

        It "uses a wildcard with a dynamic parameter" {
            SetAddressValidatorResponse "filter_comments=@sub(hello)"

            Get-Sensor -Comments hello*
        }

        It "specifies null to a a dynamic parameter" {
            SetAddressValidatorResponse "active&count=2&username"

            Get-Sensor -Comments $null -Count 2
        }

        It "throws using unsupported filters in dynamic parameters" {
            { Get-Sensor -Favorite $false } | Should Throw "Cannot filter where property 'Favorite' equals '0'."
        }
    }

    It "throws filtering by Status 0" {
        { Get-Sensor -Status 0 } | Should Throw "is not a member of type PrtgAPI.Status"
    }

    It "throws specifying an illegal string filter" {
        { flt name notequals ping | Get-Sensor } | Should Throw "Cannot filter where property 'Name' notequals 'ping'"
    }

    It "doesn't throw specifying -Illegal with an illegal string filter" {

        SetAddressValidatorResponse "filter_name=@neq(ping)"

        flt name notequals ping -Illegal | Get-Sensor
    }

    It "filters by an internal sensor type" {

        SetAddressValidatorResponse "filter_type=@sub(aggregation)&filter_type=@sub(ping)"

        Get-Sensor -Type sensorfactory,ping

        SetResponseAndClient "SensorFactorySourceResponse"

        $sensors = Get-Sensor -Type sensorfactory

        $sensors.Count | Should Be 1
        $sensors.Type.StringValue | Should Be "aggregation"
    }

    It "filters by a supported sensor type" {
        SetAddressValidatorResponse "filter_type=@sub(aggregation)&filter_type=@sub(ping)"

        Get-Sensor -Type factory,ping

        SetResponseAndClient "SensorFactorySourceResponse"

        $sensors = Get-Sensor -Type factory

        $sensors.Count | Should Be 1
        $sensors.Type.StringValue | Should Be "aggregation"
    }
}
