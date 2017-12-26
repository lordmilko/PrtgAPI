. $PSScriptRoot\Support\UnitTest.ps1

Describe "Get-Device" -Tag @("PowerShell", "UnitTest") {

    It "can deserialize" {
        $devices = Get-Device
        $devices.Count | Should Be 1
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
    }
}