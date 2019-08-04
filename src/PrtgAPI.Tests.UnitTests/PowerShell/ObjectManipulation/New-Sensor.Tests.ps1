. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

function GetService
{
    SetMultiTypeResponse | Out-Null

    $services = $device | Get-SensorTarget WmiService prtgcoreservice

    $services | Should Not BeNullOrEmpty | Out-Null
    $services.Count | Should Be 1 | Out-Null

    return $services
}

function GetSensors
{
    SetMultiTypeResponse | Out-Null

    $sensors = Get-Sensor -Count 3
    $sensors[0].Device = "dc1"
    $sensors[1].Device = "dc2"
    $sensors[2].Device = "dc3"

    return $sensors
}

Describe "New-Sensor" -Tag @("PowerShell", "UnitTest") {

    $device = Run Device { Get-Device } | Select -First 1

    Context "ExeXml" {

        It "specifies a file name" {
            SetAddressValidatorResponse @(
                [Request]::Status()
                [Request]::BeginAddSensorQuery(40, "exexml")
                [Request]::AddSensor("name_=Custom+Script&priority_=3&inherittriggers_=1&intervalgroup=1&interval_=60%7C60+seconds&errorintervalsdown_=1&tags_=xmlexesensor&exefile_=test.ps1%7Ctest.ps1%7C%7C&exeparams_=&environment_=0&usewindowsauthentication_=0&mutexname_=&timeout_=60&writeresult_=0&sensortype=exexml&id=40")
            )

            $device | New-Sensor -ExeXml "Custom Script" -ExeFile "test.ps1" -Resolve:$false
        }

        It "uses positional parameters" {
            SetAddressValidatorResponse @(
                [Request]::Status()
                [Request]::BeginAddSensorQuery(40, "exexml")
                [Request]::AddSensor("name_=Custom+Script&priority_=3&inherittriggers_=1&intervalgroup=1&interval_=60%7C60+seconds&errorintervalsdown_=1&tags_=xmlexesensor&exefile_=test.ps1%7Ctest.ps1%7C%7C&exeparams_=&environment_=0&usewindowsauthentication_=0&mutexname_=&timeout_=60&writeresult_=0&sensortype=exexml&id=40")
            )

            $device | New-Sensor -ExeXml "Custom Script" "test.ps1" -Resolve:$false
        }

        It "displays -WhatIf message" {

            SetMultiTypeResponse
            Set-PrtgClient -LogLevel None

            $expected = "Performing the operation `"New-Sensor: Name = 'test', ExeFile = 'test.ps1'`" on target `"'Probe Device' (ID: 40)`"."

            ($device | New-Sensor -ExeXml test test.ps1 -Resolve:$false -Verbose 4>&1) | Should Be $expected
        }
    }

    Context "WmiService: Default" {

        $service = GetService

        It "specifies an array of services" {

            SetAddressValidatorResponse @(
                [Request]::Status()
                [Request]::BeginAddSensorQuery(40, "wmiservice")
                [Request]::AddSensor("name_=Service&priority_=3&inherittriggers_=1&intervalgroup=1&interval_=60%7C60+seconds&errorintervalsdown_=1&tags_=wmiservicesensor+servicesensor&restart_=0&monitorchange_=1&monitorextended_=0&service_=1&sensortype=wmiservice&service__check=PRTGCoreService%7CPRTG+Core+Server+Service%7CPerforms+network+monitoring+using+various+network+protocols+(including+SNMP%2C+WMI%2C+HTTP%2C+packet+sniffing%2C+NetFlow%2C+and+others)+for+PRTG+Network+Monitor+(www.paessler.com%2Fprtg)%7CRunning%7C%7C&id=40")
            )

            $device | New-Sensor -WmiService -Service $service -Resolve:$false
        }

        It "processes additional parameters" {

            SetAddressValidatorResponse @(
                [Request]::Status()
                [Request]::BeginAddSensorQuery(40, "wmiservice")
                [Request]::AddSensor("name_=Service&priority_=3&inherittriggers_=1&intervalgroup=1&interval_=60%7C60+seconds&errorintervalsdown_=1&tags_=wmiservicesensor+servicesensor&restart_=1&monitorchange_=1&monitorextended_=0&service_=1&sensortype=wmiservice&service__check=PRTGCoreService%7CPRTG+Core+Server+Service%7CPerforms+network+monitoring+using+various+network+protocols+(including+SNMP%2C+WMI%2C+HTTP%2C+packet+sniffing%2C+NetFlow%2C+and+others)+for+PRTG+Network+Monitor+(www.paessler.com%2Fprtg)%7CRunning%7C%7C&id=40")
            )

            $device | New-Sensor -WmiService -Service $service -StartStopped $true -Resolve:$false
        }

        It "throws if no services are specified" {
            SetMultiTypeResponse

            { $device | New-Sensor -WmiService -Service @() } | Should Throw "Cannot bind argument to parameter 'Service' because it is an empty array."
        }

        It "uses positional parameters" {
            SetAddressValidatorResponse @(
                [Request]::Status()
                [Request]::BeginAddSensorQuery(40, "wmiservice")
                [Request]::AddSensor("name_=Service&priority_=3&inherittriggers_=1&intervalgroup=1&interval_=60%7C60+seconds&errorintervalsdown_=1&tags_=wmiservicesensor+servicesensor&restart_=0&monitorchange_=1&monitorextended_=0&service_=1&sensortype=wmiservice&service__check=PRTGCoreService%7CPRTG+Core+Server+Service%7CPerforms+network+monitoring+using+various+network+protocols+(including+SNMP%2C+WMI%2C+HTTP%2C+packet+sniffing%2C+NetFlow%2C+and+others)+for+PRTG+Network+Monitor+(www.paessler.com%2Fprtg)%7CRunning%7C%7C&id=40")
            )

            $device | New-Sensor -WmiService $service -Resolve:$false
        }

        It "displays -WhatIf message" {
            SetMultiTypeResponse
            Set-PrtgClient -LogLevel None

            $expected = "Performing the operation `"New-Sensor: Service = 'PRTG Core Server Service'`" on target `"'Probe Device' (ID: 40)`"."

            ($device | New-Sensor -WmiService -Service $service -Resolve:$false -Verbose 4>&1) | Should Be $expected
        }

        It "displays -WhatIf message with multiple services" {
            SetMultiTypeResponse
            Set-PrtgClient -LogLevel None

            $services = $device | Get-SensorTarget WmiService *prtg*
            $services.Count | Should Be 2

            $expected = "Performing the operation `"New-Sensor: Service = 'PRTG Core Server Service, PRTG Probe Service'`" on target `"'Probe Device' (ID: 40)`"."

            ($device | New-Sensor -WmiService -Service $services -Resolve:$false -Verbose 4>&1) | Should Be $expected
        }
    }
    
    Context "WmiService: Wildcard" {

        It "specifies a wildcard of service names" {

            SetAddressValidatorResponse @(
                [Request]::SensorTypes(40)
                [Request]::BeginAddSensorQuery(40, "wmiservice")
                [Request]::AddSensorProgress(40, 2)
                [Request]::AddSensorProgress(40, 2)
                [Request]::EndAddSensorQuery(40, 2)
                [Request]::Status()
                [Request]::BeginAddSensorQuery(40, "wmiservice")
                [Request]::AddSensor("name_=Service&priority_=3&inherittriggers_=1&intervalgroup=1&interval_=60%7C60+seconds&errorintervalsdown_=1&tags_=wmiservicesensor+servicesensor&restart_=0&monitorchange_=1&monitorextended_=0&service_=1&sensortype=wmiservice&service__check=PRTGCoreService%7CPRTG+Core+Server+Service%7CPerforms+network+monitoring+using+various+network+protocols+(including+SNMP%2C+WMI%2C+HTTP%2C+packet+sniffing%2C+NetFlow%2C+and+others)+for+PRTG+Network+Monitor+(www.paessler.com%2Fprtg)%7CRunning%7C%7C&id=40")
            )

            $device | New-Sensor -WmiService -ServiceName *prtgcore* -Resolve:$false
        }

        It "processes additional parameters" {

            $service = GetService

            SetAddressValidatorResponse @(
                [Request]::SensorTypes(40)
                [Request]::BeginAddSensorQuery(40, "wmiservice")
                [Request]::AddSensorProgress(40, 2)
                [Request]::AddSensorProgress(40, 2)
                [Request]::EndAddSensorQuery(40, 2)
                [Request]::Status()
                [Request]::BeginAddSensorQuery(40, "wmiservice")
                [Request]::AddSensor("name_=Service&priority_=3&inherittriggers_=1&intervalgroup=1&interval_=60%7C60+seconds&errorintervalsdown_=1&tags_=wmiservicesensor+servicesensor&restart_=1&monitorchange_=1&monitorextended_=0&service_=1&sensortype=wmiservice&service__check=PRTGCoreService%7CPRTG+Core+Server+Service%7CPerforms+network+monitoring+using+various+network+protocols+(including+SNMP%2C+WMI%2C+HTTP%2C+packet+sniffing%2C+NetFlow%2C+and+others)+for+PRTG+Network+Monitor+(www.paessler.com%2Fprtg)%7CRunning%7C%7C&id=40")
            )

            $device | New-Sensor -WmiService *prtgcore* -StartStopped $true -Resolve:$false
        }

        It "throws if no services are matched" {
            SetMultiTypeResponse

            { $device | New-Sensor -WmiService -ServiceName "potato" } | Should Throw "Parameter '-ServiceName' requires a value, however an empty collection was specified."
        }

        It "uses positional parameters" {

            SetAddressValidatorResponse @(
                [Request]::SensorTypes(40)
                [Request]::BeginAddSensorQuery(40, "wmiservice")
                [Request]::AddSensorProgress(40, 2)
                [Request]::AddSensorProgress(40, 2)
                [Request]::EndAddSensorQuery(40, 2)
                [Request]::Status()
                [Request]::BeginAddSensorQuery(40, "wmiservice")
                [Request]::AddSensor("name_=Service&priority_=3&inherittriggers_=1&intervalgroup=1&interval_=60%7C60+seconds&errorintervalsdown_=1&tags_=wmiservicesensor+servicesensor&restart_=0&monitorchange_=1&monitorextended_=0&service_=1&sensortype=wmiservice&service__check=PRTGCoreService%7CPRTG+Core+Server+Service%7CPerforms+network+monitoring+using+various+network+protocols+(including+SNMP%2C+WMI%2C+HTTP%2C+packet+sniffing%2C+NetFlow%2C+and+others)+for+PRTG+Network+Monitor+(www.paessler.com%2Fprtg)%7CRunning%7C%7C&id=40")
            )

            $device | New-Sensor -WmiService *prtgcore* -Resolve:$false
        }

        It "displays -WhatIf message" {
            SetMultiTypeResponse
            Set-PrtgClient -LogLevel None

            $expected = "Performing the operation `"New-Sensor: ServiceName = 'PRTG Core Server Service'`" on target `"'Probe Device' (ID: 40)`"."

            ($device | New-Sensor -WmiService -ServiceName prtgcoreservice -Resolve:$false -Verbose 4>&1) | Should Be $expected
        }

        It "displays -WhatIf message with multiple services" {
            SetMultiTypeResponse
            Set-PrtgClient -LogLevel None

            $expected = "Performing the operation `"New-Sensor: ServiceName = 'PRTG Core Server Service, PRTG Probe Service'`" on target `"'Probe Device' (ID: 40)`"."

            ($device | New-Sensor -WmiService -ServiceName *prtg* -Resolve:$false -Verbose 4>&1) | Should Be $expected
        }
    }

    Context "HTTP" {
        It "executes with only a SwitchParameter" {
            SetAddressValidatorResponse @(
                [Request]::Status()
                [Request]::BeginAddSensorQuery(40, "http")
                [Request]::AddSensor("name_=HTTP&priority_=3&inherittriggers_=1&intervalgroup=1&interval_=60%7C60+seconds&errorintervalsdown_=1&tags_=httpsensor&httpurl_=http%3A%2F%2F&httpmethod_=GET&timeout_=60&postdata_=&postcontentoptions_=0&postcontenttype_=&sni_inheritance_=0&sensortype=http&id=40")
            )

            $device | New-Sensor -Http -Resolve:$false
        }

        It "executes with only a name" {
            SetAddressValidatorResponse @(
                [Request]::Status()
                [Request]::BeginAddSensorQuery(40, "http")
                [Request]::AddSensor("name_=HTTPS&priority_=3&inherittriggers_=1&intervalgroup=1&interval_=60%7C60+seconds&errorintervalsdown_=1&tags_=httpsensor&httpurl_=http%3A%2F%2F&httpmethod_=GET&timeout_=60&postdata_=&postcontentoptions_=0&postcontenttype_=&sni_inheritance_=0&sensortype=http&id=40")
            )

            $device | New-Sensor -Http "HTTPS" -Resolve:$false
        }

        It "executes with a name and a Url" {
            SetAddressValidatorResponse @(
                [Request]::Status()
                [Request]::BeginAddSensorQuery(40, "http")
                [Request]::AddSensor("name_=HTTPS&priority_=3&inherittriggers_=1&intervalgroup=1&interval_=60%7C60+seconds&errorintervalsdown_=1&tags_=httpsensor&httpurl_=https%3A%2F%2F&httpmethod_=GET&timeout_=60&postdata_=&postcontentoptions_=0&postcontenttype_=&sni_inheritance_=0&sensortype=http&id=40")
            )

            $device | New-Sensor -Http "HTTPS" "https://" -Resolve:$false
        }

        It "processes additional parameters" {
            SetAddressValidatorResponse @(
                [Request]::Status()
                [Request]::BeginAddSensorQuery(40, "http")
                [Request]::AddSensor("name_=HTTP&priority_=3&inherittriggers_=1&intervalgroup=1&interval_=60%7C60+seconds&errorintervalsdown_=1&tags_=httpsensor&httpurl_=http%3A%2F%2F&httpmethod_=POST&timeout_=60&postdata_=&postcontentoptions_=0&postcontenttype_=&sni_inheritance_=0&sensortype=http&id=40")
            )

            $device | New-Sensor -Http -HttpRequestMethod post -Resolve:$false
        }

        It "displays -WhatIf message" {
            SetMultiTypeResponse

            Set-PrtgClient -LogLevel None

            $expected = "Performing the operation `"New-Sensor: Name = 'HTTPS', HttpRequestMethod = 'POST'`" on target `"'Probe Device' (ID: 40)`"."

            ($device | New-Sensor -Http HTTPS -HttpRequestMethod post -Resolve:$false -Verbose 4>&1) | Should Be $expected
        }
    }

    Context "Factory: Default" {

        $sensors = GetSensors

        It "creates a sensor with a sensor name and a channel name expression" {

            $channelDefinition = @(
                "%231%3Adc1%0A"
                "channel(4000%2C0)%0A"
                "%232%3Adc2%0A"
                "channel(4001%2C0)%0A"
                "%233%3Adc3%0A"
                "channel(4002%2C0)"
            ) -join ""

            SetAddressValidatorResponse @(
                [Request]::Status()
                [Request]::BeginAddSensorQuery(1001, "aggregation")
                [Request]::AddSensor("name_=CPU+Overview&priority_=3&inherittriggers_=1&intervalgroup=1&interval_=60%7C60+seconds&errorintervalsdown_=1&tags_=factorysensor&aggregationchannel_=$channelDefinition&warnonerror_=0&aggregationstatus_=&missingdata_=0&sensortype=aggregation&id=1001")
            )

            $sensors | New-Sensor -Factory "CPU Overview" { $_.Device } -DestinationId 1001 -Resolve:$false
        }

        It "specifies a channel ID" {
            $channelDefinition = @(
                "%231%3Adc1%0A"
                "channel(4000%2C1)%0A"
                "%232%3Adc2%0A"
                "channel(4001%2C1)%0A"
                "%233%3Adc3%0A"
                "channel(4002%2C1)"
            ) -join ""

            SetAddressValidatorResponse @(
                [Request]::Status()
                [Request]::BeginAddSensorQuery(1001, "aggregation")
                [Request]::AddSensor("name_=CPU+Overview&priority_=3&inherittriggers_=1&intervalgroup=1&interval_=60%7C60+seconds&errorintervalsdown_=1&tags_=factorysensor&aggregationchannel_=$channelDefinition&warnonerror_=0&aggregationstatus_=&missingdata_=0&sensortype=aggregation&id=1001")
            )

            $sensors | New-Sensor -Factory "CPU Overview" { $_.Device } 1 -DestinationId 1001 -Resolve:$false
        }

        It "processes additional parameters" {
            $channelDefinition = @(
                "%231%3Adc1%0A"
                "channel(4000%2C0)%0A"
                "%232%3Adc2%0A"
                "channel(4001%2C0)%0A"
                "%233%3Adc3%0A"
                "channel(4002%2C0)"
            ) -join ""

            SetAddressValidatorResponse @(
                [Request]::Status()
                [Request]::BeginAddSensorQuery(1001, "aggregation")
                [Request]::AddSensor("name_=CPU+Overview&priority_=3&inherittriggers_=1&intervalgroup=1&interval_=60%7C60+seconds&errorintervalsdown_=1&tags_=factorysensor&aggregationchannel_=$channelDefinition&warnonerror_=1&aggregationstatus_=&missingdata_=0&sensortype=aggregation&id=1001")
            )

            $sensors | New-Sensor -Factory "CPU Overview" { $_.Device } -DestinationId 1001 -FactoryErrorMode WarnOnError -Resolve:$false
        }

        It "throws if you forget to specify a sensor name" {

            SetMultiTypeResponse

            { $sensors | New-Sensor -Factory { $_.Device } -DestinationId 1001 -Resolve:$false } | Should Throw "Parameter set cannot be resolved"
        }

        It "throws specifying a -ChannelDefinition" {
            SetMultiTypeResponse

            { $sensors | New-Sensor -Factory -Name "CPU Overview" -ChannelName { $_.Device } -DestinationId 1001 -Resolve:$false -ChannelDefinition "a","b" } | Should Throw "Parameter set cannot be resolved"
        }

        It "pipes an empty list of sensors" {
            SetAddressValidatorResponse "address_not_called"

            { $sensors | where name -EQ "blah" | New-Sensor -Factory "CPU Overview" { $_.Device } -DestinationId 1001 -Resolve:$false } | Should Throw "Property 'ChannelDefinition' requires a value"
        }

        It "displays -WhatIf message" {
            SetMultiTypeResponse
            Set-PrtgClient -LogLevel None

            $expected = @(
                "Performing the operation `"New-Sensor: Name = 'CPU Overview', FactoryErrorMode = 'WarnOnError', ChannelDefinition =`n"
                "#1:dc1"
                "channel(4000,0)"
                "#2:dc2"
                "channel(4001,0)"
                "#3:dc3"
                "channel(4002,0)`n"
                "`" on target `"Device ID: 1001`"."
            ) -join "`n"

            ($sensors | New-Sensor -Factory "CPU Overview" { $_.Device } -DestinationId 1001 -FactoryErrorMode WarnOnError -Resolve:$false -Verbose 4>&1) | Should Be $expected
        }
    }

    Context "Factory: Manual" {
        It "specifies a -Value" {
            $channelDefinition = @(
                "%231%3ALine+at+40.2%0A"
                "40.2"
            ) -join ""

            SetAddressValidatorResponse @(
                [Request]::Status()
                [Request]::BeginAddSensorQuery(1001, "aggregation")
                [Request]::AddSensor("name_=Manual+Sensor&priority_=3&inherittriggers_=1&intervalgroup=1&interval_=60%7C60+seconds&errorintervalsdown_=1&tags_=factorysensor&aggregationchannel_=$channelDefinition&warnonerror_=0&aggregationstatus_=&missingdata_=0&sensortype=aggregation&id=1001")
            )

            New-Sensor -Factory "Manual Sensor" "Line at 40.2" -Value 40.2 -DestinationId 1001 -Resolve:$false
        }

        It "throws specifying a -ChannelDefinition" {
            SetMultiTypeResponse

            { New-Sensor -Factory -Name "Manual Sensor" -ChannelName "Line at 40.2" -Value 40.2 -DestinationId 1001 -ChannelDefinition "a","b" -Resolve:$false } | Should Throw "Parameter set cannot be resolved"
        }

        It "displays -WhatIf message" {
            SetMultiTypeResponse
            Set-PrtgClient -LogLevel None

            $expected = @(
                "Performing the operation `"New-Sensor: Name = 'Manual Sensor', FactoryErrorMode = 'WarnOnError', ChannelDefinition =`n"
                "#1:Line at 40.2"
                "40.2`n"
                "`" on target `"Device ID: 1001`"."
            ) -join "`n"

            (New-Sensor -Factory "Manual Sensor" "Line at 40.2" -Value 40.2 -DestinationId 1001 -FactoryErrorMode WarnOnError -Resolve:$false -Verbose 4>&1) | Should Be $expected
        }
    }

    Context "Factory: Aggregate" {

        $sensors = GetSensors

        It "specifies an -Aggregator" {
            $channelDefinition = @(
                "%231%3AAggregate+Channel%0A"
                "channel(4000%2C0)+%2B+channel(4001%2C0)+%2B+channel(4002%2C0)"
            ) -join ""

            SetAddressValidatorResponse @(
                [Request]::Status()
                [Request]::BeginAddSensorQuery(1001, "aggregation")
                [Request]::AddSensor("name_=CPU+Overview&priority_=3&inherittriggers_=1&intervalgroup=1&interval_=60%7C60+seconds&errorintervalsdown_=1&tags_=factorysensor&aggregationchannel_=$channelDefinition&warnonerror_=0&aggregationstatus_=&missingdata_=0&sensortype=aggregation&id=1001")
            )

            $sensors | New-Sensor -Factory "CPU Overview" "Aggregate Channel" -Aggregator Sum -DestinationId 1001 -Resolve:$false
        }

        It "specifies a finalizer" {
            $channelDefinition = @(
                "%231%3AAggregate+Channel%0A"
                "(channel(4000%2C0)+%2B+channel(4001%2C0)+%2B+channel(4002%2C0))%2F3"
            ) -join ""

            SetAddressValidatorResponse @(
                [Request]::Status()
                [Request]::BeginAddSensorQuery(1001, "aggregation")
                [Request]::AddSensor("name_=CPU+Overview&priority_=3&inherittriggers_=1&intervalgroup=1&interval_=60%7C60+seconds&errorintervalsdown_=1&tags_=factorysensor&aggregationchannel_=$channelDefinition&warnonerror_=0&aggregationstatus_=&missingdata_=0&sensortype=aggregation&id=1001")
            )

            $sensors | New-Sensor -Factory "CPU Overview" "Aggregate Channel" -Aggregator { "$acc + $expr" } -Finalizer { "($acc)/3" } -DestinationId 1001 -Resolve:$false
        }

        It "prohibits a finalizer when a well known summary mode is specified" {
            SetMultiTypeResponse

            { $sensors | New-Sensor -Factory "CPU Overview" "Aggregate Channel" -Aggregator Sum -Finalizer { "($acc)/3" } -DestinationId 1001 -Resolve:$false } | Should Throw "Cannot specify -Finalizer when -Aggregator is not a ScriptBlock."
        }

        It "throws specifying a -ChannelDefinition" {
            SetMultiTypeResponse

            { $sensors | New-Sensor -Factory -Name "CPU Overview" -ChannelName "Aggregate Channel" -Aggregator Sum -ChannelDefinition "a","b" -DestinationId 1001 -Resolve:$false } | Should Throw "Parameter set cannot be resolved"
        }

        It "displays -WhatIf message" {
            SetMultiTypeResponse
            Set-PrtgClient -LogLevel None

            $expected = @(
                "Performing the operation `"New-Sensor: Name = 'CPU Overview', FactoryErrorMode = 'WarnOnError', ChannelDefinition =`n"
                "#1:Aggregate Channel"
                "channel(4000,0) + channel(4001,0) + channel(4002,0)`n"
                "`" on target `"Device ID: 1001`"."
            ) -join "`n"

            ($sensors | New-Sensor -Factory "CPU Overview" "Aggregate Channel" -Aggregator Sum -DestinationId 1001 -FactoryErrorMode WarnOnError -Resolve:$false -Verbose 4>&1) | Should Be $expected
        }
    }

    Context "Factory: Summary" {

        $sensors = GetSensors

        It "specifies a summary" {
            $channelDefinition = @(
                "%231%3AAverage+CPU+Usage%0A"
                "(channel(4000%2C0)+%2B+channel(4001%2C0)+%2B+channel(4002%2C0))+%2F+3%0A"
                "%232%3Adc1%0A"
                "channel(4000%2C0)%0A"
                "%233%3Adc2%0A"
                "channel(4001%2C0)%0A"
                "%234%3Adc3%0A"
                "channel(4002%2C0)"
            ) -join ""

            SetAddressValidatorResponse @(
                [Request]::Status()
                [Request]::BeginAddSensorQuery(1001, "aggregation")
                [Request]::AddSensor("name_=CPU+Overview&priority_=3&inherittriggers_=1&intervalgroup=1&interval_=60%7C60+seconds&errorintervalsdown_=1&tags_=factorysensor&aggregationchannel_=$channelDefinition&warnonerror_=0&aggregationstatus_=&missingdata_=0&sensortype=aggregation&id=1001")
            )

            $sensors | New-Sensor -Factory "CPU Overview" { $_.Device } -SummaryName "Average CPU Usage" -SummaryExpression Average -DestinationId 1001 -Resolve:$false
        }

        It "specifies a finalizer" {
            $channelDefinition = @(
                "%231%3AAverage+CPU+Usage%0A"
                "(channel(4000%2C0)+%2B+channel(4001%2C0)+%2B+channel(4002%2C0))%2F3%0A"
                "%232%3Adc1%0A"
                "channel(4000%2C0)%0A"
                "%233%3Adc2%0A"
                "channel(4001%2C0)%0A"
                "%234%3Adc3%0A"
                "channel(4002%2C0)"
            ) -join ""

            SetAddressValidatorResponse @(
                [Request]::Status()
                [Request]::BeginAddSensorQuery(1001, "aggregation")
                [Request]::AddSensor("name_=CPU+Overview&priority_=3&inherittriggers_=1&intervalgroup=1&interval_=60%7C60+seconds&errorintervalsdown_=1&tags_=factorysensor&aggregationchannel_=$channelDefinition&warnonerror_=0&aggregationstatus_=&missingdata_=0&sensortype=aggregation&id=1001")
            )

            $sensors | New-Sensor -Factory "CPU Overview" { $_.Device } -SummaryName "Average CPU Usage" -SummaryExpression { "$acc + $expr" } -SummaryFinalizer { "($acc)/3" } -DestinationId 1001 -Resolve:$false
        }

        It "prohibits a finalizer when a well known summary mode is specified" {
            SetMultiTypeResponse

            { $sensors | New-Sensor -Factory "CPU Overview" { $_.Device } -SummaryName "Average CPU Usage" -SummaryExpression Sum -SummaryFinalizer { "($acc)/3" } -DestinationId 1001 -Resolve:$false } | Should Throw "Cannot specify -SummaryFinalizer when -SummaryExpression is not a ScriptBlock."
        }

        It "throws specifying a -ChannelDefinition" {
            SetMultiTypeResponse

            { $sensors | New-Sensor -Factory -Name "CPU Overview" -ChannelName { $_.Device } -SummaryName "Average CPU Usage" -SummaryExpression Sum -ChannelDefinition "a","b" } | Should Throw "Parameter set cannot be resolved"
        }

        It "specifies an alias" {
            $channelDefinition = @(
                "%231%3AAverage+CPU+Usage%0A"
                "(channel(4000%2C1)+%2B+channel(4001%2C1)+%2B+channel(4002%2C1))%2F3%0A"
                "%232%3Adc1%0A"
                "channel(4000%2C1)%0A"
                "%233%3Adc2%0A"
                "channel(4001%2C1)%0A"
                "%234%3Adc3%0A"
                "channel(4002%2C1)"
            ) -join ""

            SetAddressValidatorResponse @(
                [Request]::Status()
                [Request]::BeginAddSensorQuery(1001, "aggregation")
                [Request]::AddSensor("name_=CPU+Overview&priority_=3&inherittriggers_=1&intervalgroup=1&interval_=60%7C60+seconds&errorintervalsdown_=1&tags_=factorysensor&aggregationchannel_=$channelDefinition&warnonerror_=0&aggregationstatus_=&missingdata_=0&sensortype=aggregation&id=1001")
            )

            $sensors | New-Sensor -Factory "CPU Overview" { $_.Device } 1 -sn "Average CPU Usage" -se { "$acc + $expr" } -sf { "($acc)/3" } -DestinationId 1001 -Resolve:$false
        }

        It "displays -WhatIf message" {
            SetMultiTypeResponse
            Set-PrtgClient -LogLevel None

            $expected = @(
                "Performing the operation `"New-Sensor: Name = 'CPU Overview', FactoryErrorMode = 'WarnOnError', ChannelDefinition =`n"
                "#1:Average CPU Usage"
                "(channel(4000,0) + channel(4001,0) + channel(4002,0)) / 3"
                "#2:dc1"
                "channel(4000,0)"
                "#3:dc2"
                "channel(4001,0)"
                "#4:dc3"
                "channel(4002,0)`n"
                "`" on target `"Device ID: 1001`"."
            ) -join "`n"

            ($sensors | New-Sensor -Factory "CPU Overview" { $_.Device } -sn "Average CPU Usage" -se Average -DestinationId 1001 -FactoryErrorMode WarnOnError -Resolve:$false -Verbose 4>&1) | Should Be $expected
        }
    }

    Context "Factory: ChannelDefinition" {

        It "specifies a -ChannelDefinition" {
            $channelDefinition = @(
                "%231%3Adc1%0A"
                "channel(4000%2C0)%0A"
                "%232%3Adc2%0A"
                "channel(4001%2C0)"
            ) -join ""

            SetAddressValidatorResponse @(
                [Request]::Status()
                [Request]::BeginAddSensorQuery(1001, "aggregation")
                [Request]::AddSensor("name_=CPU+Overview&priority_=3&inherittriggers_=1&intervalgroup=1&interval_=60%7C60+seconds&errorintervalsdown_=1&tags_=factorysensor&aggregationchannel_=$channelDefinition&warnonerror_=0&aggregationstatus_=&missingdata_=0&sensortype=aggregation&id=1001")
            )

            New-Sensor -Factory "CPU Overview" -ChannelDefinition "#1:dc1","channel(4000,0)","#2:dc2","channel(4001,0)" -DestinationId 1001 -Resolve:$false
        }

        It "throws specifying a New-SensorFactoryDefinition parameter" {
            SetMultiTypeResponse

            { New-Sensor -Factory -Name "CPU Overview" -ChannelDefinition "#1:dc1","channel(4000,0)" -ChannelName "test" -DestinationId 1001 -Resolve:$false } | Should Throw "Parameter set cannot be resolved"
        }

        It "processes additional parameters" {
            $channelDefinition = @(
                "%231%3Adc1%0A"
                "channel(4000%2C0)%0A"
                "%232%3Adc2%0A"
                "channel(4001%2C0)"
            ) -join ""

            SetAddressValidatorResponse @(
                [Request]::Status()
                [Request]::BeginAddSensorQuery(1001, "aggregation")
                [Request]::AddSensor("name_=CPU+Overview&priority_=3&inherittriggers_=1&intervalgroup=1&interval_=60%7C60+seconds&errorintervalsdown_=1&tags_=factorysensor&aggregationchannel_=$channelDefinition&warnonerror_=1&aggregationstatus_=&missingdata_=0&sensortype=aggregation&id=1001")
            )

            New-Sensor -Factory "CPU Overview" -ChannelDefinition "#1:dc1","channel(4000,0)","#2:dc2","channel(4001,0)" -FactoryErrorMode WarnOnError -DestinationId 1001 -Resolve:$false
        }

        It "cannot be used positionally" {
            SetMultiTypeResponse

            { New-Sensor -Factory "CPU Overview" "#1:dc1","channel(4000,0)" -DestinationId 1001 -Resolve:$false } | Should Throw "Cannot convert 'System.Object[]' to the type 'PrtgAPI.PowerShell.NameOrScriptBlock'"
        }

        It "displays -WhatIf message" {
            SetMultiTypeResponse
            Set-PrtgClient -LogLevel None

            $expected = @(
                "Performing the operation `"New-Sensor: Name = 'CPU Overview', FactoryErrorMode = 'WarnOnError', ChannelDefinition =`n"
                "#1:dc1"
                "channel(4000,0)"
                "#2:dc2"
                "channel(4001,0)`n"
                "`" on target `"Device ID: 1001`"."
            ) -join "`n"

            (New-Sensor -Factory "CPU Overview" -ChannelDefinition "#1:dc1","channel(4000,0)","#2:dc2","channel(4001,0)" -DestinationId 1001 -FactoryErrorMode WarnOnError -Resolve:$false -Verbose 4>&1) | Should Be $expected
        }
    }

    It "resolves a created sensor" {
        SetMultiTypeResponse

        $device = Get-Device -Count 1
        
        SetResponseAndClient "DiffBasedResolveResponse"

        $sensor = $device | New-Sensor -ExeXml test test.ps1
        $sensor.Count | Should Be 2 # DiffBasedResolveResponse resolves 2 instead of 1
    }

    It "specifies common sensor parameters" {
        SetMultiTypeResponse

        $device = Get-Device -Count 1
        
        SetAddressValidatorResponse @(
            [Request]::Status()
            [Request]::BeginAddSensorQuery(3000, "exexml")
            [Request]::AddSensor("name_=test&priority_=3&inherittriggers_=1&intervalgroup=0&interval_=10%7C10+seconds&errorintervalsdown_=1&tags_=xmlexesensor&exefile_=test.ps1%7Ctest.ps1%7C%7C&exeparams_=&environment_=0&usewindowsauthentication_=0&mutexname_=&timeout_=60&writeresult_=0&sensortype=exexml&id=3000")
        )

        $device | New-Sensor -ExeXml test test.ps1 -Interval 00:00:10 -Resolve:$false
    }

    It "pipes an empty list of devices" {
        SetMultiTypeResponse

        $device = Get-Device -Count 1

        SetAddressValidatorResponse "address_not_called"

        $device | where name -EQ "blah" | New-Sensor -WmiService *
    }
    
    It "doesn't allow parameters that aren't object properties" {

        SetMultiTypeResponse

        $device = Get-Device -Count 1

        { $device | New-Sensor -ExeXml test test.ps1 -Source $device -Resolve:$false } | Should Throw "A parameter cannot be found that matches parameter name 'Source'."
    }

    It "has contexts for all sensor types" {

        GetSensorTypeContexts $PSCommandPath $true
    }
}