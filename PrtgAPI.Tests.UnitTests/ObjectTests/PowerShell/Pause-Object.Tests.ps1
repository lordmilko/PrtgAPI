. $PSScriptRoot\Support\Standalone.ps1

Describe "Pause-Object.Tests.ps1" -Tag @("PowerShell", "UnitTest") {
    
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
}