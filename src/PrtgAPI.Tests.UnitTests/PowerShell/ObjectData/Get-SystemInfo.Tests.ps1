. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

function SetSystemTypeResponse
{
    $types = "System","Hardware","Software","Process","Service","User"

    $items = $types | foreach {
        [PrtgAPI.Tests.UnitTests.Support.TestItems.SystemInfoItem]::"$($_)Item"()
    }

    $response = New-Object PrtgAPI.Tests.UnitTests.Support.TestResponses.SystemInfoResponse -ArgumentList @($items)

    SetResponseAndClientInternal $response
}

#todo: c# needs some address validation

Describe "Get-SystemInfo" -Tag @("PowerShell", "UnitTest") {

    $typeCases = @(
        @{name="System"; type="DeviceSystemInfo"}
        @{name="Hardware"; type="DeviceHardwareInfo"}
        @{name="Software"; type="DeviceSoftwareInfo"}
        @{name="Processes"; type="DeviceProcessInfo"}
        @{name="Services"; type="DeviceServiceInfo"}
        @{name="Users"; type="DeviceUserInfo"}
    )

    SetSystemTypeResponse

    $device = Run Device { Get-Device -Count 1 }

    Context "Device" {
        It "retrieves information for a device" {

            $info = $device | Get-SystemInfo

            $info.System.Count | Should Be 1
            $info.Hardware.Count | Should Be 1
            $info.Software.Count | Should Be 1
            $info.Processes.Count | Should Be 1
            $info.Services.Count | Should Be 1
            $info.Users.Count | Should Be 1

            $info.System.Name | Should Be "vmxnet3 Ethernet Adapter"
            $info.Hardware.Name | Should Be "\\.\PHYSICALDRIVE0"
            $info.Software.Name | Should Be "Configuration Manager Client"
            $info.Processes.Name | Should Be "WmiPrvSE.exe"
            $info.Services.Name | Should Be "wuauserv"
            $info.Users.Name | Should Be "PRTG-1\NETWORK SERVICE"
        }

        It "retrieves <name> for a device" -TestCases $typeCases {
            param($name, $type)

            $info = $device | Get-SystemInfo $name

            $info.GetType().Name | Should Be $type
        }

        It "retrieves types for a device" {

            $info = $device | Get-SystemInfo Software,Hardware

            $info.Hardware.Count | Should Be 1
            $info.Hardware.Count | Should Be 1
            $info.System | Should BeNullOrEmpty
        }
    }

    Context "Manual" {
        It "retrieves information for a device ID" {

            $info = Get-SystemInfo -Id 2001

            $info.System.Count | Should Be 1
            $info.Hardware.Count | Should Be 1
            $info.Software.Count | Should Be 1
            $info.Processes.Count | Should Be 1
            $info.Services.Count | Should Be 1
            $info.Users.Count | Should Be 1

            $info.System.Name | Should Be "vmxnet3 Ethernet Adapter"
            $info.Hardware.Name | Should Be "\\.\PHYSICALDRIVE0"
            $info.Software.Name | Should Be "Configuration Manager Client"
            $info.Processes.Name | Should Be "WmiPrvSE.exe"
            $info.Services.Name | Should Be "wuauserv"
            $info.Users.Name | Should Be "PRTG-1\NETWORK SERVICE"
        }

        It "retrieves <name> for a device ID" -TestCases $typeCases {
            param($name, $type)

            $info = Get-SystemInfo -Id 1001 $name

            $info.GetType().Name | Should Be $type
        }

        It "retrieves types for a device ID" {
            $info = Get-SystemInfo -Id 1001 Software,Hardware

            $info.Hardware.Count | Should Be 1
            $info.Hardware.Count | Should Be 1
            $info.System | Should BeNullOrEmpty
        }
    }
}