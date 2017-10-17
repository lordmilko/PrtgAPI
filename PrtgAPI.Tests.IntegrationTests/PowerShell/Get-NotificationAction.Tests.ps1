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
}