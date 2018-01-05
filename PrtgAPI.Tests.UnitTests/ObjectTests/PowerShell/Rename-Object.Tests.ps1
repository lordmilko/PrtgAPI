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

    It "executes with -Batch:`$true" {

        SetMultiTypeResponse

        $sensors = Get-Sensor -Count 2

        SetAddressValidatorResponse @(
            "editsettings?id=4000,4001&name_=newName&"
        )

        $sensors | Rename-Object newName -Batch:$true
    }

    It "executes with -Batch:`$false" {

        SetMultiTypeResponse

        $sensors = Get-Sensor -Count 2

        SetAddressValidatorResponse @(
            "api/rename.htm?id=4000&value=newName&"
            "api/rename.htm?id=4001&value=newName&"
        )

        $sensors | Rename-Object newName -Batch:$false
    }
}