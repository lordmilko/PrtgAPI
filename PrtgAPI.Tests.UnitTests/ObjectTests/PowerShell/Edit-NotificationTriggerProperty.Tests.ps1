. $PSScriptRoot\Support\Standalone.ps1

Describe "Edit-NotificationTriggerProperty" {

    SetResponseAndClient "SetNotificationTriggerResponse"

    It "sets a TriggerChannel from a property" {
        $sensor = Get-Sensor
        $sensor.Id = 1

        $trigger = Run NotificationTrigger {
            $sensor | Get-Trigger -Type Threshold
        }

        $trigger.Count | Should Be 1
        $trigger.Inherited | Should Be $false

        $trigger | Edit-NotificationTriggerProperty Channel Primary
    }

    It "sets an invalid property" {
        $sensor = Get-Sensor

        $sensor.Id = 0

        $trigger = $sensor | Get-Trigger

        $trigger.Inherited | Should Be $false

        { $trigger | Edit-NotificationTriggerProperty Channel Primary } | Should Throw "Property 'Channel' does not exist on triggers of type 'State'"
    }

    It "throws trying to edit an inherited trigger" {
        $trigger = Get-Sensor | Get-Trigger

        $trigger.Inherited | Should Be $true

        { $trigger | Edit-NotificationTriggerProperty Channel Primary } | Should Throw "this trigger is inherited"
    }
}