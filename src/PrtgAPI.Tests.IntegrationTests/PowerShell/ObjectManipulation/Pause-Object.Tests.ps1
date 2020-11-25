. $PSScriptRoot\..\..\Support\PowerShell\IntegrationTest.ps1

Describe "Pause-Object_IT" -Tag @("PowerShell", "IntegrationTest") {

    It "can pause indefinitely" {

        $message = "Integration Testing FTW!"
        
        $sensor = Get-Sensor -Id (Settings UpSensor)
        $sensor.Status | Should Be Up

        LogTestDetail "Pausing object indefinitely"
        $sensor | Pause-Object -Forever -Message $message

        LogTestDetail "Sleeping for 30 seconds while object refreshes"
        $pausedSensor = WaitForStatus $sensor PausedByUser 30 10

        LogTestDetail "Validating sensor status"
        $pausedSensor = Get-Sensor -Id (Settings UpSensor)

        $pausedSensor.Message | Should BeLike "*$message*"
        $pausedSensor.Status | Should Be PausedByUser

        LogTestDetail "Resuming sensor"
        $pausedSensor | Resume-Object
        LogTestDetail "Object should be unpaused. Refreshing object."

        $finalSensor = WaitForStatus $pausedSensor Up 30

        if($finalSensor.Status -eq "PausedByUser" -or $finalSensor.Status -eq "Unknown")
        {
            LogTestDetail "Sensor is still paused. Waiting 120 seconds"

            $finalSensor = WaitForStatus $pausedSensor Up 120
        }

        $finalSensor.Status | Should Be Up
    }

    It "can pause for duration" {

        $sensor = Get-Sensor -Id (Settings UpSensor)
        $sensor.Status | Should Be Up

        LogTestDetail "Pausing sensor for 1 minute"
        $sensor | Pause-Object -Duration 1

        $pausedSensor = WaitForStatus $sensor PausedUntil 30 10

        LogTestDetail "Confirming sensor is paused"
        $pausedSensor.Status | Should Be PausedUntil

        $pausedSensor = WaitForStatus $pausedSensor Up 90
        
        LogTestDetail "Object should be unpaused. Refreshing object."
        $finalSensor = WaitForStatus $pausedSensor Up 60 5
        
        $finalSensor.Status | Should Be Up
    }

    It "can pause until" {

        $sensor = Get-Sensor -Id (Settings UpSensor)
        $sensor.Status | Should Be Up

        $until = (Get-Date).AddMinutes(1)

        LogTestDetail "Pausing object until $until"
        $sensor | Pause-Object -Until $until
        
        $pausedSensor = WaitForStatus $sensor PausedUntil 30 10

        LogTestDetail "Confirming sensor is paused"
        $pausedSensor.Status | Should Be PausedUntil
        LogTestDetail "Sleeping for 90 seconds"
        $pausedSensor = WaitForStatus $pausedSensor Up 90

        LogTestDetail "Object should be unpaused. Refreshing object."
        $finalSensor = WaitForStatus $pausedSensor Up 60 5

        $finalSensor.Status | Should Be Up
    }
    
    It "can pause multiple in a single request" {
        
        $ids = ((Settings UpSensor),(Settings DownSensor))

        $sensors = Get-Sensor -Id $ids
        
        $sensors.Count | Should Be 2
        $sensors[0].Status | Should Be Up
        $sensors[1].Status | Should Be Down

        LogTestDetail "Pausing for 1 minute"
        $sensors | Pause-Object -Duration 1

        $sensors | Refresh-Object
        LogTestDetail "Sleeping for 30 seconds"
        Sleep 30

        LogTestDetail "Confirming sensors are paused"
        $sensors = Get-Sensor -Id $ids
        $sensors[0].Status | Should Be PausedUntil
        $sensors[1].Status | Should Be PausedUntil
        LogTestDetail "Sleeping for 90 seconds"
        $pausedSensors = WaitForStatus $sensors Up 90

        LogTestDetail "Object should be unpaused. Refreshing object."
        $finalSensors = WaitForStatus $pausedSensors Up 30 10

        if($finalSensors[0].Status -ne "Up")
        {
            LogTestDetail "Sleeping for 30 more seconds as object has not refreshed"
            $finalSensors = WaitForStatus $pausedSensors Up 30 10
        }

        $finalSensors[0].Status | Should Be Up
        $finalSensors[1].Status | Should Be Down
    }
}
