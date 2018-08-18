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

    It "throws when -Id is $null" {
        { Get-Sensor -Id $null } | Should Throw "The -Id parameter was specified however the parameter value was null"
    }

    It "can process several IDs" {
        Get-Sensor -Id 1001,1002
    }

    It "can process multiple statuses" {
        Get-Sensor -Status Up,Down
    }

    It "filters via OR tags" {
        WithResponseArgs "AddressValidatorResponse" "filter_tags=@sub(wmi%2cu)&filter_tags=@sub(wmimem)" {
            Get-Sensor -Tag wmi*u*,wmimem*
        }
    }

    It "filters via AND tags" {
        WithResponseArgs "AddressValidatorResponse" "filter_tags=@sub(wmi%2cu%2cwmimem)" {
            Get-Sensor -Tags wmi*u*,wmimem*
        }
    }

    Context "Group Recursion" {
        It "retrieves sensors from a uniquely named group" {
            SetResponseAndClientWithArguments "RecursiveRequestResponse" "SensorUniqueGroup"

            $sensors = Get-Group | Get-Sensor *

            $sensors.Count | Should Be 4
        }

        It "retrieves sensors from a group with a duplicated name" {
            SetResponseAndClientWithArguments "RecursiveRequestResponse" "SensorDuplicateGroup"

            $sensors = Get-Group | Get-Sensor *

            $sensors.Count | Should Be 4
        }

        It "retrieves sensors from a uniquely named group containing child groups" {
            SetResponseAndClientWithArguments "RecursiveRequestResponse" "SensorUniqueChildGroup"

            $sensors = Get-Group | Get-Sensor *

            $sensors.Count | Should Be 6
        }

        It "retrieves sensors from a group with a duplicated name containing child groups" {
            SetResponseAndClientWithArguments "RecursiveRequestResponse" "SensorDuplicateChildGroup"

            $sensors = Get-Group | Get-Sensor *

            $sensors.Count | Should Be 8
        }

        It "retrieves sensors from all groups with a duplicated name with -Recurse:`$false" {
            SetResponseAndClientWithArguments "RecursiveRequestResponse" "SensorNoRecurse"

            $sensors = Get-Group | Get-Sensor * -Recurse:$false

            $sensors.Count | Should Be 5
        }

        It "retrieves sensors from a group hierarchy with no devices in the parent group" {
            SetResponseAndClientWithArguments "RecursiveRequestResponse" "SensorDeepNesting"

            $sensors = Get-Group | Get-Sensor *

            $sensors.Count | Should Be 4
        }
    }

    It "filtering by name only uses equals" {

        SetAddressValidatorResponse "filter_name=ping"

        Get-Sensor ping
    }

    It "filtering by name and tags uses contains" {
            
        SetAddressValidatorResponse "filter_name=@sub(ping)&filter_tags=@sub(wmicpu)"

        Get-Sensor ping -Tags wmicpu*
    }

    It "filtering by name and devices uses equals" {
            
        SetAddressValidatorResponse "filter_name=ping&filter_parentid=40"

        $device = Run Device { Get-Device }

        $device | Get-Sensor ping
    }

    It "filtering by groups uses contains" {
            
        SetAddressValidatorResponse "filter_name=@sub(ping)&filter_group=Windows+Infrastructure"

        $group = Run Group { Get-Group }

        $group | Get-Sensor ping -Recurse:$false
    }

    It "throws filtering by Status 0" {
        { Get-Sensor -Status 0 } | Should Throw "is not a member of type PrtgAPI.Status"
    }
}

