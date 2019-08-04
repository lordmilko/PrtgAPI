. $PSScriptRoot\..\..\..\Support\PowerShell\ObjectProperty.ps1

Describe "Set-ObjectProperty_Devices_IT" -Tag @("PowerShell", "IntegrationTest") {

    $object = Get-Device -Id (Settings Device)

    It "Basic Device Settings" {
        SetValue "Hostv4" "127.0.0.1"
        GetValue "IPVersion" "IPv4"

        SetValue "Hostv6" "::1" $true # We can't revert Hostv6 back to its initial value, since there wasn't one!
        GetValue "IPVersion" "IPv6"

        SetValue "Host" "127.0.0.2"
        GetValue "IPVersion" "IPv4"
    }

    It "Additional Device Information" {
        SetValue "ServiceUrl" "http://192.168.1.1"
    }
}