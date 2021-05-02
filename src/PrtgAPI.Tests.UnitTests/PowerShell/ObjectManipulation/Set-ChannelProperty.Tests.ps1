. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

Describe "Set-ChannelProperty" -Tag @("PowerShell", "UnitTest") {

    SetMultiTypeResponse

    $channel = Get-Sensor | Get-Channel

    It "sets a property with an invalid type" {
        $timeSpan = New-TimeSpan -Seconds 10

        { $channel | Set-ChannelProperty LimitsEnabled $timeSpan } | Should Throw "Expected type: 'System.Boolean'. Actual type: 'System.TimeSpan'"
    }

    It "sets a property with an empty string" {
        $channel | Set-ChannelProperty LowerErrorLimit ""
    }

    It "sets a property with null on a type that allows null" {

        WithResponseArgs "AddressValidatorResponse" ([Request]::EditSettings("id=4000,4001&limitminerror_1=&limitmode_1=1")) {
            $channel | Set-ChannelProperty LowerErrorLimit $null
        }
    }

    It "sets a property with null on a type that disallows null" {
        { $channel | Set-ChannelProperty ColorMode $null } | Should Throw "Null may only be assigned to properties of type 'System.String', 'System.Int32' and 'System.Double'."
    }

    It "sets a nullable type with its underlying type" {
        $val = $true
        $val.GetType() | Should Be "bool"

        $channel | Set-ChannelProperty LimitsEnabled $val
    }

    It "requires Value be specified" {
        { $channel | Set-ChannelProperty UpperErrorLimit } | Should Throw "Value parameter is mandatory"
    }

    It "setting an invalid enum value lists all valid possibilities" {

        $expected = "'banana' is not a valid value for type 'PrtgAPI.AutoMode'. Please specify one of 'Automatic' or 'Manual'"

        { $channel | Set-ChannelProperty ColorMode "banana" } | Should Throw $expected
    }

    It "passes through with -Batch:`$false" {
        SetMultiTypeResponse

        $channel = Get-Sensor -Count 1 | Get-Channel

        $newChannel = $channel | Set-ChannelProperty LimitsEnabled $false -PassThru -Batch:$false

        $newChannel | Should Be $channel
    }

    It "passes through with -Batch:`$true" {
        SetMultiTypeResponse

        $channel = Get-Sensor -Count 1 | Get-Channel

        $newChannel = $channel | Set-ChannelProperty LimitsEnabled $false -PassThru -Batch:$true

        $newChannel | Should Be $channel
    }
    
    It "clears channel limits by setting them to `$null" {

        SetAddressValidatorResponse @(
            [Request]::EditSettings("id=1001&limitmaxerror_2=&limitmode_2=1")
        )

        Set-ChannelProperty -SensorId 1001 -ChannelId 2 -UpperErrorLimit $null
    }

    Context "Default" {

        It "sets a property with a valid type" {
            
            SetAddressValidatorResponse @(
                [Request]::Status()
                [Request]::EditSettings("id=4000,4001&limiterrormsg_1=oh+no!&limitmode_1=1")
            )

            $channel | Set-ChannelProperty ErrorLimitMessage "oh no!"
        }

        $versionCases = @(
            @{version = "17.3"; address = "id=4000,4001&limiterrormsg_1=tomato&limitmode_1=1"}
            @{version = "18.1"; address = "id=4000,4001&limiterrormsg_1=tomato&limitmode_1=1&limitmaxerror_1=100"}
        )

        It "sets a version specific property on version <version>" -TestCases $versionCases {

            param($version, $address)

            SetAddressValidatorResponse $address

            SetVersion $version

            $channel | Set-ChannelProperty ErrorLimitMessage "tomato"
        }

        It "executes with -Batch:`$true" {

            $channel.Count | Should Be 2

            SetAddressValidatorResponse @(
                [Request]::Status()
                [Request]::EditSettings("id=4000,4001&limiterrormsg_1=oh+no!&limitmode_1=1")
            )

            $channel | Set-ChannelProperty ErrorLimitMessage "oh no!" -Batch:$true
        }

        It "executes with -Batch:`$false" {

            $channel.Count | Should Be 2

            SetAddressValidatorResponse @(
                [Request]::Status()
                [Request]::EditSettings("id=4000&limiterrormsg_1=oh+no!&limitmode_1=1")
                [Request]::EditSettings("id=4001&limiterrormsg_1=oh+no!&limitmode_1=1")
            )

            $channel | Set-ChannelProperty ErrorLimitMessage "oh no!" -Batch:$false
        }

        It "sets multiple properties with -Batch:`$true" {
            $channel.Count | Should Be 2

            SetAddressValidatorResponse @(
                [Request]::EditSettings("id=4000,4001&limitmaxerror_1=100&limitmode_1=1&limitminerror_1=20&valuelookup_1=test%7Ctest&limitmaxerror_1_factor=1&limitminerror_1_factor=1")
            )

            $channel | Set-ChannelProperty -UpperErrorLimit 100 -LowerErrorLimit 20 -ValueLookup test
        }

        It "doesn't overwrite explicitly specified parameters with dependency values" {

            $channel.Count | Should Be 2

            SetAddressValidatorResponse @(
                [Request]::Status()
                [Request]::EditSettings("id=4000,4001&limitmaxerror_1=100&limitminerror_1=20&limitmode_1=0&limitmaxwarning_1=&limitminwarning_1=&limiterrormsg_1=&limitwarningmsg_1=&limitmaxerror_1_factor=1&limitminerror_1_factor=1")
            )

            $channel | Set-ChannelProperty -UpperErrorLimit 100 -LowerErrorLimit 20 -LimitsEnabled $false
        }

        It "sets multiple properties with -Batch:`$false" {

            $channel.Count | Should Be 2

            SetAddressValidatorResponse @(
                [Request]::Status()
                [Request]::EditSettings("id=4000&limitmaxerror_1=100&limitminerror_1=20&limitmode_1=0&limitmaxwarning_1=&limitminwarning_1=&limiterrormsg_1=&limitwarningmsg_1=&limitmaxerror_1_factor=1&limitminerror_1_factor=1")
                [Request]::EditSettings("id=4001&limitmaxerror_1=100&limitminerror_1=20&limitmode_1=0&limitmaxwarning_1=&limitminwarning_1=&limiterrormsg_1=&limitwarningmsg_1=&limitmaxerror_1_factor=1&limitminerror_1_factor=1")
            )

            $channel | Set-ChannelProperty -UpperErrorLimit 100 -LowerErrorLimit 20 -LimitsEnabled $false -Batch:$false
        }

        It "doesn't specify any dynamic parameters" {

            { $channel | Set-ChannelProperty } | Should Throw "At least one dynamic property or -Property and -Value must be specified."
        }

        It "splats dynamic properties" {

            $channel.Count | Should Be 2

            $response = SetAddressValidatorResponse @(
                [Request]::EditSettings("id=4000,4001&limitmaxerror_1=100&limitmode_1=1&valuelookup_1=test%7Ctest&limitminerror_1=20&limitmaxerror_1_factor=1&limitminerror_1_factor=1")
            )

            if($PSEdition -eq "Core")
            {
                $response.AllowReorder = $true
            }

            $splat = @{
                UpperErrorLimit = 100
                LowerErrorLimit = 20
                ValueLookup = "test"
            }

            $channel | Set-ChannelProperty @splat
        }

        It "sets a Channel Name" {
            # Name is a special property in that it is both a PropertyParameter for a Property and a ChannelProperty

            SetAddressValidatorResponse @(
                [Request]::EditSettings("id=4000,4001&name_1=foo")
            )

            $channel | Set-ChannelProperty Name "foo"
        }
    }

    Context "Manual" {
        It "sets a property using the manual parameter set" {

            SetAddressValidatorResponse @(
                [Request]::Status()
                [Request]::EditSettings("id=1001&limitmode_1=1")
            )

            Set-ChannelProperty -SensorId 1001 -ChannelId 1 LimitsEnabled $true
        }

        $versionCases = @(
            @{version = "17.3"; address = [Request]::EditSettings("id=1001&limiterrormsg_1=hello&limitmode_1=1")}
            @{version = "18.1"; address = @(
                    [Request]::Channels(1001)
                    [Request]::ChannelProperties(1001, 1)
                    [Request]::EditSettings("id=1001&limiterrormsg_1=hello&limitmode_1=1&limitmaxerror_1=100&limitmaxerror_1_factor=1")
                )
            }
        )

        It "sets a version specific property on version <version>" -TestCases $versionCases {

            param($version, $address)

            SetAddressValidatorResponse $address

            SetVersion $version

            Set-ChannelProperty -SensorId 1001 -ChannelId 1 ErrorLimitMessage "hello"
        }

        It "throws modifying an invalid channel ID on a version specific property" {

            SetMultiTypeResponse

            SetVersion "18.1"

            { Set-ChannelProperty -SensorId 1001 -ChannelId 2 ErrorLimitMessage "hello" } | Should Throw "Channel ID '2' does not exist on sensor ID '1001'"
        }

        It "still retrieves channels to deal with factors when setting a version specific property and a limit is included" {

            SetAddressValidatorResponse @(
                [Request]::Channels(2002)
                [Request]::ChannelProperties(2002, 1)
                [Request]::EditSettings("id=2002&limiterrormsg_1=hello&limitmode_1=1&limitminerror_1=5&limitminerror_1_factor=1")
            )

            SetVersion "18.1"

            Set-ChannelProperty -SensorId 2002 -ChannelId 1 -ErrorLimitMessage "hello" -LowerErrorLimit 5
        }

        It "executes with -Batch:`$true" {
            
            SetAddressValidatorResponse @(
                [Request]::Status()
                [Request]::EditSettings("id=1001&limitmode_1=1")
            )

            Set-ChannelProperty -SensorId 1001 -ChannelId 1 -Batch:$true LimitsEnabled $true
        }

        It "executes with -Batch:`$false" {
            SetAddressValidatorResponse @(
                [Request]::Status()
                [Request]::EditSettings("id=1001&limitmode_1=1")
            )

            Set-ChannelProperty -SensorId 1001 -ChannelId 1 -Batch:$false LimitsEnabled $true
        }

        It "sets multiple properties" {

            SetAddressValidatorResponse @(
                [Request]::Channels(1001)
                [Request]::ChannelProperties(1001, 1)
                [Request]::EditSettings("id=1001&limitmaxerror_1=100&limitmode_1=1&limitminerror_1=20&valuelookup_1=test%7Ctest&limitmaxerror_1_factor=1&limitminerror_1_factor=1")
            )

            Set-ChannelProperty -SensorId 1001 -ChannelId 1 -UpperErrorLimit 100 -LowerErrorLimit 20 -ValueLookup test
        }

        It "doesn't pipe an object or specify any dynamic parameters" {

            $messages = @(
                "*Cannot process command because of one or more missing mandatory parameters: Channel Property*"
                "*Cannot convert the `"`" value of type `"System.String`" to type `"PrtgAPI.Channel`"*"
            )

            Invoke-Interactive "Set-ChannelProperty" -AlternateExceptionMessage $messages
        }

        It "splats dynamic parameters" {

            $response = SetAddressValidatorResponse @(
                [Request]::Channels(1001)
                [Request]::ChannelProperties(1001, 1)
                [Request]::EditSettings("id=1001&limitminerror_1=20&limitmode_1=1&limitmaxerror_1=100&valuelookup_1=test%7Ctest&limitminerror_1_factor=1&limitmaxerror_1_factor=1")
            )

            if($PSEdition -eq "Core")
            {
                $response.AllowReorder = $true
            }

            $splat = @{
                SensorId = 1001
                ChannelId = 1
                UpperErrorLimit = 100
                LowerErrorLimit = 20
                ValueLookup = "test"
            }

            Set-ChannelProperty @splat
        }
    }
}