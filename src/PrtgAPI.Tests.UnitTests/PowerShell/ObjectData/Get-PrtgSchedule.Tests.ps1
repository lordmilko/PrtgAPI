. $PSScriptRoot\..\..\Support\PowerShell\UnitTest.ps1

Describe "Get-Schedule" -Tag @("PowerShell", "UnitTest") {

    It "can deserialize" {
        $schedules = Get-PrtgSchedule
        $schedules.Count | Should Be 1
    }

    Context "TimeTable" {
        
        It "displays rows by default" {

            $schedule = Get-PrtgSchedule
            $row = $schedule.TimeTable | Select -First 1

            $row.Time | Should Be "00:00"
            $row.Monday | Should Be $false
            $row.Tuesday | Should Be $true
        }

        It "can retrieve slots by int" {
            $schedule = Get-PrtgSchedule
            $timetable = $schedule.TimeTable

            $oneAM = $timetable[1]

            $oneAM[1].Day | Should Be "Tuesday"
            $oneAM[1].Active | Should Be $true
        }

        It "can retrieve slots by DayOfWeek string" {

            $schedule = Get-PrtgSchedule
            $timetable = $schedule.TimeTable

            WithStrict {
                { $timetable["Tuesday"] } | Should Throw "Cannot convert value `"Tuesday`" to type `"System.Int32`""

                $tuesday = $timetable[[DayOfWeek]::Tuesday]
                $tuesday[1].Hour | Should Be 1
                $tuesday[1].Active | Should Be $true
            }
        }

        It "can retrieve slots by int and DayOfWeek string" {
            $schedule = Get-PrtgSchedule
            $timetable = $schedule.TimeTable

            WithStrict {
                $tuesday1am = $timetable[1, "Tuesday"]

                $tuesday1am.Hour | Should Be 1
                $tuesday1am.Day | Should Be "Tuesday"
                $tuesday1am.Active | Should Be $true
            }
        }

        It "can retrieve slots by TimeSpan" {
            $schedule = Get-PrtgSchedule
            $timetable = $schedule.TimeTable

            WithStrict {
                { $timetable["00:01:00"] } | Should Throw "Cannot convert value `"00:01:00`" to type `"System.Int32`""

                $oneAM = $timetable[[TimeSpan]"01:00"]
                $oneAM[1].Day | Should Be "Tuesday"
                $oneAM[1].Active | Should Be $true
            }
        }

        It "can retrieve slots by TimeSpan and DayOfWeek string" {
            $schedule = Get-PrtgSchedule
            $timetable = $schedule.TimeTable
            
            WithStrict {
                $tuesday1am = $timetable[[TimeSpan]"01:00", "Tuesday"]

                $tuesday1am.Hour | Should Be 1
                $tuesday1am.Day | Should Be "Tuesday"
                $tuesday1am.Active | Should Be $true
            }
        }
    }

}