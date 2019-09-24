. $PSScriptRoot\..\..\Support\PowerShell\IntegrationTestSafe.ps1

Describe "Get-ObjectLog_IT" -Tag @("PowerShell", "IntegrationTest") {
    
    It "retrieves all logs from an unspecified object" {
        Get-ObjectLog -Period All
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

        $logs = Get-ObjectLog -Period LastMonth

        $last = $logs | select -Last 1

        $last.DateTime.Date | Should Be $lastMonth.Date
    }
    
    It "filters by status" {
        $log = Get-ObjectLog -Status SystemStart -Count 1

        $log.Count | Should Be 1

        $log.Status | Should Be SystemStart
    }

    It "filters by name" {
        $log = Get-ObjectLog "System Health" -Period All | select -First 1

        $log.Count | Should Be 1

        $log.Name | Should Be "System Health"
    }

    It "retrieves logs by Id" {

        $idLogs = Get-ObjectLog -Id (Settings UpSensor)
        $sensorLogs = Get-Sensor -Id (Settings UpSensor) | Get-ObjectLog

        $idLogs.Count | Should Be $sensorLogs.Count
    }

    It "retrieves logs by Id with an EndDate" {

        $date = (Get-Date).AddHours(-12)

        $idLogs = Get-ObjectLog -Id (Settings Device) -EndDate $date
        $deviceLogs = Get-Device -Id (Settings Device) | Get-ObjectLog -EndDate $date

        $idLogs.Count | Should Be $deviceLogs.Count
    }

    It "filters by name specifying a count" {
        $logs = Get-ObjectLog ping -Count 3

        $logs.Count | Should Be 3

        $logs[0].Name | Should Be "Ping"
        $logs[1].Name | Should Be "Ping"
        $logs[2].Name | Should Be "Ping"
    }

    It "throws retrieving a nonexistant object by ID" {
        { Get-ObjectLog -Id -9999 } | Should Throw (ForeignMessage "Sorry, there is no object with the specified id")
    }

    It "retrieves as a readonly user" {
        ReadOnlyClient {
            Get-ObjectLog
        }
    }
}