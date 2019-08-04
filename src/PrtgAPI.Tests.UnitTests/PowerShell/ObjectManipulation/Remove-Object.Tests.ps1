. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

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

        $command = @"
`$sensor = New-Object PrtgAPI.Sensor
`$sensor | Remove-Object
"@

        Invoke-Interactive $command
    }

    It "passes through with -Batch:`$false" {
        SetMultiTypeResponse

        $sensor = Get-Sensor -Count 1

        $newSensor = $sensor | Remove-Object -Force -PassThru -Batch:$false

        $newSensor | Should Be $sensor
    }

    It "passes through with -Batch:`$true" {
        SetMultiTypeResponse

        $sensor = Get-Sensor -Count 1

        $newSensor = $sensor | Remove-Object -Force -PassThru -Batch:$true

        $newSensor | Should Be $sensor
    }
}