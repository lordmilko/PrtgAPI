. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

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

    It "executes with -Batch:`$true" {

        SetMultiTypeResponse

        $sensors = Get-Sensor -Count 2

        SetAddressValidatorResponse @(
            [Request]::EditSettings("id=4000,4001&name_=newName")
        )

        $sensors | Rename-Object newName -Batch:$true
    }

    It "executes with -Batch:`$false" {

        SetMultiTypeResponse

        $sensors = Get-Sensor -Count 2

        SetAddressValidatorResponse @(
            [Request]::Get("api/rename.htm?id=4000&value=newName")
            [Request]::Get("api/rename.htm?id=4001&value=newName")
        )

        $sensors | Rename-Object newName -Batch:$false
    }

    It "passes through with -Batch:`$false" {
        SetMultiTypeResponse

        $sensor = Get-Sensor -Count 1

        $newSensor = $sensor | Rename-Object newName -PassThru -Batch:$false

        $newSensor | Should Be $sensor
    }

    It "passes through with -Batch:`$true" {
        SetMultiTypeResponse

        $sensor = Get-Sensor -Count 1

        $newSensor = $sensor | Rename-Object newName -PassThru -Batch:$true

        $newSensor | Should Be $sensor
    }
}