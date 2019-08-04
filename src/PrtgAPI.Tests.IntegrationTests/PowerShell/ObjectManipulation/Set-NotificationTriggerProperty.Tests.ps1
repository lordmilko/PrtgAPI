. $PSScriptRoot\..\..\Support\PowerShell\IntegrationTest.ps1

Describe "Set-NotificationTriggerProperty_IT" -Tag @("PowerShell", "IntegrationTest") {
    It "can edit OnNotificationAction" {
        $trigger = Get-Device -Id (Settings Device) | Get-NotificationTrigger -Type State -Inherited $false
        $action = Get-NotificationAction *ticket*

        $trigger.Count | Should Be 1
        $action.Count | Should Be 1

        $trigger.OnNotificationAction | Should Not Be $action.Name

        $trigger | Set-NotificationTriggerProperty OnNotificationAction $action

        $newTrigger = Get-Device -Id (Settings Device) | Get-NotificationTrigger -Type State -Inherited $false

        $newTrigger.OnNotificationAction.Name | Should Be $action.Name
    }

    It "can edit types requiring Parse()" {

        $device = Get-Device -Id (Settings Device)

        $trigger = $device | Get-Trigger -Type Threshold -Inherited $false

        $trigger | Set-NotificationTriggerProperty Channel Primary

        $postTrigger = $device | Get-Trigger -Type Threshold -Inherited $false
        $postTrigger.Channel | Should Be "Primary"

        $postTrigger | Set-NotificationTriggerProperty Channel Total

        $finalTrigger = $device | Get-Trigger -Type Threshold -Inherited $false
        $finalTrigger.Channel | Should Be "Total"
    }

    It "ignores Parse() errors" {
        $device = Get-Device -Id (Settings Device)

        $trigger = $device | Get-Trigger -Type Threshold -Inherited $false

        { $trigger | Set-NotificationTriggerProperty Channel "blah" } | Should Throw "Cannot convert value 'blah' of type 'System.String' to type 'TriggerChannel'. Value type must be convertable to one of PrtgAPI.StandardTriggerChannel, PrtgAPI.Channel or System.Int32."
    }

    It "can assign an enum value" {
        $trigger = Get-Device -Id (Settings Device) | Get-NotificationTrigger -Type State -Inherited $false
        $trigger.Count | Should Be 1

        $trigger.State | Should Not Be Warning

        $trigger | Set-NotificationTriggerProperty State Warning

        $newTrigger = Get-Device -Id (Settings Device) | Get-NotificationTrigger -Type State -Inherited $false

        $newTrigger.State | Should Be Warning

        #todo: should we maybe make state public?
    }

    It "throws editing an inherited notification trigger" {
        $device = Get-Device -Id (Settings Device)

        $trigger = @($device | Get-Trigger | where Inherited -EQ $true)

        $trigger.Count | Should Be 1
        $trigger.State | Should Be "Down"

        { $trigger | Set-TriggerProperty State Warning } | Should Throw "this trigger is inherited"
    }

    It "throws setting an Channel TriggerChannel on a device" {
        $device = Get-Device -Id (Settings Device)

        $trigger = $device | Get-Trigger -Type Threshold -Inherited $false
        
        $channel = Get-Channel -SensorId (Settings ChannelSensor) | where Id -EQ (Settings Channel)

        { $trigger | Set-NotificationTriggerProperty Channel $channel } | Should Throw "is not a valid channel"
    }

    It "throws setting an enum TriggerChannel on a sensor" {
        $sensor = Get-Sensor -Id (Settings DownSensor)
        $channel = $sensor | Get-Channel | select -First 1

        $param = New-TriggerParameters $sensor.Id Threshold
        $param.Channel = $channel

        $param | Add-Trigger

        $trigger = @($sensor | Get-Trigger -Type Threshold -Inherited $false)

        $trigger.Count | Should Be 1

        try
        {
            { $trigger | Set-TriggerProperty Channel "Primary" } | Should Throw "Channel 'Primary' does not exist on sensor 'Memory (Down Sensor)' (ID: 2060). Specify one of the following channel names and try again: 'Percent Available Memory', 'Available Memory'."
        }
        finally
        {
            $trigger | Remove-Trigger -Force
        }
    }

    It "throws setting an invalid Channel TriggerChannel on a sensor" {
        $sensor = Get-Sensor -Id (Settings DownSensor)
        $channel = $sensor | Get-Channel | select -First 1

        $param = New-TriggerParameters $sensor.Id Threshold
        $param.Channel = $channel

        $param | Add-Trigger

        $trigger = @($sensor | Get-Trigger -Type Threshold -Inherited $false)

        $trigger.Count | Should Be 1

        $badChannel = Get-Channel -SensorId (Settings ChannelSensor)|select -First 1

        try
        {
            { $trigger | Set-TriggerProperty Channel $badChannel } | Should Throw "Channel 'Disk Read Time %' does not exist on sensor 'Memory (Down Sensor)' (ID: 2060). Specify one of the following channel names and try again: 'Percent Available Memory', 'Available Memory'."
        }
        finally
        {
            $trigger | Remove-Trigger -Force
        }
    }
}