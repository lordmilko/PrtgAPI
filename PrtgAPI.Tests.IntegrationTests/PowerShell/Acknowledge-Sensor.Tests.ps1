. $PSScriptRoot\Support\IntegrationTest.ps1

Describe "Acknowledge-Sensor_IT" {

	$message = "Unit Testing FTW!"

	It "can acknowledge indefinitely" {

		$sensor = Get-Sensor -Id (Settings DownSensor)
		$sensor.Status | Should Be Down

		LogTestDetail "Acknowledging sensor indefinitely"
		$sensor | Acknowledge-Sensor -Forever -Message $message

		LogTestDetail "Refreshing object and sleeping for 30 seconds"
		$sensor | Refresh-Object
		Sleep 30

		$acknowledgedSensor = Get-Sensor -Id (Settings DownSensor)

		$acknowledgedSensor.Message | Should BeLike "*$message*"
		$acknowledgedSensor.Status | Should Be DownAcknowledged

		LogTestDetail "Pausing object for 1 minute and sleeping 5 seconds"
		$acknowledgedSensor | Pause-Object -Duration 1
		Sleep 5
		LogTestDetail "Resuming object"
		$acknowledgedSensor | Resume-Object

		LogTestDetail "Refreshing object and sleeping for 30 seconds"
		$acknowledgedSensor | Refresh-Object
		Sleep 30

		$finalSensor = Get-Sensor -Id (Settings DownSensor)
		$finalSensor.Status | Should Be Down
	}

	It "can acknowledge for duration" {
		$sensor = Get-Sensor -Id (Settings DownSensor)
		$sensor.Status | Should Be Down

		LogTestDetail "Acknowledging sensor for 1 minute"
		$sensor | Acknowledge-Sensor -Duration 1

		$acknowledgedSensor = Get-Sensor -Id (Settings DownSensor)
		$acknowledgedSensor.Status | Should Be DownAcknowledged

		LogTestDetail "Sleeping for 60 seconds"
		Sleep 60

		LogTestDetail "Refreshing object and sleeping for 30 seconds"
		$sensor | Refresh-Object
		Sleep 30

		$finalSensor = Get-Sensor -Id (Settings DownSensor)
		$finalSensor.Status | Should Be Down
	}

	It "can acknowledge until" {
		$sensor = Get-Sensor -Id (Settings DownSensor)
		$sensor.Status | Should Be Down

		$until = (Get-Date).AddMinutes(1)
		LogTestDetail "Acknowledging sensor until $until"
		$sensor | Acknowledge-Sensor -Until $until

		$acknowledgedSensor = Get-Sensor -Id (Settings DownSensor)
		$acknowledgedSensor.Status | Should Be DownAcknowledged

		LogTestDetail "Sleeping for 60 seconds"
		Sleep 60

		LogTestDetail "Refreshing object and sleeping for 30 seconds"
		$sensor | Refresh-Object
		Sleep 30

		$finalSensor = Get-Sensor -Id (Settings DownSensor)
		$finalSensor.Status | Should Be Down
	}
}