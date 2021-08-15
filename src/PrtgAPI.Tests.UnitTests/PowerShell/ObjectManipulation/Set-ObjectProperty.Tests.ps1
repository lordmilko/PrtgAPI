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

        It "specifies a PrimaryChannel with a Channel value" {

            SetMultiTypeResponse

            $sensor = Get-Sensor -Count 1

            $channel = $sensor | Get-Channel | select -First 1

            SetAddressValidatorResponse @(
                [Request]::Channels(4000)
                [Request]::ChannelProperties(4000, 1)
                [Request]::EditSettings("id=4000&primarychannel_=1%7CPercent+Available+Memory+(%25)%7C")
            )

            $sensor | Set-ObjectProperty PrimaryChannel $channel
        }

        It "specifies a PrimaryChannel with a string value" {

            SetMultiTypeResponse

            $sensor = Get-Sensor -Count 1

            $response = SetAddressValidatorResponse @(
                [Request]::Channels(4000)
                [Request]::ChannelProperties(4000, 0)
                [Request]::ChannelProperties(4000, 1)
                [Request]::EditSettings("id=4000&primarychannel_=0%7CPercent+Available+Memory0+(%25)%7C")
            )
            
            $response.CountOverride = GetCustomCountDictionary @{
                Channels = 2
            }

            $sensor | Set-ObjectProperty PrimaryChannel *memory0*
        }

        It "specifies a PrimaryChannel with a string value with -Batch:`$false" {
            SetMultiTypeResponse

            $sensor = Get-Sensor -Count 1

            $response = SetAddressValidatorResponse @(
                [Request]::Channels(4000)
                [Request]::ChannelProperties(4000, 0)
                [Request]::ChannelProperties(4000, 1)
                [Request]::EditSettings("id=4000&primarychannel_=0%7CPercent+Available+Memory0+(%25)%7C")
            )
            
            $response.CountOverride = GetCustomCountDictionary @{
                Channels = 2
            }

            $sensor | Set-ObjectProperty PrimaryChannel *memory0* -Batch:$false
        }

        It "specifies a PrimaryChannel with an ambiguous wildcard" {

            SetMultiTypeResponse

            $sensor = Get-Sensor -Count 1

            $response = SetAddressValidatorResponse @(
                [Request]::Channels(4000)
                [Request]::ChannelProperties(4000, 0)
                [Request]::ChannelProperties(4000, 1)
            )
            
            $response.CountOverride = GetCustomCountDictionary @{
                Channels = 2
            }

            { $sensor | Set-ObjectProperty PrimaryChannel *memory* } | Should Throw "Channel wildcard '*memory*' on parameter 'PrimaryChannel' is ambiguous between the channels: 'Percent Available Memory0', 'Percent Available Memory1'."
        }

        It "specifies a PrimaryChannel with a string that does not exist" {
            
            SetMultiTypeResponse

            $sensor = Get-Sensor -Count 1

            $response = SetAddressValidatorResponse @(
                [Request]::Channels(4000)
                [Request]::ChannelProperties(4000, 0)
                [Request]::ChannelProperties(4000, 1)
            )
            
            $response.CountOverride = GetCustomCountDictionary @{
                Channels = 2
            }

            { $sensor | Set-ObjectProperty PrimaryChannel foo } | Should Throw "Channel wildcard 'foo' does not exist on sensor ID 4000. Specify one of the following channel names and try again: 'Percent Available Memory0', 'Percent Available Memory1'."
        }

        It "specifies a PrimaryChannel with a Channel that does not exist" {
            SetMultiTypeResponse

            $sensor = Get-Sensor -Count 1
            $channel = $sensor | Get-Channel

            $response = SetAddressValidatorResponse @(
                [Request]::Channels(4000)
                [Request]::ChannelProperties(4000, 0)
                [Request]::ChannelProperties(4000, 1)
            )
            
            $response.CountOverride = GetCustomCountDictionary @{
                Channels = 2
            }

            { $sensor | Set-ObjectProperty PrimaryChannel $channel } | Should Throw "Channel wildcard 'Percent Available Memory' does not exist on sensor ID 4000. Specify one of the following channel names and try again: 'Percent Available Memory0', 'Percent Available Memory1'."
        }

        It "specifies a PrimaryChannel that has different channel IDs on different sensors" {
            SetMultiTypeResponse

            $sensors = Get-Sensor -Count 4

            $response = SetAddressValidatorResponse @(
                [Request]::Channels(4000)
                [Request]::ChannelProperties(4000, 0)
                [Request]::Channels(4001)
                [Request]::ChannelProperties(4001, 1)
                [Request]::Channels(4002)
                [Request]::ChannelProperties(4002, 0)
                [Request]::Channels(4003)
                [Request]::ChannelProperties(4003, 1)
                [Request]::EditSettings("id=4000,4002&primarychannel_=0%7CPercent+Available+Memory+(%25)%7C")
                [Request]::EditSettings("id=4001,4003&primarychannel_=1%7CPercent+Available+Memory+(%25)%7C")

            )

            $response.ResponseTextManipulator = [Func[[string],[string],[string]]]{
                param($text, $address)

                if($address -eq [Request]::Channels(4000) -or $address -eq [Request]::Channels(4002))
                {
                    $text = $text -replace "<objid>1</objid>","<objid>0</objid>"
                }

                return $text
            }

            $sensors | Set-ObjectProperty PrimaryChannel *memory*
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

        It "specifies a -PrimaryChannel with a Channel value" {
            SetMultiTypeResponse

            $sensor = Get-Sensor -Count 1

            $channel = $sensor | Get-Channel | select -First 1

            SetAddressValidatorResponse @(
                [Request]::Channels(4000)
                [Request]::ChannelProperties(4000, 1)
                [Request]::EditSettings("id=4000&primarychannel_=1%7CPercent+Available+Memory+(%25)%7C")
            )

            $sensor | Set-ObjectProperty -PrimaryChannel $channel
        }

        It "specifies a -PrimaryChannel with a string value" {
            SetMultiTypeResponse

            $sensor = Get-Sensor -Count 1

            $response = SetAddressValidatorResponse @(
                [Request]::Channels(4000)
                [Request]::ChannelProperties(4000, 0)
                [Request]::ChannelProperties(4000, 1)
                [Request]::EditSettings("id=4000&primarychannel_=0%7CPercent+Available+Memory0+(%25)%7C")
            )
            
            $response.CountOverride = GetCustomCountDictionary @{
                Channels = 2
            }

            $sensor | Set-ObjectProperty -PrimaryChannel *memory0*
        }

        It "specifies a -PrimaryChannel with a string value with -Batch:`$false" {
            SetMultiTypeResponse

            $sensor = Get-Sensor -Count 1

            $response = SetAddressValidatorResponse @(
                [Request]::Channels(4000)
                [Request]::ChannelProperties(4000, 0)
                [Request]::ChannelProperties(4000, 1)
                [Request]::EditSettings("id=4000&primarychannel_=0%7CPercent+Available+Memory0+(%25)%7C")
            )
            
            $response.CountOverride = GetCustomCountDictionary @{
                Channels = 2
            }

            $sensor | Set-ObjectProperty -PrimaryChannel *memory0* -Batch:$false
        }

        It "specifies a -PrimaryChannel with an ambiguous wildcard" {
            SetMultiTypeResponse

            $sensor = Get-Sensor -Count 1

            $response = SetAddressValidatorResponse @(
                [Request]::Channels(4000)
                [Request]::ChannelProperties(4000, 0)
                [Request]::ChannelProperties(4000, 1)
            )
            
            $response.CountOverride = GetCustomCountDictionary @{
                Channels = 2
            }

            { $sensor | Set-ObjectProperty -PrimaryChannel *memory* } | Should Throw "Channel wildcard '*memory*' on parameter 'PrimaryChannel' is ambiguous between the channels: 'Percent Available Memory0', 'Percent Available Memory1'."
        }

        It "specifies a -PrimaryChannel with a string that does not exist" {
            SetMultiTypeResponse

            $sensor = Get-Sensor -Count 1

            $response = SetAddressValidatorResponse @(
                [Request]::Channels(4000)
                [Request]::ChannelProperties(4000, 0)
                [Request]::ChannelProperties(4000, 1)
            )
            
            $response.CountOverride = GetCustomCountDictionary @{
                Channels = 2
            }

            { $sensor | Set-ObjectProperty -PrimaryChannel foo } | Should Throw "Channel wildcard 'foo' does not exist on sensor ID 4000. Specify one of the following channel names and try again: 'Percent Available Memory0', 'Percent Available Memory1'."
        }

        It "specifies a -PrimaryChannel with a Channel that does not exist" {
            SetMultiTypeResponse

            $sensor = Get-Sensor -Count 1
            $channel = $sensor | Get-Channel

            $response = SetAddressValidatorResponse @(
                [Request]::Channels(4000)
                [Request]::ChannelProperties(4000, 0)
                [Request]::ChannelProperties(4000, 1)
            )
            
            $response.CountOverride = GetCustomCountDictionary @{
                Channels = 2
            }

            { $sensor | Set-ObjectProperty -PrimaryChannel $channel } | Should Throw "Channel wildcard 'Percent Available Memory' does not exist on sensor ID 4000. Specify one of the following channel names and try again: 'Percent Available Memory0', 'Percent Available Memory1'."
        }

        It "specifies a -PrimaryChannel and another property" {
            SetMultiTypeResponse

            $sensor = Get-Sensor -Count 1

            $response = SetAddressValidatorResponse @(
                [Request]::Channels(4000)
                [Request]::ChannelProperties(4000, 0)
                [Request]::ChannelProperties(4000, 1)
                [Request]::EditSettings("id=4000&primarychannel_=0%7CPercent+Available+Memory0+(%25)%7C&interval_=600%7C10+minutes&intervalgroup=0")
            )
            
            $response.CountOverride = GetCustomCountDictionary @{
                Channels = 2
            }

            $sensor | Set-ObjectProperty -PrimaryChannel *memory0* -Interval 00:10:00
        }

        It "specifies a -PrimaryChannel that has different channel IDs on different sensors" {
            SetMultiTypeResponse

            $sensors = Get-Sensor -Count 4

            $response = SetAddressValidatorResponse @(
                [Request]::Channels(4000)
                [Request]::ChannelProperties(4000, 0)
                [Request]::Channels(4001)
                [Request]::ChannelProperties(4001, 1)
                [Request]::Channels(4002)
                [Request]::ChannelProperties(4002, 0)
                [Request]::Channels(4003)
                [Request]::ChannelProperties(4003, 1)
                [Request]::EditSettings("id=4000,4002&primarychannel_=0%7CPercent+Available+Memory+(%25)%7C")
                [Request]::EditSettings("id=4001,4003&primarychannel_=1%7CPercent+Available+Memory+(%25)%7C")

            )

            $response.ResponseTextManipulator = [Func[[string],[string],[string]]]{
                param($text, $address)

                if($address -eq [Request]::Channels(4000) -or $address -eq [Request]::Channels(4002))
                {
                    $text = $text -replace "<objid>1</objid>","<objid>0</objid>"
                }

                return $text
            }

            $sensors | Set-ObjectProperty -PrimaryChannel *memory*
        }

        It "specifies a -PrimaryChannel that has different channel IDs on different sensors and another property" {
            SetMultiTypeResponse

            $sensors = Get-Sensor -Count 4

            $response = SetAddressValidatorResponse @(
                [Request]::Channels(4000)
                [Request]::ChannelProperties(4000, 0)
                [Request]::Channels(4001)
                [Request]::ChannelProperties(4001, 1)
                [Request]::Channels(4002)
                [Request]::ChannelProperties(4002, 0)
                [Request]::Channels(4003)
                [Request]::ChannelProperties(4003, 1)
                [Request]::EditSettings("id=4000,4002&primarychannel_=0%7CPercent+Available+Memory+(%25)%7C&interval_=600%7C10+minutes&intervalgroup=0")
                [Request]::EditSettings("id=4001,4003&primarychannel_=1%7CPercent+Available+Memory+(%25)%7C&interval_=600%7C10+minutes&intervalgroup=0")

            )

            $response.ResponseTextManipulator = [Func[[string],[string],[string]]]{
                param($text, $address)

                if($address -eq [Request]::Channels(4000) -or $address -eq [Request]::Channels(4002))
                {
                    $text = $text -replace "<objid>1</objid>","<objid>0</objid>"
                }

                return $text
            }

            $sensors | Set-ObjectProperty -PrimaryChannel *memory* -Interval 00:10:00
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