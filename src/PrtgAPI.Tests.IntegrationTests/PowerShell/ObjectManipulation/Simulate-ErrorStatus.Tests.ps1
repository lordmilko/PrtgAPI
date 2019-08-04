. $PSScriptRoot\..\..\Support\PowerShell\IntegrationTest.ps1

Describe "Simulate-ErrorStatus_IT" -Tag @("PowerShell", "IntegrationTest") {
    It "simulates an error status" {
        $sensor = Get-Sensor -Id (Settings UpSensor)

        $sensor.Status | Should Be Up

        LogTestDetail "Simulating error status"
        $sensor | Simulate-ErrorStatus

        $sensor | Refresh-Object
        LogTestDetail "Sleeping for 30 seconds while object refreshes"
        Sleep 30

        $redSensor = Get-Sensor -Id (Settings UpSensor)

        if($redSensor.Status -eq "Up")
        {
            LogTestDetail "Status was still Up. Waiting 120 seconds"
            Sleep 120
            $redSensor = Get-Sensor -Id (Settings UpSensor)
        }

        $redSensor.Status | Should Be Down
        $redSensor.Message | Should BeLike "Simulated error*"

        LogTestDetail "Resuming object"
        $redSensor | Resume-Object

        $sensor | Refresh-Object
        LogTestDetail "Sleeping for 60 seconds while object refreshes"
        Sleep 30
        $sensor | Refresh-Object
        Sleep 30

        $finalSensor = Get-Sensor -Id (Settings UpSensor)
        $finalSensor.Status | Should Be Up
    }

    It "can simulate errors on multiple in a single request" {
        $ids = ((Settings UpSensor),(Settings ChannelSensor))

        $sensors = Get-Sensor -Id $ids
        $sensors[0].Status | Should Be Up
        $sensors[1].Status | Should Be Up

        LogTestDetail "Simulating error status on multiple sensors"
        $sensors | Simulate-ErrorStatus

        $sensors | Refresh-Object
        LogTestDetail "Sleeping for 30 seconds while objects refresh"
        Sleep 30

        $sensors | Refresh-Object
        LogTestDetail "Sleeping for 30 seconds while objects refresh"
        Sleep 30

        $redSensors = Get-Sensor -Id $ids

        if($redSensors|where { $_.Status -EQ "Up" -or $_.Status -eq "Warning" })
        {
            LogTestDetail "At least one sensor is still up or transitioning. Waiting 120 seconds"
            Sleep 120
            $redSensors = Get-Sensor -Id $ids
        }

        $redSensors[0].Status | Should Be Down
        $redSensors[0].Message | Should BeLike "Simulated error*"
        $redSensors[1].Status | Should Be Down
        $redSensors[1].Message | Should BeLike "Simulated error*"

        LogTestDetail "Resuming object"
        $redSensors | Resume-Object

        $sensors | Refresh-Object
        LogTestDetail "Sleeping for 60 seconds while objects refresh"
        Sleep 30
        $sensors | Refresh-Object
        Sleep 30

        $finalSensors = Get-Sensor -Id $ids

        if($finalSensors[0].Status -ne "Up" -or $finalSensors[1].Status -ne "Up")
        {
            LogTestDetail "Sleeping for 30 more seconds as object has not refreshed"
            $finalSensors | Refresh-Object
            Sleep 10
            $finalSensors | Refresh-Object
            Sleep 10
            $finalSensors | Refresh-Object
            Sleep 10

            $finalSensors = Get-Sensor -Id $ids
        }

        $finalSensors[0].Status | Should Be Up
        $finalSensors[1].Status | Should Be Up
    }
}