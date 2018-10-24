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
}