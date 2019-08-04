. $PSScriptRoot\..\..\Support\PowerShell\IntegrationTest.ps1

function TestSensor($arguments, $ignore)
{
    $device = Get-Device -Id (Settings Device)

    $sensor = $device | New-Sensor @arguments

    TestSensorInternal $sensor $arguments $ignore
}

function TestSensorWithTargets($sensorArgs, $targetArgs, $expectedSensorNames, $ignore)
{
    $device = Get-Device -Id (Settings Device)

    $targets = $device | Get-SensorTarget @targetArgs
    $expectedNames = $targets | foreach $expectedSensorNames

    $sensors = $device | New-Sensor @sensorArgs
    $actualNames = $sensors|select -expand Name

    try
    {
        ($expectedNames -join ",") | Assert-Equal ($actualNames -join ",") -Message "Expected sensors were not created"
    }
    catch
    {
        $sensors | Remove-Object -Force

        throw
    }

    foreach($sensor in $sensors)
    {
        TestSensorInternal $sensor $sensorArgs $ignore
    }
}

function TestSensorFactory($arguments, $expectedDefinition, $count, $throw)
{
    $expectedDefinition | Should Not BeNullOrEmpty

    $device = Get-Device -Id (Settings Device)

    $getSensorArgs = @{
        Type = "ping"
    }

    if($count)
    {
        $getSensorArgs["Count"] = $count
    }

    if($count -ne 1)
    {
        if($throw)
        {
            { Get-Sensor @getSensorArgs | New-Sensor @arguments } | Should Throw "Error in channel"
            return
        }
        else
        {
            $sensor = Get-Sensor @getSensorArgs | New-Sensor @arguments
        }        
    }    

    $excludedKeys = @(
        "ChannelName"
        "Aggregator"
        "Finalizer"
        "SummaryName"
        "SummaryExpression"
        "SummaryFinalizer"
        "DestinationId"
    )

    $newArguments = @{}

    foreach($key in $arguments.Keys)
    {
        if($key -in $excludedKeys)
        {
            continue
        }

        $newArguments[$key] = $arguments[$key]
    }

    $newArguments["ChannelDefinition"] = $expectedDefinition

    if($count -eq 1)
    {
        if($throw)
        {
            { New-Sensor @newArguments -DestinationId $arguments["DestinationId"] } | Should Throw "Error in channel"
            return
        }
        else
        {
            $sensor = New-Sensor @newArguments -DestinationId $arguments["DestinationId"]
        }
    }

    try
    {
        $sensor.ParentId | Assert-Equal $arguments["DestinationId"] -Message "Destination ID should have been <expected> but was <actual>"
    }
    catch
    {
        $sensor | Remove-Object -Force

        throw
    }

    TestSensorInternal $sensor $newArguments "Factory"
}

function TestSensorInternal($sensor, $arguments, $ignore)
{
    try
    {
        $ignore | Assert-NotNull -Message "A sensor type was not specified"

        $sensor | Assert-NotNull -Message "A sensor was not specified or the sensor could not be resolved"

        $properties = $sensor | Get-ObjectProperty

        foreach($key in $arguments.Keys)
        {
            if($key -in $ignore)
            {
                continue
            }

            $value = $arguments[$key]

            $actualValue = $properties.$key

            $actualValue | Assert-NotNull -Message "$key did not have a value"

            $actualValue -join ", " | Assert-Equal ($value -join ", ") -Message "Expected $key to be $($value) but was <actual> instead"
        }
    }
    finally
    {
        $sensor | Remove-Object -Force
    }
}

function TestFactorySummary($count, $definition, $name)
{
    $sensors = Get-Sensor -Type ping -Count $count

    $sensors.Count | Should Be $count

    $definition = [string]::Format($definition, $($sensors|select -expand Id))

    TestSensorFactory @{
        Factory = $true
        Name = "New-Sensor Factory: $name"
        ChannelName = "$name Channel"
        Aggregator = "$name"
        DestinationId = (Settings Device)
    } @("#1:$name Channel", $definition) $count
}

Describe "New-Sensor_IT" -Tag @("PowerShell", "IntegrationTest") {

    Context "ExeXml" {
        It "creates an object" {

            TestSensor @{
                ExeXml = $true
                ExeFile = "testScript.bat"
                Name = "New-Sensor ExeXml"
                Interval = "00:05:00"
                Mutex = "test mutex"
            } "ExeXml"
        }
    }

    Context "WmiService" {
        It "creates an object" {

            $sensorArgs = @{
                WmiService = $true
                ServiceName = "*prtg*"
                Interval = "00:05:00"
                StartStopped = $true
            }

            $targetArgs = @{
                Type = "WmiService"
                Name = "*prtg*"
            }

            TestSensorWithTargets $sensorArgs $targetArgs { "Service: $_" } ("WmiService", "ServiceName")
        }
    }

    Context "HTTP" {
        It "creates an object" {

            TestSensor @{
                Http = $true
                Name = "New-Sensor HTTPS"
                Url = "https://"
                Timeout = 120
                HttpRequestMethod = "POST"
            } "Http"
        }
    }

    Context "Factory" {
        It "creates an object" {

            $sensors = Get-Sensor -Type ping

            $expectedDefinition = @(
                "#1:Summary Channel"
                "(channel($($sensors[0].Id),0) + channel($($sensors[1].Id),0) + channel($($sensors[2].Id),0)) / 3"
                "#2:$($sensors[0].Device)"
                "channel($($sensors[0].Id),0)"
                "#3:$($sensors[1].Device)"
                "channel($($sensors[1].Id),0)"
                "#4:$($sensors[2].Device)"
                "channel($($sensors[2].Id),0)"
            ) -join ", "

            TestSensorFactory @{
                Factory = $true
                Name = "New-Sensor Factory"
                ChannelName = { $_.Device }
                SummaryName = "Summary Channel"
                SummaryExpression = "Average"
                FactoryMissingDataMode = "CalculateWithZero"
                DestinationId = (Settings Device)
            } $expectedDefinition
        }

        $maxCases = @(
            @{count = 1; definition = "max(channel({0},0))"}
            @{count = 2; definition = "max(channel({0},0), channel({1},0))"}
            @{count = 3; definition = "max(max(channel({0},0), channel({1},0)), channel({2},0))"}
        )

        $averageCases = @(
            @{count = 1; definition = "(channel({0},0)) / 1"}
            @{count = 2; definition = "(channel({0},0) + channel({1},0)) / 2"}
            @{count = 3; definition = "(channel({0},0) + channel({1},0) + channel({2},0)) / 3"}
        )

        $avgCases = @(
            @{count = 1; definition = "avg(channel({0},0))";                                 throw = $false}
            @{count = 2; definition = "avg(channel({0},0), channel({1},0))";                 throw = $false}
            @{count = 3; definition = "avg(channel({0},0), channel({1},0), channel({2},0))"; throw = $true}
        )

        It "specifies a max summary with <count> sensors" -TestCases $maxCases {

            param($count, $definition)

            TestFactorySummary $count $definition "Max"
        }

        It "specifies a min summary with <count> sensors" -TestCases $maxCases {
            param($count, $definition)

            $definition = $definition -replace "max","min"

            TestFactorySummary $count $definition "Min"
        }

        It "specifies an average summary with <count> sensors" -TestCases $averageCases {
            param($count, $definition)

            TestFactorySummary $count $definition "Average"
        }

        It "specifies an avg summary with <count> sensors" -TestCases $avgCases {
            
            param($count, $definition, $throw)

            $sensors = Get-Sensor -Type ping -Count $count

            $sensors.Count | Should Be $count

            $definition = [string]::Format($definition, $($sensors|select -expand Id))

            TestSensorFactory @{
                Factory = $true
                Name = "New-Sensor Factory: Avg"
                ChannelName = "Avg Channel"
                Aggregator = { "$acc, $expr" }
                Finalizer = { "avg($acc)" }
                DestinationId = (Settings Device)
            } @("#1:Avg Channel", $definition) $count $throw
        }
    }

    It "has contexts for all sensor types" {

        GetSensorTypeContexts $PSCommandPath $true
    }
}