. $PSScriptRoot\Support\IntegrationTest.ps1

Describe "Add-Device_IT" {
    It "adds a new device" {

        $name = "newDevice"
        $hostName = "192.168.0.1"

        $params = New-DeviceParameters $name $hostName

        $probe = Get-Probe -Id (Settings Probe)

        $probe | Add-Device $params

        $device = @(Get-Device $name)
        $device.Count | Should Be 1
        $device.Name | Should Be $name
        $device.Host | Should Be $hostName

        $device | Remove-Object -Force
    }
}