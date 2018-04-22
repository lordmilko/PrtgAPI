. $PSScriptRoot\Support\Standalone.ps1

Describe "Add-Device" -Tag @("PowerShell", "UnitTest") {

    It "adds a device using default parameters" {

        SetAddressValidatorResponse "adddevice2.htm?name_=dc-1&host_=dc-1&ipversion_=0&discoverytype_=0&discoveryschedule_=0&id=1&" $true

        $params = New-DeviceParameters dc-1

        $probe = Run Probe { Get-Probe }

        $probe | Add-Device $params -Resolve:$false
    }

    It "adds a device with a custom hostname" {
        
        SetAddressValidatorResponse "adddevice2.htm?name_=dc-1&host_=192.168.0.1&ipversion_=0&discoverytype_=0&discoveryschedule_=0&id=1&" $true

        $params = New-DeviceParameters dc-1 192.168.0.1

        $probe = Run Probe { Get-Probe }

        $probe | Add-Device $params -Resolve:$false
    }

    It "adds a device with the basic parameter set" {
        SetAddressValidatorResponse "adddevice2.htm?name_=dc-3&host_=dc-3&ipversion_=0&discoverytype_=0&discoveryschedule_=0&id=2211&" $true

        $group = Run Group { Get-Group }

        $group | Add-Device dc-3 -Resolve:$false
    }

    It "adds a device with the basic parameter set specifying a host and to do an auto-discovery" {
        SetAddressValidatorResponse "adddevice2.htm?name_=dc-4&host_=192.168.0.2&ipversion_=0&discoverytype_=1&discoveryschedule_=0&id=2211&" $true

        $group = Run Group { Get-Group }

        $group | Add-Device dc-4 192.168.0.2 -AutoDiscover -Resolve:$false
    }

    It "resolves a created device" {
        SetResponseAndClientWithArguments "DiffBasedResolveResponse" $false

        $params = New-DeviceParameters dc-1

        $probe = Run Probe { Get-Probe }

        $device = $probe | Add-Device $params -Resolve

        $device.Id | Should Be 1002
    }

    It "adds a device and auto-discovers with specified templates" {
        SetAddressValidatorResponse @(
            "controls/objectdata.htm?id=40&objecttype=device&"
            "adddevice2.htm?name_=dc-1&host_=dc-1&ipversion_=0&discoverytype_=2&discoveryschedule_=0&devicetemplate_=1&devicetemplate__check=Server+RDP.odt%7cRDP+Server%7c%7c&devicetemplate__check=Windows+Advanced.odt%7cWindows+(Detailed+via+WMI)%7c%7c&devicetemplate__check=Windows+Generic.odt%7cWindows+(via+WMI)%7c%7c&id=2211&"
        )

        $group = Run Group { Get-Group }

        $group | Add-Device dc-1 -AutoDiscover -Template *wmi*,*rdp* -Resolve:$false
    }

    It "throws when no valid templates are specified" {

        SetMultiTypeResponse

        $group = Get-Group -Count 1

        { $group | Add-Device dc-1 -AutoDiscover -Template *banana* -Resolve:$false } | Should Throw "No device templates could be found that match the specified template names '*banana*'"
    }
}