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

    It "specifies an ID" {

        SetAddressValidatorResponse @(
            [Request]::Get("api/discovernow.htm?id=1000")
        )

        Start-AutoDiscovery -Id 1000
    }

    It "specifies an ID with a template name" {
        SetAddressValidatorResponse @(
            [Request]::DeviceProperties(1000)
            [Request]::Get("api/discovernow.htm?id=1000&template=%22server+rdp.odt%22")
        )

        Start-AutoDiscovery -Id 1000 *rdp*
    }

    It "specifies an ID with a template target" {
        SetMultiTypeResponse

        $template = Get-DeviceTemplate *rdp*

        SetAddressValidatorResponse @(
            [Request]::Get("api/discovernow.htm?id=1000&template=%22server+rdp.odt%22")
        )

        Start-AutoDiscovery -Id 1000 $template
    }
}