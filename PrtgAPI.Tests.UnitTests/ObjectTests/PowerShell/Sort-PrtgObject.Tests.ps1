. $PSScriptRoot\Support\Standalone.ps1

Describe "Sort-PrtgObject" -Tag @("PowerShell", "UnitTest") {
    SetActionResponse

    It "can execute" {
        $device = Run Device { Get-Device }

        $device | Sort-PrtgObject
    }

    It "executes with -WhatIf" {
        $device = Run Device { Get-Device }

        $device | Sort-PrtgObject -WhatIf
    }
}