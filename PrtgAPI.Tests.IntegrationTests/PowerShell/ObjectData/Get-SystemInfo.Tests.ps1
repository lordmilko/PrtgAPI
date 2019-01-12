. $PSScriptRoot\..\..\Support\PowerShell\IntegrationTestSafe.ps1

Describe "Get-SystemInfo_IT" -Tag @("PowerShell", "IntegrationTest") {
    It "retrieves info from a device" {

        $device = Get-Device -Id (Settings Device)

        $info = $device | Get-SystemInfo

        $info.System.Count | Should BeGreaterThan 0
        $info.Hardware.Count | Should BeGreaterThan 0
        $info.Software.Count | Should BeGreaterThan 0
        $info.Processes.Count | Should BeGreaterThan 0
        $info.Services.Count | Should BeGreaterThan 0
        $info.Users.Count | Should BeGreaterThan 0
    }

    It "retrieves info from a device ID" {
        $info = Get-SystemInfo -Id (Settings Device)

        $info.System.Count | Should BeGreaterThan 0
        $info.Hardware.Count | Should BeGreaterThan 0
        $info.Software.Count | Should BeGreaterThan 0
        $info.Processes.Count | Should BeGreaterThan 0
        $info.Services.Count | Should BeGreaterThan 0
        $info.Users.Count | Should BeGreaterThan 0
    }

    It "retrieves a specified information type" {
        $system = Get-SystemInfo -Id (Settings Device) System

        $system.Count | Should BeGreaterThan 1
    }

    It "retrieves as a readonly user" {
        ReadOnlyClient {
            Get-SystemInfo -Id (Settings Device)
        }
    }
}