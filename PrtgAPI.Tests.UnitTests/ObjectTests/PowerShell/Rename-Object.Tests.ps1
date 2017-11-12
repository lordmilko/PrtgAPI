. $PSScriptRoot\Support\Standalone.ps1

Describe "Rename-Object" -Tag @("PowerShell", "UnitTest") {
    
    SetActionResponse

    function GetObj($o) { Run $o { & "Get-$o" } }

    $cases = @(
        @{obj = GetObj Sensor; name="sensor"}
        @{obj = GetObj Device; name="device"}
        @{obj = GetObj Group; name="group"}
        @{obj = GetObj Probe; name="probe"}
    )

    It "renames a <name>" -TestCases $cases {
        param($obj)

        $obj | Rename-Object newName
    }

    It "executes with -WhatIf" {
        $sensor = Run Sensor { Get-Sensor }

        $sensor | Rename-Object "newName" -WhatIf
    }
}