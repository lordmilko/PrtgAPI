. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

Describe "Set-NotificationTrigger" -Tag @("PowerShell", "UnitTest") {

    SetResponseAndClient "SetNotificationTriggerResponse"

    It "allows a channel to be set on a sensor" {

        $params = New-TriggerParameters 1001 1 Threshold

        $params.Channel = Get-Sensor | Get-Channel

        $params | Set-NotificationTrigger
    }

    It "throws when a channel does not exist on a sensor" {
        $params = New-TriggerParameters 1001 1 Threshold

        $channel = Get-Sensor | Get-Channel
        $channel.Name = "Banana"

        $params.Channel = $channel

        { $params | Set-NotificationTrigger } | Should Throw "Channel 'Banana' is not a valid channel"
    }

    It "throws when setting an enum on a sensor" {
        $params = New-TriggerParameters 1001 1 Threshold

        $params.Channel = "Primary"

        { $params | Set-NotificationTrigger } | Should Throw "Channel 'Primary' is not a valid value for sensor"
    }
}