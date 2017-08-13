. $PSScriptRoot\Support\UnitTest.ps1

Describe "Get-Channel" {
	It "can deserialize" {
		$channels = Get-Channel -SensorId 1234

		$channels.Count | Should Be 1
	}

	It "filters by a matching name" {
		$channels = Get-Channel Percent* -SensorId 1234

		$channels.Count | Should Be 1
	}

	It "filters by a non matching name" {
		$channels = Get-Channel *banana* -SensorId 1234

		$channels | Should Be $null
	}
}