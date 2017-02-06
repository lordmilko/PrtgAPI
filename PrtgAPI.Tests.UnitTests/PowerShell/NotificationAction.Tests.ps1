. $PSScriptRoot\Support\UnitTest.ps1

Describe "Get-NotificationAction" {

	It "can deserialize" {
		$actions = Get-NotificationAction
		$actions.Count | Should Be 1
	}
}