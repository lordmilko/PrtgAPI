. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

Describe "Set-ObjectPosition" -Tag @("PowerShell", "UnitTest") {
    SetActionResponse

    It "can set an absolute position" {
        $sensor = Run Sensor { Get-Sensor }

        WithResponseArgs "AddressValidatorResponse" "api/setposition.htm?id=2203&newpos=9" {
            $sensor | Set-ObjectPosition 1
        }
    }

    It "can set a relative position" {
        $sensor = Run Sensor { Get-Sensor }

        WithResponseArgs "AddressValidatorResponse" "api/setposition.htm?id=2203&newpos=up" {
            $sensor | Set-ObjectPosition up
        }
    }

    It "throws setting an invalid position value" {
        SetMultiTypeResponse

        $sensor = Get-Sensor -Count 1

        { $sensor | Set-ObjectPosition banana } | Should Throw "Cannot convert value 'banana' to an absolute or directional position"
    }

    It "can execute with -WhatIf" {
        $sensor = Run Sensor { Get-Sensor }

        $sensor | Set-ObjectPosition 1 -WhatIf
    }

    It "passes through" {
        SetMultiTypeResponse

        $device = Get-Device -Count 1

        $newDevice = $device | Set-ObjectPosition 1 -PassThru

        $newDevice | Should Be $device
    }

    It "specifies an ID" {
        SetAddressValidatorResponse @(
            [Request]::Objects("filter_objid=4000", [Request]::DefaultObjectFlags)
            [Request]::Sensors("filter_objid=4000", [Request]::DefaultObjectFlags)
            [Request]::Get("api/setposition.htm?id=4000&newpos=up")
        )

        Set-ObjectPosition -Id 4000 Up
    }

    It "throws when a specified ID is not a valid object type" {
        SetMultiTypeResponse

        { Set-ObjectPosition -Id 6000 Up } | Should Throw "Object must be a sensor, device, group or probe."
    }
}