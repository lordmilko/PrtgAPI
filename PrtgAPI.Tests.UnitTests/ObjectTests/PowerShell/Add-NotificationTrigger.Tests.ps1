. $PSScriptRoot\Support\Standalone.ps1

Describe "Add-NotificationTrigger" {

    SetResponseAndClient "SetNotificationTriggerResponse"

    It "throws setting an unsupported trigger type" {

        $params = New-TriggerParameters 1001 State

        { $params | Add-NotificationTrigger } | Should Throw "is not a valid trigger type"
    }

    It "executes with -WhatIf" {
        $params = New-TriggerParameters 1001 State

        $params | Add-NotificationTrigger -WhatIf
    }

    It "resolves a created trigger" {
        SetResponseAndClient "DiffBasedResolveResponse"

        $params = New-TriggerParameters 1001 Threshold
        $params.Channel = 1
        $params.OnNotificationAction.Id = 301

        $trigger = $params | Add-NotificationTrigger -Resolve

        $trigger.SubId | Should Be 2
    }
}