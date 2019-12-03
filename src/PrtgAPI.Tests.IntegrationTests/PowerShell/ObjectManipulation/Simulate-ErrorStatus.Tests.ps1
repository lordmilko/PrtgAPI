. $PSScriptRoot\..\..\Support\PowerShell\IntegrationTest.ps1

Describe "Simulate-ErrorStatus_IT" -Tag @("PowerShell", "IntegrationTest") {
    It "simulates an error status" {
        $sensor = Get-Sensor -Id (Settings UpSensor)

        $sensor.Status | Should Be Up

        LogTestDetail "Simulating error status"
        $sensor | Simulate-ErrorStatus

        $redSensor = WaitForStatus $sensor Down 30

        if($redSensor.Status -eq "Up")
        {
            LogTestDetail "Status was still Up. Waiting 120 seconds"
            $redSensor = WaitForStatus $sensor Down 120
        }

        $redSensor.Status | Should Be Down

        if(IsEnglish)
        {
            $redSensor.Message | Should BeLike "*simulated error*"
        }

        LogTestDetail "Resuming object"
        $redSensor | Resume-Object

        $finalSensor = WaitForStatus $redSensor Up 60
        $finalSensor.Status | Should Be Up
    }

    It "can simulate errors on multiple in a single request" {
        $ids = ((Settings UpSensor),(Settings ChannelSensor))

        $sensors = Get-Sensor -Id $ids
        $sensors[0].Status | Should Be Up
        $sensors[1].Status | Should Be Up

        LogTestDetail "Simulating error status on multiple sensors"
        $sensors | Simulate-ErrorStatus

        $redSensors = WaitForStatus $sensors Down 60

        if($redSensors|where { $_.Status -EQ "Up" -or $_.Status -eq "Warning" })
        {
            LogTestDetail "At least one sensor is still up or transitioning. Waiting 120 seconds"
            $redSensors = WaitForStatus $sensors Down 120
        }

        $redSensors[0].Status | Should Be Down
        $redSensors[1].Status | Should Be Down
        
        if(IsEnglish)
        {
            $redSensors[0].Message | Should BeLike "*simulated error*"
            $redSensors[1].Message | Should BeLike "*simulated error*"
        }

        LogTestDetail "Resuming object"
        $redSensors | Resume-Object

        LogTestDetail "Sleeping for 60 seconds while objects refresh"
        $finalSensors = WaitForStatus $redSensors Up 60

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