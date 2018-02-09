. $PSScriptRoot\Support\Standalone.ps1

function SetAddressValidatorResponse($str)
{
    SetResponseAndClientWithArguments "LogAddressValidatorResponse" $str
}

function SetAddressValidatorResponseWithCount($str, $hashtable)
{
    $dictionary = GetCustomCountDictionary $hashtable

    SetResponseAndClientWithArguments "LogAddressValidatorResponse" @($str, $dictionary)
}

Describe "Get-ObjectLog" {
    It "retrieves logs from an unspecified object" {
        SetAddressValidatorResponse "count=500&filter_drel=today"

        (Get-ObjectLog).Count | Should Be 2
    }

    It "streams from an unspecified object" {
        SetAddressValidatorResponse "count=500&filter_drel=today"

        Get-ObjectLog
    }

    It "streams from the root object" {
        SetAddressValidatorResponse "count=500&id=0&filter_drel=today"

        $group = Get-Group -count 1
        $group.Id = 0
        
        $group | Get-ObjectLog
    }

    It "streams when only a status is specified" {
        SetAddressValidatorResponse "count=500&id=0&filter_status=607&filter_drel=today"

        $group = Get-Group -Count 1
        $group.Id = 0

        $group | Get-ObjectLog -Status Up
    }

    It "streams when piped from a probe" {
        SetAddressValidatorResponse "count=500&id=1000&filter_drel=today"

        Get-Probe -Count 1 | Get-ObjectLog
    }

    It "doesn't stream when piped from a normal object" {
        SetAddressValidatorResponse "count=*&id=3000&filter_drel=7days"

        Get-Device -Count 1 | Get-ObjectLog
    }

    It "doesn't stream when a count is specified" {
        SetAddressValidatorResponse "count=5000"

        Get-ObjectLog -Count 5000
    }

    It "retrieves from the last week when a timespan isn't specified" {
        SetAddressValidatorResponse "count=*&id=3000&filter_drel=7days"

        Get-Device -Count 1 | Get-ObjectLog
    }

    It "retrieves a week from the start date when only a start date is specified" {
        $end = (Get-Date).AddDays(-3)
        $endStr = $end.ToString("yyyy-MM-dd-HH-mm-ss")

        $start = $end.AddDays(-7)
        $startStr = $start.ToString("yyyy-MM-dd-HH-mm-ss")
        
        SetAddressValidatorResponse "count=*&id=3000&filter_dend=$endStr&filter_dstart=$startStr"        

        Get-Device -Count 1 | Get-ObjectLog -StartDate $end
    }

    It "does not limit results when only an end date is specified" {
        $start = (get-date).AddDays(-14)
        $startStr = $start.ToString("yyyy-MM-dd-HH-mm-ss")

        SetAddressValidatorResponse "count=500&filter_dstart=$startStr"

        Get-ObjectLog -EndDate $start
    }

    It "forces streaming with a date filter and returns no results" {
        SetAddressValidatorResponseWithCount "count=500&filter_drel=today" @{
            messages = 0
        }

        $logs = Get-ObjectLog

        $logs.Count | Should Be 0
    }

    It "forces streaming with a date piped from a variable and returns no results" {
        SetAddressValidatorResponseWithCount "count=500&id=0&filter_drel=today" @{
            messages = 0
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
        SetAddressValidatorResponse "count=*&id=3000&filter_name=@sub(WMI Remote Ping1)&filter_drel=7days"

        $logs = Get-Device -Count 1 | Get-ObjectLog "WMI Remote Ping1"

        $logs.Count | Should Be 1

        $logs.Name | Should Be "WMI Remote Ping1"
    }

    It "streams when requesting more than 20000 items" {

        SetAddressValidatorResponseWithCount "count=500" @{
            messages = 500
        }

        $logs = Get-ObjectLog -Count 20500

        $logs.Count | Should Be 20500
    }
}