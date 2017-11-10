. $PSScriptRoot\Support\Standalone.ps1

Describe "Add-Sensor" -Tag @("PowerShell", "UnitTest") {

    SetActionResponse

    It "adds a sensor" {
        $params = New-SensorParameters ExeXml -Value "test.ps1"

        $device = Run Device { Get-Device }

        $device | Add-Sensor $params
    }

    It "adds a sensor missing a required value" {
        $params = New-SensorParameters ExeXml

        $device = Run Device { Get-Device }

        { $device | Add-Sensor $params } | Should Throw "'ExeName' requires a value"
    }
}