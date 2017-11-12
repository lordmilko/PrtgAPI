. $PSScriptRoot\Support\Standalone.ps1

Describe "Set-ObjectPosition" -Tag @("PowerShell", "UnitTest") {
    SetActionResponse

    It "can execute" {
        $sensor = Run Sensor { Get-Sensor }

        $sensor | Set-ObjectPosition 1
    }

    It "can execute with -WhatIf" {
        $sensor = Run Sensor { Get-Sensor }

        $sensor | Set-ObjectPosition 1 -WhatIf
    }
}