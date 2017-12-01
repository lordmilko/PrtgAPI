. $PSScriptRoot\..\Support\ObjectProperty.ps1

Describe "Set-ObjectProperty_DeviceOrGroup_IT" {

    It "Device Type" {

        $object = Get-Device -Id (Settings Device)

        SetValue "AutoDiscoveryMode" "AutomaticDetailed"
        SetValue "AutoDiscoverySchedule" "Hourly"
    }

    It "Group Type" {
        $object = Get-Group -Id (Settings Group)

        { SetValue "AutoDiscoveryMode" "AutomaticDetailed" } | Should Throw "IPv4 Base: Required field, not defined"
        SetValue "AutoDiscoverySchedule" "Hourly"
    }
}