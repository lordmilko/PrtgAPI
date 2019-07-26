. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

Describe "Remove-NotificationTrigger" -Tag @("PowerShell", "UnitTest") {

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

    It "can execute with -WhatIf" {
        $trigger = Get-Sensor | Get-Trigger
        $trigger.Inherited | Should Be $true

        $trigger | Remove-Trigger -Force -WhatIf
    }

    It "executes ShouldContinue" {

        $command = @"
`$trigger = New-Object PrtgAPI.NotificationTrigger
`$flags = [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance
`$field = `$trigger.GetType().GetField('onNotificationActionStr', `$flags)
`$field.SetValue(`$trigger, '-1|None')
`$trigger | Remove-NotificationTrigger
"@

        Invoke-Interactive $command
    }
}