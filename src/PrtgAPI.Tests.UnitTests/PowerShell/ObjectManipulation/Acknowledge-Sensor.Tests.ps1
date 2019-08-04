. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

Describe "Acknowledge-Sensor" -Tag @("PowerShell", "UnitTest") {

    SetActionResponse

    $sensor = Run Sensor { Get-Sensor }

    It "acknowledges for a duration" {

        $sensor | Acknowledge-Sensor -Duration 10
    }

    It "acknowledges until a specified time" {
        $sensor | Acknowledge-Sensor -Until (get-date).AddDays(1)
    }

    It "acknowledges forever" {
        $sensor | Acknowledge-Sensor -Forever
    }

    It "acknowledges with a message" {
        $sensor | Acknowledge-Sensor -Duration 10 -Message "Acknowledging object!"
    }

    It "acknowledges for 1 minute" {
        $sensor | Acknowledge-Sensor -Duration 1
    }

    It "throws when a duration is less than 1" {
        { $sensor | Acknowledge-Sensor -Until (Get-Date) } | Should Throw "Duration evaluated to less than one minute"
    }

    It "executes with -WhatIf" {
        $sensor | Acknowledge-Sensor -Duration 10 -WhatIf
    }

    It "executes with -Batch:`$true" {

        SetMultiTypeResponse

        $sensors = Get-Sensor -Count 2

        SetAddressValidatorResponse "api/acknowledgealarm.htm?id=4000,4001&duration=5&"

        $sensors | Acknowledge-Sensor -Duration 5 -Batch:$true
    }

    It "executes with -Batch:`$false" {

        SetMultiTypeResponse

        $sensors = Get-Sensor -Count 2

        SetAddressValidatorResponse @(
            [Request]::Get("api/acknowledgealarm.htm?id=4000&duration=5")
            [Request]::Get("api/acknowledgealarm.htm?id=4001&duration=5")
        )

        $sensors | Acknowledge-Sensor -Duration 5 -Batch:$false
    }

    It "passes through with -Batch:`$false" {
        SetMultiTypeResponse

        $sensor = Get-Sensor -Count 1

        $newSensor = $sensor | Acknowledge-Sensor -Forever -PassThru -Batch:$false

        $newSensor | Should Be $sensor
    }

    It "passes through with -Batch:`$true" {
        SetMultiTypeResponse

        $sensor = Get-Sensor -Count 1

        $newSensor = $sensor | Acknowledge-Sensor -Forever -PassThru -Batch:$true

        $newSensor | Should Be $sensor
    }
}