. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

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
            [Request]::Get("api/scannow.htm?id=4000,4001")
        )

        $sensors | Refresh-Object -Batch:$true
    }

    It "executes with -Batch:`$false" {

        SetMultiTypeResponse

        $sensors = Get-Sensor -Count 2

        SetAddressValidatorResponse @(
            [Request]::Get("api/scannow.htm?id=4000")
            [Request]::Get("api/scannow.htm?id=4001")
        )

        $sensors | Refresh-Object -Batch:$false
    }

    It "passes through with -Batch:`$false" {
        SetMultiTypeResponse

        $sensor = Get-Sensor -Count 1

        $newSensor = $sensor | Refresh-Object -PassThru -Batch:$false

        $newSensor | Should Be $sensor
    }

    It "passes through with -Batch:`$true" {
        SetMultiTypeResponse

        $sensor = Get-Sensor -Count 1

        $newSensor = $sensor | Refresh-Object -PassThru -Batch:$true

        $newSensor | Should Be $sensor
    }

    It "specifies an ID" {
        SetAddressValidatorResponse @(
            [Request]::Get("api/scannow.htm?id=1001")
        )

        Refresh-Object -Id 1001
    }
}