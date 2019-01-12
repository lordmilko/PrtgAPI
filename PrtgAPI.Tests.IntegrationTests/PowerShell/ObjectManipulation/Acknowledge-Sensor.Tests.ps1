. $PSScriptRoot\..\..\Support\PowerShell\IntegrationTest.ps1

Describe "Acknowledge-Sensor_IT" -Tag @("PowerShell", "IntegrationTest") {

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
        Sleep 5

        LogTestDetail "Refreshing object and sleeping for 30 seconds"
        $acknowledgedSensor | Refresh-Object
        Sleep 20
        $acknowledgedSensor | Refresh-Object
        Sleep 10

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
    
    It "can acknowledge multiple in a single request" {
        $upSensor = Get-Sensor -Id (Settings UpSensor)
        $upSensor.Status | Should Be Up

        $downSensor = Get-Sensor -Id (Settings DownSensor)
        $downSensor.Status | Should Be Down

        LogTestDetail "Simulating error status on Up Sensor"
        $upSensor | Simulate-ErrorStatus

        $upSensor | Refresh-Object
        LogTestDetail "Sleeping for 30 seconds while object refreshes"
        Sleep 30

        $newUpSensor = Get-Sensor -Id (Settings UpSensor)

        if($newUpSensor.Status -ne "Down")
        {
            LogTestDetail "Sleeping for 30 more seconds as object has not refreshed"
            $upSensor | Refresh-Object
            Sleep 10
            $upSensor | Refresh-Object
            Sleep 10
            $upSensor | Refresh-Object
            Sleep 10

            $newUpSensor = Get-Sensor -Id (Settings UpSensor)
        }

        $newUpSensor.Status | Should Be Down

        $downIds = ((Settings UpSensor),(Settings DownSensor))

        $downSensors = Get-Sensor -Id $downIds
        $downSensors.Count | Should Be 2

        LogTestDetail "Acknowledging for 1 minute"
        $downSensors | Acknowledge-Sensor -Duration 1

        $downSensors | Refresh-Object
        LogTestDetail "Sleeping for 10 seconds"
        Sleep 10

        LogTestDetail "Checking sensors are acknowledged"
        $newDownSensors = Get-Sensor -Id $downIds
        $newDownSensors[0].Status | Should Be DownAcknowledged
        $newDownSensors[1].Status | Should Be DownAcknowledged

        LogTestDetail "Sleeping for 60 seconds"
        Sleep 60

        LogTestDetail "Refreshing objects and sleeping for 30 seconds"
        $newDownSensors | Refresh-Object
        Sleep 30

        $finalSensors = Get-Sensor -Id $downIds
        $finalSensors[0].Status | Should Be Down
        $finalSensors[1].Status | Should Be Down

        $finalSensors[0] | Resume-Object
        LogTestDetail "Waiting for 30 seconds while Up Sensor resumes"
        Sleep 30

        $finalUpSensor = Get-Sensor -Id (Settings UpSensor)

        if($finalUpSensor.Status -ne "Up")
        {
            LogTestDetail "Sleeping for 30 more seconds as object has not refreshed"
            $finalSensors | Refresh-Object
            Sleep 10
            $finalSensors | Refresh-Object
            Sleep 10
            $finalSensors | Refresh-Object
            Sleep 10

            $finalUpSensor = Get-Sensor -Id (Settings UpSensor)
        }

        $finalUpSensor.Status | Should Be Up
    }
}