. $PSScriptRoot\..\..\Support\PowerShell\IntegrationTest.ps1

Describe "Set-ChannelProperty_IT" -Tag @("PowerShell", "IntegrationTest") {
    
    $channel = $null
    
    function SetValue($property, $value)
    {
        LogTestDetail "Processing property $property"

        $channel | Assert-True -Message "Channel was not initialized"

        $initialValue = (& $channel).$property

        (& $channel) | Set-ChannelProperty $property $value

        $newChannel = (& $channel)
        $newValue = $newChannel.$property

        $newValue | Assert-NotEqual $initialValue -Message "Expected initial and new value to be different, but they were both '<actual>'"
        $newValue | Should Not BeNullOrEmpty

        $newValue | Should Be $value

        (& $channel) | Set-ChannelProperty $property $initialValue
    }

    function SetChild($property, $value, $dependentProperty, $dependentValue)
    {
        LogTestDetail "Processing property $property"

        $channel | Assert-True -Message "Channel was not initialized"
        $initialValue = (& $channel).$property
        $initialDependent = (& $channel).$dependentProperty

        (& $channel) | Set-ChannelProperty $property $value

        $newChannel = (& $channel)
        $newValue = $newChannel.$property
        $newDependent = $newChannel.$dependentProperty

        $newValue | Assert-NotEqual $initialValue -Message "Expected initial and new value to be different, but they were both '<actual>'"
        $newDependent | Assert-NotEqual $initialDependent -Message "Expected initial and new dependent to be different, but they were both '<actual>'"
        $newValue | Should Not BeNullOrEmpty

        $newValue | Assert-Equal $value
        $newDependent | Assert-Equal $dependentValue

        (& $channel) | Set-ChannelProperty $dependentProperty $initialDependent
    }

    function SetLimit
    {
        $client = Get-PrtgClient

        $c1 = & $channel

        $c1.UpperErrorLimit | Assert-Equal $null -Message "Initial channel UpperErrorLimit was not null"

        Invoke-WebRequest "$($client.Server)/editsettings?id=$($c1.SensorId)&limitmaxerror_$($c1.Id)=1&limitmaxerror_$($c1.Id)_factor=$($c1.Factor)&username=$($client.UserName)&passhash=$($client.PassHash)"

        $c2 = & $channel

        $c2.UpperErrorLimit | Assert-Equal 1 -Message "Initial channel UpperErrorLimit was not 1"
    }

    function SetValueWithLimit($property, $value)
    {
        SetLimit

        SetValue $property $value
    }

    function SetChildWithLimit($property, $value, $dependentProperty, $dependentValue)
    {
        SetLimit

        SetChild $property $value $dependentProperty $dependentValue
    }

    function ClearDependents($property, $value, $dependents)
    {
        LogTestDetail "Clearing dependents of property $property"

        $initialChannel = (& $channel)

        foreach($dep in $dependents)
        {
            LogTestDetail "    Checking property $dep has an initial value"
            $initialChannel.$dep | Should Not BeNullOrEmpty
        }

        $initialChannel | Set-ChannelProperty $property $value

        $newChannel = (& $channel)

        foreach($dep in $dependents)
        {
            LogTestDetail "    Checking property $dep was cleared"
            $newChannel.$dep | Should BeNullOrEmpty
        }
    }

    function DefaultChannel
    {
        return { Get-Sensor -Id (Settings ChannelSensor) | Get-Channel (Settings ChannelName) }
    }

    It "Channel Display" {

        $channel = { Get-Sensor -Id (Settings SNMP) | Get-Channel Value }

        SetValue "Unit" "Bananas"
        SetValue "ValueLookup" "prtg.standardlookups.yesno.stateyesok"

        (& $channel) | Set-ChannelProperty ValueLookup "banana"
        $newChannel = (& $channel)
        $newChannel.ValueLookup | Should Be "None"

        $channel = { Get-Sensor -Id (Settings ExeXml) | Get-Channel Value }
        
        (& $channel) | Set-ChannelProperty ValueLookup "prtg.standardlookups.yesno.stateyesok"
        SetValue "ValueLookup" "None"
    }

    It "sets ValueLookup to None when an invalid value is specified" {
        $channel = { Get-Sensor -Id (Settings SNMP) | Get-Channel Value }

        (& $channel) | Set-ChannelProperty ValueLookup "banana"
        $newChannel = (& $channel)
        $newChannel.ValueLookup | Should Be "None"
    }

    It "Value Scaling" {
        $channel = { Get-Sensor -Id (Settings ChannelSensor) | Get-Channel "Disk Read IOs" }

        SetValue "ScalingMultiplication" 2
        SetValue "ScalingDivision" 3
    }

    It "Graph Rendering" {

        $channel = DefaultChannel

        SetValue "ShowInGraph" $false
    }

    It "Table Rendering" {
        $channel = DefaultChannel

        #SetValue "ShowInTable" $false
    }

    It "Line Color" {
        $channel = DefaultChannel

        SetChild "LineColor" "#666666" "ColorMode" "Manual"
        { SetValue "ColorMode" "Manual" } | Should Throw "Required field, not defined"
        
        (& $channel) | Set-ChannelProperty LineColor "444444"

        ClearDependents "ColorMode" "Automatic" @("LineColor")
    }

    It "Line Width" {
        $channel = DefaultChannel

        SetValue "LineWidth" 3
    }

    It "Data" {
        $channel = { Get-Sensor -Id (Settings DownSensor) | Get-Channel "Available Memory" }

        $temp = & $channel

        $temp.Factor | Assert-NotNull -Message "Channel factor must not be null"
        $temp.Factor | Assert-NotEqual 1 -Message "Channel factor must not be 1"

        SetChild "PercentValue" "30" "PercentMode" "PercentOfMax"
        
        # Logically this should throw, however prior to PRTG 18.4.1962 if you set the PercentDisplay to PercentOfMax, it doesn't make you specify a value
        # As such, we do not create a reverse dependency on PercentValue
        # In PRTG 18.4.1962 this now throws properly
        { SetValue "PercentMode" "PercentOfMax" } | Should Throw "Required field, not defined"

        (& $channel) | Set-ChannelProperty PercentValue 40

        ClearDependents "PercentMode" "Actual" @("PercentValue")
    }

    It "Value Mode" {
        $channel = DefaultChannel

        SetValue "HistoricValueMode" "Minimum"
    }

    It "Decimal Places" {
        $channel = DefaultChannel

        $temp = & $channel

        SetValue "DecimalPlaces" 3 "DecimalMode" "Custom"
        { SetValue "DecimalMode" "Custom" } | Should Throw "Required field, not defined"

        (& $channel) | Set-ChannelProperty DecimalPlaces 4

        ClearDependents "DecimalMode" "All" @("DecimalPlaces")

        $temp | Set-ChannelProperty DecimalPlaces $temp.DecimalPlaces
    }

    It "Spike Filter" {
        $channel = DefaultChannel

        SetChild "SpikeFilterMin" 10 "SpikeFilterEnabled" $true
        SetChild "SpikeFilterMax" 80 "SpikeFilterEnabled" $true
        SetValue "SpikeFilterEnabled" $true

        (& $channel) | Set-ChannelProperty SpikeFilterMin 10
        (& $channel) | Set-ChannelProperty SpikeFilterMax 80

        ClearDependents "SpikeFilterEnabled" $false @("SpikeFilterMin", "SpikeFilterMax")
    }

    <#It "Vertical Axis Scaling" {
        $channel = DefaultChannel

        SetChild "VerticalAxisMin" 10 "VerticalAxisScaling" "Manual"
        SetChild "VerticalAxisMax" 80 "VerticalAxisScaling" "Manual"
        { SetValue "VerticalAxisScaling" "Manual" } | Should Throw "Required field, not defined"

        (& $channel) | Set-ChannelProperty VerticalAxisMin 20
        (& $channel) | Set-ChannelProperty VerticalAxisMax 70

        ClearDependents "VerticalAxisScaling" "Automatic" @("VerticalAxisMin", "VerticalAxisMax")

        throw "we need to say you have to set both the min AND max; im pretty sure if we checked prtg itd show an error if you just set one of them?"
    }#>
    
    It "Limits" {
        $channel = { Get-Sensor -Id (Settings DownSensor) | Get-Channel -Id 1 }

        $temp = & $channel
        $temp | Set-ChannelProperty -UpperErrorLimit 100 -LowerErrorLimit 25 -UpperWarningLimit 75 -LowerWarningLimit 50 -ErrorLimitMessage "error" -WarningLimitMessage "warning"

        $temp.Factor | Assert-NotNull -Message "Channel factor must not be null"
        $temp.Factor | Assert-NotEqual 1 -Message "Channel factor must not be 1"

        ClearDependents "LimitsEnabled" $false @("UpperErrorLimit", "UpperWarningLimit",
            "LowerErrorLimit", "LowerWarningLimit", "ErrorLimitMessage", "WarningLimitMessage")

        SetChild "UpperErrorLimit"      300                "LimitsEnabled" $true
        SetChild "UpperWarningLimit"    200                "LimitsEnabled" $true
        SetChild "LowerErrorLimit"     -200                "LimitsEnabled" $true
        SetChild "LowerWarningLimit"   -300                "LimitsEnabled" $true
        
        { SetChild "ErrorLimitMessage"   "error! error!"     "LimitsEnabled" $true } | Should Throw "does not have a limit value defined on it"
        { SetChild "WarningLimitMessage" "warning! warning!" "LimitsEnabled" $true } | Should Throw "does not have a limit value defined on it"
        { SetValue "LimitsEnabled" $true                                           } | Should Throw "does not have a limit value defined on it"

        SetValueWithLimit "LimitsEnabled" $true
        SetChildWithLimit "ErrorLimitMessage"   "error! error!"     "LimitsEnabled" $true
        SetChildWithLimit "WarningLimitMessage" "warning! warning!" "LimitsEnabled" $true
    }

    It "can set the properties of multiple in a single request" {
        $sensor = Get-Sensor -Id (Settings ChannelSensor)

        $ids = 2,3

        $ids | foreach { Set-ChannelProperty -SensorId $sensor.id -ChannelId $_ LimitsEnabled $false }

        $channels = $sensor | Get-Channel -Id $ids

        $channels.Count | Should Be 2

        $channels[0].LimitsEnabled | Should Be $false
        $channels[1].LimitsEnabled | Should Be $false

        $channels | Set-ChannelProperty LowerWarningLimit 20

        $newChannels = $sensor | Get-Channel -Id $ids

        $newChannels[0].LimitsEnabled | Should Be $true
        $newChannels[1].LimitsEnabled | Should Be $true
    }

    It "sets multiple with dynamic parameters" {
        $sensor = Get-Sensor -Id (Settings ChannelSensor)

        $channel = $sensor | Get-Channel -Id 0
        $channel | Set-ChannelProperty LimitsEnabled $false

        $channel = $sensor | Get-Channel -Id 0
        $channel.LimitsEnabled | Should Be $false
        $channel.UpperErrorLimit | Should Be $null
        $channel.LowerErrorLimit | Should Be $null

        $channel | Set-ChannelProperty -UpperErrorLimit 100 -LowerErrorLimit 50

        $newChannel = $sensor | Get-Channel -Id 0
        $newChannel.LimitsEnabled | Should Be $true
        $newChannel.UpperErrorLimit | Should Be 100
        $newChannel.LowerErrorLimit | Should Be 50
    }
    
    function WithoutLimits($sensorId, $channelId, $scriptBlock)
    {
        $channel = Get-Channel -SensorId $sensorId -Id $channelId

        try
        {
            $channel | Set-ChannelProperty LimitsEnabled $false

            $newChannel = Get-Channel -SensorId $sensorId -Id $channelId

            $newChannel.LimitsEnabled | Assert-False -Message "LimitsEnabled was not false"
            $newChannel.UpperErrorLimit | Assert-Null -Message "UpperErrorLimit was not null"
            $newChannel.LowerErrorLimit | Assert-Null -Message "LowerErrorLimit was not null"
            $newChannel.UpperWarningLimit | Assert-Null -Message "UpperWarningLimit was not null"
            $newChannel.LowerWarningLimit | Assert-Null -Message "LowerWarningLimit was not null"
            $newChannel.ErrorLimitMessage | Assert-Null -Message "ErrorLimitMessage was not null"
            $newChannel.WarningLimitMessage | Assert-Null -Message "WarningLimitMessage was not null"

            & $scriptBlock $newChannel
        }
        finally
        {
            $channelParams = @{
                LimitsEnabled = $channel.LimitsEnabled
                UpperErrorLimit = $channel.UpperErrorLimit
                LowerErrorLimit = $channel.LowerErrorLimit
                UpperWarningLimit = $channel.UpperWarningLimit
                LowerWarningLimit = $channel.LowerWarningLimit
                ErrorLimitMessage = $channel.ErrorLimitMessage
                WarningLimitMessage = $channel.WarningLimitMessage
            }

            $channel | Set-ChannelProperty @channelParams
        }
    }

    It "overrides a dependent property" {
        $getChannel = DefaultChannel
        $channel = & $getChannel

        $channel | Set-ChannelProperty -DecimalPlaces 5

        $newChannel = & $getChannel
        $newChannel.DecimalMode | Should Be "Custom"
        $newChannel.DecimalPlaces | Should Be 5

        $newChannel | Set-ChannelProperty -DecimalPlaces 6 -DecimalMode All

        $finalChannel = & $getChannel
        $finalChannel.DecimalMode | Should Be "All"
        $finalChannel.DecimalPlaces | Should Be 6
    }

    Context "Limits" {
        It "clears the last limit when limits are already enabled" {

            WithoutLimits (Settings ChannelSensor) (Settings Channel) {
                param($channel)

                $channel | Set-ChannelProperty UpperErrorLimit 25

                $newChannel = Get-Channel -SensorId $channel.SensorId -Id $channel.Id
                $newChannel.UpperErrorLimit | Assert-Equal 25 -Message "Expected UpperErrorLimit to be <expected> however instead was <actual>"
                $newChannel.LimitsEnabled | Assert-Equal $true -Message "Expected LimitsEnabled to be <expected> however instead was <actual>"

                $newChannel | Set-ChannelProperty UpperErrorLimit $null

                $finalChannel = Get-Channel -SensorId $channel.SensorId -Id $channel.Id

                $finalChannel.UpperErrorLimit | Assert-Null -Message "Expected UpperErrorLimit to be null but it wasn't"
                $finalChannel.LimitsEnabled | Assert-Equal $true -Message "After clearing UpperErrorLimit, expected LimitsEnabled to be <expected> however instead was <actual>"
            }
        }

        It "clears the last limit when limits are already disabled" {


            WithoutLimits (Settings ChannelSensor) (Settings Channel) {
                param($channel)

                $channel | Set-ChannelProperty -UpperErrorLimit 25 -LimitsEnabled $false

                $newChannel = Get-Channel -SensorId $channel.SensorId -Id $channel.Id
                $newChannel.UpperErrorLimit | Assert-Equal 25 -Message "Expected UpperErrorLimit to be <expected> however instead was <actual>"
                $newChannel.LimitsEnabled | Assert-Equal $false -Message "Expected LimitsEnabled to be <expected> however instead was <actual>"
            }
        }

        It "clears a limit that is already null when limits are enabled" {
            WithoutLimits (Settings ChannelSensor) (Settings Channel) {
                param($channel)

                $channel | Set-ChannelProperty UpperErrorLimit 25

                $newChannel = Get-Channel -SensorId $channel.SensorId -Id $channel.Id
                $newChannel.UpperErrorLimit | Assert-Equal 25 -Message "Expected UpperErrorLimit to be <expected> however instead was <actual>"
                $newChannel.LimitsEnabled | Assert-Equal $true -Message "Expected LimitsEnabled to be <expected> however instead was <actual>"

                $newChannel | Set-ChannelProperty UpperErrorLimit $null

                $finalChannel = Get-Channel -SensorId $channel.SensorId -Id $channel.Id

                $finalChannel.UpperErrorLimit | Assert-Null -Message "Expected UpperErrorLimit to be null but it wasn't"
                $finalChannel.LimitsEnabled | Assert-Equal $true -Message "After clearing UpperErrorLimit, expected LimitsEnabled to be <expected> however instead was <actual>"

                { $finalChannel | Set-ChannelProperty UpperErrorLimit $null } | Should Throw "You have set Alerting to limit-based, but have entered no limit value"
            }
        }

        It "clears a limit that is already null when limits are disabled" {
            WithoutLimits (Settings ChannelSensor) (Settings Channel) {
                param($channel)

                $channel | Set-ChannelProperty -UpperErrorLimit $null -LimitsEnabled $false
            }
        }

        It "clears a limit that already has a value while disabling limits" {
            WithoutLimits (Settings ChannelSensor) (Settings Channel) {
                param($channel)

                $channel | Set-ChannelProperty UpperErrorLimit 25

                $newChannel = Get-Channel -SensorId $channel.SensorId -Id $channel.Id
                $newChannel.UpperErrorLimit | Assert-Equal 25 -Message "Expected UpperErrorLimit to be <expected> however instead was <actual>"
                $newChannel.LimitsEnabled | Assert-Equal $true -Message "Expected LimitsEnabled to be <expected> however instead was <actual>"

                $newChannel | Set-ChannelProperty -UpperErrorLimit $null -LimitsEnabled $false

                $finalChannel = Get-Channel -SensorId $channel.SensorId -Id $channel.Id
                $finalChannel.UpperErrorLimit | Assert-Null -Message "Expected UpperErrorLimit to be null but it wasn't"
                $finalChannel.LimitsEnabled | Assert-Equal $false -Message "After clearing UpperErrorLimit, expected LimitsEnabled to be <expected> however instead was <actual>"
            }            
        }

        It "clears a limit that already has a value while enabling limits" {
            WithoutLimits (Settings ChannelSensor) (Settings Channel) {
                param($channel)

                $channel | Set-ChannelProperty UpperErrorLimit 25

                $newChannel = Get-Channel -SensorId $channel.SensorId -Id $channel.Id
                $newChannel.UpperErrorLimit | Assert-Equal 25 -Message "Expected UpperErrorLimit to be <expected> however instead was <actual>"
                $newChannel.LimitsEnabled | Assert-Equal $true -Message "Expected LimitsEnabled to be <expected> however instead was <actual>"

                $newChannel | Set-ChannelProperty -UpperErrorLimit $null -LimitsEnabled $true

                $finalChannel = Get-Channel -SensorId $channel.SensorId -Id $channel.Id
                $finalChannel.UpperErrorLimit | Assert-Null -Message "Expected UpperErrorLimit to be null but it wasn't"
                $finalChannel.LimitsEnabled | Assert-Equal $true -Message "After clearing UpperErrorLimit, expected LimitsEnabled to be <expected> however instead was <actual>"
            }
        }
    }
}