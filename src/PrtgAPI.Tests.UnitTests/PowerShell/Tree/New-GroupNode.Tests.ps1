. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

Describe "New-GroupNode" -Tag @("PowerShell", "UnitTest") {
    It "creates a new node from a Group" {

        SetAddressValidatorResponse @(
            [Request]::Groups("filter_objid=2000", [Request]::DefaultObjectFlags)
        )

        $group = Get-Group -Id 2000

        $node = New-GroupNode $group

        $node.Type | Should Be Group
        $node.Value | Should Be $group
    }

    It "pipes in an existing Group" {
        
        SetAddressValidatorResponse @(
            [Request]::Groups("filter_objid=2000", [Request]::DefaultObjectFlags)
        )

        $group = Get-Group -Id 2000

        $node = $group | New-GroupNode

        $node.Type | Should Be Group
        $node.Value | Should Be $group
    }

    It "filters by name" {
        SetAddressValidatorResponse @(
            [Request]::Groups("filter_name=@sub(windows)", [Request]::DefaultObjectFlags)
        )

        $node = New-GroupNode *windows*
        $node.Count | Should Be 2
    }

    It "creates a tree from a ScriptBlock with a value" {

        $response = SetMultiTypeResponse
        $response.ForceTriggerUninherited = "Change"

        $group = Get-Group -Count 1

        $node = GroupNode $group {
            New-TriggerNode -ObjectId 2000 -Type Change
        }

        $node.Type | Should Be Group
        $node.Children.Count | Should Be 1
        $node.Children[0].Value.ObjectId | Should Be 2000
    }

    It "creates a tree from a ScriptBlock with an ID" {

        $response = SetMultiTypeResponse
        $response.ForceTriggerUninherited = "Change"

        $node = GroupNode -Id 2000 {
            TriggerNode -ObjectId 2000 -Type Change
        }

        $node.Type | Should Be Group
        $node.Children.Count | Should Be 1
        $node.Children[0].Value.ObjectId | Should Be 2000
    }

    It "creates a tree from a ScriptBlock with a name" {
        
        $response = SetMultiTypeResponse
        $response.ForceTriggerUninherited = "Change"

        $node = GroupNode Windows*0 {
            TriggerNode -ObjectId 2000 -Type Change
        }

        $node.Type | Should Be Group
        $node.Children.Count | Should Be 1
        $node.Children[0].Value.ObjectId | Should Be 2000
    }

    It "specifies multiple IDs" {
        SetAddressValidatorResponse @(
            [Request]::Groups("filter_objid=2000&filter_objid=2001", [Request]::DefaultObjectFlags)
        )

        $nodes = New-GroupNode -Id 2000,2001
        $nodes.Count | Should Be 2
    }

    It "pipes in multiple child nodes with value" {

        $response = SetMultiTypeResponse
        $response.ForceTriggerUninherited = "State","Speed"

        $trigger1 = New-TriggerNode -ObjectId 1000 -Type State
        $trigger2 = New-TriggerNode -ObjectId 3000 -Type Speed

        $group = Get-Group -Count 1

        $node = $trigger1,$trigger2 | New-GroupNode $group
        $node.Type | Should Be Group
        $node.Children.Count | Should Be 2
        $node.Children[0].Value | Should Be $trigger1.Value
        $node.Children[1].Value | Should Be $trigger2.Value
    }

    It "pipes in multiple child nodes with manual" {
        
        $response = SetMultiTypeResponse
        $response.ForceTriggerUninherited = "State","Speed"

        $trigger1 = New-TriggerNode -ObjectId 1000 -Type State
        $trigger2 = New-TriggerNode -ObjectId 3000 -Type Speed

        $node = $trigger1,$trigger2 | New-GroupNode -Id 2000
        $node.Type | Should Be Group
        $node.Children.Count | Should Be 2
        $node.Children[0].Value | Should Be $trigger1.Value
        $node.Children[1].Value | Should Be $trigger2.Value
    }

    It "throws attempting to create a sensor under a group" {
        { New-SensorNode -Id 4000 | New-GroupNode -Id 2000 } | Should Throw "Node 'Volume IO _Total0 (ID: 4000)' of type 'Sensor' cannot be a child of a node of type 'Group'."
    }
}