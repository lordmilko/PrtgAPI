. $PSScriptRoot\Utilities.ps1

Describe "Get-Sensor" {
	
	It "can deserialize" {
		$sensors = Get-Sensor
		$sensors.Count | Should Be 1
	}

	It "can filter valid wildcards" {
		$obj1 = GetItem
		$obj2 = GetItem

		$obj2.Tags = "testbananatest"

		WithItems ($obj1, $obj2) {
			$sensors = Get-Sensor -Tags *banana*
			$sensors.Count | Should Be 1
		}
	}

	It "can ignore invalid wildcards" {
		$obj1 = GetItem
		$obj2 = GetItem

		$obj2.Tags = "testbananatest"

		WithItems ($obj1, $obj2) {
			$sensors = Get-Sensor -Tags *apple*
			$sensors.Count | Should Be 0
		}
	}
}

Describe "Get-Device" {

	It "can deserialize" {
		$devices = Get-Device
		$devices.Count | Should Be 1
	}
}

<#Describe "Get-Group" {

	It "can deserialize" {
		$groups = Get-Group
		$groups.Count | Should Be 1
	}
}

Describe "Get-Probe" {

	It "can deserialize" {
		$probes = Get-Probe
		$probes.Count | Should Be 1
	}
}#>

Describe "Get-NotificationAction" {

	It "can deserialize" {
		$actions = Get-NotificationAction
		$actions.Count | Should Be 1
	}
}

Describe "Get-NotificationTrigger" { # notificationtrigger doesnt support getitem; how do we make it use getitems
	It "can pipe from sensors" {
		$sensors = Run Sensor { Get-Sensor }
		$sensors.Count | Should Be 1

		$actions = $sensors | Get-NotificationTrigger
		$actions.Count | Should Be 5
	}

	# should we maybe make getitem return an array, and have everyone use it?
}