. $PSScriptRoot\Support\Standalone.ps1

function GetTrigger($type)
{
    $sensor = Get-Sensor
    $sensor.Id = 1

    $trigger = Run NotificationTrigger {
        $sensor | Get-Trigger -Type $type | Select -First 1
    }

    return $trigger
}

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

    It "processes a state trigger" {
        $sensor = Get-Sensor
        $sensor.Id = 1

        $trigger = Run NotificationTrigger {
            $sensor | Get-Trigger -Type Threshold
        }

        $trigger | Edit-NotificationTriggerProperty OffNotificationAction $null
    }

    It "processes a channel trigger" {
        
        $trigger = GetTrigger "Change"
        $trigger.GetType().Name | Should Be "NotificationTrigger"

        $trigger | Edit-NotificationTriggerProperty OnNotificationAction $null
    }

    It "processes a speed trigger" {
        $trigger = GetTrigger "Speed"

        $trigger | Edit-NotificationTriggerProperty Latency 70
    }

    It "processes a volume trigger" {
        $trigger = GetTrigger "Volume"

        $trigger | Edit-NotificationTriggerProperty UnitSize KByte
    }

    It "throws trying to edit an inherited trigger" {
        $trigger = Get-Sensor | Get-Trigger

        $trigger.Inherited | Should Be $true

        { $trigger | Edit-NotificationTriggerProperty Channel Primary } | Should Throw "this trigger is inherited"
    }

    It "executes with -WhatIf" {
        $trigger = GetTrigger "Speed"

        $trigger | Edit-NotificationTriggerProperty Latency 70 -WhatIf
    }
}