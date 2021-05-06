. $PSScriptRoot\..\..\..\Support\PowerShell\ObjectProperty.ps1

Describe "Set-ObjectProperty_DeviceOrGroup_IT" -Tag @("PowerShell", "IntegrationTest") {

    It "Device Type" {

        $object = Get-Device -Id (Settings Device)

        SetValue "AutoDiscoveryMode" "AutomaticDetailed"
        SetValue "AutoDiscoverySchedule" "Hourly"
    }

    It "Group Type" {
        $object = Get-Group -Id (Settings Group)

        TestRequiredField { SetValue "AutoDiscoveryMode" "AutomaticDetailed" } (ForeignMessage "IPv4 Base: Required field, not defined")
        SetValue "AutoDiscoverySchedule" "Hourly"
    }
}