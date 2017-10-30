. $PSScriptRoot\Support\Standalone.ps1

Describe "Add-NotificationTrigger" {

    SetResponseAndClient "SetNotificationTriggerResponse"

    It "throws setting an unsupported trigger type" {

        $params = New-TriggerParameters 1001 State

        { $params | Add-NotificationTrigger } | Should Throw "is not a valid trigger type"
    }
}