. $PSScriptRoot\..\..\Support\PowerShell\UnitTest.ps1

Describe "Get-Device" -Tag @("PowerShell", "UnitTest") {

    It "can deserialize" {
        $devices = Get-Device
        $devices.Count | Should Be 1
    }

    It "can filter by status" {
        $items = (GetItem),(GetItem),(GetItem),(GetItem)
        $items.Count | Should Be 4

        $items[0].StatusRaw = "5" # Down
        $items[1].StatusRaw = "3" # Up
        $items[2].StatusRaw = "3" # Up
        $items[3].StatusRaw = "8" # PausedByDependency

        WithItems $items {
            $devices = Get-Device -Status Up,Paused

            $devices.Count | Should Be 3
        }
    }

    It "filters by group name" {
        SetAddressValidatorResponse "filter_group=@sub(2)"

        $sensors = Get-Device -Group 2*
        $sensors.Count | Should Be 0

        SetAddressValidatorResponse "filter_group=@sub(1)"

        $sensors = Get-Device -Group 1*
        $sensors.Count | Should Be 2
    }

    It "filters by probe name" {
        SetAddressValidatorResponse "filter_probe=@sub(2)"

        $sensors = Get-Device -Probe 2*
        $sensors.Count | Should Be 0

        SetAddressValidatorResponse "filter_probe=@sub(1)"

        $sensors = Get-Device -Probe 1*
        $sensors.Count | Should Be 2
    }

    Context "Group Recursion" {
        It "retrieves devices from a uniquely named group" {
            SetResponseAndClientWithArguments "RecursiveRequestResponse" "DeviceUniqueGroup"

            $devices = Get-Group | Get-Device *

            $devices.Count | Should Be 2
        }

        It "retrieves devices from a group with a duplicated name" {
            SetResponseAndClientWithArguments "RecursiveRequestResponse" "DeviceDuplicateGroup"

            $devices = Get-Group | Get-Device *

            $devices.Count | Should Be 2
        }

        It "retrieves devices from a uniquely named group containing child groups" {
            SetResponseAndClientWithArguments "RecursiveRequestResponse" "DeviceUniqueChildGroup"

            $devices = Get-Group | Get-Device *

            $devices.Count | Should Be 3
        }

        It "retrieves devices from a group with a duplicated name containing child groups" {
            SetResponseAndClientWithArguments "RecursiveRequestResponse" "DeviceDuplicateChildGroup"

            $devices = Get-Group | Get-Device *

            $devices.Count | Should Be 3
        }

        It "retrieves devices from all groups with a duplicated name with -Recurse:`$false" {
            SetResponseAndClientWithArguments "RecursiveRequestResponse" "DeviceNoRecurse"

            $devices = Get-Group | Get-Device * -Recurse:$false

            $devices.Count | Should Be 2
        }

        It "retrieves devices from a group hierarchy with no devices in the parent group" {
            SetResponseAndClientWithArguments "RecursiveRequestResponse" "DeviceDeepNesting"

            $devices = Get-Group | Get-Device *

            $devices.Count | Should Be 2
        }
    Context "Dynamic" {
        It "uses dynamic parameters" {
            SetAddressValidatorResponse "filter_position=0000000030"

            Get-Device -Position 3
        }

        It "throws using a dynamic parameter not supported by this type" {
            { Get-Device -LastValue 3 } | Should Throw "A parameter cannot be found that matches parameter name 'LastValue'"
        }

        It "uses dynamic parameters in conjunction with regular parameters" {

            SetAddressValidatorResponse "filter_name=@sub(dc)&filter_objid=3&filter_parentid=30"

            Get-Device *dc* -Id 3 -ParentId 30
        }

        It "uses wildcards with a dynamic parameter" {
            
            SetAddressValidatorResponse "filter_message=@sub(1)"

            $device = @(Get-Device -Count 3 -Message "*1")

            $device.Count | Should Be 1

            $device.Name | Should Be "Probe Device1"
        }

        It "uses a bool with a dynamic parameter" {

            SetAddressValidatorResponse "filter_favorite=1"

            Get-Device -Favorite $true
        }

        It "throws using unsupported filters in dynamic parameters" {
            { Get-Device -Favorite $false } | Should Throw "Cannot filter where property 'Favorite' equals '0'."
        }
    }
}