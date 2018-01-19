. $PSScriptRoot\Support\IntegrationTest.ps1

Describe "Clone-Object_IT" {

    Context "Sensor" {
        It "clones a sensor" {
            $newName = "newSensor"

            $sensor = Get-Sensor -Id (Settings UpSensor)

            $device = Get-Device -Id (Settings Device)

            $newSensor = $sensor | Clone-Object $device.Id $newName

            $newSensor.Name | Should Be $newName

            $newSensor | Remove-Object -Force
        }

        It "uses the original object's name when a new one isn't specified" {
            $sensor = Get-Sensor -Id (Settings UpSensor)

            $device = Get-Device -Id (Settings Device)

            $newSensor = $sensor | Clone-Object $device.Id

            $newSensor.Name | Should Be $sensor.Name

            $newSensor | Remove-Object -Force
        }
    }

    Context "Device" {
        It "clones a device" {
            $newName = "newDevice"
            $newHost = "192.168.0.1"

            $device = Get-Device -Id (Settings Device)

            $probe = Get-Probe -Id (Settings Probe)

            $newDevice = $device | Clone-Object $probe.Id $newName $newHost

            $newDevice.Name | Should Be $newName
            $newDevice.Host | Should Be $newHost

            $newDevice | Remove-Object -Force
        }

        It "prepends 'Clone of' to the original name when a new one isn't specified" {
            $newHost = "192.168.0.1"

            $device = Get-Device -Id (Settings Device)

            $probe = Get-Probe -Id (Settings Probe)

            $newDevice = $device | Clone-Object $probe.Id -Host $newHost

            $newDevice.Name | Should Be "Clone of $($device.Name)"
            $newDevice.Host | Should Be $newHost

            $newDevice | Remove-Object -Force
        }

        It "uses the source device's host when one isn't specified" {
            $newName = "newName"

            $device = Get-Device -Id (Settings Device)

            $probe = Get-Probe -Id (Settings Probe)

            $newDevice = $device | Clone-Object $probe.Id $newName

            $newDevice.Name | Should Be $newName
            $newDevice.Host | Should Be $device.Host
        }
    }

    Context "Group" {
        It "clones a group" {
            $newName = "newGroup"

            $group = Get-Group -Id (Settings Group)

            $probe = Get-Probe -Id (Settings Probe)

            $newGroup = $group | Clone-Object $probe.Id $newName

            $newGroup.Name | Should Be $newName

            $newGroup | Remove-Object -Force
        }

        It "uses the original object's name when a new one isn't specified" {
            $group = Get-Group -Id (Settings Group)

            $probe = Get-Probe -Id (Settings Probe)

            $newGroup = $group | Clone-Object $probe.Id

            $newGroup.Name | Should Be $group.Name

            $newGroup | Remove-Object -Force
        }
    }

    Context "Trigger" {
        It "clones a trigger" {
            $sensor = Get-Sensor -Id (Settings UpSensor)

            $sensorTriggers = $sensor | Get-Trigger -Inherited $false

            $sensorTriggers | Should Be $null

            $group = Get-Group -Id 0

            $triggers = $group | Get-Trigger

            $triggers.Count | Should Be 1

            $newSensorTrigger = $triggers | Clone-Object $sensor.Id
            $newSensorTrigger.OnNotificationAction.ToString() | Should Be $triggers.OnNotificationAction.ToString()

            $newSensorTrigger | Remove-NotificationTrigger -Force
        }
    }
}