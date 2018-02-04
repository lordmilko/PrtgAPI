. $PSScriptRoot\Support\IntegrationTestSafe.ps1

Describe "Get-ObjectLog_IT" {
    
    It "retrieves all logs from an unspecified object" {
        Get-ObjectLog -Since All
    }

    It "retrieves a specified number of logs" {
        $logs = Get-ObjectLog -Count 600

        $logs.Count | Should Be 600
    }
    
    It "retrieves only today's logs from the root with no parameters specified" {
        $today = (Get-Date).Date

        $logs = Get-ObjectLog

        $first = $logs | select -First 1
        $last = $logs | select -Last 1

        $first.DateTime.Date | Should Be $today
        $last.DateTime.Date | Should Be $today
    }

    It "retrieves only today's logs from a probe with no parameters specified" {
        $probe = Get-Probe -Id (Settings Probe)

        $today = (Get-Date).Date

        $logs = $probe | Get-ObjectLog

        $first = $logs | select -First 1
        $last = $logs | select -Last 1

        $first.DateTime.Date | Should Be $today
        $last.DateTime.Date | Should Be $today
    }

    It "retrieves a weeks worth of logs from a device with no parameters specified" {
        $device = Get-Device -Id (Settings Device)

        $today = (Get-Date).Date
        $lastWeek = (Get-Date).AddDays(-7).Date

        $logs = $device | Get-ObjectLog

        $first = $logs | select -First 1
        $last = $logs | select -Last 1

        $first.DateTime.Date | Should Be $today
        $last.DateTime.Date | Should Be $lastWeek
    }

    It "retrieves logs from an end date" {
        $start = (get-date).AddDays(-3)

        $logs = Get-ObjectLog -EndDate $start

        $last = $logs | select -Last 1

        $last.DateTime.Date | Should Be $start.Date
    }

    It "retrieves logs between an end date and a day prior on a probe" {
        $probe = Get-Probe -Id (Settings Probe)

        $today = (Get-Date).Date
        $yesterday = (Get-Date).AddDays(-1).Date

        $logs = $probe | Get-ObjectLog -EndDate $yesterday

        $first = $logs | select -First 1
        $last = $logs | select -Last 1

        $first.DateTime.Date | Should Be $today
        $last.DateTime.Date | Should Be $yesterday
    }

    It "retrieves logs between a start date and seven days prior on a device" {

        $device = Get-Device -Id (Settings Device)

        $start = (get-date).AddDays(-3)
        $end = $start.AddDays(-7)

        $logs = $device | Get-ObjectLog -StartDate $start

        $logs.Count | Should BeGreaterThan 0

        $first = $logs | Select -First 1
        $last = $logs | Select -Last 1

        $first.DateTime.Date | Should Be $start.Date
        $last.DateTime.Date | Should Be $end.Date
    }
    
    It "retrieves logs since a record age" {

        $lastMonth = (get-date).adddays(-30)

        $logs = Get-ObjectLog -Since LastMonth

        $last = $logs | select -Last 1

        $last.DateTime.Date | Should Be $lastMonth.Date
    }
    
    It "filters by status" {
        $log = Get-ObjectLog -Status SystemStart -Count 1

        $log.Count | Should Be 1

        $log.Status | Should Be SystemStart
    }

    It "filters by name" {
        $log = Get-ObjectLog "System Health" -Since AllTime | select -First 1

        $log.Count | Should Be 1

        $log.Name | Should Be "System Health"
    }
}