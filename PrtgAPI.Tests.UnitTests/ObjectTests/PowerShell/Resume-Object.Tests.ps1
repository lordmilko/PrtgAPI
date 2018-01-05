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

    It "executes with -Batch:`$true" {

        SetMultiTypeResponse

        $sensors = Get-Sensor -Count 2

        SetAddressValidatorResponse @(
            "api/pause.htm?id=4000,4001&action=1&"
        )

        $sensors | Resume-Object -Batch:$true
    }

    It "executes with -Batch:`$false" {

        SetMultiTypeResponse

        $sensors = Get-Sensor -Count 2

        SetAddressValidatorResponse @(
            "api/pause.htm?id=4000&action=1&"
            "api/pause.htm?id=4001&action=1&"
        )

        $sensors | Resume-Object -Batch:$false
    }
}