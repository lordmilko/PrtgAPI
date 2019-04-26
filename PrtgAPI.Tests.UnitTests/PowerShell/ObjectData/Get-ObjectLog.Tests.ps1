. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

function SetLogAddressValidatorResponse($str)
{
    SetResponseAndClientWithArguments "LogAddressValidatorResponse" $str
}

function SetLogAddressValidatorResponseWithCount($str, $hashtable)
{
    $dictionary = GetCustomCountDictionary $hashtable

    SetResponseAndClientWithArguments "LogAddressValidatorResponse" @($str, $dictionary)
}

Describe "Get-ObjectLog" -Tag @("PowerShell", "UnitTest") {

    It "retrieves logs from an unspecified object" {
        SetLogAddressValidatorResponse "count=500&filter_drel=today"

        (Get-ObjectLog).Count | Should Be 2
    }

    It "streams from an unspecified object" {
        SetLogAddressValidatorResponse "count=500&filter_drel=today"

        Get-ObjectLog
    }

    It "streams from the root object" {
        SetLogAddressValidatorResponse "count=500&id=0&filter_drel=today"

        $group = Get-Group -count 1
        $group.Id = 0
        
        $group | Get-ObjectLog
    }

    It "streams when only a status is specified" {
        SetLogAddressValidatorResponse "count=500&id=0&filter_status=607&filter_drel=today"

        $group = Get-Group -Count 1
        $group.Id = 0

        $group | Get-ObjectLog -Status Up
    }

    It "streams when piped from a probe" {
        SetLogAddressValidatorResponse "count=500&id=1000&filter_drel=today"

        Get-Probe -Count 1 | Get-ObjectLog
    }

    It "doesn't stream when piped from a normal object" {
        SetLogAddressValidatorResponse "count=*&id=3000&filter_drel=7days"

        Get-Device -Count 1 | Get-ObjectLog
    }

    It "doesn't stream when a count is specified" {
        SetLogAddressValidatorResponse "count=5000"

        Get-ObjectLog -Count 5000
    }

    It "retrieves from the last week when a timespan isn't specified" {
        SetLogAddressValidatorResponse "count=*&id=3000&filter_drel=7days"

        Get-Device -Count 1 | Get-ObjectLog
    }

    It "retrieves a week from the start date when only a start date is specified" {
        $end = (Get-Date).AddDays(-3)
        $endStr = $end.ToString("yyyy-MM-dd-HH-mm-ss")

        $start = $end.AddDays(-7)
        $startStr = $start.ToString("yyyy-MM-dd-HH-mm-ss")
        
        SetLogAddressValidatorResponse "count=*&id=3000&filter_dend=$endStr&filter_dstart=$startStr"        

        Get-Device -Count 1 | Get-ObjectLog -StartDate $end
    }

    It "does not limit results when only an end date is specified" {
        $start = (get-date).AddDays(-14)
        $startStr = $start.ToString("yyyy-MM-dd-HH-mm-ss")

        SetLogAddressValidatorResponse "count=500&filter_dstart=$startStr"

        Get-ObjectLog -EndDate $start
    }

    It "retrieves logs for the specified time period when a -Period is specified" {

        SetLogAddressValidatorResponse "count=500&filter_drel=7days"

        Get-ObjectLog -Period LastWeek
    }

    It "retrieves all logs when -Period All is specified" {
        
        SetLogAddressValidatorResponse "count=500"

        Get-ObjectLog -Period All
    }

    It "only retrieves a days worth of logs from a probe or the root node when no range specified" {

        $start = (Get-Date)
        $startStr = $start.ToString("yyyy-MM-dd-HH-mm-ss")

        $end = $start.AddDays(-1)
        $endStr = $end.ToString("yyyy-MM-dd-HH-mm-ss")

        SetLogAddressValidatorResponse "count=500&id=0&filter_dend=$startStr&filter_dstart=$endStr"

        Get-ObjectLog -Id 0 -StartDate $start
    }

    It "forces streaming with a date filter and returns no results" {
        SetLogAddressValidatorResponseWithCount "count=500&filter_drel=today" @{
            logs = 0
        }

        $logs = Get-ObjectLog

        $logs.Count | Should Be 0
    }

    It "forces streaming with a date piped from a variable and returns no results" {
        SetLogAddressValidatorResponseWithCount "count=500&id=0&filter_drel=today" @{
            logs = 0
        }
        
        $groups = Get-Group

        foreach($group in $groups)
        {
            $group.Id = 0
        }

        $groups | Get-ObjectLog

        $logs.Count | Should Be 0
    }

    It "filters by name" {
        SetLogAddressValidatorResponse "count=*&id=3000&filter_name=@sub(WMI Remote Ping1)&filter_drel=7days"

        $logs = Get-Device -Count 1 | Get-ObjectLog "WMI Remote Ping1"

        $logs.Count | Should Be 1

        $logs.Name | Should Be "WMI Remote Ping1"
    }

    It "streams when requesting more than 20000 items" {

        SetLogAddressValidatorResponseWithCount "count=500" @{
            logs = 30000
        }

        $logs = Get-ObjectLog -Count 20500

        $logs.Count | Should Be 20500
    }

    It "retrieves logs by Id" {

        SetLogAddressValidatorResponse "count=*&id=3000&filter_drel=7days"

        $logs = Get-ObjectLog -Id 3000

        $logs.Count | Should BeGreaterThan 0
    }

    It "retrieves logs by Id with an EndDate" {
        SetLogAddressValidatorResponse "count=*&id=3000&filter_dstart=2000-10-02-12-10-05"

        $date = New-Object DateTime -ArgumentList @(2000, 10, 2, 12, 10, 5, [DateTimeKind]::Utc)

        $logs = Get-ObjectLog -Id 3000 -EndDate $date

        $logs.Count | Should BeGreaterThan 0
    }

    It "filters by name specifying a count" {

        SetAddressValidatorResponse @(
            [Request]::Logs("count=3&start=1&filter_name=WMI+Remote+Ping0", [UrlFlag]::Columns)
            [Request]::Logs("count=1&columns=objid,name&filter_name=WMI+Remote+Ping0", $null)
            [Request]::Logs("count=2&start=4&filter_name=WMI+Remote+Ping0", [UrlFlag]::Columns)
        )

        $logs = Get-ObjectLog "WMI Remote Ping0" -Count 3

        $logs.Count | Should Be 1
    }

    Context "Take Iterator" {
        It "specifies a count" {
            SetResponseAndClientWithArguments "TakeIteratorResponse" "Logs"

            $logs = Get-ObjectLog -Count 2

            $logs.Count | Should Be 2
        }

        It "specifies a count greater than the number that are available" {
            SetResponseAndClientWithArguments "TakeIteratorResponse" "LogsInsufficient"

            $logs = Get-ObjectLog -Count 2

            $logs.Count | Should Be 1
        }

        It "specifies a count and a filter" {
            SetResponseAndClientWithArguments "TakeIteratorResponse" "LogsWithFilter"

            $logs = Get-ObjectLog ping -Count 2

            $logs.Count | Should Be 2
        }

        It "specifies a count and forces streaming"  {
            SetResponseAndClientWithArguments "TakeIteratorResponse" "LogsForceStream"

            $logs = Get-ObjectLog -Count 2 -Period All

            $logs.Count | Should Be 2
        }

        It "specifies a count and a filter and forces streaming"  {
            SetResponseAndClientWithArguments "TakeIteratorResponse" "LogsWithFilterForceStream"

            $logs = Get-ObjectLog ping -Count 2 -Period All

            $logs.Count | Should Be 2
        }

        It "requests a full page after repeatedly failing to retrieve all required items" {
            SetResponseAndClientWithArguments "TakeIteratorResponse" "LogsWithFilterInsufficient"

            $logs = Get-ObjectLog ping -Count 2

            $logs.Count | Should Be 1
        }

        It "forces streaming and requests a full page after repeatedly failing to retrieve all required items" {
            SetResponseAndClientWithArguments "TakeIteratorResponse" "LogsWithFilterInsufficientForceStream"

            $logs = Get-ObjectLog ping -Count 2 -Period All

            $logs.Count | Should Be 1
        }

        It "tries to request a full page but there is only 1 record left after repeatedly failing to retrieve all required items" {
            SetResponseAndClientWithArguments "TakeIteratorResponse" "LogsWithFilterInsufficientOneLeft"

            $logs = Get-ObjectLog ping -Count 2

            $logs.Count | Should Be 1
        }

        It "tries to request a full page but there are no records left after repeatedly failing to retrieve all required items" {
            SetResponseAndClientWithArguments "TakeIteratorResponse" "LogsWithFilterInsufficientNoneLeft"

            $logs = Get-ObjectLog ping -Count 2

            $logs.Count | Should Be 1
        }

        It "tries to request a full page but there are negative records left after repeatedly failing to retrieve all required items" {
            SetResponseAndClientWithArguments "TakeIteratorResponse" "LogsWithFilterInsufficientNegativeLeft"

            $logs = Get-ObjectLog ping -Count 2

            $logs.Count | Should Be 1
        }

        It "forces streaming and tries to request a full page but there is only 1 record left after repeatedly failing to retrieve all required items" {
            SetResponseAndClientWithArguments "TakeIteratorResponse" "LogsWithFilterInsufficientOneLeftForceStream"

            $logs = Get-ObjectLog ping -Count 2 -Period All

            $logs.Count | Should Be 1
        }

        It "forces streaming and tries to request a full page but there are no records left after repeatedly failing to retrieve all required items" {
            SetResponseAndClientWithArguments "TakeIteratorResponse" "LogsWithFilterInsufficientNoneLeftForceStream"

            $logs = Get-ObjectLog ping -Count 2 -Period All

            $logs.Count | Should Be 0
        }
    }
    
    Context "Watch" {
        It "watches logs" {

            SetResponseAndClientWithArguments "InfiniteLogValidatorResponse" @((Get-Date).AddMinutes(-1), "id=0&start=1")

            $logs = Get-ObjectLog -Tail -Interval 0 | select -First 7

            $logs.Count | Should Be 7
        }

        It "filters watched logs by name" {
            SetResponseAndClientWithArguments "InfiniteLogPostProcessValidatorResponse" @((Get-Date).AddMinutes(-1), "id=0&start=1&filter_name=@sub(Item+2)&filter_name=@sub(Item+7)")

            $logs = Get-ObjectLog "Item 2","Item 7" -Tail -Interval 0  | select -First 2

            $logs.Count | Should Be 2
        }

        It "filters watched logs by status" {
            SetResponseAndClientWithArguments "InfiniteLogPostProcessValidatorResponse" @((Get-Date).AddMinutes(-1), "id=2304&start=1&filter_status=612&filter_status=613")

            $logs = Get-ObjectLog -Id 2304 -Status Connected,Disconnected -Tail -Interval 0  | select -First 2

            $logs.Count | Should Be 2
        }

        It "specifies a custom start time" {

            $start = (Get-Date).AddDays(-3)

            $str = $start.ToString("yyyy-MM-dd-HH-mm-ss")

            SetAddressValidatorResponse "id=0&start=1&filter_dstart=$str&username=username"

            Get-ObjectLog -Interval 0 -Tail -StartDate $start | select -First 1
        }

        It "ignores an end date" {

            $start = (Get-Date).AddMinutes(-1)

            $str = $start.ToString("yyyy-MM-dd-HH-mm-ss")

            SetAddressValidatorResponse "id=0&start=1&filter_dstart=$str&username=username"

            $output = [string]::Join("`n",(&{try { Get-ObjectLog -Interval 0 -Tail -EndDate (Get-Date).AddDays(-3) 3>&1 | %{$_.Message} | select -First 1  } catch [exception] { }}))

            $output | Should Be "Ignoring -EndDate as cmdlet is executing in Watch Mode. To specify a start time use -StartDate"
        }
    }
}