. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

Describe "Start-AutoDiscovery" -Tag @("PowerShell", "UnitTest") {
    SetActionResponse

    It "pipes from a device" {
        $device = Run Device { Get-Device }

        WithResponseArgs "AddressValidatorResponse" "api/discovernow.htm?id=40&" {
            $device | Start-AutoDiscovery
        }
    }

    It "uses device templates" {
        
        SetMultiTypeResponse

        $device = Get-Device -Count 1

        $templates = Get-DeviceTemplate *wmi*

        WithResponseArgs "AddressValidatorResponse" "api/discovernow.htm?id=3000&template=%22windows+advanced.odt%22,%22windows+generic.odt%22" {
            $device | Start-AutoDiscovery $templates
        }
    }

    It "resolves and uses device templates" {
        SetMultiTypeResponse

        $device = Get-Device -Count 1

        SetAddressValidatorResponse @(
            [Request]::DeviceProperties(3000),
            [Request]::Get("api/discovernow.htm?id=3000&template=%22windows+advanced.odt%22,%22windows+generic.odt%22")
        )

        $device | Start-AutoDiscovery *wmi*
    }

    It "throws when no valid templates are specified" {

        SetMultiTypeResponse

        $device = Get-Device -Count 1

        { $device | Start-AutoDiscovery *banana* } | Should Throw "No device templates could be found that match the specified template names '*banana*'"
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