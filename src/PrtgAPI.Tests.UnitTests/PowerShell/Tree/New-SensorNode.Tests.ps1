. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

Describe "New-SensorNode" -Tag @("PowerShell", "UnitTest") {
    It "creates a new node from a Sensor" {

        SetAddressValidatorResponse @(
            [Request]::Sensors("filter_objid=4000", [Request]::DefaultObjectFlags)
        )

        $sensor = Get-Sensor -Id 4000

        $node = New-SensorNode $sensor

        $node.Type | Should Be Sensor
        $node.Value | Should Be $sensor
    }

    It "pipes in an existing Sensor" {
        
        SetAddressValidatorResponse @(
            [Request]::Sensors("filter_objid=4000", [Request]::DefaultObjectFlags)
        )

        $sensor = Get-Sensor -Id 4000

        $node = $sensor | New-SensorNode

        $node.Type | Should Be Sensor
        $node.Value | Should Be $sensor
    }

    It "filters by name" {
        SetAddressValidatorResponse @(
            [Request]::Sensors("filter_name=@sub(volume)", [Request]::DefaultObjectFlags)
        )

        $node = New-SensorNode *volume*
        $node.Count | Should Be 2
    }

    It "creates a tree from a ScriptBlock with a value" {

        $response = SetMultiTypeResponse
        $response.ForceTriggerUninherited = "Change"

        $sensor = Get-Sensor -Count 1

        $node = SensorNode $sensor {
            New-TriggerNode -ObjectId 4000 -Type Change
        }

        $node.Type | Should Be Sensor
        $node.Children.Count | Should Be 1
        $node.Children[0].Value.ObjectId | Should Be 4000
    }

    It "creates a tree from a ScriptBlock with an ID" {

        $response = SetMultiTypeResponse
        $response.ForceTriggerUninherited = "Change"

        $node = SensorNode -Id 4000 {
            TriggerNode -ObjectId 4000 -Type Change
        }

        $node.Type | Should Be Sensor
        $node.Children.Count | Should Be 1
        $node.Children[0].Value.ObjectId | Should Be 4000
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
            [Request]::Sensors("filter_objid=4000&filter_objid=4001", [Request]::DefaultObjectFlags)
        )

        $nodes = New-SensorNode -Id 4000,4001
        $nodes.Count | Should Be 2
    }

    It "pipes in multiple child nodes with value" {

        $response = SetMultiTypeResponse
        $response.ForceTriggerUninherited = "State","Speed"

        $trigger1 = New-TriggerNode -ObjectId 1000 -Type State
        $trigger2 = New-TriggerNode -ObjectId 3000 -Type Speed

        $sensor = Get-Sensor -Count 1

        $node = $trigger1,$trigger2 | New-SensorNode $sensor
        $node.Type | Should Be Sensor
        $node.Children.Count | Should Be 2
        $node.Children[0].Value | Should Be $trigger1.Value
        $node.Children[1].Value | Should Be $trigger2.Value
    }

    It "pipes in multiple child nodes with manual" {
        
        $response = SetMultiTypeResponse
        $response.ForceTriggerUninherited = "State","Speed"

        $trigger1 = New-TriggerNode -ObjectId 1000 -Type State
        $trigger2 = New-TriggerNode -ObjectId 3000 -Type Speed

        $node = $trigger1,$trigger2 | New-SensorNode -Id 4000
        $node.Type | Should Be Sensor
        $node.Children.Count | Should Be 2
        $node.Children[0].Value | Should Be $trigger1.Value
        $node.Children[1].Value | Should Be $trigger2.Value
    }

    It "throws when a value of an invalid type is returned from a ScriptBlock" {
        SetMultiTypeResponse
        
        { New-SensorNode -Id 4000 { "a" } } | Should Throw "Expected -ScriptBlock to return one or more values of type 'PrtgNode', however response contained an invalid value of type 'System.String'."
    }

    It "displays an error when an ID could not be resolved" {
        SetMultiTypeResponse

        { New-SensorNode -Id 1001,4000 -ErrorAction Stop } | Should Throw "Could not resolve a Sensor with ID 1001."
    }
}