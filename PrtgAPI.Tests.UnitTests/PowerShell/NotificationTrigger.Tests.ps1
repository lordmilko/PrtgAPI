. $PSScriptRoot\Utilities.ps1

Describe "Get-NotificationTrigger" { # notificationtrigger doesnt support getitem; how do we make it use getitems
	It "can pipe from sensors" {
		$sensors = Run Sensor { Get-Sensor }
		$sensors.Count | Should Be 1

		$triggers = $sensors | Get-NotificationTrigger
		$triggers.Count | Should Be 5
	}

	# should we maybe make getitem return an array, and have everyone use it?
}