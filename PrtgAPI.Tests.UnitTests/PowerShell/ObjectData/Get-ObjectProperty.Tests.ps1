. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

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

    It "retrieves a single property" {
        $device = Get-Device -Count 1

        $property = $device | Get-ObjectProperty -Property Tags

        $property[0] | Should Be "tag1"
        $property[1] | Should Be "tag2"
    }

    It "retrieves a raw property" {
        $sensor = Get-Sensor -Count 1

        $property = $sensor | Get-ObjectProperty -RawProperty name_

        $property | Should Be "testName"
    }

    It "retrieves a raw property with -Text" {

        $sensor = Get-Sensor -Count 1

        WithResponseArgs "AddressValidatorResponse" "id=4000&name=name&show=text&username=username" {
            $sensor | Get-ObjectProperty -RawProperty name_ -Text
        }
    }

    It "retrieves multiple properties" {
        $sensor = Get-Sensor -Count 1

        $properties = $sensor | Get-ObjectProperty Name,Tags

        $properties.Name | Should Be "testName"
        $properties.Tags.Count | Should Be 2
        $properties.Tags[0] | Should Be "tag1"
        $properties.Tags[1] | Should Be "tag2"
    }

    It "retrieves multiple raw properties" {
        $sensor = Get-Sensor -Count 1

        $properties = $sensor | Get-ObjectProperty -RawProperty name_,tags_

        $properties.name | Should Be "testName"
        $properties.tags | Should Be "tag1 tag2"
    }

    It "retrieves all raw properties" {
        $sensor = Get-Sensor -Count 1

        $properties = $sensor | Get-ObjectProperty -Raw

        $properties.name | Should Be "Server CPU Usage"
        $properties.interval | Should Be "60|60 seconds"
        $properties.schedule | Should Be "627|Weekdays Nights (17:00 - 9:00) [GMT+0800]|"
    }
}