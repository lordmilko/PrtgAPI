. $PSScriptRoot\Support\Standalone.ps1

Describe "Remove-NotificationTrigger" {

    SetResponseAndClient "SetNotificationTriggerResponse"

    It "removes a non inherited trigger" {
        $sensor = Get-Sensor
        $sensor.Id = 0

        $trigger = $sensor | Get-Trigger
        $trigger.Inherited | Should Be $false

        $trigger | Remove-Trigger -Force
    }

    It "throws removing an inherited trigger" {
        $trigger = Get-Sensor | Get-Trigger
        $trigger.Inherited | Should Be $true

        { $trigger | Remove-Trigger -Force } | Should Throw "as it is inherited"
    }
}