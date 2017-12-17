. $PSScriptRoot\Support\Standalone.ps1

Describe "Add-Sensor" -Tag @("PowerShell", "UnitTest") {

    SetActionResponse

    It "adds a sensor" {
        $params = New-SensorParameters ExeXml -Value "test.ps1"

        $device = Run Device { Get-Device }

        $device | Add-Sensor $params -Resolve:$false
    }

    It "adds a sensor missing a required value" {
        $params = New-SensorParameters ExeXml

        $device = Run Device { Get-Device }

        { $device | Add-Sensor $params -Resolve:$false } | Should Throw "'ExeName' requires a value"
    }

    It "executes with -WhatIf" {
        $params = New-SensorParameters ExeXml

        $device = Run Device { Get-Device }

        $device | Add-Sensor $params -Resolve:$false -WhatIf
    }

    It "resolves a created sensor" {
        SetResponseAndClient "DiffBasedResolveResponse"

        $params = New-SensorParameters ExeXml -Value "test.ps1"

        $device = Run Device { Get-Device }

        $sensor = $device | Add-Sensor $params -Resolve

        $sensor.Id | Should Be 1002
    }
}