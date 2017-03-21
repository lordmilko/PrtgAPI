. $PSScriptRoot\Support\IntegrationTestSafe.ps1

Describe "IT_Get-NotificationAction" {
	It "has correct number of actions" {
		$actions = Get-NotificationAction

		$actions.Count | Should Be (Settings NotificationActionsInTestServer)
	}

	It "can filter by name" {
		$actions = Get-NotificationAction "Ticket Notification"

		$actions.Count | Should Be 1
	}
}