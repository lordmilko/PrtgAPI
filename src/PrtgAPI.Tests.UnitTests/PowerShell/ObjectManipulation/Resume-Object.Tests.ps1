. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

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
            [Request]::Get("api/pause.htm?id=4000,4001&action=1")
        )

        $sensors | Resume-Object -Batch:$true
    }

    It "executes with -Batch:`$false" {

        SetMultiTypeResponse

        $sensors = Get-Sensor -Count 2

        SetAddressValidatorResponse @(
            [Request]::Get("api/pause.htm?id=4000&action=1")
            [Request]::Get("api/pause.htm?id=4001&action=1")
        )

        $sensors | Resume-Object -Batch:$false
    }

    It "passes through with -Batch:`$false" {
        SetMultiTypeResponse

        $sensor = Get-Sensor -Count 1

        $newSensor = $sensor | Resume-Object -PassThru -Batch:$false

        $newSensor | Should Be $sensor
    }

    It "passes through with -Batch:`$true" {
        SetMultiTypeResponse

        $sensor = Get-Sensor -Count 1

        $newSensor = $sensor | Resume-Object -PassThru -Batch:$true

        $newSensor | Should Be $sensor
    }

    It "specifies an ID" {
        SetAddressValidatorResponse @(
            [Request]::Get("api/pause.htm?id=1001&action=1")
        )

        Resume-Object -Id 1001
    }
}