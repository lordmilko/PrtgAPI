. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

Describe "New-TriggerNode" -Tag @("PowerShell", "UnitTest") {
    It "creates a new node from a Notification Trigger" {

        $response = SetMultiTypeResponse
        $response.ForceTriggerUninherited = "State"

        $trigger = Get-Device -Count 1 | Get-Trigger -Type State
        
        $node = New-TriggerNode $trigger

        $node.Type | Should Be Trigger
        $node.Value | Should Be $trigger
    }

    It "pipes in an existing Notification Trigger" {
        
        $response = SetMultiTypeResponse
        $response.ForceTriggerUninherited = "State"

        $trigger = Get-Device -Count 1 | Get-Trigger -Type State
        
        $node = $trigger | New-TriggerNode

        $node.Type | Should Be Trigger
        $node.Value | Should Be $trigger
    }

    It "filters by OnNotificationAction" {

        $response = SetMultiTypeResponse
        $response.ForceTriggerUninherited = "State"

        $node = New-TriggerNode -ObjectId 4000 *email* -Type State

        $node.Value.Type | Should Be State
    }

    It "specifies a Sub ID" {

        $response = SetMultiTypeResponse
        $response.ForceTriggerUninherited = "Volume"

        $node = New-TriggerNode -ObjectId 3000 -SubId 6
        $node.Value.Type | Should Be Volume
        $node.Value.SubId | Should Be 6
    }

    It "displays an error when a SubID could not be resolved" {
        SetMultiTypeResponse

        { New-TriggerNode -ObjectId 4000 -SubId 6,20 -ErrorAction Stop } | Should Throw "Could not resolve a Notification Trigger with SubID 20."
    }
}