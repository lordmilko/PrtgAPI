. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

Describe "Set-ObjectProperty" -Tag @("PowerShell", "UnitTest") {

    SetMultiTypeResponse

    $sensor = Get-Sensor

    Context "Default" {
        It "sets a property with a valid type" {
            $sensor | Set-ObjectProperty Name "Test"
        }

        It "sets a property with an invalid type" {
            $timeSpan = New-TimeSpan -Seconds 10

            { $sensor | Set-ObjectProperty InheritAccess $timeSpan } | Should Throw "could not be assigned to property 'InheritAccess'. Expected type: 'System.Boolean'"
        }

        It "sets a property with an empty string" {
            $sensor | Set-ObjectProperty Name ""
        }

        It "sets a property with null on a type that allows null" {

            WithResponseArgs "AddressValidatorResponse" ([Request]::EditSettings("id=4000,4001&name_=")) {
                $sensor | Set-ObjectProperty Name $null
            }
        }

        It "sets a property with null on a type that disallows null" {
            { $sensor | Set-ObjectProperty InheritAccess $null } | Should Throw "Null may only be assigned to properties of type 'System.String', 'System.Int32' and 'System.Double'."
        }

        It "sets a nullable type with its underlying type" {
            $val = $true
            $val.GetType() | Should Be "bool"

            $sensor | Set-ObjectProperty InheritAccess $val
        }

        It "requires Value be specified" {
            { $sensor | Set-ObjectProperty Name } | Should Throw "Value parameter is mandatory"
        }

        It "setting an invalid enum value lists all valid possibilities" {

            $expected = "'test' is not a valid value for type 'PrtgAPI.IntervalErrorMode'. Please specify one of " +
                "'DownImmediately', 'OneWarningThenDown', 'TwoWarningsThenDown', 'ThreeWarningsThenDown', 'FourWarningsThenDown' or 'FiveWarningsThenDown'"

            { $sensor | Set-ObjectProperty IntervalErrorMode "test" } | Should Throw $expected
        }

        $intervalCases = @(
            @{value = "ThirtySeconds"; name = "static property" }
            @{value = "00:00:30"; name = "string" }
            @{value = ([TimeSpan]"00:00:30"); name = "TimeSpan" }
        )

        It "parses a ScanningInterval from a <name>" -TestCases $intervalCases {

            param($value)

            $sensor | Set-ObjectProperty Interval $value
        }

        It "sets a numeric enum" {

            $sensor | Set-ObjectProperty Priority 2
        }

        It "sets a notification action property" {
            SetMultiTypeResponse

            $action = Get-NotificationAction -Count 1

            SetAddressValidatorResponse @(
                [Request]::EditSettings("id=300&active_=1")
            )

            $action | Set-ObjectProperty Active $true
        }

        it "sets a schedule property" {
            SetMultiTypeResponse

            $schedule = Get-PrtgSchedule -Count 1

            SetAddressValidatorResponse @(
                [Request]::EditSettings("id=623&name_=New+Schedule")
            )

            $schedule | Set-ObjectProperty Name "New Schedule"
        }

        It "executes a supported property with -WhatIf" {
            $sensor | Set-ObjectProperty InheritAccess $true -WhatIf
        }

        It "executes with -Batch:`$true" {

            SetMultiTypeResponse

            $sensors = Get-Sensor -Count 2

            SetAddressValidatorResponse @(
                [Request]::EditSettings("id=4000,4001&interval_=300%7C5+minutes&intervalgroup=0")
            )

            $sensors | Set-ObjectProperty Interval 00:05:00 -Batch:$true
        }

        It "executes with -Batch:`$false" {

            SetMultiTypeResponse

            $sensors = Get-Sensor -Count 2

            SetAddressValidatorResponse @(
                [Request]::EditSettings("id=4000&interval_=300%7C5+minutes&intervalgroup=0")
                [Request]::EditSettings("id=4001&interval_=300%7C5+minutes&intervalgroup=0")
            )

            $sensors | Set-ObjectProperty Interval 00:05:00 -Batch:$false
        }

        It "passes through with -Batch:`$false" {
            SetMultiTypeResponse

            $sensor = Get-Sensor -Count 1

            $newSensor = $sensor | Set-ObjectProperty name blah -PassThru -Batch:$false

            $newSensor | Should Be $sensor
        }

        It "passes through with -Batch:`$true" {
            SetMultiTypeResponse

            $sensor = Get-Sensor -Count 1

            $newSensor = $sensor | Set-ObjectProperty name blah -PassThru -Batch:$true

            $newSensor | Should Be $sensor
        }

        $locationCases = @(
            @{type = "string"; value = "1.234, 5.678"}
            @{type = "Object[]"; value = 1.234, 5.678}
        )

        It "sets a <type> location" -TestCases $locationCases {
            param($value, $type)

            SetMultiTypeResponse

            $sensor = Get-Sensor -Count 1

            $value.GetType().Name | Should Be $type

            SetAddressValidatorResponse @(
                [Request]::EditSettings("id=4000&location_=1.234%2C+5.678&lonlat_=5.678%2C1.234&locationgroup=0")
            )

            $sensor | Set-ObjectProperty Location $value
        }
    }

    Context "Raw" {
        SetMultiTypeResponse

        It "sets a raw property" {
            $sensor | Set-ObjectProperty -RawProperty name_ -RawValue "testName" -Force
        }

        It "sets a raw property with -Batch:`$false" {
            $sensor | Set-ObjectProperty -RawProperty name_ -RawValue "testName" -Force -Batch:$false
        }

        It "executes a raw property with -WhatIf" {
            $sensor | Set-ObjectProperty -RawProperty name_ -RawValue "testName" -Force -WhatIf
        }

        It "executes a raw property with ShouldContinue" {

            Invoke-Interactive @"
`$sensor = New-Object PrtgAPI.Sensor
`$sensor | Set-ObjectProperty -RawProperty name_ -RawValue 'testName'
"@
        }

        It "sets raw properties on multiple objects with -Batch:`$true" {

            SetMultiTypeResponse

            $sensors= Get-Sensor -Count 2

            SetAddressValidatorResponse @(
                [Request]::EditSettings("id=4000,4001&interval_=300%7C5+minutes")
            )

            $sensors | Set-ObjectProperty -RawProperty "interval_" -RawValue "300|5 minutes" -Force
        }

        It "sets multiple values for a raw property" {
            SetMultiTypeResponse

            $device = Get-Device -Count 1

            SetAddressValidatorResponse @(
                [Request]::EditSettings("id=3000&trafficmode_=errors&trafficmode_=discards")
            )

            $device | Set-ObjectProperty -RawProperty trafficmode_ -RawValue errors,discards -Force
        }

        It "sets multiple raw properties from a hashtable" {
            SetMultiTypeResponse

            $devices = Get-Device -Count 2
            $schedule = Get-PrtgSchedule | Select -First 1

            SetAddressValidatorResponse @(
                [Request]::EditSettings("id=3000,3001&scheduledependency=0&schedule_=623%7CWeekdays+%5BGMT%2B0800%5D%7C")
            )

            $table = @{
                scheduledependency = 0
                schedule_ = $schedule
            }

            $devices | Set-ObjectProperty -RawParameters $table -Force
        }

        It "sets multiple raw properties from a hashtable with -Batch:`$false" {
            SetMultiTypeResponse

            $devices = Get-Device -Count 2
            $schedule = Get-PrtgSchedule | Select -First 1

            SetAddressValidatorResponse @(
                [Request]::EditSettings("id=3000&scheduledependency=0&schedule_=623%7CWeekdays+%5BGMT%2B0800%5D%7C")
                [Request]::EditSettings("id=3001&scheduledependency=0&schedule_=623%7CWeekdays+%5BGMT%2B0800%5D%7C")
            )

            $table = @{
                scheduledependency = 0
                schedule_ = $schedule
            }

            $devices | Set-ObjectProperty -RawParameters $table -Force -Batch:$false
        }

        It "throws setting from an empty hashtable" {
            SetMultiTypeResponse

            $devices = Get-Device -Count 2
            $schedule = Get-PrtgSchedule | Select -First 1

            { $devices | Set-ObjectProperty -RawParameters @{} -Force } | Should Throw "At least one parameter must be specified"
        }
    }

    Context "Dynamic" {
        It "sets multiple properties with -Batch:`$true" {

            SetMultiTypeResponse

            $devices = Get-Device -Count 2

            SetAddressValidatorResponse @(
                [Request]::EditSettings("id=3000,3001&esxuser_=root&vmwareconnection=0&esxpassword_=topsecret")
            )

            $devices | Set-ObjectProperty -VMwareUserName root -VMwarePassword topsecret
        }

        It "sets multiple properties with -Batch:`$false" {
            SetMultiTypeResponse

            $devices = Get-Device -Count 2

            SetAddressValidatorResponse @(
                [Request]::EditSettings("id=3000&esxuser_=root&vmwareconnection=0&esxpassword_=topsecret")
                [Request]::EditSettings("id=3001&esxuser_=root&vmwareconnection=0&esxpassword_=topsecret")
            )

            $devices | Set-ObjectProperty -VMwareUserName root -VMwarePassword topsecret -Batch:$false
        }

        It "removes all but the last instance of a parameter" {
            SetMultiTypeResponse

            $devices = Get-Device -Count 2

            SetAddressValidatorResponse @(
                [Request]::EditSettings("id=3000,3001&interval_=30%7C30+seconds&intervalgroup=1")
            )

            $devices | Set-ObjectProperty -Interval "00:00:30" -InheritInterval $true
        }

        It "doesn't specify any dynamic parameters" {

            { $devices | Set-ObjectProperty } | Should Throw "At least one dynamic property or -Property and -Value must be specified."
        }

        It "splats dynamic parameters" {

            SetMultiTypeResponse

            $devices = Get-Device -Count 2

            $response = SetAddressValidatorResponse @(
                [Request]::EditSettings("id=3000,3001&esxuser_=root&vmwareconnection=0&esxpassword_=password")
            )

            if($PSEdition -eq "Core")
            {
                $response.AllowReorder = $true
            }

            $splat = @{
                VMwareUserName = "root"
                VMwarePassword = "password"
            }

            $devices | Set-ObjectProperty @splat
        }
    }

    Context "Manual" {
        It "sets a normal property" {
            SetAddressValidatorResponse @(
                [Request]::EditSettings("id=1001&name_=test")
            )

            Set-ObjectProperty -Id 1001 Name "test"
        }

        It "sets a raw property" {
            SetAddressValidatorResponse @(
                [Request]::EditSettings("id=1001&name_=test")
            )

            Set-ObjectProperty -Id 1001 -RawProperty name_ -RawValue "test" -Force
        }

        It "sets raw parameters" {
            SetMultiTypeResponse

            $schedule = Get-PrtgSchedule | Select -First 1

            SetAddressValidatorResponse @(
                [Request]::EditSettings("id=1001&scheduledependency=0&schedule_=623%7CWeekdays+%5BGMT%2B0800%5D%7C")
            )

            Set-ObjectProperty -Id 1001 -RawParameters @{
                scheduledependency = 0
                schedule_ = $schedule
            } -Force
        }

        It "sets a dynamic property" {
            SetAddressValidatorResponse @(
                [Request]::EditSettings("id=1001&name_=test&interval_=300%7C5+minutes&intervalgroup=0")
            )

            Set-ObjectProperty -Id 1001 -Name "test" -Interval 00:05:00
        }

        It "shows a warning when a raw property doesn't contain an underscore" {

            SetMultiTypeResponse

            (Set-ObjectProperty -Id 1001 -RawProperty name -RawValue value -Force 3>&1) -join "" | Should BeLike "Property 'name' does not look correct*"
        }

        It "doesn't show a warning when a known literal value doesn't contain an underscore" {
            SetMultiTypeResponse

            (Set-ObjectProperty -Id 1001 -RawProperty intervalgroup -RawValue 1 -Force 3>&1) -join "" | Should BeNullOrEmpty
        }
    }
}