
. $PSScriptRoot\Support\IntegrationTest.ps1

Describe "Edit-NotificationTriggerProperty_IT" {
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

	It "can edit types requiring Parse()" {

		$device = Get-Device -Id (Settings Device)

		$trigger = $device | Get-Trigger -Type Threshold -Inherited $false

		$trigger | Edit-NotificationTriggerProperty Channel Primary

		$postTrigger = $device | Get-Trigger -Type Threshold -Inherited $false
		$postTrigger.Channel | Should Be "Primary"

		$postTrigger | Edit-NotificationTriggerProperty Channel Total

		$finalTrigger = $device | Get-Trigger -Type Threshold -Inherited $false
		$finalTrigger.Channel | Should Be "Total"
	}

	It "ignores Parse() errors" {
		$device = Get-Device -Id (Settings Device)

		$trigger = $device | Get-Trigger -Type Threshold -Inherited $false

		{ $trigger | Edit-NotificationTriggerProperty Channel "blah" } | Should Throw "Object of type 'System.String' cannot be converted to type 'PrtgAPI.TriggerChannel'"
	}
}