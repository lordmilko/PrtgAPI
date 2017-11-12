. $PSScriptRoot\Support\Standalone.ps1

Describe "Refresh-Object" -Tag @("PowerShell", "UnitTest") {

    SetActionResponse

    It "can execute" {
        $sensor = Run Sensor { Get-Sensor }

        $sensor | Refresh-Object
    }

    It "can execute with -WhatIf" {
        $sensor = Run Sensor { Get-Sensor }

        $sensor | Refresh-Object -WhatIf
    }
}