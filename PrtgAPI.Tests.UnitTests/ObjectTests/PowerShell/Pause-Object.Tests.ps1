. $PSScriptRoot\Support\Standalone.ps1

Describe "Pause-Object" -Tag @("PowerShell", "UnitTest") {
    
    SetActionResponse

    function GetObj($o) { Run $o { & "Get-$o" } }

    $cases = @(
        @{obj = GetObj Sensor; name="sensor"}
        @{obj = GetObj Device; name="device"}
        @{obj = GetObj Group; name="group"}
        @{obj = GetObj Probe; name="probe"}
    )

    It "pauses a <name> for a duration" -TestCases $cases {
        param($obj)

        $obj | Pause-Object -Duration 10
    }

    It "pauses a <name> until a specified time" -TestCases $cases {
        param($obj)

        $obj | Pause-Object -Until (get-date).AddDays(1)
    }

    It "pauses a <name> forever" -TestCases $cases {
        param($obj)

        $obj | Pause-Object -Forever
    }

    It "pauses a <name> with a message" -TestCases $cases {
        param($obj)

        $obj | Pause-Object -Duration 10 -Message "Pausing object!"
    }

    It "pauses for 1 minute" {
        $sensor = Run Sensor { Get-Sensor }

        $sensor | Pause-Object -Duration 1
    }

    It "throws when a duration is less than 1" {

        $sensor = Run Sensor { Get-Sensor }

        { $sensor | Pause-Object -Until (Get-Date) } | Should Throw "Duration evaluated to less than one minute"
    }

    It "executes with -WhatIf" {
        $sensor = Run Sensor { Get-Sensor }

        $sensor | Pause-Object -Duration 10 -WhatIf
    }
}