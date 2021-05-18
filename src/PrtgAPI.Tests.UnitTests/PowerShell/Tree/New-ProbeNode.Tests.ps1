. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

Describe "New-ProbeNode" -Tag @("PowerShell", "UnitTest") {
    It "creates a new node from a Probe" {

        SetAddressValidatorResponse @(
            [Request]::Probes("filter_objid=1000&filter_parentid=0", [Request]::DefaultObjectFlags)
        )

        $device = Get-Probe -Id 1000

        $node = New-ProbeNode $device

        $node.Type | Should Be Probe
        $node.Value | Should Be $device
    }

    It "pipes in an existing Probe" {
        
        SetAddressValidatorResponse @(
            [Request]::Probes("filter_objid=1000&filter_parentid=0", [Request]::DefaultObjectFlags)
        )

        $device = Get-Probe -Id 1000

        $node = $device | New-ProbeNode

        $node.Type | Should Be Probe
        $node.Value | Should Be $device
    }

    It "filters by name" {
        SetAddressValidatorResponse @(
            [Request]::Probes("filter_name=@sub(127)&filter_parentid=0", [Request]::DefaultObjectFlags)
        )

        $node = New-ProbeNode *127*
        $node.Count | Should Be 2
    }

    It "creates a tree from a ScriptBlock with a value" {

        $response = SetMultiTypeResponse
        $response.ForceTriggerUninherited = "Change"

        $device = Get-Probe -Count 1

        $node = ProbeNode $device {
            New-TriggerNode -ObjectId 1000 -Type Change
        }

        $node.Type | Should Be Probe
        $node.Children.Count | Should Be 1
        $node.Children[0].Value.ObjectId | Should Be 1000
    }

    It "creates a tree from a ScriptBlock with an ID" {

        $response = SetMultiTypeResponse
        $response.ForceTriggerUninherited = "Change"

        $node = ProbeNode -Id 1000 {
            TriggerNode -ObjectId 1000 -Type Change
        }

        $node.Type | Should Be Probe
        $node.Children.Count | Should Be 1
        $node.Children[0].Value.ObjectId | Should Be 1000
    }

    It "creates a tree from a ScriptBlock with a name" {
        
        $response = SetMultiTypeResponse
        $response.ForceTriggerUninherited = "Change"

        $node = ProbeNode 127*0 {
            TriggerNode -ObjectId 1000 -Type Change
        }

        $node.Type | Should Be Probe
        $node.Children.Count | Should Be 1
        $node.Children[0].Value.ObjectId | Should Be 1000
    }

    It "specifies multiple IDs" {
        SetAddressValidatorResponse @(
            [Request]::Probes("filter_objid=1000&filter_objid=1001&filter_parentid=0", [Request]::DefaultObjectFlags)
        )

        $nodes = New-ProbeNode -Id 1000,1001
        $nodes.Count | Should Be 2
    }

    It "pipes in multiple child nodes with value" {

        $response = SetMultiTypeResponse
        $response.ForceTriggerUninherited = "State","Speed"

        $trigger1 = New-TriggerNode -ObjectId 1000 -Type State
        $trigger2 = New-TriggerNode -ObjectId 3000 -Type Speed

        $device = Get-Probe -Count 1

        $node = $trigger1,$trigger2 | New-ProbeNode $device
        $node.Type | Should Be Probe
        $node.Children.Count | Should Be 2
        $node.Children[0].Value | Should Be $trigger1.Value
        $node.Children[1].Value | Should Be $trigger2.Value
    }

    It "pipes in multiple child nodes with manual" {
        
        $response = SetMultiTypeResponse
        $response.ForceTriggerUninherited = "State","Speed"

        $trigger1 = New-TriggerNode -ObjectId 1000 -Type State
        $trigger2 = New-TriggerNode -ObjectId 3000 -Type Speed

        $node = $trigger1,$trigger2 | New-ProbeNode -Id 1000
        $node.Type | Should Be Probe
        $node.Children.Count | Should Be 2
        $node.Children[0].Value | Should Be $trigger1.Value
        $node.Children[1].Value | Should Be $trigger2.Value
    }

    It "throws attempting to create a sensor under a device" {
        { New-SensorNode -Id 4000 | New-ProbeNode -Id 1000 } | Should Throw "Node 'Volume IO _Total0 (ID: 4000)' of type 'Sensor' cannot be a child of a node of type 'Probe'."
    }
}