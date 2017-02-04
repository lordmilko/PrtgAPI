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
