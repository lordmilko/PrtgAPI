. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

Describe "Refresh-SystemInfo" -Tag @("PowerShell", "UnitTest") {

    It "processes a device" {

        $device = Run Device { Get-Device -Count 1 }

        SetAddressValidatorResponse @(
            [Request]::Get("api/sysinfochecknow.json?id=40&kind=system")
            [Request]::Get("api/sysinfochecknow.json?id=40&kind=software")
            [Request]::Get("api/sysinfochecknow.json?id=40&kind=hardware")
            [Request]::Get("api/sysinfochecknow.json?id=40&kind=loggedonusers")
            [Request]::Get("api/sysinfochecknow.json?id=40&kind=processes")
            [Request]::Get("api/sysinfochecknow.json?id=40&kind=services")
        )

        $device | Refresh-SystemInfo
    }

    It "processes an id" {

        SetAddressValidatorResponse @(
            [Request]::Get("api/sysinfochecknow.json?id=1001&kind=system")
            [Request]::Get("api/sysinfochecknow.json?id=1001&kind=software")
            [Request]::Get("api/sysinfochecknow.json?id=1001&kind=hardware")
            [Request]::Get("api/sysinfochecknow.json?id=1001&kind=loggedonusers")
            [Request]::Get("api/sysinfochecknow.json?id=1001&kind=processes")
            [Request]::Get("api/sysinfochecknow.json?id=1001&kind=services")
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
            [Request]::Get("api/sysinfochecknow.json?id=1001&kind=loggedonusers")
            [Request]::Get("api/sysinfochecknow.json?id=1001&kind=services")
        )

        Refresh-SystemInfo -Id 1001 -Type Users,Services
    }
}