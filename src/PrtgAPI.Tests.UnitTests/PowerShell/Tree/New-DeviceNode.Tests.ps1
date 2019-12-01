. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

Describe "New-DeviceNode" -Tag @("PowerShell", "UnitTest") {
    It "creates a new node from a Device" {

        SetAddressValidatorResponse @(
            [Request]::Devices("filter_objid=3000", [Request]::DefaultObjectFlags)
        )

        $device = Get-Device -Id 3000

        $node = New-DeviceNode $device

        $node.Type | Should Be Device
        $node.Value | Should Be $device
    }

    It "pipes in an existing Device" {
        
        SetAddressValidatorResponse @(
            [Request]::Devices("filter_objid=3000", [Request]::DefaultObjectFlags)
        )

        $device = Get-Device -Id 3000

        $node = $device | New-DeviceNode

        $node.Type | Should Be Device
        $node.Value | Should Be $device
    }

    It "filters by name" {
        SetAddressValidatorResponse @(
            [Request]::Devices("filter_name=@sub(probe)", [Request]::DefaultObjectFlags)
        )

        $node = New-DeviceNode *probe*
        $node.Count | Should Be 2
    }

    It "creates a tree from a ScriptBlock with a value" {

        $response = SetMultiTypeResponse
        $response.ForceTriggerUninherited = "Change"

        $device = Get-Device -Count 1
        $sensor = Get-Sensor -Count 1

        $node = DeviceNode $device {
            SensorNode $sensor {
                New-TriggerNode -ObjectId 4000 -Type Change
            }
        }

        $node.Type | Should Be Device
        $node.Children.Count | Should Be 1
        $node.Children[0].Type | Should Be Sensor
        
        $node.Children[0].Children.Count | Should Be 1
        $node.Children[0].Children[0].Value.ObjectId | Should Be 4000
    }

    It "specifies two IDs as well as a ScriptBlock" {

        $nodes = DeviceNode -Id 3000,3001 {
            SensorNode -Id 4000
        }

        $nodes.Count | Should Be 2

        $nodes[0].Children[0].Value | Should Be $nodes[1].Children[0].Value
    }

    It "specifies a ScriptBlock that invokes a single child cmdlet against two IDs" {

        $node = DeviceNode -Id 3000 {
            SensorNode -Id 4000,4001
        }

        $node.Children.Count | Should Be 2
        $node.Children[0].Value.Id | Should Be 4000
        $node.Children[1].Value.Id | Should Be 4001
    }

    It "specifies a ScriptBlock that invokes multiple child cmdlets" {

        $response = SetMultiTypeResponse
        $response.ForceTriggerUninherited = "Change"

        $node = DeviceNode -Id 3000 {
            SensorNode -Id 4000 {
                TriggerNode -ObjectId 4000 -Type Change
            }

            SensorNode -Id 4001 {
                TriggerNode -ObjectId 4001 -Type Change
            }
        }

        $node.Children.Count | Should Be 2
        $node.Children[0].Children[0].Value.ObjectId | Should Be 4000
        $node.Children[1].Children[0].Value.ObjectId | Should Be 4001
    }

    It "creates a tree from a ScriptBlock with an ID" {

        SetMultiTypeResponse

        $node = DeviceNode -Id 3000 {
            SensorNode -Id 4000
        }

        $node.Type | Should Be Device
        $node.Children.Count | Should Be 1
        $node.Children[0].Value.Id | Should Be 4000
    }

    It "creates a tree from a ScriptBlock with a name" {
        $response = SetMultiTypeResponse
        $response.ForceTriggerUninherited = "Change"

        $node = SensorNode vol*0 {
            TriggerNode -ObjectId 4000 -Type Change
        }

        $node.Type | Should Be Sensor
        $node.Children.Count | Should Be 1
        $node.Children[0].Value.ObjectId | Should Be 4000
    }

    It "specifies multiple IDs" {
        SetAddressValidatorResponse @(
            [Request]::Devices("filter_objid=3000&filter_objid=3001", [Request]::DefaultObjectFlags)
        )

        $nodes = New-DeviceNode -Id 3000,3001
        $nodes.Count | Should Be 2
    }

    It "pipes in multiple child nodes with value" {
        SetAddressValidatorResponse @(
            [Request]::Devices("filter_objid=3000", [Request]::DefaultObjectFlags)
            [Request]::Sensors("filter_objid=4000&filter_objid=4001", [Request]::DefaultObjectFlags)
        )

        $device = Get-Device -Id 3000

        $device = New-SensorNode -Id 4000,4001 | New-DeviceNode $device

        $device.Type | Should Be Device
        $device.Value.Id | Should Be 3000

        $device.Children.Count | Should Be 2
    }

    It "pipes in multiple child nodes with manual" {

        SetAddressValidatorResponse @(
            [Request]::Sensors("filter_objid=4000&filter_objid=4001", [Request]::DefaultObjectFlags)
            [Request]::Devices("filter_objid=3000", [Request]::DefaultObjectFlags)
        )

        $device = New-SensorNode -Id 4000,4001 | New-DeviceNode -Id 3000

        $device.Type | Should Be Device
        $device.Value.Id | Should Be 3000

        $device.Children.Count | Should Be 2
    }
}