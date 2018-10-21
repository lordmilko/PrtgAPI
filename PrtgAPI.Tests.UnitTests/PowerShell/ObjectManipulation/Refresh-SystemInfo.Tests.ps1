. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

Describe "Refresh-SystemInfo" -Tag @("PowerShell", "UnitTest") {

    It "processes a device" {

        $device = Run Device { Get-Device -Count 1 }

        SetAddressValidatorResponse @(
            "api/sysinfochecknow.json?id=40&kind=system&"
            "api/sysinfochecknow.json?id=40&kind=software&"
            "api/sysinfochecknow.json?id=40&kind=hardware&"
            "api/sysinfochecknow.json?id=40&kind=loggedonusers&"
            "api/sysinfochecknow.json?id=40&kind=processes&"
            "api/sysinfochecknow.json?id=40&kind=services&"
        )

        $device | Refresh-SystemInfo
    }

    It "processes an id" {

        SetAddressValidatorResponse @(
            "api/sysinfochecknow.json?id=1001&kind=system&"
            "api/sysinfochecknow.json?id=1001&kind=software&"
            "api/sysinfochecknow.json?id=1001&kind=hardware&"
            "api/sysinfochecknow.json?id=1001&kind=loggedonusers&"
            "api/sysinfochecknow.json?id=1001&kind=processes&"
            "api/sysinfochecknow.json?id=1001&kind=services&"
        )

        Refresh-SystemInfo -Id 1001
    }

    It "passes through" {

        SetMultiTypeResponse

        $device = Get-Device -Count 1

        $newDevice = $device | Refresh-SystemInfo -PassThru

        $newDevice | Should Be $device
    }

    It "processes only specified types" {
        SetAddressValidatorResponse @(
            "api/sysinfochecknow.json?id=1001&kind=loggedonusers&"
            "api/sysinfochecknow.json?id=1001&kind=services&"
        )

        Refresh-SystemInfo -Id 1001 -Type Users,Services
    }
}