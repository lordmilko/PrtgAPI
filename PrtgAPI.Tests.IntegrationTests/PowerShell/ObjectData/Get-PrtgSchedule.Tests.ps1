. $PSScriptRoot\..\..\Support\PowerShell\IntegrationTestSafe.ps1

Describe "Get-PrtgSchedule_IT" -Tag @("PowerShell", "IntegrationTest") {
    It "has the correct number of schedules" {

        $schedules = Get-PrtgSchedule
        $schedules.Count | Should Be (Settings SchedulesInTestServer)
    }

    It "can filter by wildcard contains" {

        $schedules = Get-PrtgSchedule *nights*
        $schedules.Count | Should BeGreaterThan 0
        
        ($schedules|where name -NotLike "*nights*").Count | Should Be 0
    }

    It "can filter by Id" {

        $schedule = Get-PrtgSchedule -Id (Settings Schedule)
        $schedule.Count | Should Be 1
        $schedule.Id | Should Be (Settings Schedule)
    }

    It "retrieves as a readonly user" {
        ReadOnlyClient {
            Get-PrtgSchedule
        }
    }
}