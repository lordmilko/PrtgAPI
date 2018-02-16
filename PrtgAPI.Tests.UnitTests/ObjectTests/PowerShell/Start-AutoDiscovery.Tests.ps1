. $PSScriptRoot\Support\Standalone.ps1

Describe "Start-AutoDiscovery" -Tag @("PowerShell", "UnitTest") {
    SetActionResponse

    It "pipes from a device" {
        $device = Run Device { Get-Device }

        WithResponseArgs "AddressValidatorResponse" "api/discovernow.htm?id=40&" {
            $device | Start-AutoDiscovery
        }
    }

    It "executes with -WhatIf" {
        $device = Run Device { Get-Device }

        $device | Start-AutoDiscovery -WhatIf
    }

    It "passes through" {
        SetMultiTypeResponse

        $device = Get-Device -Count 1

        $newDevice = $device | Start-AutoDiscovery -PassThru

        $newDevice | Should Be $device
    }
}