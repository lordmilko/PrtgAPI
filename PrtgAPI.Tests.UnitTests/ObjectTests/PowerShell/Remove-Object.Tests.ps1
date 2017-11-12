. $PSScriptRoot\Support\Standalone.ps1

Describe "Remove-Object" -Tag @("PowerShell", "UnitTest") {
    
    SetActionResponse

    function GetObj($o) { Run $o { & "Get-$o" } }

    $cases = @(
        @{obj = GetObj Sensor; name="sensor"}
        @{obj = GetObj Device; name="device"}
        @{obj = GetObj Group; name="group"}
        @{obj = GetObj Probe; name="probe"}
    )

    It "removes a <name>" -TestCases $cases {
        param($obj)

        $obj | Remove-Object -Force
    }

    It "executes with -WhatIf" {
        $sensor = Run Sensor { Get-Sensor }

        $sensor | Remove-Object -WhatIf
    }
    
    It "executes ShouldContinue" {
        $sensor = Run Sensor { Get-Sensor }

        try
        {
            $sensor | Remove-Object
        }
        catch
        {
        }
    }
}