. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

Describe "Sort-PrtgObject" -Tag @("PowerShell", "UnitTest") {
    SetActionResponse

    It "can execute" {
        $device = Run Device { Get-Device }

        WithResponseArgs "AddressValidatorResponse" "api/sortsubobjects.htm?id=40" {
            $device | Sort-PrtgObject
        }
    }

    It "executes with -WhatIf" {
        $device = Run Device { Get-Device }

        $device | Sort-PrtgObject -WhatIf
    }

    It "passes through" {
        SetMultiTypeResponse

        $device = Get-Device -Count 1

        $newDevice = $device | Sort-PrtgObject -PassThru

        $newDevice | Should Be $device
    }
}