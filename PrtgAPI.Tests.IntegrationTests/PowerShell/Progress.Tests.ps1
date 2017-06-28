. $PSScriptRoot\Support\IntegrationTestSafe.ps1

Describe "Test-Progress_IT" {
	It "chains two cmdlets together" {
		Get-Device | Get-Sensor
	}

	It "chains three cmdlets together" {
		Get-Probe | Get-Device | Get-Sensor | Should Be 1
	}

	It "chains two cmdlets together separated by a filter" {
		Get-Device | select -First 1 | Get-Sensor
	}
}