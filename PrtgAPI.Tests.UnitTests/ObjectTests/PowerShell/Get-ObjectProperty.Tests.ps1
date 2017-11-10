. $PSScriptRoot\Support\Standalone.ps1

Describe "Get-ObjectProperty" {

    SetMultiTypeResponse

    It "can deserialize sensor settings" {
        $sensor = Run Sensor { Get-Sensor }

        $properties = $sensor | Get-ObjectProperty

        $properties.GetType().Name | Should Be SensorSettings
    }

    It "can deserialize device settings" {
        $sensor = Run Device { Get-Device }

        $properties = $sensor | Get-ObjectProperty

        $properties.GetType().Name | Should Be DeviceSettings
    }

    It "retrieves a raw property" {
        $sensor = Run Sensor { Get-Sensor }

        $property = $sensor | Get-ObjectProperty -RawProperty name_

        $property | Should Be "testName"
    }
}