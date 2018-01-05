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

    It "executes with -Batch:`$true" {

        SetMultiTypeResponse

        $sensors = Get-Sensor -Count 2

        SetAddressValidatorResponse @(
            "api/scannow.htm?id=4000,4001&"
        )

        $sensors | Refresh-Object -Batch:$true
    }

    It "executes with -Batch:`$false" {

        SetMultiTypeResponse

        $sensors = Get-Sensor -Count 2

        SetAddressValidatorResponse @(
            "api/scannow.htm?id=4000&"
            "api/scannow.htm?id=4001&"
        )

        $sensors | Refresh-Object -Batch:$false
    }
}