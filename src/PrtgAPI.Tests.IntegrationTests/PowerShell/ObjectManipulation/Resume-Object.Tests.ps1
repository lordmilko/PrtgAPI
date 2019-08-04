. $PSScriptRoot\..\..\Support\PowerShell\IntegrationTest.ps1

Describe "Resume-Object_IT" -Tag @("PowerShell", "IntegrationTest") {
    It "resumes a paused object" {
        $sensor = Get-Sensor -Id (Settings PausedSensor)
        $sensor.Status | Should Be PausedByUser

        $sensor | Resume-Object

        $sensor | Refresh-Object
        LogTestDetail "Sleeping for 60 seconds while objects resume"
        Sleep 30
        $sensor | Refresh-Object
        Sleep 30

        $finalSensor = Get-Sensor -Id (Settings PausedSensor)
        $finalSensor.Status | Should Be Up
    }

    It "resumes a simulated error" {
        $sensor = Get-Sensor -Id (Settings UpSensor)

        $sensor.Status | Should Be Up

        LogTestDetail "Simulating error status"
        $sensor | Simulate-ErrorStatus

        $sensor | Refresh-Object
        LogTestDetail "Sleeping for 30 seconds while object refreshes"
        Sleep 30

        $redSensor = Get-Sensor -Id (Settings UpSensor)

        for($i = 0; $i -lt 5; $i++)
        {
            if($redSensor.Status -eq "Up")
            {
                LogTestDetail "Status was still Up. Waiting 30 seconds"
                Sleep 30
                $redSensor = Get-Sensor -Id (Settings UpSensor)
            }
            else
            {
                if($redSensor.Message -eq $null)
                {
                    LogTestDetail "Message was still null. Waiting 30 seconds"
                    Sleep 30
                    $redSensor = Get-Sensor -Id (Settings UpSensor)
                }
            }
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

    It "can resume multiple in a single request" {
        $upSensor = Get-Sensor -Id (Settings UpSensor)
        $pausedSensor = Get-Sensor -Id (Settings PausedSensor)

        LogTestDetail "Simulating error status on Up Sensor"
        $upSensor | Simulate-ErrorStatus

        $upSensor | Refresh-Object
        LogTestDetail "Sleeping for 30 seconds while object refreshes"
        Sleep 30

        $redSensor = Get-Sensor -Id (Settings UpSensor)

        if($redSensor.Status -eq "Up")
        {
            LogTestDetail "Status was still Up. Waiting 120 seconds"
            $redSensor | Refresh-Object
            Sleep 60
            $redSensor | Refresh-Object
            Sleep 60
            $redSensor = Get-Sensor -Id (Settings UpSensor)
        }

        $list = $redSensor,$pausedSensor

        $list | Resume-Object

        $list | Refresh-Object
        LogTestDetail "Sleeping for 60 seconds while objects resume"
        Sleep 30
        $list | Refresh-Object
        Sleep 30

        $ids = ((Settings UpSensor),(Settings PausedSensor))

        $sensors = Get-Sensor -Id $ids

        $sensors.Count | Should Be 2
        $sensors[0].Status | Should Be Up
        $sensors[1].Status | Should Be Up
    }
}