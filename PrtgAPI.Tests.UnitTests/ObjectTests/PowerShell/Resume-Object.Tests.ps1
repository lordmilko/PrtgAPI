. $PSScriptRoot\Support\Standalone.ps1

Describe "Resume-Object" -Tag @("PowerShell", "UnitTest") {
    SetActionResponse

    It "can execute" {
        $sensor = Run Sensor { Get-Sensor }

        $sensor | Resume-Object
    }

    It "can execute with -WhatIf" {
        $sensor = Run Sensor { Get-Sensor }

        $sensor | Resume-Object -WhatIf
    }
}