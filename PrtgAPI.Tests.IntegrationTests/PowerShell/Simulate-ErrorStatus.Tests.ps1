. $PSScriptRoot\Support\IntegrationTest.ps1

Describe "Simulate-ErrorStatus_IT" {
	It "simulates an error status" {
		$sensor = Get-Sensor -Id (Settings UpSensor)

		LogTestDetail "Simulating error status"
		$sensor | Simulate-ErrorStatus

		$sensor | Refresh-Object
        LogTestDetail "Sleeping for 30 seconds while object refreshes"
		Sleep 30

		$redSensor = Get-Sensor -Id (Settings UpSensor)
		$redSensor.Status | Should Be Down
		$redSensor.Message | Should BeLike "Simulated error*"

		LogTestDetail "Resuming object"
		$redSensor | Resume-Object

		$sensor | Refresh-Object
        LogTestDetail "Sleeping for 30 seconds while object refreshes"
		Sleep 30

		$finalSensor = Get-Sensor -Id (Settings UpSensor)
		$finalSensor.Status | Should Be Up
	}
}