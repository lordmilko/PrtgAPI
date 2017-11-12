. $PSScriptRoot\Support\Standalone.ps1

Describe "Simulate-ErrorStatus" -Tag @("PowerShell", "UnitTest") {
    SetActionResponse

    It "can execute" {
        $sensor = Run Sensor { Get-Sensor }

        $sensor | Simulate-ErrorStatus
    }

    It "can execute with -WhatIf" {
        $sensor = Run Sensor { Get-Sensor }

        $sensor | Simulate-ErrorStatus -WhatIf
    }
}