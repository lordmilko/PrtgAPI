. $PSScriptRoot\Support\IntegrationTest.ps1

Describe "IT_Get-NotificationTrigger" {
	It "can retrieve all triggers" {
		$triggers = Get-Device -Id (Settings Device) | Get-NotificationTrigger

		($triggers|where inherited -EQ $true).Count|Should Be 1
		($triggers|where inherited -NE $true).Count|Should Be 5
	}

	It "can retrieve uninherited triggers" {
		$triggers = Get-Device -Id (Settings Device) | Get-NotificationTrigger -Inherited $false

		($triggers|where type -EQ state).Count|Should Be 1
		($triggers|where type -EQ volume).Count|Should Be 1
		($triggers|where type -EQ speed).Count|Should Be 1
		($triggers|where type -EQ change).Count|Should Be 1
		($triggers|where type -EQ threshold).Count|Should Be 1
	}

	It "can filter by OnNotificationAction" {
		throw
	}

	It "can filter by type" {
		$triggers = Get-Device -Id (Settings Device) | Get-NotificationTrigger -Type State

		$triggers.Count|Should Be 1
	}
}

Describe "Edit-NotificationTriggerProperty" {
	It "can edit OnNotificationAction" {
		$trigger = Get-Device -Id (Settings Device) | Get-NotificationTrigger -Type State -Inherited $false
		$action = Get-NotificationAction *ticket*

		$trigger.Count | Should Be 1
		$action.Count | Should Be 1

		$trigger.OnNotificationAction | Should Not Be $action.Name

		# WARNING - when running our tests its important the assembly tests are run first. if theyre not, thats a huge problem and so we'll
		# need to do a backup from within powershell

		#maybe, in order to determine whether we've run initialization or not, we have a private static variable we set in c#. both the c# and powershell
		# versions check if its been set and if so dont do the preinit stuff

		#we dont need to implement that functionality in powershell, we can put the init stuff in c# in a static method that takes no arguments and just call it from powershell

		#throw

		$trigger | Edit-NotificationTriggerProperty OnNotificationAction $action

		$newTrigger = Get-Device -Id (Settings Device) | Get-NotificationTrigger -Type State -Inherited $false

		$newTrigger.OnNotificationAction.Name | Should Be $action.Name
	}
}