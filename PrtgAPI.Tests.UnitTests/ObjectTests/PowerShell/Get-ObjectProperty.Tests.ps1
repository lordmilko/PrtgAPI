. $PSScriptRoot\Support\Standalone.ps1

Describe "Get-ObjectProperty" {

    SetMultiTypeResponse

    It "can deserialize sensor settings" {
        $sensor = Get-Sensor -Count 1

        $properties = $sensor | Get-ObjectProperty

        $properties.GetType().Name | Should Be SensorSettings
    }

    It "can deserialize device settings" {
        $sensor = Get-Device -Count 1

        $properties = $sensor | Get-ObjectProperty

        $properties.GetType().Name | Should Be DeviceSettings
    }

    It "retrieves a raw property" {
        $sensor = Get-Sensor -Count 1

        $property = $sensor | Get-ObjectProperty -RawProperty name_

        $property | Should Be "testName"
    }

    It "retrieves all raw properties" {
        $sensor = Get-Sensor -Count 1

        $properties = $sensor | Get-ObjectProperty -Raw

        $properties.name | Should Be "Server CPU Usage"
        $properties.interval | Should Be "60|60 seconds"
        $properties.schedule | Should Be "627|Weekdays Nights (17:00 - 9:00) [GMT+1100]|"
    }
}