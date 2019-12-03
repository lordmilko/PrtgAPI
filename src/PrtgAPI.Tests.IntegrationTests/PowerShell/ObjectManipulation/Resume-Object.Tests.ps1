. $PSScriptRoot\..\..\Support\PowerShell\IntegrationTest.ps1

Describe "Resume-Object_IT" -Tag @("PowerShell", "IntegrationTest") {
    It "resumes a paused object" {
        $sensor = Get-Sensor -Id (Settings PausedSensor)
        $sensor.Status | Should Be PausedByUser

        $sensor | Resume-Object

        LogTestDetail "Sleeping for 60 seconds while objects resume"
        $finalSensor = WaitForStatus $sensor Up 60

        $finalSensor.Status | Should Be Up
    }

    It "resumes a simulated error" {
        $sensor = Get-Sensor -Id (Settings UpSensor)

        $sensor.Status | Should Be Up

        LogTestDetail "Simulating error status"
        $sensor | Simulate-ErrorStatus

        $redSensor = WaitForStatus $sensor Down 30

        for($i = 0; $i -lt 5; $i++)
        {
            if($redSensor.Status -eq "Up")
            {
                $redSensor = WaitForStatus $sensor Down 30
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

        if(IsEnglish)
        {
            $redSensor.Message | Should BeLike "*simulated error*"
        }

        LogTestDetail "Resuming object"
        $redSensor | Resume-Object

        LogTestDetail "Sleeping for 60 seconds while object refreshes"

        $finalSensor = WaitForStatus $redSensor Up 60

        $finalSensor.Status | Should Be Up
    }

    It "can resume multiple in a single request" {
        $upSensor = Get-Sensor -Id (Settings UpSensor)
        $pausedSensor = Get-Sensor -Id (Settings PausedSensor)

        LogTestDetail "Simulating error status on Up Sensor"
        $upSensor | Simulate-ErrorStatus

        $upSensor | Refresh-Object
        LogTestDetail "Sleeping for 30 seconds while object refreshes"
        $redSensor = WaitForStatus $upSensor Down 30

        if($redSensor.Status -eq "Up")
        {
            LogTestDetail "Status was still Up. Waiting 120 seconds"
            $redSensor = WaitForStatus $upSensor Down 120
        }

        $list = $redSensor,$pausedSensor

        $list | Resume-Object

        $sensors = WaitForStatus $list Up 60

        $sensors.Count | Should Be 2
        $sensors[0].Status | Should Be Up
        $sensors[1].Status | Should Be Up
    }
}