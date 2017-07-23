. $PSScriptRoot\Support\UnitTest.ps1

Describe "New-TriggerParameter" {

	$device = Run Device { Get-Device }
	$triggers = Run NotificationTrigger { $device | Get-NotificationTrigger -Type State }

	It "can create Add parameter set" {
		$triggers.Count | Should Be 1

		$parameters = New-TriggerParameter $device.Id State
	}

	It "can create Edit parameter set" {
		$triggers.Count | Should Be 1

		$parameters = New-TriggerParameter $device.Id $triggers.SubId State
	}

	It "can create From (Add) parameter set" {
		$triggers.Count | Should Be 1

		$parameters = $triggers | New-TriggerParameter $device.Id

		$parameters.Action | Should Be Add
	}

	It "can create From (Edit) parameter set" {
		$triggers.Count | Should Be 1

		$parameters = $triggers | New-TriggerParameter

		$parameters.Action | Should Be Edit
	}
}