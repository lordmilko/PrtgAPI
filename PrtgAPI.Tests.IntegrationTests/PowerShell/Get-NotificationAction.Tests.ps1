. $PSScriptRoot\Support\IntegrationTestSafe.ps1

Describe "Get-NotificationAction_IT" {
    It "has correct number of actions" {
        $actions = Get-NotificationAction

        $actions.Count | Should Be (Settings NotificationActionsInTestServer)
    }

    It "can filter by name" {
        $actions = Get-NotificationAction "Ticket Notification"

        $actions.Count | Should Be 1
    }

    It "can filter by id" {
        $actions = Get-NotificationAction -Id 301

        $actions.Count | Should Be 1
        $actions.Count | Should BeLessThan (Settings NotificationActionsInTestServer)
        $actions.Name | Should Be "Email to all members of group PRTG Users Group"
    }

    It "can filter by tags" {
        $actions = Get-NotificationAction -Tags (Settings NotificationActionTag)
        $actions.Count | Should Be 1
        $actions.Count | Should BeLessThan (Settings NotificationActionsInTestServer)
    }
}