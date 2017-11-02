. $PSScriptRoot\Support\Standalone.ps1

function SetFactorySourceResponse($channelDefinition)
{
    $channelDefinitionStr = $channelDefinition -join "`n"

    $propertyChanger = [Func[string, string]]{
        param($body)

        [PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses.SensorSettingsResponse]::SetContainerTagContents($body, $channelDefinitionStr, "textarea", "aggregationchannel_")
    }.GetNewClosure()

    SetResponseAndClientWithArguments "SensorFactorySourceResponse" $propertyChanger
}

Describe "Get-SensorFactorySource" {
    It "parses a factory with one channel" {

        SetFactorySourceResponse @(
            "#1:First Channel"
            "channel(1001, 0)"
        )

        $sensor = Get-Sensor
        $sensor.Type = "Sensor Factory"

        $sourceSensors = $sensor | Get-SensorFactorySource

        $sourceSensors.Count | Should Be 1
        $sourceSensors.GetType().Name | Should Be "Sensor"
    }

    It "parses a factory with two channels" {
        SetFactorySourceResponse @(
            "#1:First Channel"
            "channel(1001, 0)"
            "#2:Second Channel"
            "channel(1002,1)"
        )

        $sensor = Get-Sensor
        $sensor.Type = "Sensor Factory"

        $sourceSensors = $sensor | Get-SensorFactorySource

        $sourceSensors.Count | Should Be 2
    }

    It "processes an empty channel definition" {
        SetFactorySourceResponse @()

        $sensor = Get-Sensor
        $sensor.Type = "Sensor Factory"

        $sourceSensors = $sensor | Get-SensorFactorySource

        $sourceSensors.Count | Should Be 0
    }

    It "parses a factory containing a formula" {
        SetFactorySourceResponse @(
            "#1:First Channel"
            "max(channel(1000, 0), channel(1001, 0) + channel(1002, 0))"
        )

        $sensor = Get-Sensor
        $sensor.Type = "Sensor Factory"

        $sourceSensors = $sensor | Get-SensorFactorySource

        $sourceSensors.Count | Should Be 3
    }

    It "retrieves channels from a sensor factory" {

        SetFactorySourceResponse @(
            "#1:First Channel"
            "channel(1001, 1)"
            "#2: Second Channel"
            "channel(1002, 1)"
        )

        $sensor = Get-Sensor
        $sensor.Type = "Sensor Factory"

        $sourceChannels = $sensor | Get-SensorFactorySource -Channels
        $sourceChannels.Count | Should Be 2
        ($sourceChannels | select -First 1).GetType().Name | Should Be "Channel"
    }

    It "returns nothing when a channel definition is invalid" {
        SetFactorySourceResponse "test"

        $sensor = Get-Sensor
        $sensor.Type = "Sensor Factory"

        $response = $sensor | Get-SensorFactorySource

        $response | Should Be $null
    }

    It "throws when a sensor isn't a sensor factory" {
        SetResponseAndClientWithArguments "SensorFactorySourceResponse"

        $sensor = Get-Sensor

        { $sensor | Get-SensorFactorySource } | Should Throw "Only Sensor Factory objects may be specified"
    }
}