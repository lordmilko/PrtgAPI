. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

Describe "Set-ObjectPosition" -Tag @("PowerShell", "UnitTest") {
    SetActionResponse

    It "can execute" {
        $sensor = Run Sensor { Get-Sensor }

        WithResponseArgs "AddressValidatorResponse" "api/setposition.htm?id=2203&newpos=9" {
            $sensor | Set-ObjectPosition 1
        }
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