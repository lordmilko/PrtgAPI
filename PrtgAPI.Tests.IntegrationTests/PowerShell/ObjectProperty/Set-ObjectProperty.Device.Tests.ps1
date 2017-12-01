. $PSScriptRoot\..\Support\ObjectProperty.ps1

Describe "Set-ObjectProperty_Devices_IT" {

    $object = Get-Device -Id (Settings Device)

    It "Basic Device Settings" {
        SetValue "Hostv4" "127.0.0.1"
        GetValue "IPVersion" "IPv4"

        SetValue "Hostv6" "::1" $true
        GetValue "IPVersion" "IPv6"
    }

    It "Additional Device Information" {
        SetValue "ServiceUrl" "http://192.168.1.1"
    }
}