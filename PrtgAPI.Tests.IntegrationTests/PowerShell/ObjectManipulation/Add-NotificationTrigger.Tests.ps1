. $PSScriptRoot\..\..\Support\PowerShell\IntegrationTest.ps1

Describe "Add-NotificationTrigger_IT" -Tag @("PowerShell", "IntegrationTest") {
    Context "Create from scratch" {

        function AddRemoveTrigger($type) {

            $existing = Get-Group -Id (Settings Group) | Get-Trigger -Inherited $false
            $existing | Should Be $null

            $param = New-TriggerParameters (Settings Group) $type
            $param | Add-Trigger

            $new = Get-Group -Id (Settings Group) | Get-Trigger -Inherited $false

            $new | Remove-Trigger -Force

            $new.Count | Should Be 1
            $afterRemoved = Get-Group -Id (Settings Group) | Get-Trigger -Inherited $false

            $afterRemoved.Count | Should Be 0
        }

        It "creates a state trigger"  { AddRemoveTrigger "State" }
        It "creates a change trigger" { AddRemoveTrigger "Change" }
        It "creates a volume trigger" { AddRemoveTrigger "Volume" }
        It "creates a speed trigger"  { AddRemoveTrigger "Speed" }
        It "creates a threshold trigger for a group" { AddRemoveTrigger "Threshold" }

        It "creates a threshold trigger for a sensor with a Channel" {

            $channel = Get-Sensor -Id (Settings ChannelSensor) | Get-Channel (Settings ChannelName)

            $channel.Count | Should Be 1
            $channel.Name | Should Be (Settings ChannelName)

            $param = New-TriggerParameters (Settings ChannelSensor) Threshold
            $param.Channel = $channel

            $param | Add-NotificationTrigger

            $trigger = Get-Sensor -Id (Settings ChannelSensor) | Get-Trigger -Inherited $false
            $trigger.Count | Should Be 1
            $trigger.Channel | Should Be (Settings ChannelName)

            $trigger | Remove-Trigger -Force
            $triggersAfterRemoved = Get-Sensor -Id (Settings ChannelSensor) | Get-Trigger -Inherited $false
            $triggersAfterRemoved.Count | Should Be 0
        }

        It "creates a threshold trigger for a sensor with a Channel ID" {

            $param = New-TriggerParameters (Settings ChannelSensor) Threshold
            $param.Channel = (Settings Channel)

            $param | Add-NotificationTrigger

            $trigger = Get-Sensor -Id (Settings ChannelSensor) | Get-Trigger -Inherited $false
            $trigger.Count | Should Be 1
            $trigger.Channel | Should Be (Settings ChannelName)

            $trigger | Remove-Trigger -Force
            $triggersAfterRemoved = Get-Sensor -Id (Settings ChannelSensor) | Get-Trigger -Inherited $false
            $triggersAfterRemoved.Count | Should Be 0
        }

        It "resolves a new trigger" {
            $probe = Get-Probe -Id (Settings Probe)

            $originalTriggers = $probe | Get-Trigger

            $param = New-TriggerParameters (Settings Probe) State
            $newTrigger = $param | Add-Trigger

            $newTriggers = $probe | Get-Trigger

            $diffTrigger = $newTriggers|where {$_.OnNotificationAction.ToString() -EQ "None"}

            $diffTrigger.SubId | Should Be $newTrigger.SubId
        }

        It "adds an invalid channel to a sensor" {

            $param = New-TriggerParameters (Settings ChannelSensor) Threshold
            $param.Channel = 300

            { $param | Add-Trigger } | Should Throw "Channel could not be found"
        }

        It "adds an enum channel to a sensor" {
            $param = New-TriggerParameters (Settings ChannelSensor) Threshold
            $param.Channel = "Total"

            { $param | Add-Trigger } | Should Throw "must refer to a specific Channel"
        }

        It "adds an invalid channel to a device" {
            $param = New-TriggerParameters (Settings Device) Threshold
            $param.Channel = 300

            { $param | Add-Trigger } | Should Throw "must be one of"
        }
    }

    Context "Clone existing" {

        function CloneRemoveTrigger($type) {

            $existing = Get-Device -Id (Settings Device) | Get-Trigger -Type $type -Inherited $false

            $existing.Count | Should Be 1

            $param = $existing | New-TriggerParameters (Settings Device)
            $param | Add-Trigger

            $new = Get-Device -Id (Settings Device) | Get-Trigger -Type $type -Inherited $false

            $new.Count | Should Be 2

            $newOne = @($new|where SubId -NE $existing.SubId)

            $newOne.Count | Should Be 1
            $newOne | Remove-Trigger -Force
        }

        It "clones a state trigger"  { CloneRemoveTrigger "State" }
        It "clones a change trigger" { CloneRemoveTrigger "Change" }
        It "clones a volume trigger" { CloneRemoveTrigger "Volume" }
        It "clones a speed trigger"  { CloneRemoveTrigger "Speed" }
        It "clones a threshold trigger from a device" { CloneRemoveTrigger "Threshold" }

        It "clones a threshold trigger from a sensor" {

            # Arrange

            $sensor = Get-Sensor -Id (Settings ChannelSensor)
            $channel = $sensor | Get-Channel | select -First 1

            # Add the first trigger

            $param = New-TriggerParameters (Settings ChannelSensor) Threshold
            $param.Channel = $channel
            $param | Add-Trigger

            $triggers = $sensor | Get-Trigger -Inherited $false
            $triggers.Count | Should Be 1

            # Clone the trigger

            $cloneParams = $triggers | New-TriggerParameters (Settings ChannelSensor)
            $cloneParams | Add-Trigger

            $bothTriggers = $sensor |Get-Trigger -Inherited $false
            $bothTriggers.Count | Should Be 2

            # Remove the triggers

            $bothTriggers | Remove-Trigger -Force

            $finalTriggers = $sensor | Get-Trigger -Inherited $false
            $finalTriggers.Count | Should Be 0
        }

        It "clones a threshold trigger from a device to a sensor" {
            $trigger = Get-Device -Id (Settings Device) | Get-Trigger -Type Threshold

            $trigger.Count | Should Be 1

            $param = $trigger | New-TriggerParameters (Settings ChannelSensor)

            { $param | Add-Trigger } | Should Throw "Channel 'Total' is not a valid value for sensor"
        }
    }
}
