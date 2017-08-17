. $PSScriptRoot\Support\UnitTest.ps1

Describe "New-NotificationTriggerParameter" {

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

	It "can create AddFrom parameter set" {
		$triggers.Count | Should Be 1

		$parameters = $triggers | New-TriggerParameter $device.Id

		$parameters.Action | Should Be Add
	}

	It "can create EditFrom parameter set" {
		$triggers.Count | Should Be 1

		$parameters = $triggers | New-TriggerParameter

		$parameters.Action | Should Be Edit
	}

	It "can convert StandardTriggerChannel to Channel" {
		$parameters = New-TriggerParameter $device.Id Threshold

		$parameters.Channel = "Total"

		$parameters.Channel | Should Be "Total"
	}

	It "can assign integer to Channel" {
		$parameters = New-TriggerParameter $device.Id Threshold

		$parameters.Channel = 3

		$parameters.Channel | Should Be "3"
	}

	It "can assign a PrtgAPI.Channel to Channel" {

		$sensor = Run Sensor { Get-Sensor }
		$channel = Run Channel { $sensor | Get-Channel }

		$parameters = New-TriggerParameter $device.Id Threshold

		$parameters.Channel = $channel

		$parameters.Channel | Should Be "Percent Available Memory"
	}

	It "throws assigning a random value to Channel" {
		$parameters = New-TriggerParameter $device.Id Threshold

		{ $parameters.Channel = "Banana" } | Should Throw "type must be convertable"
	}
}