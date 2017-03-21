. $PSScriptRoot\Support\IntegrationTest.ps1

Describe "IT_Acknowledge-Sensor" {

	$message = "Unit Testing FTW!"

	It "can acknowledge indefinitely" {

		$sensor = Get-Sensor -Id (Settings DownSensor)
		$sensor.Status | Should Be Down

		$sensor | Acknowledge-Sensor -Forever -Message $message

		$sensor | Refresh-Object
		Sleep 30

		$acknowledgedSensor = Get-Sensor -Id (Settings DownSensor)

		$acknowledgedSensor.Message | Should BeLike "*$message*"
		$acknowledgedSensor.Status | Should Be DownAcknowledged

		$sensor | Pause-Object -Duration 1
		$sensor | Resume-Object

		$sensor | Refresh-Object
		Sleep 30

		$finalSensor = Get-Sensor -Id (Settings DownSensor)
		$finalSensor.Status | Should Not Be DownAcknowledged
	}

	It "can acknowledge for duration" {
		$sensor = Get-Sensor -Id (Settings DownSensor)
		$sensor.Status | Should Be Down

		$sensor | Acknowledge-Sensor -Duration 1

		Sleep 60

		$sensor | Refresh-Object
		Sleep 30

		$finalSensor = Get-Sensor -Id (Settings DownSensor)
		$finalSensor.Status | Should Not Be DownAcknowledged
	}

	It "can acknowledge until" {
		$sensor = Get-Sensor -Id (Settings DownSensor)
		$sensor.Status | Should Be Down

		$until = (Get-Date).AddMinutes(1)

		$sensor | Acknowledge-Sensor -Until $until

		Sleep 60

		$sensor | Refresh-Object
		Sleep 30

		$finalSensor = Get-Sensor -Id (Settings DownSensor)
		$finalSensor.Status | Should Not Be DownAcknowledged
	}
}

Describe "IT_Pause-Object" {
	it "can pause indefinitely" {
		throw
	}

	it "can pause for duration" {
		throw
	}

	it "can pause until" {
		throw
	}
}