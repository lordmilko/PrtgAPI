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

    It "resolves a new device" {
        $name = "resolveDevice"

        $group = Get-Group -Id (Settings Group)

        $originalDevices = Get-Device

        $newDevice = $group | Add-Device $name

        $newDevices = Get-Device

        $newDevices.Count | Should BeGreaterThan $originalDevices.Count

        $diffDevice = $newDevices|where name -EQ $name

        $diffDevice.Id | Should Be $newDevice.Id

        $newDevice | Remove-Object -Force
    }
}